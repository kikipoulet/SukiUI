using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using SukiUI.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reflection;

namespace SukiUI.Controls
{
    public partial class PropertyGrid : UserControl
    {
        static PropertyGrid()
        {
            ItemProperty.Changed.Subscribe(OnItemChanged);
        }

        public PropertyGrid()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<INotifyPropertyChanged?> ItemProperty = AvaloniaProperty.Register<PropertyGrid, INotifyPropertyChanged?>(nameof(Item), defaultValue: null);

        public INotifyPropertyChanged? Item
        {
            get { return GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly StyledProperty<InstanceViewModel?> InstanceProperty = AvaloniaProperty.Register<PropertyGrid, InstanceViewModel?>(nameof(Instance), defaultValue: null);

        public InstanceViewModel? Instance
        {
            get { return GetValue(InstanceProperty); }
            set { SetValue(InstanceProperty, value); }
        }

        private static void OnItemChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is PropertyGrid propertyGrid)
            {
                propertyGrid.OnItemChanged(e.OldValue, e.NewValue);
            }
        }

        private void OnItemChanged(object? oldValue, object? newValue)
        {
            SetItem(newValue as INotifyPropertyChanged);
        }

        private readonly List<IDisposable> _diposables = new();

        private void SetItem(INotifyPropertyChanged? item)
        {
            if (item is null)
            {
                return;
            }

            Instance = new InstanceViewModel(item);
        }

        private void AddProperty(DockPanel gridItem, PropertyInfo property)
        {
            var type = property.PropertyType;

            IDisposable? disposable = null;
            if (type == typeof(string))
            {
                var prop = new TextBox()
                {
                    Height = 36,
                    Width = Width / 2.5,
                    Text = property.GetValue(Item)?.ToString(),
                    Padding = new Thickness(6),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 2, 10, 2),
                    IsReadOnly = property.CanWrite,
                };

                if (property.CanWrite)
                {
                    disposable = prop.GetObservable(TextBox.TextProperty).Subscribe(value => property.SetValue(Item, value));
                }

                gridItem.Children.Add(prop);
                Grid.SetColumn(prop, 1);
            }
            else if (property.GetValue(Item) is Enum)
            {
                var names = Enum.GetNames(type);
                var prop = new ComboBox()
                {
                    Width = Width / 2.5,
                    ItemsSource = Enum.GetValues(type),
                    SelectedItem = property.GetValue(Item),
                    Height = 36,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 2, 0, 2),
                    IsEnabled = property.CanWrite,
                };

                if (property.CanWrite)
                {
                    disposable = prop.GetObservable(ComboBox.SelectedItemProperty).Subscribe(value => property.SetValue(Item, value));
                }

                gridItem.Children.Add(prop);
                Grid.SetColumn(prop, 1);
            }
            else if (type == typeof(List<int>))
            {
                var scrollviewer = new ScrollViewer();
                var stack = new StackPanel() { Orientation = Orientation.Horizontal };
                foreach (var item in (List<int>)(property.GetValue(Item)))
                {
                    stack.Children.Add(new TextBlock() { Text = item.ToString() + ", ", VerticalAlignment = VerticalAlignment.Center });
                }

                var grid = new Grid()
                {
                    Width = Width / 2.5,
                    Height = 36,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 0, 10, 2)
                };
                scrollviewer.Content = stack;
                grid.Children.Add(scrollviewer);
                gridItem.Children.Add(grid);
                Grid.SetColumn(grid, 1);
            }
            else
            {
                var prop = new Button() { Classes = { "Accent" }, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 6, 10, 2), Content = new TextBlock() { Text = "More Info" } };
                prop.Click += (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
                {
                    var content = new Border()
                    {
                        Background = new SolidColorBrush((Color)Application.Current.FindRequiredResource("SukiBackground")),
                        Width = 300,
                        Padding = new Thickness(5),
                        Child = new PropertyGrid() { Item = (INotifyPropertyChanged)property.GetValue(Item), Width = 280, HorizontalAlignment = HorizontalAlignment.Center }
                    };

                    var window = new Window()
                    {
                        Height = 500,
                        Width = 300,
                        Content = content
                    };
                    window.ShowDialog((Window)this.VisualRoot);
                };

                gridItem.Children.Add(prop);
                Grid.SetColumn(prop, 1);
            }

            if (disposable != null)
                _diposables.Add(disposable);
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);

            Instance?.Dispose();
        }
    }
}