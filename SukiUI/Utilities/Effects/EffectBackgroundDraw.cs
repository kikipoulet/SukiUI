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
        private readonly SKPaint _effectPaint = new();
        private readonly SKPaint _oldEffectPaint = new()
        {
            BlendMode = SKBlendMode.Darken
        };

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
                var shader = EffectWithUniforms();
                _effectPaint.Shader = shader;
                canvas.DrawRect(rect, _effectPaint);
            }
            if (_oldEffect is not null)
            {
                // TODO: Investigate how to blend the shaders better - currently the only problem with this system.
                // Blend modes effect the transition quite heavily, only these 3 seem to work in any reasonable way.
                // paint.BlendMode = SKBlendMode.ColorBurn; // - Okay
                // paint.BlendMode = SKBlendMode.Overlay; // - Not Great
                var lerped = InverseLerp(_transitionStartTime, _transitionEndTime, TransitionSeconds);
                if (lerped < 1)
                {
                    // Built inside the branch: on the frame that ends the transition the shader would be
                    // compiled, cached under a fresh alpha key, then dropped again without ever drawing.
                    _oldEffectPaint.Shader = EffectWithUniforms(_oldEffect, (float)(1 - lerped));
                    canvas.DrawRect(rect, _oldEffectPaint);
                    if(!AnimationEnabled) Invalidate();
                }
                else
                {
                    InvalidateShaderCache(_oldEffect);
                    _oldEffect = null;
                }
            }

            _effectPaint.Shader = null;
            _oldEffectPaint.Shader = null;
        }

        protected override void RenderSoftware(SKCanvas canvas, SKRect rect)
        {
            if (ActiveVariant == ThemeVariant.Dark)
                canvas.Clear(ActiveTheme.Background.ToSKColor());
            else
                canvas.Clear(new SKColorF(0.95f, 0.95f, 0.95f, 1f));
        }

        private static double InverseLerp(double start, double end, double value) =>
            end <= start ? 1 : Math.Max(0, Math.Min(1, (value - start) / (end - start)));

        public override void Dispose()
        {
            _oldEffect = null;
            _effectPaint.Dispose();
            _oldEffectPaint.Dispose();
            base.Dispose();
        }
    }
}
