using Avalonia;
using Avalonia.Skia;
using Avalonia.Styling;
using SkiaSharp;

namespace SukiUI.Utilities.Effects
{
    internal class EffectBackgroundDraw() : EffectDrawBase(false)
    {
        public enum Operation
        {
            EnableTransitions,
            DisableTransitions
        }

        private float TransitionSeconds => (float)CompositionNow.TotalSeconds;

        private SukiEffect? _oldEffect;
        private bool _transitionsEnabled;
        private double _transitionTime;
        private float _transitionStartTime;
        private float _transitionEndTime;
        private SKMatrix _matrix = SKMatrix.Identity;
        private SKPoint[]? _cornerRadius;

        protected override void EffectChanged(SukiEffect? oldValue, SukiEffect? newValue)
        {
            if (!_transitionsEnabled) return;
            if (oldValue is null || Equals(oldValue, newValue)) return;
            _oldEffect = oldValue;
            _transitionStartTime = TransitionSeconds;
            _transitionEndTime = TransitionSeconds + (float)Math.Max(0, _transitionTime);
        }

        public override void OnMessage(object message)
        {
            base.OnMessage(message);

            switch (message)
            {
                case Operation.EnableTransitions:
                {
                    _transitionsEnabled = true;
                    break;
                }
                case Operation.DisableTransitions:
                {
                    _transitionsEnabled = false;
                    break;
                }
                case double time:
                {
                    _transitionTime = time;
                    break;
                }
                case SKMatrix matrix:
                {
                    _matrix = matrix;
                    break;
                }
                case CornerRadius cornerRadius:
                {
                    if (cornerRadius is { IsUniform: true, TopLeft: 0d })
                    {
                        _cornerRadius = [];
                    }
                    else
                    {
                        _cornerRadius =
                        [
                            new SKPoint((float)cornerRadius.TopLeft, (float)cornerRadius.TopLeft),
                            new SKPoint((float)cornerRadius.TopRight, (float)cornerRadius.TopRight),
                            new SKPoint((float)cornerRadius.BottomRight, (float)cornerRadius.BottomRight),
                            new SKPoint((float)cornerRadius.BottomLeft, (float)cornerRadius.BottomLeft),
                        ];
                    }
                    break;
                }
            }
        }

        protected override void Render(SKCanvas canvas, SKRect rect)
        {
            if (Effect is not null)
            {
                using var paint = new SKPaint();
                using var shader = EffectWithUniforms();
                using var transformed = shader?.WithLocalMatrix(_matrix);
                paint.Shader = transformed;
                if (_cornerRadius is { Length: 4 })
                {
                    var roundRect = new SKRoundRect();
                    roundRect.SetRectRadii(rect, _cornerRadius);
                    canvas.DrawRoundRect(roundRect, paint);
                }
                else
                {
                    canvas.DrawRect(rect, paint);
                }
            }

            if (_oldEffect is not null)
            {
                using var paint = new SKPaint();
                // TODO: Investigate how to blend the shaders better - currently the only problem with this system.
                // Blend modes effect the transition quite heavily, only these 3 seem to work in any reasonable way.
                // paint.BlendMode = SKBlendMode.ColorBurn; // - Okay
                // paint.BlendMode = SKBlendMode.Overlay; // - Not Great
                paint.BlendMode = SKBlendMode.Darken; // - Best
                var lerped = InverseLerp(_transitionStartTime, _transitionEndTime, TransitionSeconds);
                using var shader = EffectWithUniforms(_oldEffect, (float)(1 - lerped));
                using var transformed = shader?.WithLocalMatrix(_matrix);
                paint.Shader = transformed;
                if (lerped < 1)
                {
                    if (_cornerRadius is { Length: 4 })
                    {
                        var roundRect = new SKRoundRect();
                        roundRect.SetRectRadii(rect, _cornerRadius);
                        canvas.DrawRoundRect(roundRect, paint);
                    }
                    else
                    {
                        canvas.DrawRect(rect, paint);
                    }
                    if (!AnimationEnabled) Invalidate();
                }
                else
                {
                    _oldEffect = null;
                }
            }
        }

        protected override void RenderSoftware(SKCanvas canvas, SKRect rect)
        {
            if (ActiveVariant == ThemeVariant.Dark)
                canvas.Clear(ActiveTheme.Background.ToSKColor());
            else
                canvas.Clear(new SKColorF(0.95f, 0.95f, 0.95f, 1f));
        }

        private static double InverseLerp(double start, double end, double value) =>
            Math.Max(0, Math.Min(1, (value - start) / (end - start)));
    }
}