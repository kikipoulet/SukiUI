using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using SukiUI.Content;

namespace SukiUI.Controls
{
    public class Stepper : TemplatedControl
    {
        public static readonly StyledProperty<bool> AlternativeStyleProperty =
            AvaloniaProperty.Register<Stepper, bool>(nameof(AlternativeStyle));


        public static readonly StyledProperty<int> IndexProperty =
            AvaloniaProperty.Register<Stepper, int>(nameof(Index));

        public static readonly StyledProperty<IEnumerable?> StepsProperty =
            AvaloniaProperty.Register<Stepper, IEnumerable?>(nameof(Steps));

        private Grid? _grid;
        private INotifyCollectionChanged? _stepsCollection;
        private readonly List<ContentControl> _wrappedStepContents = new();

        public bool AlternativeStyle
        {
            get => GetValue(AlternativeStyleProperty);
            set => SetValue(AlternativeStyleProperty, value);
        }

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

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            if (e.NameScope.Get<Grid>("PART_GridStepper") is not { } grid)
            {
                return;
            }

            _grid = grid;
            AttachStepsCollection();
            RefreshSteps();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == StepsProperty)
                AttachStepsCollection();
            if (change.Property == IndexProperty || change.Property == StepsProperty ||
                change.Property == AlternativeStyleProperty)
                RefreshSteps();
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            DetachStepsCollection();
            base.OnDetachedFromLogicalTree(e);
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            AttachStepsCollection();
            RefreshSteps();
        }

        private void RefreshSteps()
        {
            var steps = Steps?.Cast<object>().ToArray() ?? [];

            // Detach previously wrapped step content before rebuilding, otherwise a
            // reused (non-string) step object would still have a logical parent from
            // its old ContentControl and throw when it's re-parented below.
            foreach (var contentControl in _wrappedStepContents)
                contentControl.Content = null;
            _wrappedStepContents.Clear();

            if (AlternativeStyle)
                UpdateAlternate(steps);
            else
                Update(steps);
        }

        private Control WrapStepContent(object step)
        {
            var contentControl = new ContentControl { Content = step };
            _wrappedStepContents.Add(contentControl);
            return contentControl;
        }

        private void AttachStepsCollection()
        {
            DetachStepsCollection();
            _stepsCollection = Steps as INotifyCollectionChanged;
            if (_stepsCollection is not null)
                _stepsCollection.CollectionChanged += OnStepsCollectionChanged;
        }

        private void DetachStepsCollection()
        {
            if (_stepsCollection is not null)
                _stepsCollection.CollectionChanged -= OnStepsCollectionChanged;
            _stepsCollection = null;
        }

        private void OnStepsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshSteps();
        }

        #region StepperBaseStyle

        private void Update(object[] steps)
        {
            if (_grid is null)
            {
                return;
            }

            _grid.Children.Clear();

            SetColumnDefinitions(_grid, steps);

            for (var i = 0; i < steps.Length; i++)
            {
                AddStep(steps[i], i, _grid, steps.Length);
            }
        }

        private void SetColumnDefinitions(Grid grid, object[] steps)
        {
            var columns = new ColumnDefinitions();
            for (var i = 0; i < steps.Length; i++)
            {
                columns.Add(new ColumnDefinition());
            }

            grid.ColumnDefinitions = columns;
        }

        private void AddStep(object step, int index, Grid grid, int stepCount)
        {
            var griditem = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions
                {
                    new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                }
            };

            var icon = new PathIcon
            {
                Height = 10, Width = 10, Opacity = 0.8, Data = Icons.ChevronRight, Margin = new Thickness(10, 0, 20, 0),
                Classes = { "Flippable" }
            };
            if (index == stepCount - 1)
            {
                icon.IsVisible = false;
            }

            Grid.SetColumn(icon, 2);
            griditem.Children.Add(icon);

            var circle = new GlassCard
            {
                Margin = new Thickness(0, 0, 0, 2),
                Height = 24, BorderThickness = new Thickness(1.2),
                Width = 24, Padding = new Thickness(0),
                CornerRadius = new CornerRadius(25),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (index <= Index)
            {
                circle[!BackgroundProperty] = new DynamicResourceExtension("SukiPrimaryColor");


                circle.Content = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = (index + 1).ToString(),
                    FontSize = 13,
                    Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap
                };
            }
            else
            {
                circle[!BackgroundProperty] = new DynamicResourceExtension("SukiControlBorderBrush");


                circle.Content = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = (index + 1).ToString(),
                    FontSize = 13,
                    Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap
                };
            }

            Grid.SetColumn(circle, 0);
            griditem.Children.Add(circle);
            Control content = step switch
            {
                string s => new TextBlock
                {
                    FontWeight = index <= Index
                        ? TryGetResource("DefaultDemiBold", ActualThemeVariant, out var fontWeight)
                            ? (FontWeight)fontWeight!
                            : FontWeight.Bold
                        : FontWeight.Normal,
                    Margin = new Thickness(10, 0, 0, 0),
                    Text = s,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left, TextWrapping = TextWrapping.Wrap
                },
                _ => WrapStepContent(step)
            };

            Grid.SetColumn(content, 1);
            griditem.Children.Add(content);

            if (Index != index)
            {
                griditem.Opacity = 0.7;
                griditem.RenderTransformOrigin = RelativePoint.Center;
                griditem.RenderTransform = new ScaleTransform(0.8, 0.8);
            }

            Grid.SetColumn(griditem, index);

            grid.Children.Add(griditem);
        }

        #endregion

        #region StepperAlternateStyle

        public void UpdateAlternate(object[] steps)
        {
            if (_grid is null)
            {
                return;
            }

            _grid.Children.Clear();

            SetColumnDefinitionsAlternate(_grid, steps);

            for (var i = 0; i < steps.Length; i++)
            {
                AddStepAlternate(steps[i], i, _grid, steps);
            }
        }

        private static void SetColumnDefinitionsAlternate(Grid grid, object[] steps)
        {
            var columns = new ColumnDefinitions();
            foreach (var _ in steps)
            {
                columns.Add(new ColumnDefinition());
            }

            grid.ColumnDefinitions = columns;
        }

        private void AddStepAlternate(object step, int index, Grid grid, object[] steps)
        {
            var gridItem = new Grid
                { ColumnDefinitions = new ColumnDefinitions { new ColumnDefinition(), new ColumnDefinition() } };


            var line = new Border
            {
                CornerRadius = new CornerRadius(3), Margin = new Thickness(-5, 0, 23, 0),
                Height = 2,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                [!BackgroundProperty] = new DynamicResourceExtension("SukiControlBorderBrush")
            };
            var line1 = new Border
            {
                CornerRadius = new CornerRadius(3), Margin = new Thickness(23, 0, -5, 0),
                Height = 2, HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                [!BackgroundProperty] = new DynamicResourceExtension("SukiControlBorderBrush")
            };

            if (index == 0)
            {
                line.IsVisible = false;
            }

            if (index == steps.Length - 1)
            {
                line1.IsVisible = false;
            }

            if (index == Index)
            {
                line[!BackgroundProperty] = new DynamicResourceExtension("SukiPrimaryColor");
            }

            if (index < Index)
            {
                line1[!BackgroundProperty] = new DynamicResourceExtension("SukiPrimaryColor");
                line[!BackgroundProperty] = new DynamicResourceExtension("SukiPrimaryColor");
            }

            Grid.SetColumn(line, 0);
            Grid.SetColumn(line1, 1);

            gridItem.Children.Add(line);
            gridItem.Children.Add(line1);

            var gridBorder = new Grid();

            var circle = new Border
            {
                Margin = new Thickness(0, 0, 0, 2),
                Height = 30,
                Width = 30,
                CornerRadius = new CornerRadius(25),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (index == Index)
            {
                circle[!BackgroundProperty] = new DynamicResourceExtension("SukiPrimaryColor");
                circle.BorderThickness = new Thickness(0);
                circle.Child = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = (index + 1).ToString(),
                    Foreground = Brushes.White
                };
            }
            else if (index < Index)
            {
                circle.Background = Brushes.Transparent;
                circle.BorderThickness = new Thickness(1.5);
                circle[!BorderBrushProperty] = new DynamicResourceExtension("SukiPrimaryColor");
                circle.Child = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = (index + 1).ToString(),
                    [!ForegroundProperty] = new DynamicResourceExtension("SukiPrimaryColor")
                };
            }
            else
            {
                circle.Background = Brushes.Transparent;
                circle.BorderThickness = new Thickness(1.5);
                circle[!BorderBrushProperty] = new DynamicResourceExtension("SukiControlBorderBrush");
                circle.Child = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = (index + 1).ToString(),
                    [!ForegroundProperty] = new DynamicResourceExtension("SukiControlBorderBrush")
                };
            }

            gridBorder.Children.Add(circle);

            gridBorder.Children.Add(new TextBlock
            {
                FontWeight = index == Index ? FontWeight.Medium : FontWeight.Normal,
                Text = step.ToString(), VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 55, 0, 0)
            });


            Grid.SetColumn(gridItem, index);
            Grid.SetColumn(gridBorder, index);

            grid.Children.Add(gridItem);
            grid.Children.Add(gridBorder);
        }

        #endregion
    }
}