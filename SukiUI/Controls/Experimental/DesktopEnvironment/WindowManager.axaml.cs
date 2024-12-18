using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls.Experimental.DesktopEnvironment
{
    public partial class WindowManager : UserControl
    {
        private Canvas _canvas;

        public WindowManager()
        {
            _canvas = new Canvas();
            this.Content = _canvas;
        }

        public void OpenWindow(InternalWindow window)
        {

        
            window.Closed += (s, e) => _canvas.Children.Remove(window);

            _canvas.Children.Add(window);

            // Positionner la fenêtre
            Canvas.SetLeft(window, 0);
            Canvas.SetTop(window, 0);
        }
    }
}