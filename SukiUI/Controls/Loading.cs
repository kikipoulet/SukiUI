using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Rendering.Composition;
using SkiaSharp;
using SukiUI.Extensions;
using SukiUI.Utilities.Effects;

namespace SukiUI.Controls
{
    public class Loading : Control
    {
        public static readonly StyledProperty<LoadingStyle> LoadingStyleProperty =
            AvaloniaProperty.Register<Loading, LoadingStyle>(nameof(LoadingStyle), defaultValue: LoadingStyle.Simple);

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
                { LoadingStyle.Simple, SukiEffect.FromEmbeddedResource("simple") },
                { LoadingStyle.Glow, SukiEffect.FromEmbeddedResource("glow") },
                { LoadingStyle.Pellets, SukiEffect.FromEmbeddedResource("pellets") },
            };
        
        private CompositionCustomVisual? _customVisual;
        
        public Loading()
        {
            Width = 50;
            Height = 50;
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
            if (Foreground is ImmutableSolidColorBrush brush)
                brush.Color.ToFloatArrayNonAlloc(_color);
            _customVisual.SendHandlerMessage(_color);
            _customVisual.SendHandlerMessage(Effects[LoadingStyle]);
            Update();
        }
        
        private void Update()
        {
            if (_customVisual == null) return;
            _customVisual.Size = new Vector(Bounds.Width, Bounds.Height);
        }

        private readonly float[] _color = new float[3];

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == BoundsProperty)
                Update();
            else if (change.Property == ForegroundProperty && Foreground is ImmutableSolidColorBrush brush)
            {
                brush.Color.ToFloatArrayNonAlloc(_color);
                _customVisual?.SendHandlerMessage(_color);
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
                throw new System.NotImplementedException();
            }

            public override void OnMessage(object message)
            {
                base.OnMessage(message);
                if (message is float[] color)
                    _color = color;
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