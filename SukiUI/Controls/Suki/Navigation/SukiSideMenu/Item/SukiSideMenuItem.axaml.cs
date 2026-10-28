using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace SukiUI.Controls;

public class SukiSideMenuItem : TreeViewItem
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, object?>(nameof(Icon));

    private Border? _border;
    private INameScope? _parts;

    private static readonly Point s_invalidPoint = new Point(double.NaN, double.NaN);
    private Point _pointerDownPoint = s_invalidPoint;
    
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<object> PageContentProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, object>(nameof(PageContent));

    public object PageContent
    {
        get => GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate?> SideDisplayTemplateProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, IDataTemplate?>(nameof(SideDisplayTemplate));

    public IDataTemplate? SideDisplayTemplate
    {
        get => GetValue(SideDisplayTemplateProperty);
        set => SetValue(SideDisplayTemplateProperty, value);
    }

    public void Show() => IsFiltered = false;

    public void Hide() => IsFiltered = true;

    // ---- state the motion reads (SukiSideMenuItemMotion) -----------------------------------

    /// <summary>Whether the selection plate is filled. Follows <c>IsSelected</c>, one frame late and
    /// in step with its neighbours: the menu hands the plate over (see <see cref="SukiSideMenu"/>).</summary>
    internal static readonly StyledProperty<bool> IsPlateActiveProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, bool>(nameof(IsPlateActive));

    /// <summary>The direction the selection travelled when this row was chosen or left: 1 downwards,
    /// -1 upwards, 0 unknown (first selection, or a hand-off between rows that cannot be placed).</summary>
    internal static readonly StyledProperty<int> SelectionFlowProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, int>(nameof(SelectionFlow));

    /// <summary>The pointer is over the row itself — not over its sub-items.</summary>
    internal static readonly StyledProperty<bool> IsHeaderPointerOverProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, bool>(nameof(IsHeaderPointerOver));

    internal static readonly StyledProperty<bool> IsHeaderPressedProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, bool>(nameof(IsHeaderPressed));

    /// <summary>Folded away by the menu's search.</summary>
    internal static readonly StyledProperty<bool> IsFilteredProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, bool>(nameof(IsFiltered));

    internal bool IsPlateActive
    {
        get => GetValue(IsPlateActiveProperty);
        set => SetValue(IsPlateActiveProperty, value);
    }

    internal int SelectionFlow
    {
        get => GetValue(SelectionFlowProperty);
        set => SetValue(SelectionFlowProperty, value);
    }

    internal bool IsHeaderPointerOver
    {
        get => GetValue(IsHeaderPointerOverProperty);
        private set => SetValue(IsHeaderPointerOverProperty, value);
    }

    internal bool IsHeaderPressed
    {
        get => GetValue(IsHeaderPressedProperty);
        private set => SetValue(IsHeaderPressedProperty, value);
    }

    internal bool IsFiltered
    {
        get => GetValue(IsFilteredProperty);
        private set => SetValue(IsFilteredProperty, value);
    }

    /// <summary>The row (icon, title, plates).</summary>
    internal Border? Row => _border;

    /// <summary>A named part of the applied template (the motion animates them).</summary>
    internal Control? Part(string name) => _parts?.Find<Control>(name);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_border is not null)
        {
            _border.PointerEntered -= BorderOnPointerEntered;
            _border.PointerExited -= BorderOnPointerExited;
            _border.PointerPressed -= BorderOnPointerPressed;
            _border.PointerReleased -= BorderOnPointerReleased;
            _border.PointerCaptureLost -= BorderOnPointerCaptureLost;
        }

        _border = e.NameScope.Get<Border>("PART_Border");
        _parts = e.NameScope;

        // The row has its own pointer state: the item as a whole also covers its sub-items, which
        // must not light the row of their parent.
        _border.PointerEntered += BorderOnPointerEntered;
        _border.PointerExited += BorderOnPointerExited;
        _border.AddHandler(PointerPressedEvent, BorderOnPointerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
        _border.AddHandler(PointerReleasedEvent, BorderOnPointerReleased, RoutingStrategies.Bubble, handledEventsToo: true);
        _border.PointerCaptureLost += BorderOnPointerCaptureLost;
    }

    private void BorderOnPointerEntered(object? sender, PointerEventArgs e) => IsHeaderPointerOver = true;

    private void BorderOnPointerExited(object? sender, PointerEventArgs e)
    {
        IsHeaderPointerOver = false;
        IsHeaderPressed = false;
    }

    private void BorderOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(_border).Properties.IsLeftButtonPressed)
            IsHeaderPressed = true;
    }

    private void BorderOnPointerReleased(object? sender, PointerReleasedEventArgs e) => IsHeaderPressed = false;

    private void BorderOnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e) => IsHeaderPressed = false;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsSelectedProperty)
        {
            // The plate follows the selection through the menu, which hands it from the row that
            // is left to the row that is chosen. Outside a menu, it simply follows.
            if (this.FindAncestorOfType<SukiSideMenu>() is { } menu)
                menu.QueuePlateSync(this);
            else
                IsPlateActive = IsSelected;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        _pointerDownPoint = s_invalidPoint;

        if (e.Handled)
            return;

        if (!e.Handled && ItemsControl.ItemsControlFromItemContainer(this) is SukiSideMenu owner)
        {
            var p = e.GetCurrentPoint(this);

            if (p.Properties.PointerUpdateKind is PointerUpdateKind.LeftButtonPressed or
                PointerUpdateKind.RightButtonPressed)
            {
                if (p.Pointer.Type == PointerType.Mouse)
                {
                    // If the pressed point comes from a mouse, perform the selection immediately.
                    
                    
                    // /!\ WARNING /!\ - Line commented to make it work with subitems. Doesn't seem to break anything on my
                        // e.Handled = owner.UpdateSelectionFromPointerEvent(this);
                }
                else
                {
                    // Otherwise perform the selection when the pointer is released as to not
                    // interfere with gestures.
                    _pointerDownPoint = p.Position;

                    // Ideally we'd set handled here, but that would prevent the scroll gesture
                    // recognizer from working.
                    ////e.Handled = true;
                }
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!e.Handled && 
            !double.IsNaN(_pointerDownPoint.X) &&
            e.InitialPressMouseButton is MouseButton.Left or MouseButton.Right)
        {
            var point = e.GetCurrentPoint(this);
            var settings = (e.Source as Visual)?.GetPlatformSettings();
            var tapSize = settings?.GetTapSize(point.Pointer.Type) ?? new Size(4, 4);
            var tapRect = new Rect(_pointerDownPoint, new Size())
                .Inflate(new Thickness(tapSize.Width, tapSize.Height));

            if (new Rect(Bounds.Size).ContainsExclusive(point.Position) &&
                tapRect.ContainsExclusive(point.Position) &&
                ItemsControl.ItemsControlFromItemContainer(this) is SukiSideMenu owner)
            {
                if (owner.UpdateSelectionFromPointerEvent(this))
                {
                    // As we only update selection from touch/pen on pointer release, we need to raise
                    // the pointer event on the owner to trigger a commit.
                    if (e.Pointer.Type != PointerType.Mouse)
                    {
                        var sourceBackup = e.Source;
                        owner.RaiseEvent(e);
                        e.Source = sourceBackup;
                    }

                    e.Handled = true;
                }
            }
        }

        _pointerDownPoint = s_invalidPoint;
    }
    
    public static readonly StyledProperty<bool> IsContentMovableProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, bool>(nameof(IsContentMovable), defaultValue: true);

    public bool IsContentMovable
    {
        get => GetValue(IsContentMovableProperty);
        set => SetValue(IsContentMovableProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsTopMenuExpandedProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, bool>(nameof(IsTopMenuExpanded), defaultValue: true);

    public bool IsTopMenuExpanded
    {
        get => GetValue(IsTopMenuExpandedProperty);
        set => SetValue(IsTopMenuExpandedProperty, value);
    }

}
