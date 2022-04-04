using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Diagnostics;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace SukiUI.Controls
{
    public partial class PropertyGrid : UserControl
    {
        public PropertyGrid()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<object> ItemProperty = AvaloniaProperty.Register<PropertyGrid, object>(nameof(Item), defaultValue: new object());

        public object Item
        {
            get { return GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); SetItem(value); }
        }


        public string GetCategory(PropertyInfo property)
        {
            CategoryAttribute[] attributes = (CategoryAttribute[])property.GetCustomAttributes(typeof(CategoryAttribute), false);

            if (attributes.Any())
                return attributes[0].Category;

            return "Informations";
        }

        public void SetItem(object item)
        {
            var stackProperties = this.FindControl<StackPanel>("StackProperties");
            stackProperties.Children.Clear();

            List<PropertyInfo> properties = item.GetType().GetProperties().ToList();

            //  List<string> categories =  properties.Select(p => (CategoryAttribute)(TypeDescriptor.GetAttributes(p)[typeof(CategoryAttribute)])).Select(c => c.Category).Distinct().ToList();

            List<string> categories = properties.Select(prop => GetCategory(prop)).Distinct().ToList();

            foreach(var category in categories)
            {
                StackPanel panel = new StackPanel() { Margin = new Thickness(0,-10,-20,-12), Width = Math.Abs( Width-20), HorizontalAlignment = HorizontalAlignment.Center};

                foreach (PropertyInfo property in properties.Where(prop => GetCategory(prop).Equals(category)))
                {
                    var gridItem = new DockPanel() { Margin= new Thickness(0,1)};
                    var text = new TextBlock()
                    {
                        Text = property.Name,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(8,4,4,4),
                        FontSize = 14,
                        Width = Math.Abs(Width / 2 - 40)

                    };
                   
                    gridItem.Children.Add(text);
                    DockPanel.SetDock(text, Dock.Left);

                    AddProperty(gridItem, property);

                    panel.Children.Add(gridItem);
                }

                Expander groupBox = new Expander() { Header = category, Content = panel, Margin = new Thickness(0,4), IsExpanded = true};
                stackProperties.Children.Add(groupBox);

            }


            
        }

     /*   public void SetItem(object item)
        {
            var stackProperties = this.FindControl<StackPanel>("StackProperties");
            stackProperties.Children.Clear();

            List<PropertyInfo> properties = item.GetType().GetProperties().ToList();

            //  List<string> categories =  properties.Select(p => (CategoryAttribute)(TypeDescriptor.GetAttributes(p)[typeof(CategoryAttribute)])).Select(c => c.Category).Distinct().ToList();

            List<string> categories = properties.Select(prop => GetCategory(prop)).Distinct().ToList();

            foreach (var category in categories)
            {
                StackPanel panel = new StackPanel() { Margin = new Thickness(-10), Width = Math.Abs(Width - 40), HorizontalAlignment = HorizontalAlignment.Center };

                foreach (PropertyInfo property in properties.Where(prop => GetCategory(prop).Equals(category)))
                {
                    var gridItem = new Grid()
                    {
                        Width = double.NaN,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Margin = new Thickness(0, 2, 0, 0),
                        ColumnDefinitions = new ColumnDefinitions() { new ColumnDefinition() { }, new ColumnDefinition() { } }
                    };
                    var text = new TextBlock()
                    {
                        Text = property.Name,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(4),
                        FontSize = 14,
                        Width = double.NaN

                    };
                    gridItem.Children.Add(text);
                    Grid.SetColumn(text, 0);

                    AddProperty(gridItem, property);

                    panel.Children.Add(gridItem);
                }

                Expander groupBox = new Expander() { Header = category, Content = panel, Margin = new Thickness(0, 4) };
                stackProperties.Children.Add(groupBox);

            }



        } */

        public void AddProperty(DockPanel gridItem, PropertyInfo property)
        {
            if(property.GetValue(Item) == null) 
                return;

            if(property.GetValue(Item).GetType() == typeof(string))
            {
                var prop = new TextBox() { Height = 36, Width = Width/2 , Text = property.GetValue(Item).ToString(), Padding = new Thickness(6),HorizontalAlignment = HorizontalAlignment.Right, HorizontalContentAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,2,10,2)};
                prop.GetObservable(TextBox.TextProperty).Subscribe(value => property.SetValue(Item, value));
                gridItem.Children.Add(prop);
                Grid.SetColumn(prop, 1);
         
            }

            else if (property.GetValue(Item).GetType() == typeof(int))
            {
                var prop = new NumericUpDown() {  Width = Width / 2 +20, Height = 36, Value = Double.Parse(property.GetValue(Item).ToString()), Increment = 1, Padding = new Thickness(0), HorizontalAlignment = HorizontalAlignment.Right, HorizontalContentAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,2)};
                prop.GetObservable(NumericUpDown.ValueProperty).Subscribe(value => property.SetValue(Item,Int32.Parse( value.ToString())));
                gridItem.Children.Add(prop);
                Grid.SetColumn(prop, 1);
            }

            else if (property.GetValue(Item).GetType() == typeof(double))
            {
                var prop = new NumericUpDown() { Width = Width / 2 + 20, Height = 36, Value = (double)property.GetValue(Item), Increment= 0.001, Padding = new Thickness(0), HorizontalAlignment = HorizontalAlignment.Right, HorizontalContentAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 2) };
                prop.GetObservable(NumericUpDown.ValueProperty).Subscribe(value => property.SetValue(Item, value));
                gridItem.Children.Add(prop);
                Grid.SetColumn(prop, 1);
            }

            else if (property.GetValue(Item).GetType() == typeof(bool))
            {
                var prop = new ToggleSwitch() {  OnContent = new Grid() { Background = Brushes.Transparent}, Height = 36,  OffContent = new Grid() { Background = Brushes.Transparent }, HorizontalContentAlignment = HorizontalAlignment.Right, HorizontalAlignment = HorizontalAlignment.Right, IsChecked = (bool)property.GetValue(Item) };
                prop.GetObservable(ToggleSwitch.IsCheckedProperty).Subscribe(value => property.SetValue(Item, value));
                gridItem.Children.Add(prop);
                Grid.SetColumn(prop, 1);
            } 
            
            else if (property.GetValue(Item) is Enum)
            {
                var type = property.GetValue(Item).GetType();
                var names = Enum.GetNames(type);
                var prop = new ComboBox() { Width = Width / 2 , Items = Enum.GetValues(type), SelectedItem = property.GetValue(Item), Height = 36, HorizontalContentAlignment = HorizontalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,2,10, 2) };
                prop.GetObservable(ComboBox.SelectedItemProperty).Subscribe(value => property.SetValue(Item, value));
                gridItem.Children.Add(prop);
                Grid.SetColumn(prop, 1);
            }
            else
            {
                var prop = new Button() { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,2,10,2), Content = "More Info"};
                prop.Click += (object sender, Avalonia.Interactivity.RoutedEventArgs e) => {

                    var content = new Border() { Background = Brushes.White, Width = 300, Padding = new Thickness(5), 
                        Child = new PropertyGrid() { Item = property.GetValue(Item), Width = 280, HorizontalAlignment = HorizontalAlignment.Center } };
                

                    var window = new Window() { Height = 500, Width = 300, Content = content };
                    window.ShowDialog((Window)this.VisualRoot);
                };

                gridItem.Children.Add(prop);
                Grid.SetColumn(prop, 1);
            }
            
        }

       
    }
}
