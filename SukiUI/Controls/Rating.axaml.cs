using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Material.Icons.Avalonia;

namespace SukiUI.Controls;

 public partial class Rating : UserControl
    {

        
        public static readonly StyledProperty<int> MaxStarsProperty =
            AvaloniaProperty.Register<Rating, int>(nameof(MaxStars), 5);

        public static readonly StyledProperty<int> SelectedStarsProperty =
            AvaloniaProperty.Register<Rating, int>(nameof(SelectedStars));

        public Rating()
        {
            InitializeComponent();
            InitializeStars();
            
        }
        
       


        public int MaxStars
        {
            get => GetValue(MaxStarsProperty);
            set
            {
                SetValue(MaxStarsProperty, value);
                InitializeStars();
              
            }
        }

        public int SelectedStars
        {
            get => GetValue(SelectedStarsProperty);
            set { SetValue(SelectedStarsProperty, value); UpdateStarSelection(); }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InitializeStars()
        {
            var starPanel = this.FindControl<StackPanel>("StarPanel");
            starPanel.Children.Clear();

            for (int i = 1; i <= MaxStars; i++)
            {
                var starButton = new ToggleButton { Content = i.ToString(), Classes = new Classes("Star") };
                starButton.Click += OnStarClick;
                
                starPanel.Children.Add(starButton);
            }
        }

        private void OnStarClick(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button && int.TryParse(button.Content.ToString(), out int star))
            {
                SelectedStars = star;
                UpdateStarSelection();
            }
        }

        private void UpdateStarSelection()
        {
            var starPanel = this.FindControl<StackPanel>("StarPanel");

            for (int i = 1; i <= MaxStars; i++)
            {
                var starButton = (ToggleButton)starPanel.Children[i - 1];
                starButton.IsChecked = i <= SelectedStars;
            }
        }
    }