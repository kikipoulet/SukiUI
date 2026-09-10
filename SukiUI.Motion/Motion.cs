using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SukiUI.Motion
{
    /// <summary>
    /// The trigger wiring of one behavior description: routes the pointer events of one
    /// element to its programs. The subscription semantics are the proven engine's —
    /// pressed/released go through routed handlers with handledEventsToo (buttons and combo
    /// boxes mark those handled in their class handlers, so plain CLR subscriptions would
    /// never fire); entered/exited/capture-lost go through the CLR wrappers (direct routed
    /// events — AddHandler with a routing strategy would never match them); detach runs the
    /// pose program. <see cref="Dispose"/> unwires everything and leaves the control
    /// functional.
    /// </summary>
    internal sealed class Mover : IDisposable
    {
        private readonly InputElement _element;
        private Program? _entered, _exited, _pressed, _released, _captureLost;
        private PoseProgram? _detached;
        private bool _wired;

        internal Mover(InputElement element) => _element = element;

        internal Mover OnPointerEntered(Program program)
        {
            _entered = program;
            Wire();
            return this;
        }

        internal Mover OnPointerExited(Program program)
        {
            _exited = program;
            Wire();
            return this;
        }

        internal Mover OnPointerPressed(Program program)
        {
            _pressed = program;
            Wire();
            return this;
        }

        internal Mover OnPointerReleased(Program program)
        {
            _released = program;
            Wire();
            return this;
        }

        internal Mover OnPointerCaptureLost(Program program)
        {
            _captureLost = program;
            Wire();
            return this;
        }

        internal Mover OnDetachedFromVisualTree(PoseProgram pose)
        {
            _detached = pose;
            Wire();
            return this;
        }

        private void Wire()
        {
            if (_wired)
                return;
            _wired = true;
            _element.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed,
                RoutingStrategies.Bubble, handledEventsToo: true);
            _element.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased,
                RoutingStrategies.Bubble, handledEventsToo: true);
            _element.PointerEntered += OnPointerEntered;
            _element.PointerExited += OnPointerExited;
            _element.PointerCaptureLost += OnPointerCaptureLost;
            _element.DetachedFromVisualTree += OnDetachedFromVisualTree;
        }

        private void Unwire()
        {
            if (!_wired)
                return;
            _wired = false;
            _element.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
            _element.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
            _element.PointerEntered -= OnPointerEntered;
            _element.PointerExited -= OnPointerExited;
            _element.PointerCaptureLost -= OnPointerCaptureLost;
            _element.DetachedFromVisualTree -= OnDetachedFromVisualTree;
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e) => Offer(_pressed);

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e) => Offer(_released);

        private void OnPointerEntered(object? sender, PointerEventArgs e) => Offer(_entered);

        private void OnPointerExited(object? sender, PointerEventArgs e) => Offer(_exited);

        private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e) => Offer(_captureLost);

        private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) =>
            _detached?.Channel.Offer(_detached);

        private static void Offer(Program? program) => program?.Channel.Offer(program);

        /// <summary>Unwires everything and rests the channel at its detach pose — the control
        /// stays functional, just unanimated.</summary>
        public void Dispose()
        {
            Unwire();
            _detached?.Channel.Offer(_detached);
        }
    }
}
