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

    private class BlurBehindRenderOperation : ICustomDrawOperation
    {
  
        private readonly Rect _bounds;
        private SKImage? _cachedBackground;
        private readonly bool _isDynamic;
        private readonly double _blurFactor;
        private readonly bool _isDarkTheme;
        private SKRuntimeEffect? _effect;
        
        public BlurBehindRenderOperation(Rect bounds, bool isDynamic, double blurFactor, bool isDarkTheme)
        {
            _bounds = bounds;
            _isDynamic = isDynamic;
            _blurFactor = blurFactor;
            _isDarkTheme = isDarkTheme;
        }

        public void Dispose()
        {
            _effect?.Dispose();
            _cachedBackground?.Dispose();
        }

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


                if (_isDynamic)
                {
                    _cachedBackground?.Dispose();
                    _cachedBackground = surface.Snapshot();
                }
                else
                {
                    if (_cachedBackground == null)
                        _cachedBackground = surface.Snapshot();
                }
                


                if(_cachedBackground == null)
                    return;

                var grContext = lease.GrContext;
                if (grContext == null)
                {
                    IsGpuBlurAvailable = false;
                    return;
                }
            
                using var backdropShader = SKShader.CreateImage(_cachedBackground, SKShaderTileMode.Clamp,
                    SKShaderTileMode.Clamp, currentInvertedTransform);

                using var blurred = SKSurface.Create(grContext, false,
                    new SKImageInfo((int)Math.Ceiling(_bounds.Width), (int)Math.Ceiling(_bounds.Height),
                        SKImageInfo.PlatformColorType, SKAlphaType.Premul));
                if (blurred is null)
                {
                    IsGpuBlurAvailable = false;
                    return;
                }

                var sigma = _isDarkTheme ? (_bounds.Width + _bounds.Height) / 42 : 50;

                if (sigma < 20)
                    sigma = 20;

               sigma *= _blurFactor;

                using (var filter = SKImageFilter.CreateBlur((float)sigma, (float)sigma))
                using (var blurPaint = new SKPaint())
                {
                    blurPaint.Shader = backdropShader;
                    blurPaint.ImageFilter = filter;
                    blurred.Canvas.DrawRect(0, 0, (float)_bounds.Width, (float)_bounds.Height, blurPaint);
                }

                using (var blurSnap = blurred.Snapshot())
                    
                using (var blurSnapShader = SKShader.CreateImage(blurSnap))
                {
                    if (_effect == null)
                    {
                        _effect = SKRuntimeEffect.CreateShader(clampLumaSkSL, out var error);
                        if (_effect == null)
                            throw new Exception($"SKRuntimeEffect error: {error}");
                    }

                    float minLuma = _isDarkTheme ? 0f : 0.8f;
                    float maxLuma = _isDarkTheme ? 0.12f : 1f;

                    using var uniforms = new SKRuntimeEffectUniforms(_effect)
                    {
                        ["minLuma"] = minLuma,
                        ["maxLuma"] = maxLuma
                    };

                    using var children = new SKRuntimeEffectChildren(_effect)
                    {
                        ["src"] = blurSnapShader
                    };
                    using var clampShader = _effect.ToShader(uniforms, children, SKMatrix.CreateIdentity());

                    using var paint = new SKPaint();
                    paint.Shader = clampShader;
                    paint.IsAntialias = false;

                    canvas.DrawRect(0, 0, (float)_bounds.Width, (float)_bounds.Height, paint);
                }
         
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
            ActualThemeVariant == ThemeVariant.Dark));
    }
}
