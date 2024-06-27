using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SukiUI.Utilities.Effects;

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

        private static readonly IReadOnlyDictionary<LoadingStyle, SukiEffect> Effects =
            new Dictionary<LoadingStyle, SukiEffect>()
            {
                { LoadingStyle.Glow, SukiEffect.FromEmbeddedResource("glow") },
                { LoadingStyle.Pellets, SukiEffect.FromEmbeddedResource("pellets") }
            };

        private readonly GenericEffectDraw _draw;

        public LoadingTest()
        {
            Width = 50;
            Height = 50;
            _draw = new GenericEffectDraw(Bounds);
        }
        
        public override void Render(DrawingContext context)
        {
            _draw.Bounds = Bounds;
            _draw.Effect = Effects[LoadingStyle];
            context.Custom(_draw);
        }
    }

    public enum LoadingStyle
    {
        Glow,
        Pellets
    }
}