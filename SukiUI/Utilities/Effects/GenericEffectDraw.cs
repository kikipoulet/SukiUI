using Avalonia;
using SkiaSharp;

namespace SukiUI.Utilities.Effects
{
    public class GenericEffectDraw : EffectDrawBase
    {
        public GenericEffectDraw(Rect bounds) : base(bounds)
        {
            AnimationEnabled = true;
            AnimationSpeedScale = 2f;
        }

        protected override void Render(SKCanvas canvas, SKRect rect)
        {
            canvas.Scale(1,-1);
            canvas.Translate(0, (float)-Bounds.Height);
            using var mainShaderPaint = new SKPaint();
            
            if (Effect is not null)
            {
                using var shader = EffectWithUniforms();
                mainShaderPaint.Shader = shader;
                canvas.DrawRect(rect, mainShaderPaint);
            }
            canvas.Restore();
        }
    }
}