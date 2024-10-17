using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Skia;
using Avalonia.Styling;
using SkiaSharp;

namespace SukiUI.Utilities.Effects
{
    internal class EffectBackgroundDraw : EffectDrawBase
    {
        public static readonly object EnableTransitions = new(), DisableTransitions = new();
        
        internal bool TransitionsEnabled { get; set; }
        internal double TransitionTime { get; set; }

        private float TransitionSeconds => (float)CompositionNow.TotalSeconds;

        private SukiEffect? _oldEffect;
        private float _transitionStartTime;
        private float _transitionEndTime;

        public EffectBackgroundDraw() : base(false)
        {
            
        }
        
        protected override void EffectChanged(SukiEffect? oldValue, SukiEffect? newValue)
        {
            if (!TransitionsEnabled) return;
            if (oldValue is null || Equals(oldValue, newValue)) return;
            _oldEffect = oldValue;
            _transitionStartTime = TransitionSeconds;
            _transitionEndTime = TransitionSeconds + (float)Math.Max(0, TransitionTime);
        }

        public override void OnMessage(object message)
        {
            base.OnMessage(message);
            if (message == EnableTransitions) TransitionsEnabled = true;
            else if (message == DisableTransitions) TransitionsEnabled = false;
            if (message is double time) TransitionTime = time;
        }

        protected override void Render(SKCanvas canvas, SKRect rect)
        {
            if (Effect is not null)
            {
                using var paint = new SKPaint();
                using var shader = EffectWithUniforms();
                paint.Shader = shader;
                canvas.DrawRect(rect, paint);
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
                paint.Shader = shader;
                if (lerped < 1)
                {
                    canvas.DrawRect(rect, paint);
                    if(!AnimationEnabled) Invalidate();
                }
                else
                    _oldEffect = null;
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