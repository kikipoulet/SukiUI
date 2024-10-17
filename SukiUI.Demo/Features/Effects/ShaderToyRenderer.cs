using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using SkiaSharp;
using SukiUI.Utilities.Effects;

namespace SukiUI.Demo.Features.Effects
{
    public class ShaderToyRenderer : Control
    {
        private CompositionCustomVisual? _customVisual;
        private SukiEffect? _sukiEffect;

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            var comp = ElementComposition.GetElementVisual(this)?.Compositor;
            if (comp == null || _customVisual?.Compositor == comp) return;
            var visualHandler = new ShaderToyDraw();
            _customVisual = comp.CreateCustomVisual(visualHandler);
            ElementComposition.SetElementChildVisual(this, _customVisual);
            _customVisual.SendHandlerMessage(EffectDrawBase.StartAnimations);
            if(_sukiEffect != null) _customVisual.SendHandlerMessage(_sukiEffect);
            Update();
        }
        
        private void Update()
        {
            if (_customVisual == null) return;
            _customVisual.Size = new Vector(Bounds.Width, Bounds.Height);
        }

        public void SetEffect(SukiEffect effect)
        {
            _sukiEffect = effect;
            _customVisual?.SendHandlerMessage(effect);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if(change.Property == BoundsProperty)
                Update();
        }

        private class ShaderToyDraw : EffectDrawBase
        {
            public ShaderToyDraw()
            {
                AnimationEnabled = true;
                AnimationSpeedScale = 2f;
            }

            protected override void Render(SKCanvas canvas, SKRect rect)
            {
                using var mainShaderPaint = new SKPaint();
            
                if (Effect is not null)
                {
                    using var shader = EffectWithUniforms();
                    mainShaderPaint.Shader = shader;
                    canvas.DrawRect(rect, mainShaderPaint);
                }
            }

            protected override void RenderSoftware(SKCanvas canvas, SKRect rect)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}