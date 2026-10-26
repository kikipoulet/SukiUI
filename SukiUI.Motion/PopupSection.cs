using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace SukiUI.Motion
{
    /// <summary>
    /// The popup of a control, animated open and closed. The section owns the real popup's
    /// <c>IsOpen</c>: it reveals the popup once the opening start pose is written (no
    /// one-frame flash), hides it only when the closing motion went to the end, plays the
    /// opening again if the popup is reopened mid-collapse (from the live pose and velocity),
    /// and keeps the host in sync when the popup closes behind its back.
    /// </summary>
    public sealed class PopupSection<TControl, TProfile> where TControl : TemplatedControl
    {
        internal readonly PopupDef Def = new();

        internal PopupSection()
        {
            var def = Def;
            Root = MotionTarget.FromContext((ctx, _) => PopupRuntime.For(ctx.ControlVisual, def)?.RootControl);
            Items = new MotionItems(ctx => PopupRuntime.For(ctx.ControlVisual, def)?.CollectItems() ?? Array.Empty<Visual>());
        }

        /// <summary>The animated content root of the popup.</summary>
        public MotionTarget Root { get; }

        /// <summary>The children of the popup's items panel, for a cascade.</summary>
        public MotionItems Items { get; }

        /// <summary>The popup is a named <see cref="Popup"/> of the control's template.</summary>
        public PopupSection<TControl, TProfile> PopupPart(string name) => PopupPart(_ => name);

        /// <inheritdoc cref="PopupPart(string)"/>
        public PopupSection<TControl, TProfile> PopupPart(Func<TControl, string> name)
        {
            Def.ResolvePopup = host => FindTemplatePopup(host, name((TControl)host));
            return this;
        }

        /// <summary>The popup is found by your own rule (a popup built in code).</summary>
        public PopupSection<TControl, TProfile> ResolvedBy(Func<TControl, Popup?> resolve)
        {
            Def.ResolvePopup = host => resolve((TControl)host);
            return this;
        }

        /// <summary>The animated root: the popup's child when it has this name, otherwise the
        /// part of that name inside the child's template.</summary>
        public PopupSection<TControl, TProfile> RootPart(string name)
        {
            Def.RootName = name;
            return this;
        }

        /// <summary>The ItemsPresenter (inside the root) whose items the cascade animates.</summary>
        public PopupSection<TControl, TProfile> ItemsPresenterPart(string name)
        {
            Def.ItemsPresenterName = name;
            return this;
        }

        /// <summary>The bool property of the host that says whether it wants its popup open
        /// (IsDropDownOpen, IsSubMenuOpen, IsOpen…). Watched, and written back on outside
        /// presses and abnormal closes.</summary>
        public PopupSection<TControl, TProfile> OpenWhen(AvaloniaProperty<bool> property) => OpenWhen(_ => property);

        /// <inheritdoc cref="OpenWhen(AvaloniaProperty{bool})"/>
        public PopupSection<TControl, TProfile> OpenWhen(Func<TControl, AvaloniaProperty?> property)
        {
            Def.OpenProperty = host => property((TControl)host);
            return this;
        }

        /// <summary>How to close the host when the section must (default: set the open
        /// property to false — override it when that property is read-only).</summary>
        public PopupSection<TControl, TProfile> CloseHostWith(Action<TControl> close)
        {
            Def.CloseHost = host => close((TControl)host);
            return this;
        }

        /// <summary>Any press in the host's window outside the popup closes it.</summary>
        public PopupSection<TControl, TProfile> CloseOnPointerPressedOutside(Func<TControl, bool>? when = null)
        {
            Def.CloseOnOutsidePress = when is null ? static _ => true : host => when((TControl)host);
            return this;
        }

        /// <summary>Presses inside the popup stop at the popup and never reach the host. For a
        /// host that only knows a stand-in popup (the ComboBox keeps an empty PART_Popup for
        /// its own logic): it would take a press on the real drop-down for an outside press,
        /// and close on it.</summary>
        public PopupSection<TControl, TProfile> KeepPressesInside(Func<TControl, bool>? when = null)
        {
            Def.KeepPressesInside = when is null ? static _ => true : host => when((TControl)host);
            return this;
        }

        /// <summary>A click that selects one of the host's items closes the host (animated). For
        /// an items host whose own close-after-selection targets a stand-in popup and so does
        /// nothing (the ComboBox closes its PART_Popup).</summary>
        public PopupSection<TControl, TProfile> CloseWhenItemClicked(Func<TControl, bool>? when = null)
        {
            Def.CloseWhenItemClicked = when is null ? static _ => true : host => when((TControl)host);
            return this;
        }

        /// <summary>The popup closes at once, without animation, when its window loses focus.</summary>
        public PopupSection<TControl, TProfile> CloseInstantlyWhenWindowDeactivated()
        {
            Def.CloseOnDeactivated = true;
            return this;
        }

        /// <summary>What plays when the popup opens (the popup is revealed after the From poses are written).</summary>
        public StepStarter<TControl, TProfile, EventArgs> OnOpening() => new(ChainState.ForReaction(Def.Open));

        /// <summary>What plays when the popup closes (the popup is hidden only if this goes to the end).</summary>
        public StepStarter<TControl, TProfile, EventArgs> OnClosing() => new(ChainState.ForReaction(Def.Close));

        private static Popup? FindTemplatePopup(TemplatedControl host, string name) =>
            host.GetTemplateDescendants().OfType<Popup>().FirstOrDefault(p => p.Name == name);
    }

    public static class PopupSectionExtensions
    {
        /// <summary>Declares the popup of a templated control.</summary>
        public static PopupSection<TControl, TProfile> Popup<TControl, TProfile>(this MotionBuilder<TControl, TProfile> motion)
            where TControl : TemplatedControl
        {
            var section = new PopupSection<TControl, TProfile>();
            motion.Definition.Sections.Add(section.Def);
            return section;
        }

        /// <summary>A ContextMenu closes itself (light dismiss): its <c>Closing</c> is cancelled
        /// while the closing motion plays, and the menu really closes at its end. A deactivated
        /// window is never held — the menu closes at once.</summary>
        public static PopupSection<ContextMenu, TProfile> HoldOpenThroughCollapse<TProfile>(this PopupSection<ContextMenu, TProfile> section)
        {
            section.Def.HoldClosing = true;
            return section;
        }
    }

    internal sealed class PopupDef : ISectionDef
    {
        public Func<TemplatedControl, Popup?>? ResolvePopup;
        public string? RootName;
        public string? ItemsPresenterName;
        public Func<TemplatedControl, AvaloniaProperty?>? OpenProperty;
        public Action<TemplatedControl>? CloseHost;
        public Func<TemplatedControl, bool>? CloseOnOutsidePress;
        public Func<TemplatedControl, bool>? KeepPressesInside;
        public Func<TemplatedControl, bool>? CloseWhenItemClicked;
        public bool CloseOnDeactivated;
        public bool HoldClosing;
        public readonly ReactionDef Open = new();
        public readonly ReactionDef Close = new();

        public void Validate()
        {
            if (ResolvePopup is null)
                throw new InvalidOperationException("Popup() needs PopupPart(...) or ResolvedBy(...).");
            if (RootName is null)
                throw new InvalidOperationException("Popup() needs RootPart(...).");
            if (OpenProperty is null)
                throw new InvalidOperationException("Popup() needs OpenWhen(...).");
            Open.Validate();
            Close.Validate();
        }

        public IDisposable? Attach(Interactive control, Func<MotionContext> createContext) =>
            control is TemplatedControl host ? PopupRuntime.Create(host, this, createContext) : null;
    }

    /// <summary>The popup section wired on one host.</summary>
    internal sealed class PopupRuntime : IDisposable
    {
        private static readonly ConditionalWeakTable<Visual, List<PopupRuntime>> ByHost = new();

        private readonly TemplatedControl _host;
        private readonly PopupDef _def;
        private readonly Func<MotionContext> _createContext;
        private readonly AvaloniaProperty _openProperty;

        private Popup? _popup;
        private Control? _root;
        private ItemsPresenter? _itemsPresenter;
        private Run? _current;
        private ReactionDef? _pending;   // parked while the popup content does not resolve yet
        private bool _pendingHostOpen;   // the host intent the parked reaction was played for
        private bool _selfClosing;       // our own close at the end of the closing motion
        private TopLevel? _ambient;
        private Window? _window;

        private PopupRuntime(TemplatedControl host, PopupDef def, Func<MotionContext> createContext, AvaloniaProperty openProperty)
        {
            _host = host;
            _def = def;
            _createContext = createContext;
            _openProperty = openProperty;
            ByHost.GetValue(host, static _ => new List<PopupRuntime>()).Add(this);

            host.TemplateApplied += OnTemplateApplied;
            host.AttachedToLogicalTree += OnHostAttachedToLogicalTree;
            host.AttachedToVisualTree += OnHostAttachedToVisualTree;
            host.DetachedFromVisualTree += OnHostDetachedFromVisualTree;
            host.PropertyChanged += OnHostPropertyChanged;
            if (def.HoldClosing && host is ContextMenu menu)
                menu.Closing += OnMenuClosing;

            ResolveParts(); // the template may already be applied
            WireAmbient();
        }

        public static PopupRuntime? Create(TemplatedControl host, PopupDef def, Func<MotionContext> createContext)
        {
            var openProperty = def.OpenProperty!(host);
            if (openProperty is null || openProperty.PropertyType != typeof(bool))
            {
                Debug.WriteLine($"Popup motion: '{host.GetType().Name}' needs a bool open property (OpenWhen) — ignored.");
                return null;
            }
            return new PopupRuntime(host, def, createContext, openProperty);
        }

        public static PopupRuntime? For(Visual host, PopupDef def)
        {
            if (ByHost.TryGetValue(host, out var list))
                foreach (var runtime in list)
                    if (runtime._def == def)
                        return runtime;
            return null;
        }

        public Control? RootControl => _root;

        public Visual[] CollectItems()
        {
            if (_itemsPresenter is not { } presenter || presenter.Panel is not Panel panel)
                return Array.Empty<Visual>();
            return panel.Children.ToArray<Visual>();
        }

        private bool HostIsOpen() => _host.GetValue(_openProperty) is true;

        private void CloseHost()
        {
            if (_def.CloseHost is { } close)
                close(_host);
            else
                _host.SetValue(_openProperty, false);
        }

        // ---- playing --------------------------------------------------------------------

        private void Play(ReactionDef reaction)
        {
            // The template walk at TemplateApplied time can still see nothing (the template
            // root is not in the visual tree at that instant): re-resolve here.
            if (_popup is null || _root is null)
                ResolveParts();
            if (_root is null)
            {
                // The content does not resolve yet (a code-built popup only gets its
                // template once hosted): park the reaction until it does.
                StopCurrent();
                _pending = reaction;
                _pendingHostOpen = HostIsOpen();
                return;
            }
            _pending = null; // superseded: a parked reaction must never replay later
            Start(reaction);
        }

        private void Start(ReactionDef reaction)
        {
            // Not stopped first: the new motion takes the properties over from their live
            // pose and velocity (a reopen mid-collapse resumes where the popup is).
            var previous = _current;
            bool opening = reaction == _def.Open;
            _current = Run.Play(reaction, _createContext(), TickOwner,
                beforeStart: opening ? Show : null,
                onCompleted: opening ? null : Hide);
            if (previous is { IsAlive: true } && previous != _current)
                previous.Interrupt();
        }

        private void Close()
        {
            // Already hidden (deactivated window, abnormal close): nothing to animate.
            if (_popup is { IsOpen: true })
                Play(_def.Close);
        }

        private void StopCurrent()
        {
            _current?.Interrupt();
            _current = null;
        }

        private void Show()
        {
            if (_popup is { } popup)
                popup.IsOpen = true;
        }

        /// <summary>The end of the closing motion — the only legitimate close.</summary>
        private void Hide()
        {
            if (_root is { } root)
                SukiMotion.BlurProperty.Write(root, 0.0); // no shader left behind
            if (_popup is { IsOpen: true } popup)
            {
                _selfClosing = true;
                try { popup.IsOpen = false; }
                finally { _selfClosing = false; }
            }
        }

        private void InstantClose() => StopAndRest(() =>
        {
            if (_popup is { IsOpen: true } popup)
                popup.IsOpen = false;
        });

        /// <summary>Abandons the popup outright: the next opening poses its start values again.</summary>
        private void StopAndRest(Action? after = null)
        {
            StopCurrent();
            after?.Invoke();
        }

        // The popup content lives under its own top level once open: drive the frames from
        // the root when it is attached, from the host otherwise.
        private Visual TickOwner => _root is { } root && TopLevel.GetTopLevel(root) is not null ? root : _host;

        // ---- parts & lifecycle ------------------------------------------------------------

        private void ResolveParts()
        {
            UnwirePopup();
            _root = null;
            _itemsPresenter = null;
            _popup = _def.ResolvePopup!(_host);
            if (_popup is not { } popup)
                return;
            popup.PropertyChanged += OnPopupPropertyChanged;
            popup.Opened += OnPopupOpened;
            // On the Popup itself: events from its content bubble through it to the host.
            popup.AddHandler(InputElement.PointerPressedEvent, OnPopupPointerPressed);
            popup.AddHandler(InputElement.PointerReleasedEvent, OnPopupPointerReleased, handledEventsToo: true);
            if (FindRoot(popup) is { } root)
            {
                _root = root;
                _itemsPresenter = _def.ItemsPresenterName is { } name
                    ? root.GetLogicalDescendants().OfType<ItemsPresenter>().FirstOrDefault(i => i.Name == name)
                    : null;
            }
        }

        private Control? FindRoot(Popup popup)
        {
            if (popup.Child is not { } child)
                return null;
            if (child.Name == _def.RootName)
                return child;
            return MotionTarget.FindTemplatePart(child, _def.RootName!);
        }

        private void UnwirePopup()
        {
            if (_popup is { } popup)
            {
                popup.PropertyChanged -= OnPopupPropertyChanged;
                popup.Opened -= OnPopupOpened;
                popup.RemoveHandler(InputElement.PointerPressedEvent, OnPopupPointerPressed);
                popup.RemoveHandler(InputElement.PointerReleasedEvent, OnPopupPointerReleased);
            }
            _popup = null;
        }

        private void StartPending()
        {
            if (_root is null || _pending is not { } reaction)
                return;
            _pending = null;
            // The host changed its mind while the content was missing: the parked motion is obsolete.
            if (HostIsOpen() != _pendingHostOpen)
                return;
            StopCurrent();
            Start(reaction);
        }

        private void OnHostPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property != _openProperty)
                return;
            if (e.NewValue is true)
                Play(_def.Open);
            else if (e.NewValue is false)
                Close();
        }

        private void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e) =>
            // A re-applied template aborts any motion over now-dead parts; a parked reaction
            // starts on the fresh content.
            StopAndRest(() =>
            {
                ResolveParts();
                StartPending();
            });

        private void OnHostAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
        {
            // A code-built popup (ContextMenu) parents its host only at the first open.
            ResolveParts();
            StartPending();
        }

        private void OnHostAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) => WireAmbient();

        private void OnHostDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            StopAndRest(() =>
            {
                if (_popup is { IsOpen: true } popup)
                    popup.IsOpen = false;
            });
            UnwireAmbient(); // the top level is going away with the host
        }

        private void OnPopupPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property != Popup.IsOpenProperty || e.NewValue is not false || _selfClosing)
                return;
            // The section is the only legitimate closer: reaching here with the host still
            // open means the popup closed behind its back (window teardown…). Sync at once.
            if (HostIsOpen())
                StopAndRest(CloseHost);
        }

        private void OnPopupOpened(object? sender, EventArgs e)
        {
            // Safety net: something opened the popup without going through the host property.
            // Never while a motion is current: Opened also fires inside our own reveal.
            if (HostIsOpen() && _current is null)
                Play(_def.Open);
        }

        private void OnPopupPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_def.KeepPressesInside?.Invoke(_host) == true)
                e.Handled = true;
        }

        private void OnPopupPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            // The item decides the selection before the release reaches the popup, and marks
            // the event handled when it selected: a handled release over a selected container
            // of the host was a selecting click.
            if (!e.Handled || _def.CloseWhenItemClicked?.Invoke(_host) != true || !HostIsOpen())
                return;
            if (_host is not ItemsControl items || e.Source is not Visual source)
                return;
            var container = source.GetSelfAndVisualAncestors().OfType<Control>()
                .FirstOrDefault(c => items.IndexFromContainer(c) >= 0);
            if (container is ISelectable { IsSelected: true })
                CloseHost(); // the animated close follows through the property change
        }

        private void OnMenuClosing(object? sender, CancelEventArgs e)
        {
            // Hold the popup open through the collapse — unless the closing motion itself is
            // closing it, or the window was deactivated (close at once, never animate there).
            e.Cancel = !_selfClosing && HostWindowActive() && _popup is { IsOpen: true };
            if (e.Cancel)
                Close();
        }

        private bool HostWindowActive() =>
            _host.GetLogicalParent() is not Popup { PlacementTarget: { } target }
            || TopLevel.GetTopLevel(target) is not Window window
            || window.IsActive;

        // ---- the host's window --------------------------------------------------------------

        private void WireAmbient()
        {
            if (_ambient is not null || TopLevel.GetTopLevel(_host) is not { } topLevel)
                return;
            _ambient = topLevel;
            if (_def.CloseOnOutsidePress is not null)
                topLevel.AddHandler(InputElement.PointerPressedEvent, OnWindowPointerPressed,
                    RoutingStrategies.Direct | RoutingStrategies.Bubble, handledEventsToo: true);
            if (_def.CloseOnDeactivated && topLevel is Window window)
            {
                _window = window;
                window.Deactivated += OnWindowDeactivated;
            }
        }

        private void UnwireAmbient()
        {
            _ambient?.RemoveHandler(InputElement.PointerPressedEvent, OnWindowPointerPressed);
            _ambient = null;
            if (_window is { } window)
                window.Deactivated -= OnWindowDeactivated;
            _window = null;
        }

        private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Presses inside the popup bubble up to the window too (through the popup's
            // logical parent): only a press outside it is an outside press.
            if (e.Source is Visual source && _popup?.IsInsidePopup(source) == true)
                return;
            if (_def.CloseOnOutsidePress?.Invoke(_host) == true && HostIsOpen())
                CloseHost(); // the animated close follows through the property change
        }

        private void OnWindowDeactivated(object? sender, EventArgs e)
        {
            InstantClose();
            if (HostIsOpen())
                CloseHost();
        }

        /// <summary>Disable: unwire everything and leave the popup working without animation.</summary>
        public void Dispose() => StopAndRest(() =>
        {
            _host.TemplateApplied -= OnTemplateApplied;
            _host.AttachedToLogicalTree -= OnHostAttachedToLogicalTree;
            _host.AttachedToVisualTree -= OnHostAttachedToVisualTree;
            _host.DetachedFromVisualTree -= OnHostDetachedFromVisualTree;
            _host.PropertyChanged -= OnHostPropertyChanged;
            if (_host is ContextMenu menu)
                menu.Closing -= OnMenuClosing;
            UnwireAmbient();
            _pending = null;
            var popup = _popup;
            UnwirePopup();
            if (popup is not null)
                popup.IsOpen = HostIsOpen();
            if (ByHost.TryGetValue(_host, out var list))
                list.Remove(this);
        });
    }
}
