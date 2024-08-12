using System.Threading.Tasks;
using Avalonia.Media;

namespace SukiUI.Toasts
{
    public interface ISukiToast
    {
        public string Title { get; set; }
        public object? Content { get; set; }
        public object? Icon { get; set; }
        public bool CanDismissByClicking { get; set; }
        public IBrush? Foreground { get; set; }
        ISukiToastManager Manager { get; set; }
        public void AnimateShow();
        public void AnimateDismiss();
    }
}