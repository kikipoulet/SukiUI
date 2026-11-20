using System;
using System.Diagnostics;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class GuardedToggle
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private const double RodLength = 36, OpenCoverScale = -0.28;

        private HyperLayer _rod = null!, _tip = null!, _cover = null!, _coverHinge = null!;
        private SKPaint _paint = null!;
        private SKFont _stateFont = null!;
        private HyperSmooth _lever, _coverScale;
        private bool _init;
        private GuardedToggleState _state;

        protected override void OnBind(IHyperBinder ui)
        {
            _rod = ui.Layer("rod");
            _tip = ui.Layer("tip");
            _cover = ui.Layer("cover");
            _coverHinge = ui.Layer("coverHinge");
            _paint = ui.Own(new SKPaint { IsAntialias = true });
            _stateFont = ui.Own(new SKFont(SkeumoSkia.BoldTypeface, 10f));
        }

        protected override bool OnFrame(in HyperFrame frame, in GuardedToggleState state)
        {
            _state = state;
            var targetLever = state.IsOn ? -1.0 : 1.0;

            var coverOpen = state.RequiresCover && state.CoverOpen &&
                            (frame.Now - state.CoverOpenedStamp) / (double)Stopwatch.Frequency < AutoCloseSeconds;
            var targetCover = coverOpen ? OpenCoverScale : 1.0;

            if (!_init)
            {
                _init = true;
                _lever.Follow(targetLever, in frame, 0.001);
                _coverScale.Follow(targetCover, in frame, 0.001);
            }

            var m1 = _lever.Follow(targetLever, in frame, 0.065);
            var m2 = _coverScale.Follow(targetCover, in frame, 0.075);

            _rod.ScaleY = _lever.Value;
            _tip.Y = (_lever.Value - 1.0) * RodLength;

            _cover.Visible = state.RequiresCover;
            _coverHinge.Visible = state.RequiresCover;
            _cover.ScaleY = _coverScale.Value;

            return m1 || m2 || coverOpen;
        }

        protected override void OnDraw(SKCanvas canvas, double opacity)
        {
            base.OnDraw(canvas, opacity);
            var pal = _state.Palette;

            // LED + halo (at 55, 25)
            _paint.Style = SKPaintStyle.Fill;
            _paint.Shader = null;
            if (_state.IsOn)
            {
                _paint.Color = SkeumoSkia.ScaleAlpha(pal.Accent, 0.33 * opacity);
                canvas.DrawCircle(55f, 25f, 5f, _paint);
                _paint.Color = SkeumoSkia.ScaleAlpha(pal.Accent, opacity);
            }
            else
            {
                _paint.Color = SkeumoSkia.ScaleAlpha(pal.LedOff, opacity);
            }
            canvas.DrawCircle(55f, 25f, 2.2f, _paint);

            // Engraved labels
            SkeumoSkia.DrawLabel(canvas, _stateFont, _paint, _state.OnLabel, 35f, 36f, 40f, SkeumoSkia.ScaleAlpha(_state.IsOn ? pal.Text : pal.LowText, opacity));
            SkeumoSkia.DrawLabel(canvas, _stateFont, _paint, _state.OffLabel, 35f, 132f, 40f, SkeumoSkia.ScaleAlpha(!_state.IsOn ? pal.Text : pal.LowText, opacity));
        }
    }
}
