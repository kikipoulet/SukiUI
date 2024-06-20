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

        private SukiBackgroundEffect? _effect;
        internal SukiBackgroundEffect? Effect
        {
            get => _effect;
            set
            {
                _effect?.Dispose();
                _effect = value;
            }
        }

        private bool _animEnabled;
        internal bool AnimEnabled
        {
            get => _animEnabled;
            set
            {
                if (value) Sw.Start();
                else Sw.Stop();
                _animEnabled = value;
            }
        }

        private static readonly Stopwatch Sw = Stopwatch.StartNew();
        
        private ThemeVariant _activeVariant = ThemeVariant.Dark;

        // TODO: Look more in depth at these
        private static readonly float[] Black = { 0.1f, 0.1f, 0.1f };
        private static readonly float[] White = { 0.9f, 0.9f, 0.9f };
        
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
            Effect?.Dispose();
        }

        public bool HitTest(Point p) => false;

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature is null) throw new InvalidOperationException("Unable to lease Skia API");
            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint();
            
            if (_effect is not null)
            {
                var suki = SukiTheme.GetInstance();
                var acc = ToFloat(suki.ActiveColorTheme!.Accent);
                var prim = ToFloat(suki.ActiveColorTheme.Primary);
                var inputs = new SKRuntimeEffectUniforms(_effect.Effect)
                {
                    { "iResolution", new[] { (float)Bounds.Width, (float)Bounds.Height, 0f } },
                    { "iTime", (float)Sw.Elapsed.TotalSeconds * 0.1f },
                    { "iBase", _activeVariant == ThemeVariant.Dark ? Black : White },
                    { "iAccent", new[] { acc.r, acc.g, acc.b } },
                    { "iPrimary", new[] { prim.r, prim.g, prim.b } }
                };
                using var shader = _effect.Effect.ToShader(false, inputs);
                paint.Shader = shader;
            }
            
            canvas.DrawRect(SKRect.Create((float)Bounds.Width, (float)Bounds.Height), paint);
        }
        
        private static (float r, float g, float b) ToFloat(Color col)
        {
            return (col.R / 255f, col.G / 255f, col.B / 255f);
        }
    }
}