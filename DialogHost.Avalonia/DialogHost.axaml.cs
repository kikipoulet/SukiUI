using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace DialogHost {
    public class DialogHost : ContentControl {
        public const string ContentCoverGridName = "PART_ContentCoverGrid";
        public const string OverlayLayerName = "PART_OverlayLayer";

        private static readonly HashSet<DialogHost> LoadedInstances = new();

        public static readonly DirectProperty<DialogHost, string?> IdentifierProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, string?>(
                nameof(Identifier),
                o => o.Identifier,
                (o, v) => o.Identifier = v);

        public static readonly StyledProperty<object> DialogContentProperty =
            AvaloniaProperty.Register<DialogHost, object>(nameof(DialogContent));

        public static readonly StyledProperty<IDataTemplate> DialogContentTemplateProperty =
            AvaloniaProperty.Register<DialogHost, IDataTemplate>(nameof(DialogContentTemplate));

        public static readonly StyledProperty<IBrush> OverlayBackgroundProperty =
            AvaloniaProperty.Register<DialogHost, IBrush>(nameof(OverlayBackground));

        public static readonly StyledProperty<Thickness> DialogMarginProperty =
            AvaloniaProperty.Register<DialogHost, Thickness>(nameof(DialogMargin));

        public static readonly DirectProperty<DialogHost, bool> IsOpenProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, bool>(
                nameof(IsOpen),
                o => o.IsOpen,
                (o, v) => o.IsOpen = v);

        public static readonly RoutedEvent DialogOpenedEvent =
            RoutedEvent.Register<DialogHost, DialogOpenedEventArgs>(nameof(DialogOpened), RoutingStrategies.Bubble);

        public static readonly DirectProperty<DialogHost, bool> CloseOnClickAwayProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, bool>(
                nameof(CloseOnClickAway),
                o => o.CloseOnClickAway,
                (o, v) => o.CloseOnClickAway = v);

        public static readonly DirectProperty<DialogHost, object?> CloseOnClickAwayParameterProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, object?>(
                nameof(CloseOnClickAwayParameter),
                o => o.CloseOnClickAwayParameter,
                (o, v) => o.CloseOnClickAwayParameter = v);

        public static readonly RoutedEvent DialogClosingEvent =
            RoutedEvent.Register<DialogHost, DialogClosingEventArgs>(nameof(DialogClosing), RoutingStrategies.Bubble);

        public static readonly DirectProperty<DialogHost, DialogClosingEventHandler> DialogClosingCallbackProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, DialogClosingEventHandler>(
                nameof(DialogClosingCallback),
                o => o.DialogClosingCallback,
                (o, v) => o.DialogClosingCallback = v);

        public static readonly DirectProperty<DialogHost, DialogOpenedEventHandler?> DialogOpenedCallbackProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, DialogOpenedEventHandler?>(
                nameof(DialogOpenedCallback),
                o => o.DialogOpenedCallback,
                (o, v) => o.DialogOpenedCallback = v);

        public static readonly DirectProperty<DialogHost, ICommand> OpenDialogCommandProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, ICommand>(
                nameof(OpenDialogCommand),
                o => o.OpenDialogCommand);

        public static readonly DirectProperty<DialogHost, ICommand> CloseDialogCommandProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, ICommand>(
                nameof(CloseDialogCommand),
                o => o.CloseDialogCommand);

        public static readonly StyledProperty<IControlTemplate> PopupTemplateProperty =
            AvaloniaProperty.Register<DialogHost, IControlTemplate>(nameof(PopupTemplate));
        
        public static readonly DirectProperty<DialogHost, bool> DisableOpeningAnimationProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, bool>(
                nameof(DisableOpeningAnimation),
                o => o.DisableOpeningAnimation,
                (o, v) => o.DisableOpeningAnimation = v);

        private bool _disableOpeningAnimation;

        private DialogClosingEventHandler? _asyncShowClosingEventHandler;
        private DialogOpenedEventHandler? _asyncShowOpenedEventHandler;

        private ICommand _closeDialogCommand;

        private bool _closeOnClickAway;

        private object? _closeOnClickAwayParameter;

        private DialogClosingEventHandler? _dialogClosingCallback;

        private DialogOpenedEventHandler? _dialogOpenedCallback;

        private TaskCompletionSource<object?>? _dialogTaskCompletionSource;

        private string? _identifier = default;

        private bool _internalIsOpen;

        private bool _isOpen;

        private ICommand _openDialogCommand;

        private OverlayLayer? _overlayLayer;
        private DialogOverlayPopupHost? _overlayPopupHost;
        private IInputElement? _restoreFocusDialogClose;

        private IDisposable? _templateDisposables;

        public DialogHost() {
            _closeDialogCommand = new DialogHostCommandImpl(InternalClose, o => IsOpen);
            _openDialogCommand = new DialogHostCommandImpl(o => ShowInternal(o, null, null), o => !IsOpen);
        }

        public IControlTemplate PopupTemplate {
            get => GetValue(PopupTemplateProperty);
            set => SetValue(PopupTemplateProperty, value);
        }

        public DialogOpenedEventHandler? DialogOpenedCallback {
            get => _dialogOpenedCallback;
            set => SetAndRaise(DialogOpenedCallbackProperty, ref _dialogOpenedCallback, value);
        }

        public ICommand OpenDialogCommand {
            get => _openDialogCommand;
            private set => SetAndRaise<ICommand>(OpenDialogCommandProperty, ref _openDialogCommand, value);
        }

        public ICommand CloseDialogCommand {
            get => _closeDialogCommand;
            private set => SetAndRaise<ICommand>(CloseDialogCommandProperty, ref _closeDialogCommand, value);
        }

        public string? Identifier {
            get => _identifier;
            set => SetAndRaise(IdentifierProperty, ref _identifier, value);
        }

        public object DialogContent {
            get => GetValue(DialogContentProperty);
            set => SetValue(DialogContentProperty, value);
        }

        public IDataTemplate DialogContentTemplate {
            get => GetValue(DialogContentTemplateProperty);
            set => SetValue(DialogContentTemplateProperty, value);
        }

        public IBrush OverlayBackground {
            get => GetValue(OverlayBackgroundProperty);
            set => SetValue(OverlayBackgroundProperty, value);
        }

        public Thickness DialogMargin {
            get => GetValue(DialogMarginProperty);
            set => SetValue(DialogMarginProperty, value);
        }

        public bool IsOpen {
            get => _isOpen;
            set {
                SetAndRaise(IsOpenProperty, ref _isOpen, value);
                IsOpenPropertyChangedCallback(this, value);
            }
        }

        public bool CloseOnClickAway {
            get => _closeOnClickAway;
            set => SetAndRaise(CloseOnClickAwayProperty, ref _closeOnClickAway, value);
        }

        public object? CloseOnClickAwayParameter {
            get => _closeOnClickAwayParameter;
            set => SetAndRaise(CloseOnClickAwayParameterProperty, ref _closeOnClickAwayParameter, value);
        }

        public bool DisableOpeningAnimation {
            get => _disableOpeningAnimation;
            set => SetAndRaise(DisableOpeningAnimationProperty, ref _disableOpeningAnimation, value);
        }

        /// <summary>
        /// Returns a DialogSession for the currently open dialog for managing it programmatically. If no dialog is open, CurrentSession will return null
        /// </summary>
        public DialogSession? CurrentSession { get; private set; }

        public DialogClosingEventHandler DialogClosingCallback {
            get => _dialogClosingCallback;
            set => SetAndRaise(DialogClosingCallbackProperty, ref _dialogClosingCallback, value);
        }

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static async Task<object?> Show(object content)
            => await Show(content, dialogIdentifier: null);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>        
        /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>        
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static async Task<object?> Show(object content, DialogOpenedEventHandler openedEventHandler)
            => await Show(content, null, openedEventHandler, null);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static async Task<object?> Show(object content, DialogClosingEventHandler closingEventHandler)
            => await Show(content, null, null, closingEventHandler);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>        
        /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>
        /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static async Task<object?> Show(object content, DialogOpenedEventHandler? openedEventHandler, DialogClosingEventHandler? closingEventHandler)
            => await Show(content, null, openedEventHandler, closingEventHandler);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="dialogIdentifier"><see cref="Identifier"/> of the instance where the dialog should be shown. Typically this will match an identifer set in XAML. <c>null</c> is allowed.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static async Task<object?> Show(object content, string? dialogIdentifier)
            => await Show(content, dialogIdentifier, null, null);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="dialogIdentifier"><see cref="Identifier"/> of the instance where the dialog should be shown. Typically this will match an identifer set in XAML. <c>null</c> is allowed.</param>
        /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content, string? dialogIdentifier, DialogOpenedEventHandler openedEventHandler)
            => Show(content, dialogIdentifier, openedEventHandler, null);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="dialogIdentifier"><see cref="Identifier"/> of the instance where the dialog should be shown. Typically this will match an identifer set in XAML. <c>null</c> is allowed.</param>        
        /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content, string? dialogIdentifier, DialogClosingEventHandler closingEventHandler)
            => Show(content, dialogIdentifier, null, closingEventHandler);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="dialogIdentifier"><see cref="Identifier"/> of the instance where the dialog should be shown. Typically this will match an identifer set in XAML. <c>null</c> is allowed.</param>
        /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>
        /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static async Task<object?> Show(object content, string? dialogIdentifier, DialogOpenedEventHandler? openedEventHandler,
            DialogClosingEventHandler? closingEventHandler) {
            if (content is null) throw new ArgumentNullException(nameof(content));
            return await GetInstance(dialogIdentifier).ShowInternal(content, openedEventHandler, closingEventHandler);
        }

        /// <summary>
        ///  Close a modal dialog.
        /// </summary>
        /// <param name="dialogIdentifier"> of the instance where the dialog should be closed. Typically this will match an identifer set in XAML. </param>
        public static void Close(string? dialogIdentifier)
            => Close(dialogIdentifier, null);

        /// <summary>
        ///  Close a modal dialog.
        /// </summary>
        /// <param name="dialogIdentifier"> of the instance where the dialog should be closed. Typically this will match an identifer set in XAML. </param>
        /// <param name="parameter"> to provide to close handler</param>
        public static void Close(string? dialogIdentifier, object? parameter) {
            DialogHost dialogHost = GetInstance(dialogIdentifier);
            if (dialogHost.CurrentSession is { } currentSession) {
                currentSession.Close(parameter);
                return;
            }

            throw new InvalidOperationException("DialogHost is not open.");
        }

        /// <summary>
        /// Retrieve the current dialog session for a DialogHost
        /// </summary>
        /// <param name="dialogIdentifier">The identifier to use to retrieve the DialogHost</param>
        /// <returns>The DialogSession if one is in process, or null</returns>
        public static DialogSession? GetDialogSession(string? dialogIdentifier) {
            DialogHost dialogHost = GetInstance(dialogIdentifier);
            return dialogHost.CurrentSession;
        }

        /// <summary>
        /// dialog instance exists
        /// </summary>
        /// <param name="dialogIdentifier">of the instance where the dialog should be closed. Typically this will match an identifer set in XAML.</param>
        /// <returns></returns>
        public static bool IsDialogOpen(string? dialogIdentifier) => GetDialogSession(dialogIdentifier)?.IsEnded == false;

        private static DialogHost GetInstance(string? dialogIdentifier) {
            if (LoadedInstances.Count == 0)
                throw new InvalidOperationException("No loaded DialogHost instances.");

            var targets = LoadedInstances.Where(dh => dialogIdentifier == null || Equals(dh.Identifier, dialogIdentifier)).ToList();
            if (targets.Count == 0)
                throw new InvalidOperationException(
                    $"No loaded DialogHost have an {nameof(Identifier)} property matching {nameof(dialogIdentifier)} ('{dialogIdentifier}') argument.");
            if (targets.Count > 1)
                throw new InvalidOperationException(
                    "Multiple viable DialogHosts. Specify a unique Identifier on each DialogHost, especially where multiple Windows are a concern.");

            return targets[0];
        }

        internal async Task<object?> ShowInternal(object content, DialogOpenedEventHandler? openedEventHandler,
            DialogClosingEventHandler? closingEventHandler) {
            if (IsOpen)
                throw new InvalidOperationException("DialogHost is already open.");

            _dialogTaskCompletionSource = new TaskCompletionSource<object?>();

            if (content != null)
                DialogContent = content;

            _asyncShowOpenedEventHandler = openedEventHandler;
            _asyncShowClosingEventHandler = closingEventHandler;
            IsOpen = true;

            object? result = await _dialogTaskCompletionSource.Task;

            _asyncShowOpenedEventHandler = null;
            _asyncShowClosingEventHandler = null;

            return result;
        }

        private static void IsOpenPropertyChangedCallback(DialogHost dialogHost, bool newValue) {
            if (newValue) {
                dialogHost.CurrentSession = new DialogSession(dialogHost);
                dialogHost._restoreFocusDialogClose = FocusManager.Instance.Current;
                
                if (dialogHost._overlayPopupHost != null)
                    dialogHost._overlayPopupHost.IsOpen = true;

                //multiple ways of calling back that the dialog has opened:
                // * routed event
                // * straight forward dependency property 
                // * handler provided to the async show method
                var dialogOpenedEventArgs = new DialogOpenedEventArgs(dialogHost.CurrentSession, DialogOpenedEvent);
                dialogHost.OnDialogOpened(dialogOpenedEventArgs);
                dialogHost.DialogOpenedCallback?.Invoke(dialogHost, dialogOpenedEventArgs);
                dialogHost._asyncShowOpenedEventHandler?.Invoke(dialogHost, dialogOpenedEventArgs);

                dialogHost._overlayPopupHost?.ConfigurePosition(dialogHost._overlayLayer, PlacementMode.AnchorAndGravity, new Point());
            }
            else {
                object? closeParameter = null;
                if (dialogHost.CurrentSession is { } session) {
                    if (!session.IsEnded) {
                        session.Close(session.CloseParameter);
                    }

                    //DialogSession.Close may attempt to cancel the closing of the dialog.
                    //When the dialog is closed in this manner it is not valid
                    if (!session.IsEnded) {
                        throw new InvalidOperationException($"Cannot cancel dialog closing after {nameof(IsOpen)} property has been set to {bool.FalseString}");
                    }

                    closeParameter = session.CloseParameter;
                    dialogHost.CurrentSession = null;
                }

                if (dialogHost._overlayPopupHost != null)
                    dialogHost._overlayPopupHost.IsOpen = false;
                //NB: _dialogTaskCompletionSource is only set in the case where the dialog is shown with Show
                dialogHost._dialogTaskCompletionSource?.TrySetResult(closeParameter);

                dialogHost._restoreFocusDialogClose?.Focus();
            }

            dialogHost.RaiseCommandsCanExecuteChanged();
        }

        protected void RaiseCommandsCanExecuteChanged() {
            (_openDialogCommand as DialogHostCommandImpl)?.OnCanExecuteChanged();
            (_closeDialogCommand as DialogHostCommandImpl)?.OnCanExecuteChanged();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            _templateDisposables?.Dispose();

            _overlayLayer = e.NameScope.Find<OverlayLayer>(OverlayLayerName);
            _overlayPopupHost = new DialogOverlayPopupHost(_overlayLayer) {
                Content = DialogContent, ContentTemplate = DialogContentTemplate, Template = PopupTemplate,
                Padding = DialogMargin, ClipToBounds = false, DisableOpeningAnimation = DisableOpeningAnimation
            };

            if (IsOpen) {
                _overlayPopupHost.IsOpen = true;
                _overlayPopupHost?.ConfigurePosition(_overlayLayer, PlacementMode.AnchorAndGravity, new Point());
            }

            _templateDisposables = new CompositeDisposable() {
                this.GetObservable(BoundsProperty)
                    .Subscribe(rect => _overlayPopupHost?.ConfigurePosition(_overlayLayer, PlacementMode.AnchorAndGravity, new Point())),
                _overlayPopupHost!.Bind(DisableOpeningAnimationProperty, this.GetBindingObservable(DisableOpeningAnimationProperty)),
                _overlayPopupHost!.Bind(ContentProperty, this.GetBindingObservable(DialogContentProperty)),
                _overlayPopupHost!.Bind(ContentTemplateProperty, this.GetBindingObservable(DialogContentTemplateProperty)),
                _overlayPopupHost!.Bind(TemplateProperty, this.GetBindingObservable(PopupTemplateProperty)),
                _overlayPopupHost!.Bind(PaddingProperty, this.GetBindingObservable(DialogMarginProperty)),
                e.NameScope.Find<Grid>(ContentCoverGridName)?.AddDisposableHandler(PointerReleasedEvent, ContentCoverGrid_OnPointerReleased) ?? Disposable.Empty
            };
            base.OnApplyTemplate(e);
        }

        private void ContentCoverGrid_OnPointerReleased(object sender, PointerReleasedEventArgs e) {
            if (CloseOnClickAway && CurrentSession != null) {
                InternalClose(CloseOnClickAwayParameter);
            }
        }

        protected void OnDialogOpened(DialogOpenedEventArgs dialogOpenedEventArgs) => RaiseEvent(dialogOpenedEventArgs);

        /// <summary>
        /// Raised when a dialog is opened.
        /// </summary>
        public event DialogOpenedEventHandler DialogOpened {
            add => AddHandler(DialogOpenedEvent, value);
            remove => RemoveHandler(DialogOpenedEvent, value);
        }

        /// <summary>
        /// Raised just before a dialog is closed.
        /// </summary>
        public event EventHandler<DialogClosingEventArgs> DialogClosing {
            add => AddHandler(DialogClosingEvent, value);
            remove => RemoveHandler(DialogClosingEvent, value);
        }

        protected void OnDialogClosing(DialogClosingEventArgs eventArgs) => RaiseEvent(eventArgs);

        internal void InternalClose(object? parameter) {
            var currentSession = CurrentSession ?? throw new InvalidOperationException($"{nameof(DialogHost)} does not have a current session");

            currentSession.CloseParameter = parameter;
            currentSession.IsEnded = true;

            //multiple ways of calling back that the dialog is closing:
            // * routed event
            // * straight forward IsOpen dependency property 
            // * handler provided to the async show method
            var dialogClosingEventArgs = new DialogClosingEventArgs(currentSession, DialogClosingEvent);
            OnDialogClosing(dialogClosingEventArgs);
            DialogClosingCallback?.Invoke(this, dialogClosingEventArgs);
            _asyncShowClosingEventHandler?.Invoke(this, dialogClosingEventArgs);

            if (dialogClosingEventArgs.IsCancelled) {
                currentSession.IsEnded = false;
                return;
            }

            IsOpen = false;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            LoadedInstances.Add(this);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnDetachedFromVisualTree(e);
            LoadedInstances.Remove(this);
        }
    }
}