using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;

namespace SukiUI.Utilities
{
    public class ShaderEffectDraw : ICustomDrawOperation
    {
        public Rect Bounds { get; }
        
        public void Render(ImmediateDrawingContext context)
        {
            throw new System.NotImplementedException();
        }
        
        public void Dispose()
        {
            // no-op
        }
        
        public bool Equals(ICustomDrawOperation other) => false;
        
        public bool HitTest(Point p) => false;
    }
}