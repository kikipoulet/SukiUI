using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;

namespace DialogHost {
    internal class DialogPopupPositioner : IPopupPositioner {
        private IManagedPopupPositionerPopup _popup;

        public DialogPopupPositioner(IManagedPopupPositionerPopup popup) {
            _popup = popup;
        }

        public void Update(PopupPositionerParameters parameters) {
            // Simplify calculations
            var horizontalMargin = (parameters.AnchorRectangle.Width - parameters.Size.Width) / 2;
            var verticalMargin = (parameters.AnchorRectangle.Height - parameters.Size.Height) / 2;
            _popup.MoveAndResize(new Point(horizontalMargin, verticalMargin), parameters.Size / _popup.Scaling);
        }
    }
}