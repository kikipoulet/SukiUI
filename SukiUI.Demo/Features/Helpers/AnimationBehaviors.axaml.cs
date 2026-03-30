using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using SukiUI.Animations;

namespace SukiUI.Demo.Features.Helpers
{
    public partial class AnimationBehaviors : UserControl
    {
        private int _fadeInCounter;

        private static readonly Color[] CardColors =
        {
            Colors.DodgerBlue,
            Colors.MediumSeaGreen,
            Colors.MediumPurple,
            Colors.HotPink,
            Colors.Orange,
            Colors.Crimson,
            Colors.Gold,
        };

        public AnimationBehaviors()
        {
            InitializeComponent();

            var slider = this.Get<Slider>("HoverScale");
            var text = this.Get<TextBlock>("HoverScaleText");
            text.Text = slider.Value.ToString("F1");
            slider.PropertyChanged += (_, e) =>
            {
                if (e.Property.Name == "Value")
                    text.Text = ((Slider)e.Sender).Value.ToString("F1");
            };
        }

        private void AddFadeInCard(object? sender, RoutedEventArgs e)
        {
            var container = this.Get<StackPanel>("FadeInContainer");
            var color = CardColors[_fadeInCounter % CardColors.Length];
            _fadeInCounter++;

            var card = new Border
            {
                Background = new SolidColorBrush(color),
                CornerRadius = new CornerRadius(10),
                Height = 50,
                Width = 280,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                Child = new TextBlock
                {
                    Text = $"Card #{_fadeInCounter}",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                }
            };

            FadeInBehavior.SetEnable(card, true);
            FadeInBehavior.SetDuration(card, TimeSpan.FromMilliseconds(600));
            FadeInBehavior.SetScale(card, 0.6);

            container.Children.Add(card);
        }
    }
}
