using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Helpers;

namespace SukiUI.Demo.Features.Helpers
{
    public partial class EasingDemoBoxControl : UserControl
    {
        
        public static readonly StyledProperty<Easing> EaseProperty =
            AvaloniaProperty.Register<EasingDemoBoxControl, Easing>(nameof(Ease), defaultValue: new SineEaseIn());

        public Easing Ease
        {
            get => GetValue(EaseProperty);
            set => SetValue(EaseProperty, value);
        }
        
        public EasingDemoBoxControl()
        {
            InitializeComponent();
        }

        private void LaunchBox(object? sender, RoutedEventArgs e)
        {
            var box = this.Get<Border>("Box");
            if(box.Margin.Left == 0)
                 box.Animate(MarginProperty)
                    .From(new Thickness(0))
                    .To(new Thickness(240, 0, 0, 0))
                    .WithEasing(Ease)
                    .WithDuration(TimeSpan.FromSeconds(1))
                    .Start();
            else
                box.Animate(MarginProperty)
                    .To(new Thickness(0))
                    .From(new Thickness(240, 0, 0, 0))
                    .WithEasing(Ease)
                    .WithDuration(TimeSpan.FromSeconds(1))
                    .Start();
        }
    }
}