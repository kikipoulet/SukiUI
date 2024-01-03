using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Threading;
using SukiUI.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;

namespace SukiUI.Controls
{
    public class Stepper : TemplatedControl
    {
        public static readonly StyledProperty<int> IndexProperty =
            AvaloniaProperty.Register<Stepper, int>(nameof(Index));

        public int Index
        {
            get => GetValue(IndexProperty);
            set => SetValue(IndexProperty, value);
        }

        public static readonly StyledProperty<IEnumerable?> StepsProperty =
            AvaloniaProperty.Register<Stepper, IEnumerable?>(nameof(Steps));

        public IEnumerable? Steps
        {
            get => GetValue(StepsProperty);
            set => SetValue(StepsProperty, value);
        }

        private Grid? _grid;

        private static readonly IBrush DisabledColor = new SolidColorBrush(Color.FromArgb(100, 150, 150, 150));

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            if (e.NameScope.Get<Grid>("PART_GridStepper") is not { } grid) return;
            _grid = grid;
            this.GetObservable(IndexProperty)
                .ObserveOn(new AvaloniaSynchronizationContext())
                .Subscribe(_ => StepsChangedHandler(Steps));
            this.GetObservable(StepsProperty)
                .ObserveOn(new AvaloniaSynchronizationContext())
                .Subscribe(StepsChangedHandler);
        }

        private void StepsChangedHandler(IEnumerable? newSteps)
        {
            if (newSteps is null) return;
            if (newSteps is not IEnumerable<object> stepsEnumerable) return;
            var steps = stepsEnumerable.ToArray();
            Update(steps);
            if (newSteps is INotifyCollectionChanged notify)
                notify.CollectionChanged += (_, _) => Update(steps);
        }

        private void Update(object[] steps)
        {
            if (_grid is null) return;
            _grid.Children.Clear();

            SetColumnDefinitions(_grid, steps);

            for (var i = 0; i < steps.Length; i++)
                AddStep(steps[i], i, _grid, steps.Length);
        }

        private void SetColumnDefinitions(Grid grid, object[] steps)
        {
            var columns = new ColumnDefinitions();
            foreach (var s in steps)
                columns.Add(new ColumnDefinition());
            grid.ColumnDefinitions = columns;
        }

        private void AddStep(object step, int index, Grid grid, int stepCount)
        {
            var griditem = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitions()
                    { new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto) }
            };

            var icon = new PathIcon()
            { Height = 10, Width = 10, Data = Icons.ChevronRight, Margin = new Thickness(0, 0, 20, 0) };
            if (index == stepCount - 1)
                icon.IsVisible = false;

            Grid.SetColumn(icon, 2);
            griditem.Children.Add(icon);

            var circle = new Border()
            {
                Margin = new Thickness(0, 0, 0, 2),
                Height = 24,
                Width = 24,
                CornerRadius = new CornerRadius(25),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (index <= Index)
            {
                circle[!BackgroundProperty] = new DynamicResourceExtension("SukiPrimaryColor");

                circle.BorderThickness = new Thickness(0);
                circle.Child = new TextBlock()
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
                circle.Background = DisabledColor;

                circle.BorderThickness = new Thickness(0);
                circle.Child = new TextBlock()
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
                string s => new TextBlock()
                {
                    FontWeight = index <= Index ? FontWeight.DemiBold : FontWeight.Normal,
                    Margin = new Thickness(10, 0, 0, 0),
                    Text = s,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left, TextWrapping = TextWrapping.Wrap
                },
                _ => new ContentControl() { Content = step }
            };

            Grid.SetColumn(content, 1);
            griditem.Children.Add(content);

            Grid.SetColumn(griditem, index);

            grid.Children.Add(griditem);
        }
    }
}