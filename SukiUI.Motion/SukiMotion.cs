using Avalonia;
using Avalonia.Interactivity;

namespace SukiUI.Motion
{
    /// <summary>
    /// The shared Enable/lifecycle plumbing of every motion: flipping the attached
    /// Enable property attaches and disposes the <see cref="Mover"/> returned by
    /// <see cref="Attach"/>, exactly once per element (style re-application included).
    /// Each closed generic owns its own XAML-visible Enable, so motions never interfere:
    /// <c>MyMotion.Enable</c> only ever drives <c>MyMotion</c>. Behaviors declare their
    /// host contract themselves in <see cref="Attach"/> — an unsupported element type is
    /// a logged no-op. This base and <see cref="Simulate"/> are part of the framework's
    /// public surface: the XAML and programmatic entry points; the description
    /// vocabulary itself stays internal where it can.
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
        /// single handle disposed on disable (the Mover owns the whole gesture lifecycle).
        /// Null = Enable is a logged no-op on this element (an unsupported host type).</summary>
        internal abstract Mover? Attach(AvaloniaObject element);

        /// <summary>Gets (or lazily attaches) the element's Mover — the exact instance the
        /// Enable wiring uses, no duplicate.</summary>
        internal static Mover? EnsureMover(AvaloniaObject element)
        {
            if (element.GetValue(MoverProperty) is { } mover)
                return mover;
            var attached = Instance.Attach(element);
            element.SetValue(MoverProperty, attached);
            return attached;
        }

        /// <summary>Fires every trigger registered for <paramref name="event"/> as if the
        /// event had been raised — the programmatic drive (no pointer input can be
        /// synthesized in Avalonia; benchmark pages drive real descriptions this way).
        /// Real events keep working alongside. Reads as
        /// <c>MyMotion.Simulate(button, InputElement.PointerPressedEvent)</c>.</summary>
        public static void Simulate(AvaloniaObject element, RoutedEvent @event) =>
            EnsureMover(element)?.Simulate(@event);
    }
}
