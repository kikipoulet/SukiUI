using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using SkiaSharp;
using SukiUI.Extensions;
using SukiUI.Utilities.Effects;

namespace SukiUI.Controls
{
    public class Loading : Control
    {
        public static readonly StyledProperty<LoadingStyle> LoadingStyleProperty =
            AvaloniaProperty.Register<Loading, LoadingStyle>(nameof(LoadingStyle), LoadingStyle.Simple,
                coerce: (_, value) => Enum.IsDefined(typeof(LoadingStyle), value) ? value : LoadingStyle.Simple);

        public static readonly StyledProperty<IBrush?> ForegroundProperty =
            AvaloniaProperty.Register<Loading, IBrush?>(nameof(Foreground));

        private static readonly IReadOnlyDictionary<LoadingStyle, SukiEffect> Effects =
            new Dictionary<LoadingStyle, SukiEffect>
            {
                { LoadingStyle.Simple, SukiEffect.FromEmbeddedResource("simple") },
                { LoadingStyle.Glow, SukiEffect.FromEmbeddedResource("glow") },
                { LoadingStyle.Pellets, SukiEffect.FromEmbeddedResource("pellets") }
            };

        private readonly float[] _color = new float[3];

        private CompositionCustomVisual? _customVisual;

        public Loading()
        {
            Width = 50;
            Height = 50;
        }

        public LoadingStyle LoadingStyle
        {
            get => GetValue(LoadingStyleProperty);
            set => SetValue(LoadingStyleProperty, value);
        }

        public IBrush? Foreground
        {
            get => GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            var comp = ElementComposition.GetElementVisual(this)?.Compositor;
            if (comp == null || _customVisual?.Compositor == comp) return;
            var visualHandler = new LoadingEffectDraw();
            _customVisual = comp.CreateCustomVisual(visualHandler);
            ElementComposition.SetElementChildVisual(this, _customVisual);
            _customVisual.SendHandlerMessage(EffectDrawBase.StartAnimations);
            if (Foreground is null)
                this[!ForegroundProperty] = new DynamicResourceExtension("SukiPrimaryColor");
            if (Foreground is ISolidColorBrush brush)
                brush.Color.ToFloatArrayNonAlloc(_color);
            _customVisual.SendHandlerMessage((float[])_color.Clone());
            _customVisual.SendHandlerMessage(Effects[LoadingStyle]);
            Update();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _customVisual?.SendHandlerMessage(EffectDrawBase.StopAnimations);
            ElementComposition.SetElementChildVisual(this, null);
            _customVisual = null;
            base.OnDetachedFromVisualTree(e);
        }

        private void Update()
        {
            if (_customVisual == null) return;
            _customVisual.Size = new Vector(Bounds.Width, Bounds.Height);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == BoundsProperty)
                Update();
            else if (change.Property == ForegroundProperty && Foreground is ISolidColorBrush brush)
            {
                brush.Color.ToFloatArrayNonAlloc(_color);
                _customVisual?.SendHandlerMessage((float[])_color.Clone());
            }
            else if (change.Property == LoadingStyleProperty)
                _customVisual?.SendHandlerMessage(Effects[LoadingStyle]);
        }

        public class LoadingEffectDraw : EffectDrawBase
        {
            private float[] _color = { 1.0f, 0f, 0f };

            public LoadingEffectDraw()
            {
                AnimationSpeedScale = 2f;
            }

            protected override void Render(SKCanvas canvas, SKRect rect)
            {
                using var mainShaderPaint = new SKPaint();

                if (Effect is not null)
                {
                    using var shader = EffectWithCustomUniforms(effect => new SKRuntimeEffectUniforms(effect)
                    {
                        { "iForeground", _color }
                    });
                    mainShaderPaint.Shader = shader;
                    canvas.DrawRect(rect, mainShaderPaint);
                }
            }

            // I'm not really sure how to render this properly in software fallback scenarios.
            // This is likely to cause issues with the previewer.
            // Might be worth just drawing a circle or something...
            protected override void RenderSoftware(SKCanvas canvas, SKRect rect)
            {
                var strokeWidth = Math.Max(2f, Math.Min(rect.Width, rect.Height) * 0.1f);
                var inset = strokeWidth / 2f;
                var oval = new SKRect(rect.Left + inset, rect.Top + inset, rect.Right - inset, rect.Bottom - inset);
                var startAngle = AnimationSeconds * 360f;

                using var paint = new SKPaint
                {
                    IsAntialias = true,
                    Color = new SKColor(
                        (byte)(Math.Clamp(_color[0], 0f, 1f) * byte.MaxValue),
                        (byte)(Math.Clamp(_color[1], 0f, 1f) * byte.MaxValue),
                        (byte)(Math.Clamp(_color[2], 0f, 1f) * byte.MaxValue)),
                    Style = SKPaintStyle.Stroke,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeWidth = strokeWidth
                };
                canvas.DrawArc(oval, startAngle, 270f, false, paint);
            }

            public override void OnMessage(object message)
            {
                base.OnMessage(message);
                if (message is float[] color)
                    _color = (float[])color.Clone();
            }
        }
    }

    public enum LoadingStyle
    {
        Simple,
        Glow,
        Pellets
    }
}
