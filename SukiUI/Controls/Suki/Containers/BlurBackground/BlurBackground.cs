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

    // The blur-result cache: with CONSTANT sigma the blurred image is a pure function
    // of (backdrop, sigma, bounds, canvas transform) — recomputed only when one of
    // them changes. A fade frame that only flips the paint alpha reuses the cached
    // pass. IsDynamic glass stays live by construction: its per-frame snapshot is a
    // NEW source image, which fails the reference check and recomputes every frame.
    // NOTE: SKSurface.Snapshot() shares the surface's GPU texture — the snapshot is
    // valid only while the SURFACE lives, so the cache owns both.
    private SKSurface? _cachedBlurSurface;
    private SKImage? _cachedBlur;
    private SKImage? _cachedBlurSource; // a reference, never disposed here
    private double _cachedBlurSigma;
    private int _cachedBlurW, _cachedBlurH;
    private SKMatrix _cachedBlurMatrix; // animating transforms (the dialog springs) invalidate

    // Hoisted off the render operation: the op is recreated at every invalidation,
    // and SkSL compilation is not a per-frame cost.
    private static SKRuntimeEffect? _clampEffect;

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _underlayerDirty = true;
        _underlayer = null;
        _cachedBlur?.Dispose();
        _cachedBlur = null;
        _cachedBlurSurface?.Dispose();
        _cachedBlurSurface = null;
        _cachedBlurSource = null;
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
    // The frost is applied at full strength inside the layer (opacity = 1);
    // the LAYER itself fades in over the content beneath through the paint's
    // alpha (a constant-sigma blur — the blur never animates, only its pixels'
    // visibility does).
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
            // The underlayer belongs to the owner control (shared across ops); only a
            // dynamic per-frame snapshot is owned by this op instance. (The cached blur
            // result and the compiled SkSL effect are owner/static.)
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
                // CONSTANT sigma — OverlayOpacity fades the blurred pixels' alpha (the
                // paint below), never the blur strength.

                var mi = (int)Math.Round(Math.Min(3.0 * sigma + 2.0, 160.0));
                var iw = (int)Math.Ceiling(_bounds.Width);
                var ih = (int)Math.Ceiling(_bounds.Height);

                // The blur pass runs only when (backdrop, sigma, bounds, transform)
                // changed — a fade frame reuses the cached result and only flips the
                // paint alpha. The _isDynamic term is redundant with the reference
                // check but keeps the live-glass contract explicit.
                SKImage blurSnap;
                if (_isDynamic
                    || _owner._cachedBlur is null
                    || !ReferenceEquals(_owner._cachedBlurSource, _cachedBackground)
                    || !_owner._cachedBlurSigma.Equals(sigma)
                    || _owner._cachedBlurW != iw || _owner._cachedBlurH != ih
                    || !_owner._cachedBlurMatrix.Equals(canvas.TotalMatrix))
                {
                    // Invalidate the previous pair first — they die together.
                    _owner._cachedBlur?.Dispose();
                    _owner._cachedBlurSurface?.Dispose();
                    _owner._cachedBlur = null;
                    _owner._cachedBlurSurface = null;

                    var blurred = SKSurface.Create(grContext, false,
                        new SKImageInfo(iw + 2 * mi, ih + 2 * mi,
                            SKImageInfo.PlatformColorType, SKAlphaType.Premul));
                    if (blurred is null)
                    {
                        IsGpuBlurAvailable = false;
                        return;
                    }
                    _owner._cachedBlurSurface = blurred; // ownership transferred — the snapshot's backing

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

                    blurSnap = mi > 0 && iw > 0 && ih > 0
                        ? blurred.Snapshot(SKRectI.Create(mi, mi, iw, ih))
                        : blurred.Snapshot();

                    _owner._cachedBlur = blurSnap;
                    _owner._cachedBlurSource = _cachedBackground;
                    _owner._cachedBlurSigma = sigma;
                    _owner._cachedBlurW = iw;
                    _owner._cachedBlurH = ih;
                    _owner._cachedBlurMatrix = canvas.TotalMatrix;
                }
                else
                {
                    blurSnap = _owner._cachedBlur; // the fade pays no blur
                }

                using var blurSnapShader = SKShader.CreateImage(blurSnap);
                {
                    if (_clampEffect == null)
                    {
                        _clampEffect = SKRuntimeEffect.CreateShader(clampLumaSkSL, out var error);
                        if (_clampEffect == null)
                            throw new Exception($"SKRuntimeEffect error: {error}");
                    }

                    float minLuma = _isDarkTheme ? 0f : 0.8f;
                    float maxLuma = _isDarkTheme ? 0.12f : 1f;

                    var uniforms = new SKRuntimeEffectUniforms(_clampEffect)
                    {
                        ["minLuma"] = minLuma,
                        ["maxLuma"] = maxLuma,
                        // The frost is at full strength INSIDE the layer — the layer
                        // itself fades (paint alpha below).
                        ["opacity"] = 1f
                    };

                    var children = new SKRuntimeEffectChildren(_clampEffect)
                    {
                        ["src"] = blurSnapShader
                    };
                    using var clampShader = _clampEffect.ToShader(uniforms, children, SKMatrix.CreateIdentity());

                    using var paint = new SKPaint();
                    paint.Shader = clampShader;
                    // OverlayOpacity = the alpha of the blurred pixels.
                    paint.Color = SKColors.White.WithAlpha((byte)Math.Round(255 * _opacity));
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

