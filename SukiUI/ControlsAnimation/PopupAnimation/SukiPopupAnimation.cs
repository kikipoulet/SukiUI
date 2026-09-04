using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// Unified open/close animation facade for template popups, over the shared
    /// <see cref="SukiPopupPhysics"/> engine driven by the single <see cref="SukiTicker"/>
    /// loop (see <see cref="SukiPopupProfile"/> for the calibrated feels). Enable it with
    /// <c>SukiPopupAnimation.Enable="True"</c> from a style setter and optionally pick a
    /// feel with <c>SukiPopupAnimation.Preset</c> (ComboBox by default).
    /// Template contract: the host's template must contain a <c>Popup</c> named
    /// <c>PART_SukiPopup</c> whose content root is a control named
    /// <c>PART_LayoutTransform</c> (the animated root), with an optional
    /// <c>PART_ItemsPresenter</c> for the item cascade. Host support is resolved through
    /// <c>SukiPopupHosts</c> (ComboBox today); Enable on an unsupported control type is a
    /// logged no-op.
    /// </summary>
    public class SukiPopupAnimation
    {
        public static readonly AttachedProperty<bool> EnableProperty =
            AvaloniaProperty.RegisterAttached<SukiPopupAnimation, TemplatedControl, bool>("Enable");

        public static readonly AttachedProperty<SukiPopupPreset> PresetProperty =
            AvaloniaProperty.RegisterAttached<SukiPopupAnimation, TemplatedControl, SukiPopupPreset>("Preset", SukiPopupPreset.ComboBox);

        private static readonly AttachedProperty<SukiPopupPhysics?> PhysicsProperty =
            AvaloniaProperty.RegisterAttached<SukiPopupAnimation, TemplatedControl, SukiPopupPhysics?>("Physics");

        static SukiPopupAnimation()
        {
            EnableProperty.Changed.AddClassHandler<TemplatedControl>(OnEnableChanged);
            PresetProperty.Changed.AddClassHandler<TemplatedControl>(OnPresetChanged);
        }

        public static bool GetEnable(TemplatedControl element) => element.GetValue(EnableProperty);
        public static void SetEnable(TemplatedControl element, bool value) => element.SetValue(EnableProperty, value);

        public static SukiPopupPreset GetPreset(TemplatedControl element) => element.GetValue(PresetProperty);
        public static void SetPreset(TemplatedControl element, SukiPopupPreset value) => element.SetValue(PresetProperty, value);

        // Unlike SukiPress (engine created lazily on the first gesture), the popup engine
        // must exist BEFORE any gesture: it owns the popup lifecycle wiring from the start.
        private static void OnEnableChanged(TemplatedControl element, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is true)
            {
                if (element.GetValue(PhysicsProperty) is { })
                    return; // already wired (style re-application)
                if (SukiPopupHosts.Resolve(element) is not { } hostAdapter)
                {
                    Debug.WriteLine($"SukiPopupAnimation: no host adapter for '{element.GetType().Name}' — Enable ignored.");
                    return;
                }
                var physics = new SukiPopupPhysics(element, SukiPopupProfile.For(GetPreset(element)), hostAdapter);
                element.SetValue(PhysicsProperty, physics);
            }
            else
            {
                element.GetValue(PhysicsProperty)?.Dispose();
                element.SetValue(PhysicsProperty, null);
            }
        }

        // Preset swapped at runtime (style change): the wiring must stay alive, so dispose
        // and recreate immediately (same shape as Enable, different from the lazy press).
        private static void OnPresetChanged(TemplatedControl element, AvaloniaPropertyChangedEventArgs e)
        {
            if (element.GetValue(EnableProperty) is not true)
                return;
            element.GetValue(PhysicsProperty)?.Dispose();
            if (SukiPopupHosts.Resolve(element) is not { } hostAdapter)
                return;
            var physics = new SukiPopupPhysics(element, SukiPopupProfile.For(GetPreset(element)), hostAdapter);
            element.SetValue(PhysicsProperty, physics);
        }
    }
}
