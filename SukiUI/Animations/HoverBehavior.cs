using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Animation.Easings;
using Avalonia.Threading;
using System;

namespace SukiUI.Animations;

public static class HoverBehavior
{
    private const int DefaultDurationMs = 500;
    private const double FrameIntervalMs = 16; 

    public static readonly AttachedProperty<double> ScaleProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "Scale",
            typeof(HoverBehavior),
            defaultValue: 1.0);

    public static readonly AttachedProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.RegisterAttached<Control, TimeSpan>(
            "Duration",
            typeof(HoverBehavior),
            defaultValue: TimeSpan.FromMilliseconds(DefaultDurationMs));

    private static readonly AttachedProperty<HoverState?> StateProperty =
        AvaloniaProperty.RegisterAttached<Control, HoverState?>(
            "State",
            typeof(HoverBehavior),
            defaultValue: null);

    static HoverBehavior()
    {
        ScaleProperty.Changed.AddClassHandler<Control>(OnScaleChanged);
    }

    public static void SetScale(Control element, double value)
        => element.SetValue(ScaleProperty, value);

    public static double GetScale(Control element)
        => element.GetValue(ScaleProperty);

    public static void SetDuration(Control element, TimeSpan value)
        => element.SetValue(DurationProperty, value);

    public static TimeSpan GetDuration(Control element)
        => element.GetValue(DurationProperty);

    private static HoverState? GetState(Control control)
        => control.GetValue(StateProperty);

    private static void SetState(Control control, HoverState? value)
        => control.SetValue(StateProperty, value);

    private static void OnScaleChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not double newScale)
            return;

        var isActive = newScale > 1.0;

        if (isActive)
        {
            EnableHover(control);
        }
        else
        {
            DisableHover(control);
        }
    }

    private static void EnableHover(Control control)
    {
        EnsureScaleTransform(control);
        
        var state = new HoverState
        {
            ScaleTransform = (ScaleTransform)control.RenderTransform!,
            CurrentScale = 1.0,
            TargetScale = 1.0,
            IsHovering = false
        };
        
        SetState(control, state);

        var targetScale = GetScale(control);
        var duration = GetDuration(control);

        control.PointerEntered += (_, _) => OnPointerEntered(control, targetScale, duration, state);
        control.PointerExited += (_, _) => OnPointerExited(control, duration, state);
    }

    private static void DisableHover(Control control)
    {
        StopAnimationLoop(GetState(control));
        
        if (control.RenderTransform is ScaleTransform scaleTransform)
        {
            scaleTransform.ScaleX = 1.0;
            scaleTransform.ScaleY = 1.0;
        }

        SetState(control, null);
    }

    private static void OnPointerEntered(Control control, double targetScale, TimeSpan duration, HoverState state)
    {
        if (state.IsHovering)
            return;

        state.IsHovering = true;
        state.TargetScale = targetScale;
        state.StartScale = state.CurrentScale;
        
        StartAnimationLoop(state, duration);
    }

    private static void OnPointerExited(Control control, TimeSpan duration, HoverState state)
    {
        if (!state.IsHovering)
            return;

        state.IsHovering = false;
        state.TargetScale = 1.0;
        state.StartScale = state.CurrentScale;
        
        RestartAnimationLoop(state, duration);
    }

    private static void StartAnimationLoop(HoverState state, TimeSpan duration)
    {
        if (state.Timer != null && state.Timer.IsEnabled)
            return;

        state.Timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(FrameIntervalMs)
        };

        var startTime = DateTime.Now;
        var durationMs = duration.TotalMilliseconds;

        state.Timer.Tick += OnTimerTick;

        void OnTimerTick(object? sender, EventArgs e)
        {
            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
            double t = Math.Min(elapsed / durationMs, 1.0);
            double easeValue = new CubicEaseInOut().Ease(t);

            state.CurrentScale = Lerp(state.StartScale, state.TargetScale, easeValue);
            
            ApplyScale(state.ScaleTransform, state.CurrentScale);

            if (t >= 1.0)
            {
                state.Timer!.Tick -= OnTimerTick;
                state.Timer.Stop();
                state.Timer = null;
                
                state.CurrentScale = state.TargetScale;
                ApplyScale(state.ScaleTransform, state.CurrentScale);
                
                if (!state.IsHovering)
                    EnsureAtBaseScale(state.ScaleTransform);
            }
        }

        state.Timer.Start();
    }

    private static void RestartAnimationLoop(HoverState state, TimeSpan duration)
    {
        StopAnimationLoop(state);
        StartAnimationLoop(state, duration);
    }

    private static void StopAnimationLoop(HoverState? state)
    {
        if (state?.Timer == null || !state.Timer.IsEnabled)
            return;

        state.Timer.Stop();
        state.Timer = null;
    }

    private static double Lerp(double from, double to, double t)
    {
        return from + (to - from) * t;
    }

    private static void ApplyScale(ScaleTransform transform, double scaleValue)
    {
        transform.ScaleX = scaleValue;
        transform.ScaleY = scaleValue;
    }

    private static void EnsureScaleTransform(Control control)
    {
        if (control.RenderTransform is ScaleTransform)
            return;

        var scaleTransform = new ScaleTransform(1.0, 1.0);
        control.RenderTransform = scaleTransform;
        control.RenderTransformOrigin = RelativePoint.Center;
    }

    private static void EnsureAtBaseScale(ScaleTransform transform)
    {
        transform.ScaleX = 1.0;
        transform.ScaleY = 1.0;
    }

    private record HoverState
    {
        public DispatcherTimer? Timer { get; set; }
        public bool IsHovering { get; set; }
        public double CurrentScale { get; set; }
        public double TargetScale { get; set; }
        public double StartScale { get; set; }
        public ScaleTransform ScaleTransform { get; set; } = null!;
    }
}
