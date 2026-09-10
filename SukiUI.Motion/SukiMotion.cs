using Avalonia;

namespace SukiUI.Motion
{
    /// <summary>
    /// The shared Enable/lifecycle plumbing of every Suki motion (press today, popup and
    /// dialog with the Motion port tomorrow): flipping the attached Enable property attaches
    /// and disposes the Mover returned by <see cref="Attach"/>, exactly once per element
    /// (style re-application included). Each closed generic owns its own XAML-visible
    /// Enable, so motions never interfere: SukiPressMotion.Enable only ever drives presses.
    /// TTarget is the motion's natural host type (InputElement for pointer gestures,
    /// TemplatedControl for popup/dialog hosts). The class lives in the engine assembly —
    /// it is motions' base vocabulary; Attach stays engine-internal like the Mover it hands.
    /// </summary>
    public abstract class SukiMotion<TSelf, TTarget>
        where TSelf : SukiMotion<TSelf, TTarget>, new()
        where TTarget : AvaloniaObject
    {
        // One stateless description instance per closed motion — the handler is static,
        // Attach is the motion's (instance) declaration.
        private static readonly TSelf Instance = new();

        public static readonly AttachedProperty<bool> EnableProperty =
            AvaloniaProperty.RegisterAttached<TSelf, TTarget, bool>("Enable");

        private static readonly AttachedProperty<Mover?> MoverProperty =
            AvaloniaProperty.RegisterAttached<TSelf, TTarget, Mover?>("Mover");

        static SukiMotion()
        {
            EnableProperty.Changed.AddClassHandler<TTarget>(OnEnableChanged);
        }

        public static bool GetEnable(TTarget element) => element.GetValue(EnableProperty);
        public static void SetEnable(TTarget element, bool value) => element.SetValue(EnableProperty, value);

        private static void OnEnableChanged(TTarget element, AvaloniaPropertyChangedEventArgs e)
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
        /// single handle disposed on disable. Null = Enable is a logged no-op on this
        /// element. Internal like Mover: the declaration seam lives in the host library.</summary>
        internal abstract Mover? Attach(TTarget element);
    }
}
