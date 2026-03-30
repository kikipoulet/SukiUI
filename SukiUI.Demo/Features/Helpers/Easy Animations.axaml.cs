using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using SukiUI.Animations;
using SukiUI.Helpers;

namespace SukiUI.Demo.Features.Helpers
{
    public partial class Easy_Animations : UserControl
    {
        private CancellationTokenSource? _repeatCts;

        public Easy_Animations()
        {
            InitializeComponent();
        }

        private void BasicAnimation(object? sender, RoutedEventArgs e)
        {
            var border = this.Get<Border>("BasicBorder");
            if (border.Width >= 200)
                border.Animate(WidthProperty).From(200).To(50).Start();
            else
                border.Animate(WidthProperty).From(50).To(200).Start();
        }

        private void DurationEasingAnimation(object? sender, RoutedEventArgs e)
        {
            var border = this.Get<Border>("DurationBorder");
            if (border.Width >= 200)
                border.Animate(WidthProperty).From(300).To(50)
                    .WithDuration(TimeSpan.FromSeconds(1.2))
                    .WithEasing(new SukiEaseOutBack { BounceIntensity = EasingIntensity.Normal })
                    .Start();
            else
                border.Animate(WidthProperty).From(50).To(300)
                    .WithDuration(TimeSpan.FromSeconds(1.2))
                    .WithEasing(new SukiEaseOutBack { BounceIntensity = EasingIntensity.Normal })
                    .Start();
        }

        private void DelayAnimation(object? sender, RoutedEventArgs e)
        {
            var border = this.Get<Border>("DelayBorder");
            border.Animate(OpacityProperty)
                .From(1).To(0)
                .WithDelay(TimeSpan.FromSeconds(0.8))
                .WithDuration(TimeSpan.FromSeconds(1))
                .ContinueWith(() =>
                {
                    border.Animate(OpacityProperty).From(0).To(1).WithDuration(TimeSpan.FromSeconds(0.5)).Start();
                })
                .Start();
        }

        private void CallbackAnimation(object? sender, RoutedEventArgs e)
        {
            var border = this.Get<Border>("CallbackBorder");
            var text = this.Get<TextBlock>("CallbackText");

            text.Text = "Animating...";
            border.Animate(WidthProperty)
                .From(50).To(300)
                .WithDuration(TimeSpan.FromSeconds(1))
                .ContinueWith(() => text.Text = "Done!")
                .Start();
        }

        private void ParallelAnimation(object? sender, RoutedEventArgs e)
        {
            var border = this.Get<Border>("ParallelBorder");
            if (border.Width >= 200)
                border.Animate(WidthProperty).From(300).To(50)
                    .And()
                    .Animate(HeightProperty).From(120).To(50)
                    .And()
                    .Animate(Border.CornerRadiusProperty).From(new CornerRadius(60)).To(new CornerRadius(8))
                    .Start();
            else
                border.Animate(WidthProperty).From(50).To(300)
                    .And()
                    .Animate(HeightProperty).From(50).To(120)
                    .And()
                    .Animate(Border.CornerRadiusProperty).From(new CornerRadius(8)).To(new CornerRadius(60))
                    .Start();
        }

        private void SequentialAnimation(object? sender, RoutedEventArgs e)
        {
            var border = this.Get<Border>("SequentialBorder");

            if (border.Opacity < 1)
            {
                border.Animate(OpacityProperty).From(0).To(1).WithDuration(TimeSpan.FromSeconds(0.3)).Start();
                return;
            }

            border.Animate(WidthProperty)
                .From(50).To(250)
                .WithDuration(TimeSpan.FromSeconds(0.4))
                .Then()
                .Animate(HeightProperty)
                .From(50).To(120)
                .WithDuration(TimeSpan.FromSeconds(0.4))
                .Then()
                .Animate(OpacityProperty)
                .From(1).To(0)
                .WithDuration(TimeSpan.FromSeconds(0.6))
                .Start();
        }

        private void StartRepeatAnimation(object? sender, RoutedEventArgs e)
        {
            _repeatCts?.Cancel();
            var border = this.Get<Border>("RepeatBorder");
            _repeatCts = border.Animate(OpacityProperty)
                .From(1).To(0.2)
                .WithDuration(TimeSpan.FromSeconds(0.8))
                .RepeatForever();
        }

        private void StopRepeatAnimation(object? sender, RoutedEventArgs e)
        {
            _repeatCts?.Cancel();
            var border = this.Get<Border>("RepeatBorder");
            border.Animate(OpacityProperty).From(border.Opacity).To(1).WithDuration(TimeSpan.FromSeconds(0.2)).Start();
        }

        private void RepeatNAnimation(object? sender, RoutedEventArgs e)
        {
            var border = this.Get<Border>("RepeatNBorder");
            border.Animate(WidthProperty)
                .From(50).To(200)
                .WithDuration(TimeSpan.FromSeconds(0.4))
                .Repeat(3);
        }

        private async void AsyncAnimation(object? sender, RoutedEventArgs e)
        {
            var border = this.Get<Border>("AsyncBorder");

            await border.Animate(WidthProperty)
                .From(50).To(250)
                .WithDuration(TimeSpan.FromSeconds(0.5))
                .RunAsync();

            border.Background = new SolidColorBrush(Colors.Green);

            await Task.Delay(1000);

            await border.Animate(WidthProperty)
                .From(250).To(50)
                .WithDuration(TimeSpan.FromSeconds(0.5))
                .RunAsync();

            border.Background = new SolidColorBrush(Colors.Crimson);
        }

        private void ComboAnimation(object? sender, RoutedEventArgs e)
        {
            var border = this.Get<Border>("ComboBorder");

            if (border.Opacity < 1)
            {
                border.Animate(OpacityProperty).From(border.Opacity).To(1).WithDuration(TimeSpan.FromSeconds(0.2)).Start();
                return;
            }

            border.Animate(WidthProperty)
                .From(50).To(250)
                .And()
                .Animate(HeightProperty)
                .From(50).To(120)
                .Then()
                .Animate(Border.CornerRadiusProperty)
                .From(new CornerRadius(8)).To(new CornerRadius(60))
                .And()
                .Animate(OpacityProperty)
                .From(1).To(0.3)
                .Start();
        }
    }
}
