using System;
using System.Linq;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.VisualTree;
using SukiUI.Controls.GlassMorphism;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    // Inside this namespace the simple name "Motion" would bind to the SIBLING NAMESPACE
    // SukiUI.Motion before any outer using is consulted — this alias restores the class.
    using Motion = SukiUI.Motion.Motion;

    /// <summary>
    /// The dialog open / close / pinned-shake choreography described declaratively over
    /// the SukiUI.Motion engine (see SukiUI.Motion/Plan.md) — SukiDialogPhysics retold:
    /// the open rises from the summoning click on a spring calibrated against the
    /// dialog's measured area (small ones bounce, big ones land heavy); the close sinks
    /// back below the rest place on the same calibration; the pinned shake is a struck
    /// spring (an armed velocity kick from the pose actually on screen) whose settle
    /// restores the rest pose, resuming whatever travel a mid-flight interruption froze.
    /// Owned by SukiDialogHost — the host keeps pointer tracking, manager events and
    /// pooling, and calls the four entry points. The Avalonia-Transitions hybrid and its
    /// same-frame-attach glass hack are gone: every phase is engine trajectories over
    /// the shared transform block (translate · scale — the proven dialog order).
    /// </summary>
    internal sealed class SukiDialogMotion : IDisposable
    {
        private readonly Func<SukiDialogProfile> _getProfile;
        private SukiDialogProfile _profile; // snapshot per open/close/shake, re-read at entry

        // Targets. PART_DialogContent is re-created by host template re-application; the
        // DoF surface and the glass live INSIDE the SukiDialog's ControlTheme (the host's
        // template walk does not reach them — hence the visual walk, re-checked on every
        // open: a re-applied template brings a fresh, uninitialized surface and glass).
        private ContentControl? _content;
        private Surface _s = null!;
        private Surface _surfaceS = null!;
        private Surface _glassS = null!;
        private Border? _surfaceTarget;
        private BlurBackground? _glassTarget;

        private Choreography? _current;

        // Last open calibration — the close replays it (the old engine replayed the very
        // transitions built at open).
        private TimeSpan _openDuration = TimeSpan.FromMilliseconds(500);
        private Func<Easing> _openEase = static () => new LinearEasing();

        internal SukiDialogMotion(Func<SukiDialogProfile> getProfile) => _getProfile = getProfile;

        /// <summary>
        /// Opens with a rise steered toward the click that summoned the dialog. The
        /// spring is calibrated against the measured area: small dialogs replay the
        /// button's release spring exactly, bigger ones get more mass — slower, more
        /// damped, no rebound.
        /// </summary>
        internal void PlayOpen(ContentControl content, double width, double height, (double Dx, double Dy) emergence)
        {
            _profile = _getProfile();
            EnsureTargets(content);

            // Size calibration (the old formulas, verbatim), resolved NOW — this gesture.
            double area = width * height;
            double sizeT = Math.Clamp(
                (area - _profile.SmallDialogArea) / (_profile.LargeDialogArea - _profile.SmallDialogArea), 0.0, 1.0);
            double sizeCurve = Math.Pow(sizeT, _profile.DampingCurveExponent);
            double durationMs = Integrator.Lerp(
                _profile.OpenTransformDurationSmallMs, _profile.OpenTransformDurationLargeMs, sizeCurve);
            double omega = Integrator.Lerp(_profile.OpenOmegaSmall, _profile.OpenOmegaLarge, sizeT);
            double zeta = Integrator.Lerp(_profile.OpenZetaSmall, _profile.OpenZetaLarge, sizeCurve);
            double fromScale = Integrator.Lerp(_profile.OpenFromScaleSmall, _profile.OpenFromScaleLarge, sizeT);

            // The close replays the open calibration (the closure captures this gesture's
            // omega/zeta — the very transitions the old engine kept).
            _openDuration = TimeSpan.FromMilliseconds(durationMs);
            _openEase = () => new SukiSpringEaseOut { Omega = omega, Decay = 2.0 * zeta * omega };

            // The Froms pre-pose the emerged-from-the-click pose BEFORE the members start
            // (the no-flash rule) — in flight (a re-shown pooled dialog mid-close), they
            // are skipped and the rise resumes from the pose currently on screen.
            var open = new Choreography()
                .And(_s.TranslateX.From(emergence.Dx).To(0.0).Over(_openDuration).Ease(_openEase))
                .And(_s.TranslateY.From(emergence.Dy).To(0.0).Over(_openDuration).Ease(_openEase))
                .And(_s.Scale.From(fromScale).To(1.0).Over(_openDuration).Ease(_openEase))
                .And(_s.Opacity.From(0.0).To(1.0).Over(TimeSpan.FromMilliseconds(_profile.OpenOpacityDurationMs)))
                .And(_surfaceS.Blur.From(_profile.BlurredRadius).To(0.0)
                    .Over(TimeSpan.FromMilliseconds(_profile.SurfaceTransitionDurationMs)))
                .And(_glassS.Property(BlurBackground.OverlayOpacityProperty).To(1.0)
                    .Over(TimeSpan.FromMilliseconds(_profile.GlassFadeMilliseconds)));
            Play(open);
        }

        /// <summary>
        /// Closes downward: the dialog sinks below its resting place, dissolving back
        /// into blur and glass — on the open's calibration, regardless of where the
        /// dismissal interaction happened.
        /// </summary>
        internal void PlayClose(ContentControl content)
        {
            _profile = _getProfile();
            EnsureTargets(content);

            var close = new Choreography()
                .And(_s.TranslateX.To(0.0).Over(_openDuration).Ease(_openEase))
                .And(_s.TranslateY.To(_profile.EmergenceVertical).Over(_openDuration).Ease(_openEase))
                .And(_s.Scale.To(_profile.CloseScale).Over(_openDuration).Ease(_openEase))
                .And(_s.Opacity.To(0.0).Over(TimeSpan.FromMilliseconds(_profile.OpenOpacityDurationMs)))
                .And(_surfaceS.Blur.To(_profile.BlurredRadius)
                    .Over(TimeSpan.FromMilliseconds(_profile.SurfaceTransitionDurationMs)))
                .And(_glassS.Property(BlurBackground.OverlayOpacityProperty).To(0.0)
                    .Over(TimeSpan.FromMilliseconds(_profile.GlassFadeMilliseconds)));
            Play(close);
        }

        /// <summary>
        /// Struck-spring shake for a pinned dialog pressed on the backdrop: the vertical
        /// offset starts from the pose actually on screen, kicked by the impulse, and
        /// integrates back to rest. Everything else freezes (the running choreography is
        /// stopped); at settle the rest pose resumes — a shake that interrupted an
        /// opening mid-flight lets the remaining travel continue.
        /// </summary>
        internal void StartShake(ContentControl content, double initialVelocity)
        {
            _profile = _getProfile();
            EnsureTargets(content);

            var shake = _s.TranslateY
                .To(0.0)
                .Spring(() => new Spring(_profile.ShakeOmega, _profile.ShakeDecay));
            shake.SeedVelocity(initialVelocity); // the kick — armed BEFORE Play; Run's ambient carry must not touch it

            var choreography = new Choreography()
                .And(shake)
                .Then(() => Rest(content));
            Play(choreography);
        }

        /// <summary>Stops whatever runs (host detach). The poses freeze where they are.</summary>
        internal void Stop() => _current?.Stop();

        public void Dispose() => Stop();

        // ---- engine plumbing -----------------------------------------------------------

        private void Rest(ContentControl content)
        {
            _profile = _getProfile();
            var rest = new Choreography()
                .And(_s.TranslateX.To(0.0).Over(_openDuration).Ease(_openEase))
                .And(_s.TranslateY.To(0.0).Over(_openDuration).Ease(_openEase))
                .And(_s.Scale.To(1.0).Over(_openDuration).Ease(_openEase))
                .And(_s.Opacity.To(1.0).Over(TimeSpan.FromMilliseconds(_profile.OpenOpacityDurationMs)))
                .And(_surfaceS.Blur.To(0.0).Over(TimeSpan.FromMilliseconds(_profile.SurfaceTransitionDurationMs)))
                .And(_glassS.Property(BlurBackground.OverlayOpacityProperty).To(1.0)
                    .Over(TimeSpan.FromMilliseconds(_profile.GlassFadeMilliseconds)));
            Play(rest);
        }

        private void Play(Choreography choreography)
        {
            _current?.Stop();
            _current = choreography;
            choreography.Start(_content!);
        }

        private void EnsureTargets(ContentControl content)
        {
            if (!ReferenceEquals(_content, content))
            {
                _content = content;
                _s = Motion.For(content);
                _surfaceS = new Surface(content, () => _surfaceTarget);
                _glassS = new Surface(content, () => _glassTarget);
            }

            // Re-checked on every entry: a re-applied template (or a new pooled dialog)
            // brings a fresh, uninitialized surface and glass.
            _surfaceTarget = content.GetVisualDescendants()
                .OfType<Border>()
                .FirstOrDefault(b => b.Name == "PART_DialogSurface");
            _glassTarget = content.GetVisualDescendants()
                .OfType<BlurBackground>()
                .FirstOrDefault();
        }
    }
}
