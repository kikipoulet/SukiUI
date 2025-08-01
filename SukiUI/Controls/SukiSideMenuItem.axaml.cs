using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace SukiUI.Controls;

public class SukiSideMenuItem : TreeViewItem
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, object?>(nameof(Icon));

    private Border? _border;

    private static readonly Point s_invalidPoint = new Point(double.NaN, double.NaN);
    private Point _pointerDownPoint = s_invalidPoint;
    
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, string?>(nameof(Header));

    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
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

    public void Show()
    {
        if (_border == null)
        {
            return;
        }

        _border.MaxHeight = 200.0;
    }

    public void Hide()
    {
        if (_border == null)
        {
            return;
        }

        _border.MaxHeight = 0.0;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _border = e.NameScope.Get<Border>("PART_Border");

        
        // /!\ WARNING /!\ - Commented to make SubMenus work.
        
    /*    if (e.NameScope.Get<ContentPresenter>("PART_AltDisplay") is { } contentControl) 
        {
            if (Header is not null || Icon is not null)
            {
                contentControl.IsVisible = false;
            }
        } */
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
            var settings = TopLevel.GetTopLevel(e.Source as Visual)?.PlatformSettings;
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
