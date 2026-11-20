using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class RockerSwitch
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private static readonly SKColor Neon = new(0xFF, 0xB3, 0x47);

        private HyperLayer _paddleOff = null!, _paddleOn = null!;
        private SKPaint _paint = null!;
        private SKFont _markFont = null!;
        private HyperSmooth _on;
        private bool _init;
        private RockerSwitchState _state;

        protected override void OnBind(IHyperBinder ui)
        {
            _paddleOff = ui.Layer("paddleOff");
            _paddleOn = ui.Layer("paddleOn");
            _paint = ui.Own(new SKPaint { IsAntialias = true });
            _markFont = ui.Own(new SKFont(SkeumoSkia.BoldTypeface, 11f));
        }

        protected override bool OnFrame(in HyperFrame frame, in RockerSwitchState state)
        {
            _state = state;
            var target = state.IsOn ? 1.0 : 0.0;
            if (!_init)
            {
                _init = true;
                _on.Follow(target, in frame, 0.001);
            }
            var moving = _on.Follow(target, in frame, 0.04);
            _paddleOn.Opacity = _on.Value;
            _paddleOff.Opacity = 1.0 - _on.Value * 0.999;
            return moving;
        }

        protected override void OnDraw(SKCanvas canvas, double opacity)
        {
            base.OnDraw(canvas, opacity);
            var pal = _state.Palette;

            SkeumoSkia.DrawLabel(canvas, _markFont, _paint, "I", 20f, 34f, 40f, SkeumoSkia.ScaleAlpha(_state.IsOn ? pal.Text : pal.LowText, opacity));
            SkeumoSkia.DrawLabel(canvas, _markFont, _paint, "O", 20f, 66f, 40f, SkeumoSkia.ScaleAlpha(!_state.IsOn ? pal.Text : pal.LowText, opacity));

            var onVal = _on.Value;
            if (onVal > 0.01)
            {
                using var halo = SKShader.CreateRadialGradient(
                    new SKPoint(40f, 54f), 6f,
                    new[] { SkeumoSkia.WithAlpha(Neon, 0.7 * onVal * opacity), new SKColor(0xFF, 0xB3, 0x47, 0) },
                    new[] { 0f, 1f }, SKShaderTileMode.Clamp);
                _paint.Style = SKPaintStyle.Fill;
                _paint.Color = SKColors.White;
                _paint.Shader = halo;
                canvas.DrawCircle(40f, 54f, 6f, _paint);
                _paint.Shader = null;
            }

            _paint.Style = SKPaintStyle.Fill;
            _paint.Shader = null;
            _paint.Color = SkeumoSkia.ScaleAlpha(onVal > 0.5 ? Neon : pal.LedOff, opacity);
            canvas.DrawCircle(40f, 54f, 2.5f, _paint);
        }
    }
}
