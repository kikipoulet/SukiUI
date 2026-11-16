using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Skia;
using Avalonia.Styling;
using SkiaSharp;

namespace SukiUI.Utilities.Effects
{
    internal class EffectBackgroundDraw : EffectDrawBase
    {
        public static readonly object EnableTransitions = new(), DisableTransitions = new();
        
        internal bool TransitionsEnabled { get; set; }
        internal double TransitionTime { get; set; }

        private float TransitionSeconds => (float)CompositionNow.TotalSeconds;

        private SukiEffect? _oldEffect;
        private float _transitionStartTime;
        private float _transitionEndTime;

        // A still background is drawn once into this image, then the image is drawn on each repaint.
        // The gradients are smooth, so half resolution is enough and uses 4 times less memory.
        private const float CacheResolution = 0.5f;
        private SKSurface? _cacheSurface;
        private SKImage? _cacheImage;
        private SKSizeI _cacheSize;
        private SukiEffect? _cacheEffect;
        private GRContext? _cacheContext;
        private bool _cacheDirty = true;

        public EffectBackgroundDraw() : base(false)
        {
            
        }
        
        protected override void EffectChanged(SukiEffect? oldValue, SukiEffect? newValue)
        {
            if (!TransitionsEnabled) return;
            if (oldValue is null || Equals(oldValue, newValue)) return;
            _oldEffect = oldValue;
            _transitionStartTime = TransitionSeconds;
            _transitionEndTime = TransitionSeconds + (float)Math.Max(0, TransitionTime);
        }

        public override void OnMessage(object message)
        {
            base.OnMessage(message);
            if (message == EnableTransitions) TransitionsEnabled = true;
            else if (message == DisableTransitions) TransitionsEnabled = false;
            if (message is double time) TransitionTime = time;
        }

        protected override void ThemeChanged() => _cacheDirty = true;

        public override void Dispose()
        {
            ReleaseCache();
            base.Dispose();
        }

        protected override void Render(SKCanvas canvas, SKRect rect)
        {
            if (AnimationEnabled || _oldEffect is not null)
            {
                ReleaseCache();
                RenderLive(canvas, rect);
            }
            else if (Effect is not null)
            {
                RenderCached(canvas, rect);
            }
        }

        private void RenderCached(SKCanvas canvas, SKRect rect)
        {
            var matrix = canvas.TotalMatrix;
            var scale = (float)Math.Sqrt(matrix.ScaleX * matrix.ScaleX + matrix.SkewY * matrix.SkewY) * CacheResolution;
            var size = new SKSizeI(
                Math.Max(1, (int)Math.Ceiling(rect.Width * scale)),
                Math.Max(1, (int)Math.Ceiling(rect.Height * scale)));

            if (_cacheDirty || _cacheImage is null || size != _cacheSize
                || !ReferenceEquals(Effect, _cacheEffect) || !ReferenceEquals(GrContext, _cacheContext))
                RebuildCache(size, rect);

            if (_cacheImage is not null)
                canvas.DrawImage(_cacheImage, rect, new SKSamplingOptions(SKFilterMode.Linear));
        }

        private void RebuildCache(SKSizeI size, SKRect rect)
        {
            ReleaseCache();
            var info = new SKImageInfo(size.Width, size.Height);
            _cacheSurface = (GrContext is not null ? SKSurface.Create(GrContext, false, info) : null)
                            ?? SKSurface.Create(info);
            if (_cacheSurface is null) return;

            var cacheCanvas = _cacheSurface.Canvas;
            // The shader works in logical units (iResolution), so only the canvas is scaled.
            cacheCanvas.Scale(size.Width / rect.Width, size.Height / rect.Height);
            using (var paint = new SKPaint())
            using (var shader = EffectWithUniforms())
            {
                paint.Shader = shader;
                cacheCanvas.DrawRect(rect, paint);
            }

            // On the GPU, the snapshot shares the surface texture: keep both alive together.
            _cacheImage = _cacheSurface.Snapshot();
            _cacheSize = size;
            _cacheEffect = Effect;
            _cacheContext = GrContext;
            _cacheDirty = false;
        }

        private void ReleaseCache()
        {
            _cacheImage?.Dispose();
            _cacheImage = null;
            _cacheSurface?.Dispose();
            _cacheSurface = null;
            _cacheEffect = null;
            _cacheContext = null;
        }

        private void RenderLive(SKCanvas canvas, SKRect rect)
        {
            if (Effect is not null)
            {
                using var paint = new SKPaint();
                using var shader = EffectWithUniforms();
                paint.Shader = shader;
                canvas.DrawRect(rect, paint);
            }
            if (_oldEffect is not null)
            {
                using var paint = new SKPaint();
                // TODO: Investigate how to blend the shaders better - currently the only problem with this system.
                // Blend modes effect the transition quite heavily, only these 3 seem to work in any reasonable way.
                // paint.BlendMode = SKBlendMode.ColorBurn; // - Okay
                // paint.BlendMode = SKBlendMode.Overlay; // - Not Great
                paint.BlendMode = SKBlendMode.Darken; // - Best
                var lerped = InverseLerp(_transitionStartTime, _transitionEndTime, TransitionSeconds);
                using var shader = EffectWithUniforms(_oldEffect, (float)(1 - lerped));
                paint.Shader = shader;
                if (lerped < 1)
                {
                    canvas.DrawRect(rect, paint);
                    if(!AnimationEnabled) Invalidate();
                }
                else
                    _oldEffect = null;
            }
        }

        protected override void RenderSoftware(SKCanvas canvas, SKRect rect)
        {
            if (ActiveVariant == ThemeVariant.Dark)
                canvas.Clear(ActiveTheme.Background.ToSKColor());
            else
                canvas.Clear(new SKColorF(0.95f, 0.95f, 0.95f, 1f));
        }

        private static double InverseLerp(double start, double end, double value) =>
            Math.Max(0, Math.Min(1, (value - start) / (end - start)));
    }
}