using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Styling;
using SkiaSharp;
using System;

namespace SukiUI.Controls.GlassMorphism;

public class BlurBackground : Control
{
    
    public override void BeginInit()
    {
        base.BeginInit();

        darkmode = Application.Current.ActualThemeVariant == ThemeVariant.Dark;
    }

    private bool darkmode = false;

    private SKImage? _cachedBackground = null;
    
    
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
        
        public BlurBehindRenderOperation( Rect bounds, ref SKImage? cachedBackground, bool IsDark)
        {
      
            _bounds = bounds;
            _cachedBackground = cachedBackground;
            IsDarkTheme = IsDark;
        }

        public void Dispose()
        {
        }

        public bool HitTest(Point p) => _bounds.Contains(p);

        private bool IsDarkTheme;
        
       public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            if (!canvas.TotalMatrix.TryInvert(out var currentInvertedTransform))
                return;

            if (_cachedBackground == null)  
                _cachedBackground = lease.SkSurface.Snapshot();

            using var backdropShader = SKShader.CreateImage(_cachedBackground, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, currentInvertedTransform);

            using var blurred = SKSurface.Create(lease.GrContext, false, new SKImageInfo((int)Math.Ceiling(_bounds.Width), (int)Math.Ceiling(_bounds.Height), SKImageInfo.PlatformColorType, SKAlphaType.Premul));
    
            var sigma = IsDarkTheme ? ( _bounds.Width + _bounds.Height)/42 : 50;
            
            if(sigma <20)
                sigma = 20;
            

            
            using (var filter = SKImageFilter.CreateBlur((float)sigma, (float)sigma))
            using (var blurPaint = new SKPaint
            {
                Shader = backdropShader ,
                ImageFilter = filter
            }) 
            blurred.Canvas.DrawRect(0, 0, (float)_bounds.Width, (float)_bounds.Height, blurPaint);

            using (var blurSnap = blurred.Snapshot())
                
            using (var blurSnapShader = SKShader.CreateImage(blurSnap))
            {
                var effect = SKRuntimeEffect.CreateShader(clampLumaSkSL, out var error);
                if (effect == null) 
                    throw new Exception($"SKRuntimeEffect error: {error}");

                float minLuma = IsDarkTheme ? 0f : 0.8f;
                float maxLuma = IsDarkTheme ? 0.12f : 1f; 

                var uniforms = new SKRuntimeEffectUniforms(effect)
                {
                    ["minLuma"] = minLuma,
                    ["maxLuma"] = maxLuma
                };
                
                var children = new SKRuntimeEffectChildren(effect)
                {
                    ["src"] = blurSnapShader
                };
                using var clampShader = effect.ToShader(uniforms, children, SKMatrix.CreateIdentity());

                using var paint = new SKPaint
                {
                    Shader = clampShader,
                    IsAntialias = false
                };

                canvas.DrawRect(0, 0, (float)_bounds.Width, (float)_bounds.Height, paint);
            }
   
        }
       
        public Rect Bounds => _bounds.Inflate(4);

        public bool Equals(ICustomDrawOperation? other)
        {
            return other is BlurBehindRenderOperation op && op._bounds == _bounds ;
        }
    }

    public override void Render(DrawingContext context)
    {
       
        context.Custom(new BlurBehindRenderOperation( new Rect(default, Bounds.Size), ref _cachedBackground, darkmode));
    }
}
