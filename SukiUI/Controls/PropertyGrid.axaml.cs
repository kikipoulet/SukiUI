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
                StackPanel panel = new StackPanel() { Margin = new Thickness(-10), HorizontalAlignment = HorizontalAlignment.Stretch };

                foreach (PropertyInfo property in properties.Where(prop => GetCategory(prop).Equals(category)))
                {
                    var gridItem = new Grid()
                    {
                        Width = double.NaN,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Margin = new Thickness(0, 2, 0, 0),
                        ColumnDefinitions = new ColumnDefinitions() { new ColumnDefinition() {  }, new ColumnDefinition() { } }
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

                    AddProperty(gridItem, property.GetValue(item));

                    panel.Children.Add(gridItem);
                }

                Expander groupBox = new Expander() { Header = category, Content = panel, Margin = new Thickness(0,4)};
                stackProperties.Children.Add(groupBox);

            }


            
        }

        public void AddProperty(Grid gridItem, object property)
        {
            if(property.GetType() == typeof(int) || property.GetType() == typeof(string))
            {
                var prop = new TextBox() { Text = property.ToString(), Padding = new Thickness(6),HorizontalContentAlignment = HorizontalAlignment.Right, Margin = new Thickness(2), IsEnabled = false};
                AddInGrid(gridItem,prop);
                return;
            }
            if(property.GetType() == typeof(bool))
            {
                var prop = new ToggleSwitch() {OnContent = null, IsEnabled = false ,  OffContent = null,HorizontalContentAlignment = HorizontalAlignment.Left, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(6), IsChecked = (bool)property};
                AddInGrid(gridItem,prop);
                return;
            }
            
        }

        public void AddInGrid(Grid griditem, object o)
        {
            ScrollViewer scroll = WrapInScrollView(o);
            griditem.Children.Add(scroll);
            Grid.SetColumn(scroll,1);
        }

        public ScrollViewer WrapInScrollView(object item)
        {
            var scroll = new ScrollViewer(){HorizontalScrollBarVisibility = ScrollBarVisibility.Auto};
            scroll.Content = item;
            return scroll;
        }
    }
}
