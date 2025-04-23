using System.Diagnostics;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using SukiUI.Animations;
using SukiUI.Controls;
using SukiUI.Helpers;

namespace SukiUI.Demo.Features.Helpers
{
    public partial class PullingEffect : UserControl
    {
        
        private Point? _startPoint;
        private readonly ScaleTransform     _scaleTf;
        private readonly SkewTransform      _skewTf;
        private readonly TranslateTransform _transTf;

        private Control ReactiveControl;

        public PullingEffect()
        {
            InitializeComponent();
            
            ReactiveControl = this.FindControl<Border>("RB");

            _scaleTf = new ScaleTransform(1, 1);
            _skewTf  = new SkewTransform(0, 0);
            _transTf = new TranslateTransform(0, 0);
            
            var tg = new TransformGroup
            {
                Children = new Transforms
                {
                    _scaleTf,
                    _skewTf,
                    _transTf
                }
            };
            
            ReactiveControl.RenderTransform = tg;

            ReactiveControl.PointerPressed  += OnPointerPressed;
            ReactiveControl.PointerMoved    += OnPointerMoved;
            ReactiveControl.PointerReleased += OnPointerReleased;
        }

        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            _startPoint = e.GetPosition(ReactiveControl);
            ReactiveControl.CapturePointer(e.Pointer);
        }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (_startPoint is null) return;

            var current = e.GetPosition(ReactiveControl);
            var dx = current.X - _startPoint.Value.X;
            var dy = current.Y - _startPoint.Value.Y;

            const double skewFactor      = 0.02;   
            const double translateFactor = 0.05;    
            const double scaleFactor     = 0.0001; 
            
            var Xangle = dx * skewFactor;
            if (Xangle > 3)
                Xangle = 3;
            if (Xangle < -3)
                Xangle = -3;
            
            var Yangle = -dy * skewFactor;
            if (Yangle > 3)
                Yangle = 3;
            if (Yangle < -3)
                Yangle = -3;

            _skewTf.AngleX =   (Yangle < 0 ? Xangle : - Xangle) * Math.Abs(Yangle) * 0.3;
            _skewTf.AngleY =  (Xangle < 0 ? Yangle : - Yangle) * Math.Abs(Xangle) * 0.3;

            _transTf.X = dx * translateFactor;
            _transTf.Y = dy * translateFactor;

            _scaleTf.ScaleX = 1 - Math.Abs(dx) * scaleFactor;
            _scaleTf.ScaleY = 1 - Math.Abs(dy) * scaleFactor;
        }

        private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            ReactiveControl.ReleasePointerCapture(e.Pointer);
            _startPoint = null;

            double startSkewX   = _skewTf.AngleX;
            double startSkewY   = _skewTf.AngleY;
            double startTransX  = _transTf.X;
            double startTransY  = _transTf.Y;
            double startScaleX  = _scaleTf.ScaleX;
            double startScaleY  = _scaleTf.ScaleY;

            var easing = new SukiSpringEase() // Find a way to adapt the curve from the deformation to feel even more natural ?
            {
                Mass = 1,
                Damping = 7, 
                Stiffness = 50
            };

            const int durationMs = 700;
            var startTime = DateTime.Now;

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) 
            };

            timer.Tick += (s, _) =>
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                double t = Math.Min(elapsed / durationMs, 1.0);
                
                double ease = easing.Ease(t);

                _skewTf.AngleX    = startSkewX   * (1 - ease);
                _skewTf.AngleY    = startSkewY   * (1 - ease);
                _transTf.X        = startTransX  * (1 - ease);
                _transTf.Y        = startTransY  * (1 - ease);
                _scaleTf.ScaleX   = startScaleX  + (1 - startScaleX) * ease;
                _scaleTf.ScaleY   = startScaleY  + (1 - startScaleY) * ease;

                if (t >= 1.0)
                {
                    _skewTf.AngleX    = 0;
                    _skewTf.AngleY    = 0;
                    _transTf.X        = 0;
                    _transTf.Y        = 0;
                    _scaleTf.ScaleX   = 1;
                    _scaleTf.ScaleY   = 1;

                    timer.Stop();
                }
            };

            timer.Start();
        }
    }


}
