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

        private void TryBeginTransition(SukiEffect? oldValue, SukiEffect? newValue)
        {
            if (!TransitionsEnabled) return;
            if (oldValue is null || Equals(oldValue, newValue)) return;
            _oldEffect = oldValue;
            _transitionStartTime = TransitionTick.Elapsed.TotalSeconds;
            _transitionEndTime = _transitionStartTime + Math.Max(0, TransitionTime);
        }

        private static Color GetBackgroundColor(Color input)
        {
            int r = input.R;
            int g = input.G;
            int b = input.B;

            var minValue = Math.Min(Math.Min(r, g), b);
            var maxValue = Math.Max(Math.Max(r, g), b);

            r = (r == minValue) ? 37 : ((r == maxValue) ? 37 : 26);
            g = (g == minValue) ? 37 : ((g == maxValue) ? 37 : 26);
            b = (b == minValue) ? 37 : ((b == maxValue) ? 37 : 26);
            return new Color(255, (byte)r, (byte)g, (byte)b);
        }

        public ShaderBackgroundDraw(Rect bounds)
        {
            Bounds = bounds;
            var sTheme = SukiTheme.GetInstance();
            sTheme.OnBaseThemeChanged += v => _activeVariant = v;
            _activeVariant = SukiTheme.GetInstance().ActiveBaseTheme;
        }

        public bool Equals(ICustomDrawOperation other) => false;

        public void Dispose()
        {
            //_oldEffect?.Dispose();
            //Effect?.Dispose();
        }

        public bool HitTest(Point p) => false;

        private static readonly float[] White = { 0.95f, 0.95f, 0.95f };

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature is null) throw new InvalidOperationException("Unable to lease Skia API");
            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            canvas.Clear(SKColors.Transparent);
            using var mainShaderPaint = new SKPaint();
            var rect = SKRect.Create((float)Bounds.Width, (float)Bounds.Height);

            if (Effect is not null)
            {
                using var shader = CreateShader(Effect, 1f);
                mainShaderPaint.Shader = shader;
                canvas.DrawRect(rect, mainShaderPaint);
            }

            if (_oldEffect is not null)
            {
                using var oldShaderPaint = new SKPaint();
                // Blend modes effect the transition quite heavily, only these 3 seem to work in any reasonable way.
                // oldShaderPaint.BlendMode = SKBlendMode.ColorBurn; // - Okay
                // oldShaderPaint.BlendMode = SKBlendMode.Overlay; // - Not Great
                oldShaderPaint.BlendMode = SKBlendMode.Darken; // - Best
                var lerped = InverseLerp(_transitionStartTime, _transitionEndTime, TransitionTick.Elapsed.TotalSeconds);
                using var shader = CreateShader(_oldEffect, (float)(1 - lerped)); //(float)SineIn(1 - lerped));
                oldShaderPaint.Shader = shader;
                if (lerped <= 1)
                    canvas.DrawRect(rect, oldShaderPaint);
                else
                    _oldEffect = null;
            }
        }

        private readonly object _obj = new();

        private SKShader CreateShader(SukiEffect effect, float alpha)
        {
            var suki = SukiTheme.GetInstance();
            var acc = ToFloat(suki.ActiveColorTheme!.Accent);
            var prim = ToFloat(suki.ActiveColorTheme.Primary);
            var darkBackground = ToFloat(GetBackgroundColor(suki.ActiveColorTheme.Primary));
            var inputs = new SKRuntimeEffectUniforms(effect.Effect)
            {
                { "iResolution", new[] { (float)Bounds.Width, (float)Bounds.Height, 0f } },
                { "iTime", (float)AnimationTick.Elapsed.TotalSeconds * 0.1f },
                {
                    "iBase",
                    _activeVariant == ThemeVariant.Dark
                        ? new[] { darkBackground.r, darkBackground.g, darkBackground.b }
                        : White
                },
                { "iAccent", new[] { acc.r / 1.3f, acc.g / 1.3f, acc.b / 1.3f } },
                { "iPrimary", new[] { prim.r / 1.3f, prim.g / 1.3f, prim.b / 1.3f } },
                { "iDark", _activeVariant == ThemeVariant.Dark ? 1f : 0f },
                { "iAlpha", alpha }
            };
            return effect.Effect.ToShader(false, inputs);
        }

        private static double InverseLerp(double start, double end, double value) =>
            Math.Max(0, Math.Min(1, (value - start) / (end - start)));

        private static (float r, float g, float b) ToFloat(Color col) =>
            (col.R / 255f, col.G / 255f, col.B / 255f);

        public static double SineIn(double x) => 1 - Math.Cos(x * Math.PI / 2f);
    }
}