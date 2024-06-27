using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Styling;
using SkiaSharp;

namespace SukiUI.Utilities.Background
{
    internal class ShaderBackgroundDraw : ICustomDrawOperation
    {
        public Rect Bounds { get; internal set; }

        private SukiEffect? _effect;

        internal SukiEffect? Effect
        {
            get => _effect;
            set
            {
                var old = _effect;
                _effect = value;
                TryBeginTransition(old, _effect);
            }
        }

        private bool _animEnabled;

        internal bool AnimEnabled
        {
            get => _animEnabled;
            set
            {
                if (value) AnimationTick.Start();
                else AnimationTick.Stop();
                _animEnabled = value;
            }
        }

        internal bool TransitionsEnabled { get; set; }
        internal double TransitionTime { get; set; }

        private static readonly Stopwatch AnimationTick = Stopwatch.StartNew();
        private static readonly Stopwatch TransitionTick = Stopwatch.StartNew();

        private ThemeVariant _activeVariant;

        private SukiEffect? _oldEffect;
        private double _transitionStartTime;
        private double _transitionEndTime;
        
        public ShaderBackgroundDraw(Rect bounds)
        {
            Bounds = bounds;
            var sTheme = SukiTheme.GetInstance();
            sTheme.OnBaseThemeChanged += v => _activeVariant = v;
            _activeVariant = SukiTheme.GetInstance().ActiveBaseTheme;
        }

        private void TryBeginTransition(SukiEffect? oldValue, SukiEffect? newValue)
        {
            if (!TransitionsEnabled) return;
            if (oldValue is null || Equals(oldValue, newValue)) return;
            _oldEffect = oldValue;
            _transitionStartTime = TransitionTick.Elapsed.TotalSeconds;
            _transitionEndTime = _transitionStartTime + Math.Max(0, TransitionTime);
        }

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature is null) throw new InvalidOperationException("Unable to lease Skia API");
            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            //canvas.Clear(SKColors.Transparent);
            using var mainShaderPaint = new SKPaint();
            var rect = SKRect.Create((float)Bounds.Width, (float)Bounds.Height);

            if (Effect is not null)
            {
                using var shader = Effect.ToShaderWithUniforms(
                    (float)AnimationTick.Elapsed.TotalSeconds,
                    _activeVariant,
                    Bounds,
                    1f);
                mainShaderPaint.Shader = shader;
                canvas.DrawRect(rect, mainShaderPaint);
            }

            if (_oldEffect is null)
            {
                return;
            }
            
            using var oldShaderPaint = new SKPaint();
            // TODO: Investigate how to blend the shaders better - currently the only problem with this system.
            // Blend modes effect the transition quite heavily, only these 3 seem to work in any reasonable way.
            // oldShaderPaint.BlendMode = SKBlendMode.ColorBurn; // - Okay
            // oldShaderPaint.BlendMode = SKBlendMode.Overlay; // - Not Great
            oldShaderPaint.BlendMode = SKBlendMode.Darken; // - Best
            var lerped = InverseLerp(_transitionStartTime, _transitionEndTime, TransitionTick.Elapsed.TotalSeconds);
            using var oldShader = _oldEffect.ToShaderWithUniforms(
                (float)AnimationTick.Elapsed.TotalSeconds,
                _activeVariant,
                Bounds,
                (float)(1 - lerped));
            oldShaderPaint.Shader = oldShader;
            if (lerped <= 1)
                canvas.DrawRect(rect, oldShaderPaint);
            else
                _oldEffect = null;
        }

        private static double InverseLerp(double start, double end, double value) =>
            Math.Max(0, Math.Min(1, (value - start) / (end - start)));
        
        
        public void Dispose()
        {
            // no-op
        }
        public bool Equals(ICustomDrawOperation other) => false;
        public bool HitTest(Point p) => false;
    }
}