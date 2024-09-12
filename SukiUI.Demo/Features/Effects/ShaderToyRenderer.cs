using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using SkiaSharp;
using SukiUI.Utilities.Effects;

namespace SukiUI.Demo.Features.Effects
{
    public class ShaderToyRenderer : Control
    {
        private readonly ShaderToyDraw _draw;
        
        public ShaderToyRenderer()
        {
            _draw = new ShaderToyDraw(Bounds);
        }

        public override void Render(DrawingContext context)
        {
            _draw.Bounds = Bounds;
            context.Custom(_draw);
        }

        public void SetEffect(SukiEffect effect)
        {
            _draw.Effect = effect;
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }

        private class ShaderToyDraw : EffectDrawBase
        {
            public ShaderToyDraw(Rect bounds) : base(bounds)
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

            protected override void RenderSoftware(SKCanvas canvas, SKRect rect)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}