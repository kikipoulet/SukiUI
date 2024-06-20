using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using SukiUI.Utilities.Background;

namespace SukiUI.Controls
{
    public class SukiBackground : Control
    {
        private readonly ShaderBackgroundDraw _draw;

        internal SukiBackgroundEffect BackgroundEffect { get; set; }
        internal bool AnimationEnabled { get; set; }
        
        public SukiBackground()
        {
            _draw = new ShaderBackgroundDraw(new Rect(0, 0, Bounds.Width, Bounds.Height));
        }
        
        public override void Render(DrawingContext context)
        {
            _draw.Bounds = Bounds;
            _draw.Effect = BackgroundEffect;
            _draw.AnimEnabled = AnimationEnabled;
            context.Custom(_draw);
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }
    }
}