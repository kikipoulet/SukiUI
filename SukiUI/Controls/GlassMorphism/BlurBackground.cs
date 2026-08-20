using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Styling;
using SkiaSharp;

namespace SukiUI.Controls.GlassMorphism;

public class BlurBackground : Control
{
    private static readonly TimeSpan DynamicRefreshInterval = TimeSpan.FromMilliseconds(33);

    public static bool IsGpuBlurAvailable { get; set; } = true;

    public static readonly StyledProperty<bool> IsDynamicProperty = AvaloniaProperty.Register<BlurBackground, bool>(
        nameof(IsDynamic), defaultValue: false);

    public bool IsDynamic
    {
        get => GetValue(IsDynamicProperty);
        set => SetValue(IsDynamicProperty, value);
    }
    
    public static readonly StyledProperty<double> IntensityFactorProperty =
        AvaloniaProperty.Register<BlurBackground, double>(nameof(IntensityFactor), 1d,
            coerce: (_, value) => Math.Max(0, value));
    
    public double IntensityFactor
    {
        get => GetValue(IntensityFactorProperty);
        set => SetValue(IntensityFactorProperty, value);
    }
    
    static BlurBackground()
    {
        AffectsRender<BlurBackground>(IsDynamicProperty, IntensityFactorProperty);
    }
    
    
    private static string clampLumaSkSL = @"
uniform shader src;
uniform float maxLuma;
uniform float minLuma;

half4 main(float2 coord) {
    half4 c = src.eval(coord);
    float lum = 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;
    float scale = 1.0;
    if (lum > maxLuma) {
        scale = maxLuma / lum;
    } else if (lum < minLuma && lum > 0.0) {
        scale = minLuma / lum;
    }
    
    if (lum == 0.0) scale = 1.0;
    c.rgb *= scale;
    return c;
}
";

    private readonly BlurCache _cache = new();

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _cache.Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    private sealed class BlurCache : IDisposable
    {
        public SKImage? Image;
        public SKRuntimeEffect? Effect;
        public Size Size;
        public double BlurFactor;
        public bool IsDarkTheme;
        public long LastRefreshTimestamp;

        public bool Matches(Size size, double blurFactor, bool isDarkTheme) =>
            Image is not null && Size == size && BlurFactor.Equals(blurFactor) && IsDarkTheme == isDarkTheme;

        public void ReplaceImage(SKImage image, Size size, double blurFactor, bool isDarkTheme, long timestamp)
        {
            Image?.Dispose();
            Image = image;
            Size = size;
            BlurFactor = blurFactor;
            IsDarkTheme = isDarkTheme;
            LastRefreshTimestamp = timestamp;
        }

        public void Dispose()
        {
            Image?.Dispose();
            Image = null;
            Effect?.Dispose();
            Effect = null;
            LastRefreshTimestamp = 0;
        }
    }

    private sealed class BlurBehindRenderOperation : ICustomDrawOperation
    {
  
        private readonly Rect _bounds;
        private readonly bool _isDynamic;
        private readonly double _blurFactor;
        private readonly bool _isDarkTheme;
        private readonly BlurCache _cache;
        
        public BlurBehindRenderOperation(Rect bounds, bool isDynamic, double blurFactor, bool isDarkTheme, BlurCache cache)
        {
            _bounds = bounds;
            _isDynamic = isDynamic;
            _blurFactor = blurFactor;
            _isDarkTheme = isDarkTheme;
            _cache = cache;
        }

        public void Dispose() { }

        public bool HitTest(Point p) => _bounds.Contains(p);

