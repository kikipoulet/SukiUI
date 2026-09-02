using System;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace SukiUI.Animations
{

    public class SukiComboBoxPress
    {
        // Softer than the button across the board (button: depth 0.96, deep floor 0.87,
        // omega 16, decay 9.333). Same hover scale and phase timings.
        private const double HoverScale = 1.02;
        private const double PressDepth = 0.982;
        private const double DeepFloor = 0.94;
        private static readonly TimeSpan PressDuration = TimeSpan.FromMilliseconds(150);
        private static readonly TimeSpan DeepDuration = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan HoverDuration = TimeSpan.FromMilliseconds(150);

        // Rebound: slower and more damped than the button — zeta = 13.5 / (2 * 12) = 0.56
        // (button: 0.29) — a barely-there overshoot that settles gently.
        private const double SpringOmega = 12.0;
        private const double SpringDecay = 13.5;
        private const double FrameIntervalMs = 16;

        private static readonly Easing PressEase = new SukiEaseElasticIn { Damping = 2.5, Frequency = 3 };
        private static readonly Easing DeepEase = new LinearEasing();
        private static readonly Easing HoverEase = new CubicEaseOut();

        public static readonly AttachedProperty<bool> EnableProperty =
            AvaloniaProperty.RegisterAttached<SukiComboBoxPress, InputElement, bool>("Enable");

        private sealed class PressState
        {
            public bool Pressed;
            public bool Pressing;
            public bool Deepening;
            public bool Releasing;
            public DispatcherTimer? Timer;

            // Release spring state.
            public double SpringX = 1.0;
            public double SpringV;
            public double SpringTarget = 1.0;
            public DateTime SpringLastTick;
        }

        private static readonly AttachedProperty<PressState> StateProperty =
            AvaloniaProperty.RegisterAttached<SukiComboBoxPress, InputElement, PressState>("PressState");

        static SukiComboBoxPress()
        {
            EnableProperty.Changed.AddClassHandler<InputElement>(OnEnableChanged);
        }

        public static bool GetEnable(InputElement element) => element.GetValue(EnableProperty);
        public static void SetEnable(InputElement element, bool value) => element.SetValue(EnableProperty, value);

        private static void OnEnableChanged(InputElement element, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is true)
            {
                // The ComboBox marks pointer events as handled in its class handlers,
                // so plain CLR subscriptions would never fire: handledEventsToo is required.
                element.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
                element.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Bubble, handledEventsToo: true);
                // PointerEntered/Exited are direct routed events: subscribe through the CLR
                // wrappers (AddHandler with a routing strategy would never match them).
                element.PointerEntered += OnPointerEntered;
                element.PointerExited += OnPointerExited;
                element.PointerCaptureLost += OnPointerCaptureLost;
                element.DetachedFromVisualTree += OnDetachedFromVisualTree;
            }
            else
            {
                element.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
                element.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
                element.PointerEntered -= OnPointerEntered;
                element.PointerExited -= OnPointerExited;
                element.PointerCaptureLost -= OnPointerCaptureLost;
                element.DetachedFromVisualTree -= OnDetachedFromVisualTree;
                CancelAndReset(element);
            }
        }

        private static void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is InputElement element)
                CancelAndReset(element);
        }

        private static void CancelAndReset(InputElement element)
        {
            if (element.GetValue(StateProperty) is not { } state)
                return;
            StopTimer(state);
            state.Pressed = false;
            state.Pressing = false;
            state.Deepening = false;
            state.Releasing = false;
            WriteScale(element, 1.0);
        }

        private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not InputElement element)
                return;

            var state = element.GetValue(StateProperty) ?? CreateState(element);

            // A new press interrupts anything running (hover settle, mid-bounce re-click).
            StopTimer(state);
            state.Pressed = true;
            state.Pressing = true;
            state.Deepening = false;
            state.Releasing = false;

            double from = Math.Clamp(ReadScale(element), DeepFloor, HoverScale);
            LaunchTimed(element, state, Phase.Press, from, PressDepth, PressDuration, PressEase);
        }

        private static void OnPointerReleased(object? sender, PointerReleasedEventArgs e) => StartRelease(sender);

        private static void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e) => StartRelease(sender);

        private static void StartRelease(object? sender)
        {
            if (sender is not InputElement element)
                return;
            if (element.GetValue(StateProperty) is not { } state)
                return;

            state.Pressed = false;

            // Phase 1 still running: its completion tick will start the spring by itself.
            if (state.Pressing || state.Releasing)
                return;

            // Released during the deep stretch or while holding at the bottom.
            StartReleaseSpring(element, state);
        }

        private static void OnPointerEntered(object? sender, PointerEventArgs e)
        {
            if (sender is not InputElement element)
                return;
            var state = element.GetValue(StateProperty) ?? CreateState(element);

            if (state.Releasing)
            {
                // Mid-bounce: physically move the spring's resting point up.
                state.SpringTarget = HoverScale;
                return;
            }
            if (state.Pressing || state.Deepening)
                return; // the press chain owns the transform and will release toward the right scale.
            LaunchTimed(element, state, Phase.Hover, ReadScale(element), HoverScale, HoverDuration, HoverEase);
        }

        private static void OnPointerExited(object? sender, PointerEventArgs e)
        {
            if (sender is not InputElement element)
                return;
            if (element.GetValue(StateProperty) is not { } state)
                return;

            if (state.Releasing)
            {
                // Mid-bounce: physically move the spring's resting point down; the elastic
                // bends toward the new equilibrium instead of finishing at 1.02 then dropping.
                state.SpringTarget = 1.0;
                return;
            }
            if (state.Pressing || state.Deepening)
                return;
            LaunchTimed(element, state, Phase.Hover, ReadScale(element), 1.0, HoverDuration, HoverEase);
        }

        private static PressState CreateState(InputElement element)
        {
            var state = new PressState();
            element.SetValue(StateProperty, state);
            return state;
        }

        private enum Phase { Press, Deep, Release, Hover }

        private static void LaunchTimed(
            InputElement element, PressState state, Phase phase, double from, double to, TimeSpan duration, Easing easing)
        {
            StopTimer(state);
            var timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(FrameIntervalMs)
            };
            var start = DateTime.Now;
            double durationMs = duration.TotalMilliseconds;

            timer.Tick += (_, _) =>
            {
                double t = Math.Min((DateTime.Now - start).TotalMilliseconds / durationMs, 1.0);
                WriteScale(element, Lerp(from, to, easing.Ease(t)));

                if (t < 1.0)
                    return;

                StopTimer(state);
                OnTimedPhaseComplete(element, state, phase, to);
            };

            state.Timer = timer;
            timer.Start();
        }

        private static void OnTimedPhaseComplete(InputElement element, PressState state, Phase phase, double final)
        {
            switch (phase)
            {
                case Phase.Press:
                    state.Pressing = false;
                    if (!state.Pressed)
                    {
                        // Short click: bounce straight from the minimal depth.
                        StartReleaseSpring(element, state);
                    }
                    else
                    {
                        // Long press: keep tensioning the elastic down to the deep maximum.
                        state.Deepening = true;
                        LaunchTimed(element, state, Phase.Deep, final, DeepFloor, DeepDuration, DeepEase);
                    }
                    break;

                case Phase.Deep:
                    // Deep stretch finished: hold at the bottom while the pointer stays down.
                    state.Deepening = false;
                    WriteScale(element, final);
                    break;

                case Phase.Hover:
                    // Nothing to chain.
                    break;
            }
        }

        private static void StartReleaseSpring(InputElement element, PressState state)
        {
            StopTimer(state);
            state.SpringX = Math.Clamp(ReadScale(element), DeepFloor, HoverScale);
            state.SpringV = 0.0; // the elastic is released from rest
            state.SpringTarget = element.IsPointerOver ? HoverScale : 1.0;
            state.Releasing = true;

            var timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(FrameIntervalMs)
            };
            state.SpringLastTick = DateTime.Now;

            timer.Tick += (_, _) =>
            {
                if (!state.Releasing)
                {
                    StopTimer(state);
                    return;
                }

                double dt = Math.Min((DateTime.Now - state.SpringLastTick).TotalSeconds, 0.05);
                state.SpringLastTick = DateTime.Now;
                StepSpring(state, dt);
                WriteScale(element, state.SpringX);

                if (Math.Abs(state.SpringX - state.SpringTarget) < 0.0005 && Math.Abs(state.SpringV) < 0.02)
                {
                    // Settled: snap exactly onto the resting point.
                    state.SpringX = state.SpringTarget;
                    WriteScale(element, state.SpringX);
                    state.Releasing = false;
                    StopTimer(state);
                }
            };

            state.Timer = timer;
            timer.Start();
        }

        private static void StepSpring(PressState state, double dt)
        {
            // x'' = -omega^2 * (x - target) - decay * x'  (semi-implicit Euler, fixed substeps)
            int steps = Math.Max(1, (int)Math.Ceiling(dt / 0.008));
            double h = dt / steps;
            for (int i = 0; i < steps; i++)
            {
                double accel = -SpringOmega * SpringOmega * (state.SpringX - state.SpringTarget) - SpringDecay * state.SpringV;
                state.SpringV += accel * h;
                state.SpringX += state.SpringV * h;
            }
        }

        private static void StopTimer(PressState state)
        {
            state.Timer?.Stop();
            state.Timer = null;
        }

        private static double Lerp(double from, double to, double t) => from + (to - from) * t;

        private static double ReadScale(InputElement element) =>
            element.RenderTransform is ScaleTransform transform ? transform.ScaleX : 1.0;

        private static void WriteScale(InputElement element, double scale)
        {
            var transform = element.RenderTransform as ScaleTransform;
            if (transform is null)
            {
                transform = new ScaleTransform(1, 1);
                element.RenderTransform = transform;
            }
            transform.ScaleX = scale;
            transform.ScaleY = scale;
        }
    }
}
