using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Threading;
using Avalonia.Styling;

namespace SukiUI.Controls
{
    public class VerticalStepper : TemplatedControl
    {
        public static FuncValueConverter<int, int> IndexToDisplayConverter { get; } = new(x => x + 1);

        public static readonly StyledProperty<int> IndexProperty =
            AvaloniaProperty.Register<VerticalStepper, int>(nameof(Index));

        public static readonly StyledProperty<IEnumerable?> StepsProperty =
            AvaloniaProperty.Register<VerticalStepper, IEnumerable?>(nameof(Steps));

        public static readonly StyledProperty<bool> ShowCheckMarkProperty =
            AvaloniaProperty.Register<VerticalStepper, bool>(nameof(ShowCheckMark), true);

        public int Index
        {
            get => GetValue(IndexProperty);
            set => SetValue(IndexProperty, value);
        }

        public IEnumerable? Steps
        {
            get => GetValue(StepsProperty);
            set => SetValue(StepsProperty, value);
        }

        public bool ShowCheckMark
        {
            get => GetValue(ShowCheckMarkProperty);
            set => SetValue(ShowCheckMarkProperty, value);
        }

        private readonly List<VerticalStepperItem> _stepItems = new();
        private ItemsControl? _itemsControl;
        private ScrollViewer? _scrollViewer;
        private CancellationTokenSource? _scrollCts;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _scrollViewer = e.NameScope.Get<ScrollViewer>("PART_ScrollViewer");
            _itemsControl = e.NameScope.Get<ItemsControl>("PART_ItemsControl");
            RefreshSteps();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == IndexProperty)
            {
                UpdateStates();
                ScrollToActiveStep();
            }
            else if (change.Property == StepsProperty)
                RefreshSteps();
            else if (change.Property == ShowCheckMarkProperty)
                UpdateStates();
        }

        private void RefreshSteps()
        {
            _stepItems.Clear();

            if (Steps is null)
            {
                if (_itemsControl is not null)
                    _itemsControl.ItemsSource = null;
                return;
            }

            var index = 0;
            foreach (var step in Steps)
            {
                var item = new VerticalStepperItem
                {
                    StepIndex = index,
                    Title = step switch
                    {
                        string s => s,
                        VerticalStepItem v => v.Title ?? "",
                        _ => step?.ToString() ?? ""
                    },
                    Description = step is VerticalStepItem vStep ? vStep.Description : null,
                    ShowCheckMark = ShowCheckMark && index < Index
                };
                
                _stepItems.Add(item);
                index++;
            }
            
            if (_itemsControl is not null)
                _itemsControl.ItemsSource = _stepItems;

            UpdateStates();

            if (Steps is INotifyCollectionChanged notify)
                notify.CollectionChanged += (_, _) => RefreshSteps();
        }

        private void UpdateStates()
        {
            for (var i = 0; i < _stepItems.Count; i++)
            {
                var item = _stepItems[i];
                item.IsCompleted = i < Index;
                item.IsActive = i == Index;
                item.IsPending = i > Index;
                item.ShowCheckMark = ShowCheckMark && item.IsCompleted;
                
                item.HasConnectorUp = i > 0;
                item.HasConnectorDown = item.IsCompleted && i < _stepItems.Count - 1;
                item.ConnectorDownToActive = item.HasConnectorDown && i + 1 == Index;
            }
        }

        private async void ScrollToActiveStep()
        {
            if (_scrollViewer is null || Index < 0 || Index >= _stepItems.Count)
                return;

            var activeItem = _stepItems[Index];

            if (!activeItem.IsInitialized)
            {
                activeItem.Initialized += (_, _) => ScrollToActiveStep();
                return;
            }

            _scrollCts?.Cancel();
            _scrollCts = new CancellationTokenSource();
            var token = _scrollCts.Token;

            try
            {
                await Task.Delay(700, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (token.IsCancellationRequested)
                return;

            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Loaded);
            if (token.IsCancellationRequested)
                return;

            var itemPosition = activeItem.TranslatePoint(new Point(0, 0), _scrollViewer);
            if (itemPosition is null)
            {
                _scrollViewer.InvalidateArrange();
                await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);
                itemPosition = activeItem.TranslatePoint(new Point(0, 0), _scrollViewer);
                if (itemPosition is null)
                    return;
            }

            var itemHeight = activeItem.Bounds.Height;
            var viewportHeight = _scrollViewer.Viewport.Height;
            var extentHeight = _scrollViewer.Extent.Height;
            var maxOffset = Math.Max(0, extentHeight - viewportHeight);

            var targetOffset = _scrollViewer.Offset.Y + itemPosition.Value.Y - (viewportHeight / 2) + (itemHeight / 2);
            targetOffset = Math.Max(0, Math.Min(targetOffset, maxOffset));

            var currentOffset = _scrollViewer.Offset.Y;
            if (Math.Abs(currentOffset - targetOffset) < 0.5)
                return;

            try
            {
                var animation = new Avalonia.Animation.Animation
                {
                    Duration = TimeSpan.FromMilliseconds(300),
                    Easing = new CubicEaseOut(),
                    FillMode = FillMode.None,
                    Children =
                    {
                        new KeyFrame
                        {
                            Setters = { new Setter { Property = ScrollViewer.OffsetProperty, Value = _scrollViewer.Offset } },
                            KeyTime = TimeSpan.Zero
                        },
                        new KeyFrame
                        {
                            Setters = { new Setter { Property = ScrollViewer.OffsetProperty, Value = new Vector(0, targetOffset) } },
                            Cue = new Cue(1.0)
                        }
                    }
                };

                await animation.RunAsync(_scrollViewer, token);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    public class VerticalStepperItem : TemplatedControl
    {
        public static readonly StyledProperty<int> StepIndexProperty =
            AvaloniaProperty.Register<VerticalStepperItem, int>(nameof(StepIndex));

        public static readonly StyledProperty<string?> TitleProperty =
            AvaloniaProperty.Register<VerticalStepperItem, string?>(nameof(Title));

        public static readonly StyledProperty<string?> DescriptionProperty =
            AvaloniaProperty.Register<VerticalStepperItem, string?>(nameof(Description));

        public static readonly StyledProperty<bool> IsCompletedProperty =
            AvaloniaProperty.Register<VerticalStepperItem, bool>(nameof(IsCompleted));

        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<VerticalStepperItem, bool>(nameof(IsActive));

        public static readonly StyledProperty<bool> IsPendingProperty =
            AvaloniaProperty.Register<VerticalStepperItem, bool>(nameof(IsPending));

        public static readonly StyledProperty<bool> ShowCheckMarkProperty =
            AvaloniaProperty.Register<VerticalStepperItem, bool>(nameof(ShowCheckMark));

        public static readonly StyledProperty<bool> HasConnectorUpProperty =
            AvaloniaProperty.Register<VerticalStepperItem, bool>(nameof(HasConnectorUp));

        public static readonly StyledProperty<bool> HasConnectorDownProperty =
            AvaloniaProperty.Register<VerticalStepperItem, bool>(nameof(HasConnectorDown));

        public static readonly StyledProperty<bool> ConnectorDownToActiveProperty =
            AvaloniaProperty.Register<VerticalStepperItem, bool>(nameof(ConnectorDownToActive));

        public int StepIndex
        {
            get => GetValue(StepIndexProperty);
            set => SetValue(StepIndexProperty, value);
        }

        public string? Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string? Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public bool IsCompleted
        {
            get => GetValue(IsCompletedProperty);
            set => SetValue(IsCompletedProperty, value);
        }

        public bool IsActive
        {
            get => GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public bool IsPending
        {
            get => GetValue(IsPendingProperty);
            set => SetValue(IsPendingProperty, value);
        }

        public bool ShowCheckMark
        {
            get => GetValue(ShowCheckMarkProperty);
            set => SetValue(ShowCheckMarkProperty, value);
        }

        public bool HasConnectorUp
        {
            get => GetValue(HasConnectorUpProperty);
            set => SetValue(HasConnectorUpProperty, value);
        }

        public bool HasConnectorDown
        {
            get => GetValue(HasConnectorDownProperty);
            set => SetValue(HasConnectorDownProperty, value);
        }

        public bool ConnectorDownToActive
        {
            get => GetValue(ConnectorDownToActiveProperty);
            set => SetValue(ConnectorDownToActiveProperty, value);
        }
    }

    public class VerticalStepItem
    {
        public string? Title { get; set; }
        public string? Description { get; set; }

        public VerticalStepItem() { }

        public VerticalStepItem(string title, string? description = null)
        {
            Title = title;
            Description = description;
        }
    }
}
