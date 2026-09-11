using Avalonia;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// The shared Enable/lifecycle plumbing of every Suki motion: flipping the attached
    /// Enable property attaches and disposes the Mover returned by <see cref="Attach"/>,
    /// exactly once per element (style re-application included). Each closed generic owns
    /// its own XAML-visible Enable, so motions never interfere. Behaviors declare their
    /// host contract themselves in <see cref="Attach"/> — an unsupported element type is
    /// a logged no-op, same as an unsupported popup host. Attach stays internal: the
    /// SukiUI.Motion vocabulary is an engine-internal layer, never a public API.
    /// </summary>
    public abstract class SukiMotion<TSelf>
        where TSelf : SukiMotion<TSelf>, new()
    {
        // One stateless description instance per closed motion — the handler is static,
        // Attach is the motion's (instance) declaration.
        private static readonly TSelf Instance = new();

        public static readonly AttachedProperty<bool> EnableProperty =
            AvaloniaProperty.RegisterAttached<TSelf, AvaloniaObject, bool>("Enable");

        private static readonly AttachedProperty<Mover?> MoverProperty =
            AvaloniaProperty.RegisterAttached<TSelf, AvaloniaObject, Mover?>("Mover");

        static SukiMotion()
        {
            EnableProperty.Changed.AddClassHandler<AvaloniaObject>(OnEnableChanged);
        }

        public static bool GetEnable(AvaloniaObject element) => element.GetValue(EnableProperty);
        public static void SetEnable(AvaloniaObject element, bool value) => element.SetValue(EnableProperty, value);

        private static void OnEnableChanged(AvaloniaObject element, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is true)
            {
                if (element.GetValue(MoverProperty) is { })
                    return; // already wired (style re-application)
                element.SetValue(MoverProperty, Instance.Attach(element));
            }
            else
            {
                element.GetValue(MoverProperty)?.Dispose();
                element.SetValue(MoverProperty, null);
            }
        }

        /// <summary>Runs the motion's declaration on the element and returns its Mover — the
        /// single handle disposed on disable (the Mover owns the whole gesture lifecycle, and
        /// with the Motion port, popup/dialog lifecycles too). Null = Enable is a logged
        /// no-op on this element (an unsupported host type or popup host).</summary>
        internal abstract Mover? Attach(AvaloniaObject element);
    }
}
