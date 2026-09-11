using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    // Inside this namespace the simple name "Motion" would bind to the SIBLING NAMESPACE
    // SukiUI.Motion (a member of SukiUI) before any outer using is consulted — this alias,
    // declared in the namespace body, restores the class binding so descriptions read
    // Motion.For(...) exactly as written in Plan.md.
    using Motion = SukiUI.Motion.Motion;

    /// <summary>
    /// The ToggleSwitch knob behavior described declaratively over the SukiUI.Motion
    /// engine (see SukiUI.Motion/Plan.md): the knob is a physical puck — it SNAPS between
    /// its rest poses on a lightly damped spring (a slight overshoot against the end wall)
    /// while smearing (velocity-driven blur) and stretching along its travel (squash &amp;
    /// stretch derived from the same velocity), crisp and round at rest. The knob's resting
    /// shadow stays the template's BoxShadow: Blur and Shadow share the single Effect slot,
    /// so the smear owns the slot only while moving. Enable (from
    /// <see cref="SukiMotion{TSelf}"/>) and Preset attached properties, profile resolved
    /// per gesture through <see cref="SukiAnimationTheme"/> so a live switch applies to the
    /// NEXT gesture. Template contract: a knob part named PART_Knob with
    /// RenderTransformOrigin 50%,50% — positioning stays the template's, the motion only
    /// renders the travel on top of it.
    /// </summary>
    public class SukiToggleSwitchMotion : SukiMotion<SukiToggleSwitchMotion>
    {
        public static readonly AttachedProperty<SukiTogglePreset> PresetProperty =
            AvaloniaProperty.RegisterAttached<SukiToggleSwitchMotion, TemplatedControl, SukiTogglePreset>(
                "Preset", SukiTogglePreset.ToggleSwitch);

        public static SukiTogglePreset GetPreset(TemplatedControl element) => element.GetValue(PresetProperty);
        public static void SetPreset(TemplatedControl element, SukiTogglePreset value) => element.SetValue(PresetProperty, value);

        /// <summary>
        /// The snap behavior description. A re-toggle mid-flight preempts the spring with
        /// velocity carry — the knob keeps its momentum and turns around.
        /// </summary>
        internal override Mover? Attach(AvaloniaObject owner)
        {
            if (owner is not ToggleButton toggle)
            {
                Debug.WriteLine($"SukiToggleSwitchMotion: '{owner.GetType().Name}' is not a ToggleButton — Enable ignored.");
                return null;
            }

            var knob = Motion.For(toggle).Part("PART_Knob");

            // Per-gesture profile snapshot (resolved when each program starts — a live
            // SukiAnimationTheme switch applies to the NEXT gesture, never mid-flight).
            SukiToggleProfile P() => SukiAnimationTheme.Current.Toggle[GetPreset(toggle)];
            bool IsOn() => toggle.IsChecked == true;

            // The snap: a spring toward the live pose.
            var slide = knob.TranslateX
                .To(() => IsOn() ? P().Travel : 0.0)
                .Spring(() => new Spring(P().SpringOmega, P().SpringDecay));

            // One source of truth — the spring's |velocity| — drives every derived
            // channel: the smear and the squash/stretch, all gone at rest.
            double Speed() => Math.Abs(slide.Velocity);

            var snap = slide
                .And(new DerivedTrajectory(knob.Blur,
                    () => Math.Min(Speed() * P().BlurFactor, P().MaxBlur),
                    () => slide.Done))
                .And(new DerivedTrajectory(knob.ScaleX,
                    () => 1.0 + Math.Min(Speed() * P().SquashFactor, P().MaxSquash),
                    () => slide.Done))
                .And(new DerivedTrajectory(knob.ScaleY,
                    () => 1.0 - Math.Min(Speed() * P().SquashFactor, P().MaxSquash),
                    () => slide.Done));

            // Boot & template re-apply: pose the knob outright at its rest pose — no
            // visible slide for a switch born checked, and a fresh part re-poses. Both
            // start paths guard on staging: Attach runs during styling, before the visual
            // tree exists (no TopLevel — the ticker throws), and so can an early bound
            // IsChecked; whatever is skipped pre-staging is corrected by this same pose
            // at TemplateApplied, which always fires after the control is staged.
            void Pose()
            {
                if (TopLevel.GetTopLevel(toggle) is null)
                    return;
                new Choreography()
                    .And(knob.TranslateX.Pose(IsOn() ? P().Travel : 0.0))
                    .Start(toggle);
            }

            Pose();
            void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e) => Pose();
            toggle.TemplateApplied += OnTemplateApplied;

            return new Mover(toggle)
                .OnPropertyChanged(ToggleButton.IsCheckedProperty, () =>
                {
                    if (TopLevel.GetTopLevel(toggle) is not null)
                        snap.Start(toggle);
                })
                .OnDispose(() => toggle.TemplateApplied -= OnTemplateApplied);
        }
    }
}
