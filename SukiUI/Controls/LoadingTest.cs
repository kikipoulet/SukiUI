using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SukiUI.Controls
{
    public class LoadingTest : Control
    {
        public static readonly StyledProperty<LoadingStyle> LoadingStyleProperty = 
            AvaloniaProperty.Register<LoadingTest, LoadingStyle>(nameof(LoadingStyle));

        public LoadingStyle LoadingStyle
        {
            get => GetValue(LoadingStyleProperty);
            set => SetValue(LoadingStyleProperty, value);
        }

        
        
        public override void Render(DrawingContext context)
        {
            base.Render(context);
        }
    }

    public enum LoadingStyle
    {
        Glow,
        Smooth
    }
}