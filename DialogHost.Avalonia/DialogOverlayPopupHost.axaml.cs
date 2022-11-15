using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace DialogHost {
    public class DialogOverlayPopupHost : ContentControl, IPopupHost, IInteractive, IManagedPopupPositionerPopup, ICustomKeyboardNavigation {
        public static readonly DirectProperty<DialogOverlayPopupHost, bool> IsOpenProperty =
            AvaloniaProperty.RegisterDirect<DialogOverlayPopupHost, bool>(
                nameof(IsOpen),
                o => o.IsOpen,
                (o, v) => o.IsOpen = v);
        
        public static readonly DirectProperty<DialogOverlayPopupHost, bool> IsAtBottomProperty =
            AvaloniaProperty.RegisterDirect<DialogOverlayPopupHost, bool>(
                nameof(IsAtBottom),
                o => o.IsAtBottom,
                (o, v) => o.IsAtBottom = v);

        public static readonly DirectProperty<DialogOverlayPopupHost, bool> IsActuallyOpenProperty =
            AvaloniaProperty.RegisterDirect<DialogOverlayPopupHost, bool>(
                nameof(IsActuallyOpen),
                o => o.IsActuallyOpen,
                (o, v) => o.IsActuallyOpen = v);

        public static readonly DirectProperty<DialogOverlayPopupHost, bool> DisableOpeningAnimationProperty =
            DialogHost.DisableOpeningAnimationProperty.AddOwner<DialogOverlayPopupHost>(
                host => host.DisableOpeningAnimation,
                (host, b) => host.DisableOpeningAnimation = b);

        private readonly OverlayLayer _overlayLayer;

        private bool _isActuallyOpen;
        private bool _disableOpeningAnimation;
        private bool _isOpen;
        private bool _isAtBottom;
        private Point _lastRequestedPosition;
        private DialogPopupPositioner _popupPositioner;
        private PopupPositionerParameters _positionerParameters = new PopupPositionerParameters();
        private bool _shown;

        public DialogOverlayPopupHost(OverlayLayer overlayLayer)
        {
            _overlayLayer = overlayLayer;
            _popupPositioner = new DialogPopupPositioner(this);
        }

        public bool IsOpen {
            get => _isOpen;
            set {
                SetAndRaise(IsOpenProperty, ref _isOpen, value);
                if (value) IsActuallyOpen = true;
            }
        }
        
        public bool IsAtBottom {
            get => _isAtBottom;
            set {
                SetAndRaise(IsAtBottomProperty, ref _isAtBottom, value);
             
            }
        }

        /// <summary>
        /// Controls <see cref="Show"/> and <see cref="Hide"/> calls. Used for closing animations
        /// </summary>
        /// <remarks>
        /// Actually you should use <see cref="IsOpen"/> for opening and closing dialog 
        /// </remarks>
        public bool IsActuallyOpen {
            get => _isActuallyOpen;
            set {
                // Styling system artifacts, don't process them
                if (IsOpen && !value) return;
                
                var previousValue = _isActuallyOpen;
                SetAndRaise(IsActuallyOpenProperty, ref _isActuallyOpen, value); 
                switch (previousValue) {
                    case true when !value:
                        Hide();
                        break;
                    case false when value:
                        Show();
                        break;
                }
            }
        }

        public bool DisableOpeningAnimation {
            get => _disableOpeningAnimation;
            set => SetAndRaise(DisableOpeningAnimationProperty, ref _disableOpeningAnimation, value);
        }

        /// <inheritdoc/>
        IInteractive IInteractive.InteractiveParent => Parent;

        IReadOnlyList<ManagedPopupPositionerScreenInfo> IManagedPopupPositionerPopup.Screens
        {
            get
            {
                var rc = new Rect(default, _overlayLayer.AvailableSize);
                return new[] {new ManagedPopupPositionerScreenInfo(rc, rc)};
            }
        }

        Rect IManagedPopupPositionerPopup.ParentClientAreaScreenGeometry =>
            new Rect(default, _overlayLayer.Bounds.Size);

        void IManagedPopupPositionerPopup.MoveAndResize(Point devicePoint, Size virtualSize)
        {
            _lastRequestedPosition = devicePoint;
            Dispatcher.UIThread.Post(() =>
            {
                OverlayLayer.SetLeft(this, _lastRequestedPosition.X);
                OverlayLayer.SetTop(this, _lastRequestedPosition.Y);
            }, DispatcherPriority.Layout);
        }

        double IManagedPopupPositionerPopup.Scaling => 1;

        public void SetChild(IControl control)
        {
            Content = control;
        }

        public IVisual HostedVisualTreeRoot => null;

        public bool Topmost { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Transform? Transform { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Dispose() => Hide();


        public void Show()
        {
            _overlayLayer.Children.Add(this);
            _shown = true;
            Focus();
            UpdatePosition();
        }

        public void Hide()
        {
            _overlayLayer.Children.Remove(this);
            _shown = false;
        }

        public IDisposable BindConstraints(AvaloniaObject popup, StyledProperty<double> widthProperty, StyledProperty<double> minWidthProperty,
                                           StyledProperty<double> maxWidthProperty, StyledProperty<double> heightProperty, StyledProperty<double> minHeightProperty,
                                           StyledProperty<double> maxHeightProperty, StyledProperty<bool> topmostProperty)
        {
            // Topmost property is not supported
            var bindings = new List<IDisposable>();

            void Bind(AvaloniaProperty what, AvaloniaProperty to) => bindings.Add(this.Bind(what, popup[~to]));
            Bind(WidthProperty, widthProperty);
            Bind(MinWidthProperty, minWidthProperty);
            Bind(MaxWidthProperty, maxWidthProperty);
            Bind(HeightProperty, heightProperty);
            Bind(MinHeightProperty, minHeightProperty);
            Bind(MaxHeightProperty, maxHeightProperty);
            
            return Disposable.Create(() =>
            {
                foreach (var x in bindings)
                    x.Dispose();
            });
        }

        public void ConfigurePosition(IVisual target, PlacementMode placement, Point offset,
                                      PopupAnchor anchor = PopupAnchor.None, PopupGravity gravity = PopupGravity.None,
                                      PopupPositionerConstraintAdjustment constraintAdjustment = PopupPositionerConstraintAdjustment.All,
                                      Rect? rect = null)
        {
            // This code handles only PlacementMode.AnchorAndGravity and other default values
            // Suitable only for current implementation of DialogHost
            _positionerParameters.AnchorRectangle = new Rect(default, target.Bounds.Size);
            
            UpdatePosition();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_positionerParameters.Size != finalSize)
            {
                _positionerParameters.Size = finalSize;
                UpdatePosition();
            }
            return base.ArrangeOverride(finalSize);
        }


        private void UpdatePosition()
        {
            // Don't bother the positioner with layout system artifacts
            if (_positionerParameters.Size.Width == 0 || _positionerParameters.Size.Height == 0)
                return;
            if (_shown)
            {
                if(_isAtBottom)
                    _popupPositioner.UpdateAtBottom(_positionerParameters);
                else
                    _popupPositioner.Update(_positionerParameters);
            }
        }

        public (bool handled, IInputElement? next) GetNext(IInputElement element, NavigationDirection direction) {
            if (!element.Equals(this)) {
                return (false, null);
            }
            var focusable = this.GetVisualDescendants().OfType<IInputElement>().FirstOrDefault(visual => visual.Focusable);
            return (true, focusable);
        }
    }
}