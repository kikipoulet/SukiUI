using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;
using System.Collections.ObjectModel;

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
        
        
        private int _index;
    
        public static readonly DirectProperty<Stepper, int> IndexProperty =
            AvaloniaProperty.RegisterDirect<Stepper, int>(nameof(Index), numpicker => numpicker.Index,
                (numpicker, v) => numpicker.Index = v, defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

        public int Index
        {
            get { return _index; }
            set
            {
                SetAndRaise(IndexProperty, ref _index, value);
                Update();
            }
        }



        private ObservableCollection<string> _steps;

        public static readonly DirectProperty<Stepper, ObservableCollection<string>> StepsProperty =
            AvaloniaProperty.RegisterDirect<Stepper, ObservableCollection<string>>(nameof(Steps), numpicker => numpicker.Steps,
                (numpicker, v) => numpicker.Steps = v, defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

        public ObservableCollection<string> Steps
        {
            get { return _steps; }
            set
            {
                SetAndRaise(StepsProperty, ref _steps, value);
                Update();
            }
        }


        public void Update()
        {
            try
            {
                Grid grid = this.FindControl<Grid>("gridStepper");
                grid.Children.Clear();

                SetColumnDefinitions(grid);

                for (var i = 0; i < Steps.Count; i++)
                {
                    AddStep(Steps[i], i, grid);
                }
            }catch(Exception exc) { }
        }

        private void SetColumnDefinitions(Grid grid)
        {
            var columns = new ColumnDefinitions();
            foreach(var s in Steps)
                columns.Add(new ColumnDefinition());
            grid.ColumnDefinitions = columns;
        }

        private void AddStep(string step, int index,Grid grid)
        {

         
            Brush PrimaryColor = new SolidColorBrush( (Color)Application.Current.FindResource("SukiPrimaryColor"));
            Brush DisabledColor =  new SolidColorBrush( (Color)Application.Current.FindResource("SukiControlBorderBrush"));
            
            var griditem = new Grid(){ ColumnDefinitions = new ColumnDefinitions(){new ColumnDefinition( GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)}};

            var icon = new MaterialIcon() { Kind = MaterialIconKind.ChevronRight, Margin = new Thickness(0,0,20,0)};
            if (index == Steps.Count - 1)
                icon.IsVisible = false;
            
   
            Grid.SetColumn(icon,2);
            griditem.Children.Add(icon);

            var gridBorder = new Grid();
            
            var circle = new Border()
                { Margin = new Thickness(0,0,0,2), Height = 24, Width = 24, CornerRadius = new CornerRadius(25), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

            if (index <= Index)
            {
                circle.Background = PrimaryColor;
                
                circle.BorderThickness = new Thickness(0);
                circle.Child = new TextBlock() {VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Text = (index + 1).ToString(), FontSize = 13, Foreground = Brushes.White};
            }
            else
            {
                circle.Background = DisabledColor;
                
                circle.BorderThickness = new Thickness(0);
                circle.Child = new TextBlock() {VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Text = (index + 1).ToString(), FontSize = 13, Foreground = Brushes.White};
            }
            
            Grid.SetColumn(circle,0);
            griditem.Children.Add(circle);

            var t = new TextBlock()
            {
                FontWeight = index <= Index ? FontWeight.DemiBold : FontWeight.Normal, Margin = new Thickness(10,0,0,0),
                Text = step, VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left, 
            };
            
            Grid.SetColumn(t,1);
            griditem.Children.Add(t);
            
            Grid.SetColumn(griditem,index);
           
            grid.Children.Add(griditem);
         
        }
    }
}
