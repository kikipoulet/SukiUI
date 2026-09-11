using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// How a popup motion reads and writes the open state of a popup host control. This is
    /// the single host-specific seam of the popup motion: supporting a new control type
    /// (AutoCompleteBox, …) means adding one adapter here — everything else (springs,
    /// blur, cascade, lifecycle) is generic.
    /// </summary>
    internal interface ISukiPopupHost
    {
        /// <summary>The open/close property watched on the host (drives Open/Close).</summary>
        AvaloniaProperty OpenProperty { get; }

        bool IsOpen(TemplatedControl host);

        void SetOpen(TemplatedControl host, bool open);
    }

    internal sealed class ComboBoxPopupHost : ISukiPopupHost
    {
        public static readonly ComboBoxPopupHost Instance = new();

        public AvaloniaProperty OpenProperty => ComboBox.IsDropDownOpenProperty;

        public bool IsOpen(TemplatedControl host) => ((ComboBox)host).IsDropDownOpen;

        public void SetOpen(TemplatedControl host, bool open) => ((ComboBox)host).IsDropDownOpen = open;
    }

    internal static class SukiPopupHosts
    {
        /// <summary>
        /// Resolves the host adapter for a control type. Unsupported types return null —
        /// <c>SukiPopupMotion.Enable</c> then no-ops (with a debug trace) instead of throwing.
        /// </summary>
        public static ISukiPopupHost? Resolve(AvaloniaObject control) => control switch
        {
            ComboBox => ComboBoxPopupHost.Instance,
            _ => null,
        };
    }
}
