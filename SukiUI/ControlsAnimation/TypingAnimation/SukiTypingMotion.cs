using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using SukiUI.Motion;
using SukiUI.Theme;

namespace SukiUI.ControlsAnimation
{
    // The SukiPressMotion precedent: the simple name "Motion" would bind to the
    // sibling namespace — this alias restores the class binding.
    using Motion = SukiUI.Motion.Motion;

    /// <summary>
    /// The TextBox typing-glow behavior described declaratively over the SukiUI.Motion
    /// engine: the primary border overlay is a STRUCK SPRING. Its resting point is the
    /// lazy target — 50% opacity while focused or hovered, 0 otherwise — and every
    /// keystroke kicks the spring's velocity (the dialog-shake pattern), so the border
    /// brightens continuously with typing frequency and settles back on pause, with no
    /// discrete steps and no timers. Renders through an overlay Border independent of
    /// GlassCardBorder, so it works identically in light and dark themes.
    /// </summary>
    public class SukiTypingMotion : SukiMotion<SukiTypingMotion>
    {
        internal override Mover? Attach(AvaloniaObject owner)
        {
            if (owner is not TextBox box)
            {
                Debug.WriteLine($"SukiTypingMotion: '{owner.GetType().Name}' is not a TextBox — Enable ignored.");
                return null;
            }

            var energy = Motion.For(box).Property(TextBoxExtensions.TypingIntensityProperty);

            // The full legitimate pose range: kicks legitimately push the pose above the
            // 0.5 rest point, and the channel's defensive pose-clamp window must never
            // snap it back (the clamp window grows from tracked targets only — without
            // this, every kick above rest would flash down to 0.5).
            energy.Track(0.0);
            energy.Track(1.0);

            // Per-gesture profile snapshot (a live SukiAnimationTheme switch applies to
            // the NEXT gesture, never mid-flight).
            SukiTypingProfile P() => SukiAnimationTheme.Current.Typing[SukiTypingPreset.TextBox];

            // The resting point re-resolves live: focus/hover mid-flight retargets the
            // spring without touching pose or velocity (the mid-bounce retarget rule).
            double Rest() => box.IsFocused || box.IsPointerOver ? 0.5 : 0.0;

            // Overdamped on purpose (zeta > 1): a kick is nearly invisible the frame it
            // lands and builds into a delayed hump that merges with the next keystrokes —
            // the "everything deferred and aggregated" rise, never a per-key flash.
            var settle = energy.To(Rest).Spring(() => new Spring(P().SpringOmega, P().SpringDecay));

            // The probe: a plain timed trajectory toward the same rest, offered on
            // hover/focus changes — an idle channel ramps over 150ms (the historical
            // BrushTransition feel), a spring in flight consumes it as a Retarget().
            var probe = energy.To(Rest).Over(TimeSpan.FromMilliseconds(150));

            // The kick: live velocity + kick, restarted through a choreography (Run alone
            // does not subscribe the channel to the ticker — the choreography owns the
            // frame subscription). One kick choreography at a time, the dialog's Play
            // rule: the previous is stopped frozen at its pose, the restarted spring
            // resumes pose + the armed kick.
            Choreography? current = null;
            void Kick()
            {
                double kick = P().Kick;
                if (kick <= 0)
                    return;
                settle.SeedVelocity(energy.Velocity + kick);
                current?.Stop();
                current = new Choreography().And(settle);
                current.Start(box);
            }

            return new Mover(box)
                .OnEvent(InputElement.PointerEnteredEvent, probe)
                .OnEvent(InputElement.PointerExitedEvent, probe)
                .OnPropertyChanged(InputElement.IsFocusedProperty, probe)
                // TextBox marks TextInput handled in its class handler.
                .OnEvent(InputElement.TextInputEvent, Kick, handledEventsToo: true)
                .OnDetachedFromVisualTree(energy.Pose(0.0));
        }
    }
}
