using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Layout;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;

namespace SukiUI.Controls
{
    public partial class Stepper : UserControl
    {
        public Stepper()
        {
            InitializeComponent();
            Update();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
        }

        public static readonly StyledProperty<int> IndexProperty = AvaloniaProperty.Register<Stepper, int>(nameof(Index), defaultValue: 0);

        public int Index
        {
            get { return GetValue(IndexProperty); }
            set
            {
                if (value < 0 || value > Steps.Count -1)
                    return;
                SetValue(IndexProperty, value ); 
                Update(); }
        }

        public static readonly StyledProperty<List<string>> StepsProperty =
          AvaloniaProperty.Register<Stepper, List<string>>(nameof(Steps), defaultValue: new List<string>() { "stepper"});

        public List<string> Steps
        {
            get { return GetValue(StepsProperty); }
            set { SetValue(StepsProperty, value); Update(); }
        }

        public void Update()
        {
            Grid grid = this.FindControl<Grid>("gridStepper");
            grid.Children.Clear();

            SetColumnDefinitions(grid);
            
            for (var i = 0; i < Steps.Count; i++)
            {
                AddStep(Steps[i], i, grid);
            }
            
        }

        private void SetColumnDefinitions(Grid grid)
        {
            var columns = new ColumnDefinitions();
            Steps.ForEach(s => columns.Add(new ColumnDefinition()));
            grid.ColumnDefinitions = columns;
        }

        private void AddStep(string step, int index,Grid grid)
        {

         
            Brush PrimaryColor = new SolidColorBrush( (Color)Application.Current.FindResource("SukiPrimaryColor"));
            Brush DisabledColor =  new SolidColorBrush( (Color)Application.Current.FindResource("SukiControlBorderBrush"));
            
            var griditem = new Grid(){ ColumnDefinitions = new ColumnDefinitions(){new ColumnDefinition(), new ColumnDefinition()}};

            var line = new Border() { CornerRadius = new CornerRadius(3),  Margin = new Thickness(-5,0,23,0), Background = DisabledColor, Height = 2, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center };
            var line1 = new Border() { CornerRadius = new CornerRadius(3),  Margin = new Thickness(23,0,-5,0), Background = DisabledColor, Height = 2, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center };

            if (index == 0)
                line.IsVisible = false;
            if (index == Steps.Count -1)
                line1.IsVisible = false;

            if (index == Index)
                line.Background = PrimaryColor;

            if (index < Index)
            {
                line1.Background = PrimaryColor;
                line.Background = PrimaryColor;
            }
            
            Grid.SetColumn(line,0);
            Grid.SetColumn(line1,1);
            
            griditem.Children.Add(line);
            griditem.Children.Add(line1);

            var gridBorder = new Grid();
            
            var circle = new Border()
                { Margin = new Thickness(0,0,0,2), Height = 30, Width = 30, CornerRadius = new CornerRadius(25), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

            if (index == Index)
            {
                circle.Background = PrimaryColor;
                
                circle.BorderThickness = new Thickness(0);
                circle.Child = new TextBlock() {VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Text = (index + 1).ToString(), Foreground = Brushes.White};
            }
            else if (index < Index)
            {
                circle.Background = Brushes.Transparent;
                circle.BorderThickness = new Thickness(1.5);
                circle.BorderBrush = PrimaryColor;
                circle.Child = new TextBlock() {VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Text = (index + 1).ToString(), Foreground = PrimaryColor};
            }
            else
            {
                circle.Background = Brushes.Transparent;
                circle.BorderThickness = new Thickness(1.5);
                circle.BorderBrush = DisabledColor;
                circle.Child = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Text = (index + 1).ToString(), Foreground = DisabledColor};
            }
            

            
            
            gridBorder.Children.Add(circle);
            
            gridBorder.Children.Add(new TextBlock()
            {
                FontWeight = index == Index ? FontWeight.Medium: FontWeight.Normal,
                Text = step, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0,55,0,0)
            });
            
            Grid.SetColumn(griditem,index);
            Grid.SetColumn(gridBorder,index);
            grid.Children.Add(griditem);
            grid.Children.Add(gridBorder);
        }
    }
}
