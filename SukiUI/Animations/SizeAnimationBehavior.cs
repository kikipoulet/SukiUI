using System.Diagnostics;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace SukiUI.Animations;

public static class SizeAnimationBehavior
{
    private const int DefaultDurationMs = 300;

    public static readonly AttachedProperty<bool> EnableProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "Enable", typeof(SizeAnimationBehavior), defaultValue: false);

    public static readonly AttachedProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.RegisterAttached<Control, TimeSpan>(
            "Duration", typeof(SizeAnimationBehavior),
            defaultValue: TimeSpan.FromMilliseconds(DefaultDurationMs));

    private static readonly AttachedProperty<SizeAnimationState?> StateProperty =
        AvaloniaProperty.RegisterAttached<Control, SizeAnimationState?>(
            "State", typeof(SizeAnimationBehavior), defaultValue: null);

    static SizeAnimationBehavior()
    {
        EnableProperty.Changed.AddClassHandler<Control>(OnEnableChanged);
    }

    public static void SetEnable(Control element, bool value)
        => element.SetValue(EnableProperty, value);

    public static bool GetEnable(Control element)
        => element.GetValue(EnableProperty);

    public static void SetDuration(Control element, TimeSpan value)
        => element.SetValue(DurationProperty, value);

    public static TimeSpan GetDuration(Control element)
        => element.GetValue(DurationProperty);

    private static SizeAnimationState? GetState(Control control)
        => control.GetValue(StateProperty);

    private static void SetState(Control control, SizeAnimationState? value)
        => control.SetValue(StateProperty, value);

    private static void OnEnableChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not bool enable)
            return;

        if (enable)
            EnableAnimation(control);
        else
            DisableAnimation(control);
    }

    private static void EnableAnimation(Control control)
    {
        DisableAnimation(control);

        var state = new SizeAnimationState();
        SetState(control, state);

        if (control.IsAttachedToVisualTree())
        {
            state.Attach(control);
        }
        else
        {
            var token = new Guid();
            state.PendingToken = token;

            control.AttachedToVisualTree += handler;

            void handler(object? s, VisualTreeAttachmentEventArgs a)
            {
                control.AttachedToVisualTree -= handler;
                var current = GetState(control);
                if (current != null && current.PendingToken == token && GetEnable(control))
                    current.Attach(control);
            }
        }
    }

    private static void DisableAnimation(Control control)
    {
        var state = GetState(control);
        if (state == null)
            return;

        state.Detach(control);
        SetState(control, null);
    }

    private class SizeAnimationState
    {
        private DispatcherTimer? _timer;
        private bool _isAnimating;
        private int _ignoreBoundsCount;
        private bool _originalClipToBounds;
        private bool _savedClipToBounds;
        private const int FrameIntervalMs = 16;
        private static readonly CubicEaseInOut Easing = new();

        public Guid? PendingToken { get; set; }

        public void Attach(Control control)
        {
            _savedClipToBounds = control.ClipToBounds;
            _originalClipToBounds = control.ClipToBounds;
            control.ClipToBounds = true;
            control.PropertyChanged += OnControlPropertyChanged;
        }

        public void Detach(Control control)
        {
            PendingToken = null;
            control.PropertyChanged -= OnControlPropertyChanged;
            _timer?.Stop();
            _timer = null;
            _isAnimating = false;
            _ignoreBoundsCount = 0;
            control.Clip = null;
            control.ClipToBounds = _originalClipToBounds;
            if (double.IsNaN(control.Width))
            { }
            else
            {
                control.Width = double.NaN;
                control.InvalidateMeasure();
            }
            if (double.IsNaN(control.Height))
            { }
            else
            {
                control.Height = double.NaN;
                control.InvalidateMeasure();
            }
        }

        private void OnControlPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (_isAnimating)
                return;

            if (_ignoreBoundsCount > 0)
            {
                _ignoreBoundsCount--;
                return;
            }

            if (sender is not Control control)
                return;

            if (e.Property != Layoutable.BoundsProperty)
                return;

            if (e.OldValue is not Rect oldRect || e.NewValue is not Rect newRect)
                return;

            if (oldRect.Width < 1 || oldRect.Height < 1)
                return;

            if (newRect.Width < 1 || newRect.Height < 1)
                return;

            bool widthChanged = Math.Abs(oldRect.Width - newRect.Width) >= 1;
            bool heightChanged = Math.Abs(oldRect.Height - newRect.Height) >= 1;

            if (!widthChanged && !heightChanged)
                return;

            Size fromSize = oldRect.Size;
            Size toSize = newRect.Size;

            bool widthShrinking = newRect.Width < oldRect.Width - 1;
            bool heightShrinking = newRect.Height < oldRect.Height - 1;
            bool isShrinking = widthShrinking || heightShrinking;

            _isAnimating = true;

            if (isShrinking)
            {
                if (widthShrinking)
                    control.Width = fromSize.Width;
                if (heightShrinking)
                    control.Height = fromSize.Height;
            }

            StartAnimation(control, fromSize, toSize, isShrinking, widthShrinking, heightShrinking);
        }

        private void StartAnimation(Control control, Size from, Size to,
            bool isShrinking, bool widthShrinking, bool heightShrinking)
        {
            if (!isShrinking)
                control.Clip = new RectangleGeometry(new Rect(0, 0, from.Width, from.Height));

            var durationMs = GetDuration(control).TotalMilliseconds;
            var stopwatch = Stopwatch.StartNew();

            _timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(FrameIntervalMs)
            };

            _timer.Tick += (_, _) =>
            {
                var elapsed = stopwatch.Elapsed.TotalMilliseconds;
                double t = Math.Min(elapsed / durationMs, 1.0);
                double eased = Easing.Ease(t);

                double currentW = from.Width + (to.Width - from.Width) * eased;
                double currentH = from.Height + (to.Height - from.Height) * eased;

                if (isShrinking)
                {
                    if (widthShrinking)
                        control.Width = currentW;
                    if (heightShrinking)
                        control.Height = currentH;
                }
                else
                {
                    control.Clip = new RectangleGeometry(new Rect(0, 0, currentW, currentH));
                }

                if (t >= 1.0)
                {
                    _timer.Stop();
                    _timer = null;

                    if (isShrinking)
                    {
                        if (widthShrinking)
                            control.Width = double.NaN;
                        if (heightShrinking)
                            control.Height = double.NaN;
                        _ignoreBoundsCount = 2;
                        control.InvalidateMeasure();
                    }
                    else
                    {
                        control.Clip = null;
                    }

                    _isAnimating = false;
                }
            };

            _timer.Start();
        }
    }
}
