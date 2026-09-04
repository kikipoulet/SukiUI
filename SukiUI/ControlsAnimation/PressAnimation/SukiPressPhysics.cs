using System;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Input;
using Avalonia.Media;
using SukiUI.Animations;   // SukiEaseElasticIn — the easings stay in Animations/

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// The complete hover + press -&gt; release scale behavior, shared by every control
    /// enabled through <see cref="SukiPress"/> (see <see cref="SukiPressProfile"/>),
    /// driven frame by frame by the single shared
    /// <see cref="SukiTicker"/> loop instead of one DispatcherTimer per control:
    /// hover: gentle scale to HoverScale while the pointer is over the control;
    /// phase 1 (PressDuration, elastic-in): down to the press depth on any click, always
    /// played to completion even on the shortest press;
    /// phase 1.5 (long press, DeepDuration, linear): keeps tensioning down to the deep floor
    /// and holds there while the pointer stays down;
    /// phase 2: a real damped-spring integration from the depth actually reached back to the
    /// hover scale or 1.0. The spring equilibrium is retargeted live when the pointer enters
    /// or leaves the bounds mid-bounce, so the elastic physically adapts.
    /// One instance per control (created lazily on the first gesture, then reused), writing
    /// directly to the control's RenderTransform (render-only, no layout).
    /// </summary>
    public sealed class SukiPressPhysics : IDisposable
    {
        private static readonly Easing PressEase = new SukiEaseElasticIn { Damping = 2.5, Frequency = 3 };
        private static readonly Easing DeepEase = new LinearEasing();
        private static readonly Easing HoverEase = new CubicEaseOut();

        private enum Phase { None, Press, Deep, Hover, Release }

        private readonly InputElement _element;
        private readonly SukiPressProfile _preset;
        private readonly Func<double> _getPressDepth;
        private IDisposable? _ticker;

        private Phase _phase;
        private bool _pressed;

        // Timed phases (Press/Deep/Hover): evaluated analytically on each shared frame.
        private double _from, _to;
        private TimeSpan _start, _duration;
        private Easing _ease = HoverEase;

        // Release spring state: x'' = -omega^2(x - target) - decay x' (semi-implicit Euler,
        // fixed substeps), exactly as before — only the driver changed.
        private double _springX = 1.0, _springV, _springTarget = 1.0;
        private TimeSpan _springLast;

        /// <summary>Creates the engine for one control. Press depth is read lazily so a
        /// <c>SukiPress.PressDepth</c> override is honored from the first frame.</summary>
        public SukiPressPhysics(InputElement element, SukiPressProfile preset, Func<double> getPressDepth)
        {
            _element = element;
            _preset = preset;
            _getPressDepth = getPressDepth;
        }

        private double PressDepth => _getPressDepth();

        private double DeepFloor => PressDepth - _preset.ExtraDeepRange;

        /// <summary>Pointer pressed: a new press interrupts anything running (hover settle,
        /// mid-bounce re-click) and starts phase 1 from the pose currently on screen.</summary>
        public void Press()
        {
            _pressed = true;
            _phase = Phase.Press;
            _from = Math.Clamp(ReadScale(), DeepFloor, _preset.HoverScale);
            _to = PressDepth;
            _ease = PressEase;
            _start = SukiTicker.Now;
            _duration = _preset.PressDuration;
            Subscribe();
        }

        /// <summary>Pointer released or capture lost. During phase 1 the completion tick will
        /// start the spring by itself; during the deep stretch or the hold it springs back now.</summary>
        public void Release()
        {
            _pressed = false;

            // Phase 1 still running: its completion tick will start the spring by itself;
            // a release event while a spring runs (capture lost twice, etc.) changes nothing.
            if (_phase is Phase.Press or Phase.Release)
                return;

            StartReleaseSpring();
        }

        /// <summary>Pointer entered. Mid-bounce: physically move the spring's resting point up.</summary>
        public void PointerEnter()
        {
            switch (_phase)
            {
                case Phase.Release:
                    _springTarget = _preset.HoverScale;
                    return;
                case Phase.Press or Phase.Deep:
                    return; // the press chain owns the transform and releases toward the right scale
                default:
                    LaunchHover(_preset.HoverScale);
                    return;
            }
        }

        /// <summary>Pointer exited. Mid-bounce: physically move the resting point down.</summary>
        public void PointerExit()
        {
            switch (_phase)
            {
                case Phase.Release:
                    _springTarget = 1.0;
                    return;
                case Phase.Press or Phase.Deep:
                    return;
                default:
                    LaunchHover(1.0);
                    return;
            }
        }

        /// <summary>Control detached or behavior disabled: stop everything and rest at 1.0.</summary>
        public void Cancel()
        {
            Unsubscribe();
            _pressed = false;
            _phase = Phase.None;
            WriteScale(1.0);
        }

        public void Dispose() => Cancel();

        private void LaunchHover(double to)
        {
            _phase = Phase.Hover;
            _from = ReadScale();
            _to = to;
            _ease = HoverEase;
            _start = SukiTicker.Now;
            _duration = _preset.HoverDuration;
            Subscribe();
        }

        private void StartReleaseSpring()
        {
            _springX = Math.Clamp(ReadScale(), DeepFloor, _preset.HoverScale);
            _springV = 0.0; // the elastic is released from rest
            _springTarget = _element.IsPointerOver ? _preset.HoverScale : 1.0;
            _springLast = SukiTicker.Now;
            _phase = Phase.Release;
            Subscribe();
        }

        private void Subscribe()
        {
            if (_ticker is not null)
                return;
            _ticker = SukiTicker.Subscribe(_element, Advance);
            // Prime the pump: a frame only renders when something invalidates, and the first
            // write here attaches the ScaleTransform (a real property change), scheduling the
            // frame this animation's first callback will ride on.
            Advance(SukiTicker.Now);
        }

        private void Unsubscribe()
        {
            _ticker?.Dispose();
            _ticker = null;
        }

        /// <summary>
        /// The single frame callback (shared clock). Advances the current phase and, when the
        /// chain reaches rest, unsubscribes — an idle control costs zero frame callbacks.
        /// </summary>
        private void Advance(TimeSpan now)
        {
            switch (_phase)
            {
                case Phase.Press:
                {
                    double t = Progress(now);
                    WriteScale(Lerp(_from, _to, _ease.Ease(t)));
                    if (t >= 1.0)
                    {
                        if (!_pressed)
                        {
                            // Short click: bounce straight from the minimal depth.
                            StartReleaseSpring();
                        }
                        else
                        {
                            // Long press: keep tensioning the elastic down to the deep floor.
                            _phase = Phase.Deep;
                            _from = _to;
                            _to = DeepFloor;
                            _ease = DeepEase;
                            _start = now;
                            _duration = _preset.DeepDuration;
                        }
                    }
                    break;
                }

                case Phase.Deep:
                {
                    double t = Progress(now);
                    WriteScale(Lerp(_from, _to, _ease.Ease(t)));
                    if (t >= 1.0)
                        _phase = Phase.None; // deep stretch finished: hold at the bottom
                    break;
                }

                case Phase.Hover:
                {
                    double t = Progress(now);
                    WriteScale(Lerp(_from, _to, _ease.Ease(t)));
                    if (t >= 1.0)
                        _phase = Phase.None;
                    break;
                }

                case Phase.Release:
                {
                    double dt = Math.Min((now - _springLast).TotalSeconds, 0.05);
                    _springLast = now;
                    StepSpring(dt);
                    WriteScale(_springX);

                    if (Math.Abs(_springX - _springTarget) < 0.0005 && Math.Abs(_springV) < 0.02)
                    {
                        // Settled: snap exactly onto the resting point.
                        _springX = _springTarget;
                        WriteScale(_springX);
                        _phase = Phase.None;
                    }
                    break;
                }
            }

            if (_phase == Phase.None)
                Unsubscribe();
        }

        private double Progress(TimeSpan now) =>
            Math.Min((now - _start).TotalMilliseconds / _duration.TotalMilliseconds, 1.0);

        private void StepSpring(double dt)
        {
            int steps = Math.Max(1, (int)Math.Ceiling(dt / 0.008));
            double h = dt / steps;
            for (int i = 0; i < steps; i++)
            {
                double accel = -_preset.SpringOmega * _preset.SpringOmega * (_springX - _springTarget)
                               - _preset.SpringDecay * _springV;
                _springV += accel * h;
                _springX += _springV * h;
            }
        }

        private double ReadScale() =>
            _element.RenderTransform is ScaleTransform transform ? transform.ScaleX : 1.0;

        private void WriteScale(double scale)
        {
            var transform = _element.RenderTransform as ScaleTransform;
            if (transform is null)
            {
                transform = new ScaleTransform(1, 1);
                _element.RenderTransform = transform;
            }
            transform.ScaleX = scale;
            transform.ScaleY = scale;
        }

        private static double Lerp(double from, double to, double t) => from + (to - from) * t;
    }
}
