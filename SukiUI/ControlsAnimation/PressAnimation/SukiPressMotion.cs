using System.Diagnostics;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Input;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    // Inside this namespace, the simple name "Motion" would bind to the SIBLING NAMESPACE
    // SukiUI.Motion (a member of SukiUI) before any outer using is consulted — this alias,
    // declared in the namespace body, restores the class binding so descriptions read
    // Motion.For(...) exactly as written in Plan.md.
    using Motion = SukiUI.Motion.Motion;

    /// <summary>
    /// The button/combobox press behavior described declaratively over the SukiUI.Motion
    /// engine (see SukiUI.Motion/Plan.md): channels → named trajectories → trigger wiring.
    /// Enable (from <see cref="SukiMotion{TSelf}"/>)/Preset/PressDepth attached properties,
    /// profile resolved per gesture through <see cref="SukiAnimationTheme"/> so a live
    /// switch applies to the NEXT gesture — with every line of ticker/integration/transform
    /// plumbing living in the engine instead of an imperative physics class.
    /// </summary>
    public class SukiPressMotion : SukiMotion<SukiPressMotion>
    {
        public static readonly AttachedProperty<SukiPressPreset> PresetProperty =
            AvaloniaProperty.RegisterAttached<SukiPressMotion, InputElement, SukiPressPreset>(
                "Preset", SukiPressPreset.Button);

        // NaN sentinel = "follow the preset's DefaultPressDepth" (Button 0.96, ComboBox 0.982).
        public static readonly AttachedProperty<double> PressDepthProperty =
            AvaloniaProperty.RegisterAttached<SukiPressMotion, InputElement, double>(
                "PressDepth", double.NaN);

        public static SukiPressPreset GetPreset(InputElement element) => element.GetValue(PresetProperty);
        public static void SetPreset(InputElement element, SukiPressPreset value) => element.SetValue(PresetProperty, value);

        public static double GetPressDepth(InputElement element) => element.GetValue(PressDepthProperty);
        public static void SetPressDepth(InputElement element, double value) => element.SetValue(PressDepthProperty, value);

        /// <summary>
        /// The press behavior description — SukiPressPhysics retold in ~25 lines. Hover eases
        /// up; the elastic descent is guaranteed to play to the end (a release meanwhile is
        /// memorized, a re-press purges and re-arms from the current pose); the deep stretch
        /// tensions down to the floor while the press holds; the release is a real spring
        /// whose resting point re-resolves live when the pointer enters/leaves mid-bounce.
        /// All values are read from the active profile when each program starts.
        /// </summary>
        internal override Mover? Attach(AvaloniaObject owner)
        {
            if (owner is not InputElement element)
            {
                Debug.WriteLine($"SukiPressMotion: '{owner.GetType().Name}' is not an InputElement — Enable ignored.");
                return null;
            }

            var scale = Motion.For(element).Scale;
            

            // Per-gesture profile snapshot (resolved when each program starts — a live
            // SukiAnimationTheme switch applies to the NEXT gesture, never mid-animation).
            SukiPressProfile P() => SukiAnimationTheme.Current.Press[GetPreset(element)];
            double Depth()
            {
                double depth = GetPressDepth(element);
                return double.IsNaN(depth) ? P().DefaultPressDepth : depth;
            }

            var hoverEase = new CubicEaseOut();
            var pressEase = new SukiEaseElasticIn { Damping = 2.5, Frequency = 3 };
            var deepEase = new LinearEasing();

            var hoverIn = scale
                .To(() => P().HoverScale)
                .Over(() => P().HoverDuration).Ease(hoverEase);
            var hoverOut = scale
                .To(1.0)
                .Over(() => P().HoverDuration).Ease(hoverEase);

            // Release equilibrium, re-resolved at pointer entries/exits: mid-bounce, the
            // target moves without a snap — from pose + velocity.
            var relax = scale
                .To(() => element.IsPointerOver ? P().HoverScale : 1.0)
                .Spring(() => new Spring(P().SpringOmega, P().SpringDecay));

            // MustFinish means that this animation will be played to the end even if pressed state is interrupted by release
            // We wants that because if we aimed a 'compression' state of 95% and use a translation, the click release will interrupt the compression animation before we even reach it. if we increase the animation speed to match the click duration, it looks worse
            // So we need to configure a "mandatory" animation to our depth that mustfinish and will trigger the released animation once we reach the compression we wanted, while permitting to do a long click that will compress the button a little more, while the release will adapt to the compression position
            // Immersivity is the goal here, we wants animations that adapt to user click, and could adapt to more (button weight, ..)
            var pressChain = scale
                .To(Depth)
                .Over(() => P().PressDuration).Ease(pressEase).MustFinish()
                .Then(scale.To(() => Depth() - P().ExtraDeepRange) // This part can be interrupted
                    .Over(() => P().DeepDuration).Ease(deepEase));

            return new Mover(element)
                .OnEvent(InputElement.PointerEnteredEvent, hoverIn)
                .OnEvent(InputElement.PointerExitedEvent, hoverOut)
                // Buttons and combo boxes mark these handled in their class handlers.
                .OnEvent(InputElement.PointerPressedEvent, pressChain, handledEventsToo: true)
                .OnEvent(InputElement.PointerReleasedEvent, relax, handledEventsToo: true)
                .OnEvent(InputElement.PointerCaptureLostEvent, relax)
                .OnDetachedFromVisualTree(scale.Pose(1.0));
        }
    }
}
