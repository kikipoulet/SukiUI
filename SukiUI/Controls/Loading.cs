using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using SkiaSharp;
using SukiUI.Extensions;
using SukiUI.Utilities.Effects;

namespace SukiUI.Controls
{
    public class Loading : Control
    {
        public static readonly StyledProperty<LoadingStyle> LoadingStyleProperty =
            AvaloniaProperty.Register<Loading, LoadingStyle>(nameof(LoadingStyle), defaultValue: LoadingStyle.Glow);

        public LoadingStyle LoadingStyle
        {
            get => GetValue(LoadingStyleProperty);
            set => SetValue(LoadingStyleProperty, value);
        }

        public static readonly StyledProperty<IBrush?> ForegroundProperty =
            AvaloniaProperty.Register<Loading, IBrush?>(nameof(Foreground));

        public IBrush? Foreground
        {
            get => GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        private static readonly IReadOnlyDictionary<LoadingStyle, SukiEffect> Effects =
            new Dictionary<LoadingStyle, SukiEffect>()
            {
                { LoadingStyle.Glow, SukiEffect.FromEmbeddedResource("glow") },
                { LoadingStyle.Pellets, SukiEffect.FromEmbeddedResource("pellets") },
            };

        private readonly LoadingEffectDraw _draw;

        public Loading()
        {
            Width = 50;
            Height = 50;
            _draw = new LoadingEffectDraw(Bounds);
        }

        public override void Render(DrawingContext context)
        {
            _draw.Bounds = Bounds;
            _draw.Effect = Effects[LoadingStyle];
            if (Foreground is ImmutableSolidColorBrush brush)
                brush.Color.ToFloatArrayNonAlloc(_draw.Color);
            context.Custom(_draw);
        }

        public class LoadingEffectDraw : EffectDrawBase
        {
            public float[] Color { get; } = { 1.0f, 1.0f, 1.0f };

            public LoadingEffectDraw(Rect bounds) : base(bounds)
            {
                AnimationEnabled = true;
                AnimationSpeedScale = 2f;
            }

            protected override void Render(SKCanvas canvas, SKRect rect)
            {
                canvas.Scale(1, -1);
                canvas.Translate(0, (float)-Bounds.Height);
                using var mainShaderPaint = new SKPaint();

                if (Effect is not null)
                {
                    using var shader = EffectWithCustomUniforms(effect => new SKRuntimeEffectUniforms(effect)
                    {
                        { "iForeground", Color }
                    });
                    mainShaderPaint.Shader = shader;
                    canvas.DrawRect(rect, mainShaderPaint);
                }

                canvas.Restore();
            }

            // I'm not really sure how to render this properly in software fallback scenarios.
            // This is likely to cause issues with the previewer.
            // Might be worth just drawing a circle or something...
            protected override void RenderSoftware(SKCanvas canvas, SKRect rect)
            {
                throw new System.NotImplementedException();
            }
        }
    }

    public enum LoadingStyle
    {
        Glow,
        Pellets
    }
}