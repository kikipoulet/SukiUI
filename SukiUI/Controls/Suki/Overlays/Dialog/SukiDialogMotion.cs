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
    // Inside the SukiUI namespace, the simple name "Motion" is the sibling namespace
    // SukiUI.Motion: this alias, declared in the namespace body, names the entry class.
    using Motion = SukiUI.Motion.Motion;

    /// <summary>
    /// The dialog open / close / pinned shake. The open rises from the click that summoned the
    /// dialog on a curve calibrated against the dialog's measured area (small ones bounce, big
    /// ones land heavy); the close sinks back below the resting place on the same calibration;
    /// the pinned shake is a struck spring — an impulse from the pose on screen — after which
    /// the dialog returns to rest. Owned by SukiDialogHost, which keeps pointer tracking,
    /// manager events and pooling, and calls the entry points below.
    /// </summary>
    internal sealed class SukiDialogMotion : IDisposable
    {
        private readonly Func<SukiDialogProfile> _getProfile;
        private ContentControl? _content;

        // The last open calibration — the close and the rest replay it.
        private TimeSpan _duration = TimeSpan.FromMilliseconds(500);
        private Easing _easing = new LinearEasing();

        internal SukiDialogMotion(Func<SukiDialogProfile> getProfile) => _getProfile = getProfile;

        /// <summary>Opens with a rise steered toward the click that summoned the dialog: small
        /// dialogs replay the button's release feel, bigger ones get more mass — slower, more
        /// damped, no rebound.</summary>
        internal void PlayOpen(ContentControl content, double width, double height, (double Dx, double Dy) emergence)
        {
            var p = _getProfile();
            _content = content;

            double sizeT = Math.Clamp((width * height - p.SmallDialogArea) / (p.LargeDialogArea - p.SmallDialogArea), 0.0, 1.0);
            double sizeCurve = Math.Pow(sizeT, p.DampingCurveExponent);
            double omega = Lerp(p.OpenOmegaSmall, p.OpenOmegaLarge, sizeT);
            double zeta = Lerp(p.OpenZetaSmall, p.OpenZetaLarge, sizeCurve);
            double fromScale = Lerp(p.OpenFromScaleSmall, p.OpenFromScaleLarge, sizeT);
            _duration = TimeSpan.FromMilliseconds(Lerp(p.OpenTransformDurationSmallMs, p.OpenTransformDurationLargeMs, sizeCurve));
            _easing = new SukiSpringEaseOut { Omega = omega, Decay = 2.0 * zeta * omega };

            // The From values are written before the dialog shows; a pooled dialog re-shown
            // mid-close rises from the pose on screen instead.
            _ = Motion.Of(content)
                .Animate(SukiMotion.TranslateXProperty).From(emergence.Dx).To(0.0).Over(_duration).WithEasing(_easing)
                .And()
                .Animate(SukiMotion.TranslateYProperty).From(emergence.Dy).To(0.0).Over(_duration).WithEasing(_easing)
                .And()
                .Animate(SukiMotion.ScaleProperty).From(fromScale).To(1.0).Over(_duration).WithEasing(_easing)
                .And()
                .Animate(Visual.OpacityProperty).From(0.0).To(1.0).Over(Ms(p.OpenOpacityDurationMs))
                .And()
                .Animate(Surface(content), SukiMotion.BlurProperty).From(p.BlurredRadius).To(0.0).Over(Ms(p.SurfaceTransitionDurationMs))
                .And()
                .Animate(Glass(content), BlurBackground.OverlayOpacityProperty).To(1.0).Over(Ms(p.GlassFadeMilliseconds))
                .PlayAsync();
        }

        /// <summary>Closes downward: the dialog sinks below its resting place, dissolving back
        /// into blur and glass, on the open's calibration.</summary>
        internal void PlayClose(ContentControl content)
        {
            var p = _getProfile();
            _content = content;

            _ = Motion.Of(content)
                .Animate(SukiMotion.TranslateXProperty).To(0.0).Over(_duration).WithEasing(_easing)
                .And()
                .Animate(SukiMotion.TranslateYProperty).To(p.EmergenceVertical).Over(_duration).WithEasing(_easing)
                .And()
                .Animate(SukiMotion.ScaleProperty).To(p.CloseScale).Over(_duration).WithEasing(_easing)
                .And()
                .Animate(Visual.OpacityProperty).To(0.0).Over(Ms(p.OpenOpacityDurationMs))
                .And()
                .Animate(Surface(content), SukiMotion.BlurProperty).To(p.BlurredRadius).Over(Ms(p.SurfaceTransitionDurationMs))
                .And()
                .Animate(Glass(content), BlurBackground.OverlayOpacityProperty).To(0.0).Over(Ms(p.GlassFadeMilliseconds))
                .PlayAsync();
        }

        /// <summary>The pinned dialog, pressed on its backdrop, is struck: the vertical offset
        /// gets an impulse from the pose on screen and springs back. Everything else stops
        /// where it is; once the shake settles, the dialog returns to rest — a shake that
        /// interrupted the opening lets the remaining travel continue.</summary>
        internal async void StartShake(ContentControl content, double initialVelocity)
        {
            var p = _getProfile();
            _content = content;

            bool settled = await Motion.Of(content)
                .Animate(SukiMotion.TranslateYProperty)
                    .To(0.0)
                    .WithSpring(p.ShakeSpring)
                    .AddVelocity(initialVelocity)
                .PlayAsync();

            if (settled)
                PlayRest(content);
        }

        /// <summary>Puts a dialog at once in its open or closed pose (fresh template).</summary>
        internal static void Pose(ContentControl content, bool open) =>
            _ = Motion.Of(content)
                .Set(SukiMotion.TranslateXProperty).To(0.0)
                .And()
                .Set(SukiMotion.TranslateYProperty).To(0.0)
                .And()
                .Set(SukiMotion.ScaleProperty).To(open ? 1.0 : 0.72)
                .And()
                .Set(Visual.OpacityProperty).To(open ? 1.0 : 0.0)
                .PlayAsync();

        /// <summary>Stops whatever runs (host detach): the poses freeze where they are.</summary>
        internal void Stop()
        {
            if (_content is { } content)
                Motion.Stop(content);
        }

        public void Dispose() => Stop();

        private void PlayRest(ContentControl content)
        {
            var p = _getProfile();
            _ = Motion.Of(content)
                .Animate(SukiMotion.TranslateXProperty).To(0.0).Over(_duration).WithEasing(_easing)
                .And()
                .Animate(SukiMotion.TranslateYProperty).To(0.0).Over(_duration).WithEasing(_easing)
                .And()
                .Animate(SukiMotion.ScaleProperty).To(1.0).Over(_duration).WithEasing(_easing)
                .And()
                .Animate(Visual.OpacityProperty).To(1.0).Over(Ms(p.OpenOpacityDurationMs))
                .And()
                .Animate(Surface(content), SukiMotion.BlurProperty).To(0.0).Over(Ms(p.SurfaceTransitionDurationMs))
                .And()
                .Animate(Glass(content), BlurBackground.OverlayOpacityProperty).To(1.0).Over(Ms(p.GlassFadeMilliseconds))
                .PlayAsync();
        }

        // The surface and the glass live inside the SukiDialog's own template: found on every
        // entry (a re-applied template or a new pooled dialog brings fresh ones).
        private static Border? Surface(ContentControl content) =>
            content.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.Name == "PART_DialogSurface");

        private static BlurBackground? Glass(ContentControl content) =>
            content.GetVisualDescendants().OfType<BlurBackground>().FirstOrDefault();

        private static TimeSpan Ms(double milliseconds) => TimeSpan.FromMilliseconds(milliseconds);

        private static double Lerp(double from, double to, double t) => from + (to - from) * t;
    }
}
