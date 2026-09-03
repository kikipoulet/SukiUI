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

   
    public static readonly StyledProperty<double> OverlayOpacityProperty =
        AvaloniaProperty.Register<BlurBackground, double>(nameof(OverlayOpacity), 1d,
            coerce: (_, value) => Math.Clamp(value, 0d, 1d));

    public double OverlayOpacity
    {
        get => GetValue(OverlayOpacityProperty);
        set => SetValue(OverlayOpacityProperty, value);
    }

    static BlurBackground()
    {
        AffectsRender<BlurBackground>(IsDynamicProperty, IntensityFactorProperty, OverlayOpacityProperty);
        OverlayOpacityProperty.Changed.Subscribe(
            new Avalonia.Reactive.AnonymousObserver<AvaloniaPropertyChangedEventArgs<double>>(e =>
            {
             
                if (e.Sender is BlurBackground { OverlayOpacity: <= 0.0 } control)
                    control._underlayerDirty = true;
            }));
    }

    private SKImage? _underlayer;
    private volatile bool _underlayerDirty = true;

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _underlayerDirty = true;
        _underlayer = null; 
        base.OnDetachedFromVisualTree(e);
    }
    
    
    private static string clampLumaSkSL = @"
uniform shader src;
uniform float maxLuma;
uniform float minLuma;
uniform float opacity;

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
    half3 clamped = c.rgb * scale;
    // Materialize, don't alpha-fade: the paint stays opaque and its CONTENT slides
    // from the raw source (whose blur strength tracks the same opacity) to the
    // fully tinted frost. Alpha-fading instead mixed a sharp base with a blurred
    // frost, which damped the blur progression until it read as a binary step.
    // At opacity 0 this equals the unblurred source â€” a visual no-op; at 1 it is
    // exactly the stock tint.
    c.rgb = mix(c.rgb, clamped, opacity);
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
        private readonly double _opacity;
        private readonly BlurBackground _owner;
        private SKRuntimeEffect? _effect;

        public BlurBehindRenderOperation(Rect bounds, bool isDynamic, double blurFactor, bool isDarkTheme,
            double opacity, BlurBackground owner)
        {
            _bounds = bounds;
            _isDynamic = isDynamic;
            _blurFactor = blurFactor;
            _isDarkTheme = isDarkTheme;
            _opacity = opacity;
            _owner = owner;
        }

        public void Dispose()
        {
            _effect?.Dispose();
            // The underlayer belongs to the owner control (shared across ops); only a
            // dynamic per-frame snapshot is owned by this op instance.
            if (!ReferenceEquals(_cachedBackground, _owner._underlayer))
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

                    if (_owner._underlayer is not { } underlayer || _owner._underlayerDirty)
                    {
                        underlayer = surface.Snapshot();
                        _owner._underlayer = underlayer;
                        _owner._underlayerDirty = false;
                    }
                    _cachedBackground = underlayer;
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

                var sigma = _isDarkTheme ? (_bounds.Width + _bounds.Height) / 42 : 50;
                if (sigma < 20)
                    sigma = 20;
                sigma *= _blurFactor;
                sigma *= _opacity;

                var mi = (int)Math.Round(Math.Min(3.0 * sigma + 2.0, 160.0));
                var iw = (int)Math.Ceiling(_bounds.Width);
                var ih = (int)Math.Ceiling(_bounds.Height);

                using var blurred = SKSurface.Create(grContext, false,
                    new SKImageInfo(iw + 2 * mi, ih + 2 * mi,
                        SKImageInfo.PlatformColorType, SKAlphaType.Premul));
                if (blurred is null)
                {
                    IsGpuBlurAvailable = false;
                    return;
                }

                var off = blurred.Canvas;
                off.Save();
                off.Translate(mi, mi);
                using (var filter = SKImageFilter.CreateBlur((float)sigma, (float)sigma))
                using (var blurPaint = new SKPaint())
                {
                    blurPaint.Shader = backdropShader;
                    blurPaint.ImageFilter = filter;
                    off.DrawRect(-mi, -mi, iw + 2 * mi, ih + 2 * mi, blurPaint);
                }
                off.Restore();

                using var blurSnap = mi > 0 && iw > 0 && ih > 0
                    ? blurred.Snapshot(SKRectI.Create(mi, mi, iw, ih))
                    : blurred.Snapshot();

                using var blurSnapShader = SKShader.CreateImage(blurSnap);
                {
                    if (_effect == null)
                    {
                        _effect = SKRuntimeEffect.CreateShader(clampLumaSkSL, out var error);
                        if (_effect == null)
                            throw new Exception($"SKRuntimeEffect error: {error}");
                    }

                    float minLuma = _isDarkTheme ? 0f : 0.8f;
                    float maxLuma = _isDarkTheme ? 0.12f : 1f;

                    var uniforms = new SKRuntimeEffectUniforms(_effect)
                    {
                        ["minLuma"] = minLuma,
                        ["maxLuma"] = maxLuma,
                        ["opacity"] = (float)Math.Pow(_opacity, 2.5)
                    };

                    var children = new SKRuntimeEffectChildren(_effect)
                    {
                        ["src"] = blurSnapShader
                    };
                    using var clampShader = _effect.ToShader(uniforms, children, SKMatrix.CreateIdentity());

                    using var paint = new SKPaint();
                    paint.Shader = clampShader;
                    paint.IsAntialias = false;

                  
                    using (var restorePaint = new SKPaint { IsAntialias = false })
                    {
                        canvas.Save();
                        canvas.ResetMatrix();
                        canvas.DrawImage(_cachedBackground, 0, 0, restorePaint);
                        canvas.Restore();
                    }
                    if (_opacity > 0.0)
                        canvas.DrawRect(0, 0, (float)_bounds.Width, (float)_bounds.Height, paint);
                }
         
        }
       
        public Rect Bounds => _bounds.Inflate(4);

        public bool Equals(ICustomDrawOperation? other)
        {
            return other is BlurBehindRenderOperation op && op._bounds == _bounds &&
                   op._isDynamic == _isDynamic && op._blurFactor.Equals(_blurFactor) &&
                   op._isDarkTheme == _isDarkTheme && op._opacity.Equals(_opacity);
        }
    }

    public override void Render(DrawingContext context)
    {

        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;
        context.Custom(new BlurBehindRenderOperation(new Rect(default, Bounds.Size), IsDynamic, IntensityFactor,
            ActualThemeVariant == ThemeVariant.Dark, OverlayOpacity, this));
    }
}

