using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace SukiUI.Controls
{
    public class VerticalStepper : TemplatedControl
    {
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

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _itemsControl = e.NameScope.Get<ItemsControl>("PART_ItemsControl");
            RefreshSteps();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == IndexProperty)
                UpdateStates();
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
            foreach (var item in _stepItems)
            {
                item.IsCompleted = item.StepIndex < Index;
                item.IsActive = item.StepIndex == Index;
                item.IsPending = item.StepIndex > Index;
                item.ShowCheckMark = ShowCheckMark && item.IsCompleted;
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