        public void Render(ImmediateDrawingContext context)
        {
            if (context is null)
                return;
                var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
                if (leaseFeature is null)
                {
                    IsGpuBlurAvailable = false;
                    return;
                }
                using var lease = leaseFeature.Lease();
                var canvas = lease.SkCanvas;
                var surface = lease.SkSurface;
                if (surface is null)
                {
                    IsGpuBlurAvailable = false;
                    return;
                }

                if (!canvas.TotalMatrix.TryInvert(out var currentInvertedTransform))
                    return;


                var grContext = lease.GrContext;
                if (grContext == null)
                {
                    IsGpuBlurAvailable = false;
                    return;
                }

                var now = Stopwatch.GetTimestamp();
                var refreshRequired = !_cache.Matches(_bounds.Size, _blurFactor, _isDarkTheme) ||
                                      (_isDynamic && Stopwatch.GetElapsedTime(_cache.LastRefreshTimestamp, now) >= DynamicRefreshInterval);
                if (refreshRequired && !RefreshBlurredImage(surface, grContext, currentInvertedTransform, now))
                    return;

                if (_cache.Image is null)
                    return;

                using var blurShader = SKShader.CreateImage(_cache.Image);
                if (_cache.Effect == null)
                {
                    _cache.Effect = SKRuntimeEffect.CreateShader(clampLumaSkSL, out var error);
                    if (_cache.Effect == null)
                        throw new Exception($"SKRuntimeEffect error: {error}");
                }

                float minLuma = _isDarkTheme ? 0f : 0.8f;
                float maxLuma = _isDarkTheme ? 0.12f : 1f;

                using var uniforms = new SKRuntimeEffectUniforms(_cache.Effect)
                {
                    ["minLuma"] = minLuma,
                    ["maxLuma"] = maxLuma
                };

                using var children = new SKRuntimeEffectChildren(_cache.Effect)
                {
                    ["src"] = blurShader
                };
                using var clampShader = _cache.Effect.ToShader(uniforms, children, SKMatrix.CreateIdentity());

                using var paint = new SKPaint();
                paint.Shader = clampShader;
                paint.IsAntialias = false;

                canvas.DrawRect(0, 0, (float)_bounds.Width, (float)_bounds.Height, paint);
         
        }

        private bool RefreshBlurredImage(SKSurface surface, GRContext grContext, SKMatrix currentInvertedTransform, long timestamp)
        {
            using var background = surface.Snapshot();
            using var backdropShader = SKShader.CreateImage(background, SKShaderTileMode.Clamp,
                SKShaderTileMode.Clamp, currentInvertedTransform);
            using var blurred = SKSurface.Create(grContext, false,
                new SKImageInfo((int)Math.Ceiling(_bounds.Width), (int)Math.Ceiling(_bounds.Height),
                    SKImageInfo.PlatformColorType, SKAlphaType.Premul));
            if (blurred is null)
            {
                IsGpuBlurAvailable = false;
                return false;
            }

            var sigma = _isDarkTheme ? (_bounds.Width + _bounds.Height) / 42 : 50;
            sigma = Math.Max(sigma, 20) * _blurFactor;

            using var filter = SKImageFilter.CreateBlur((float)sigma, (float)sigma);
            using var blurPaint = new SKPaint { Shader = backdropShader, ImageFilter = filter };
            blurred.Canvas.DrawRect(0, 0, (float)_bounds.Width, (float)_bounds.Height, blurPaint);

            _cache.ReplaceImage(blurred.Snapshot(), _bounds.Size, _blurFactor, _isDarkTheme, timestamp);
            return true;
        }
       
        public Rect Bounds => _bounds.Inflate(4);

        public bool Equals(ICustomDrawOperation? other)
        {
            return other is BlurBehindRenderOperation op && op._bounds == _bounds &&
                   op._isDynamic == _isDynamic && op._blurFactor.Equals(_blurFactor) &&
                   op._isDarkTheme == _isDarkTheme;
        }
    }

    public override void Render(DrawingContext context)
    {
       
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;
        context.Custom(new BlurBehindRenderOperation(new Rect(default, Bounds.Size), IsDynamic, IntensityFactor,
            ActualThemeVariant == ThemeVariant.Dark, _cache));
    }
}
