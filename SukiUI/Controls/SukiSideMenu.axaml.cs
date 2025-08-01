using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System.Globalization;
using Avalonia.Interactivity;
using Avalonia.Layout;
using SukiUI.Enums;
using Avalonia.Data.Converters;

namespace SukiUI.Controls;

public class SukiSideMenu : TreeView
{
    public static readonly StyledProperty<string?> SearchTextProperty =
        AvaloniaProperty.Register<SukiSideMenu, string?>(nameof(SearchText));

    public static readonly StyledProperty<bool> IsSearchEnabledProperty =
        AvaloniaProperty.Register<SukiSideMenu, bool>(nameof(IsSearchEnabled), defaultValue: false);
    
    public static readonly StyledProperty<bool> IsContentVisibleProperty =
        AvaloniaProperty.Register<SukiSideMenu, bool>(nameof(IsContentVisible), defaultValue: true);

    public static readonly StyledProperty<bool> SidebarToggleEnabledProperty = AvaloniaProperty.Register<SukiSideMenu, bool>(nameof(SidebarToggleEnabled), defaultValue: true);
    
    public bool IsContentVisible
    {
        get => GetValue(IsContentVisibleProperty);
        set => SetValue(IsContentVisibleProperty, value);
    }

    public bool SidebarToggleEnabled
    {
        get => GetValue(SidebarToggleEnabledProperty);
        set => SetValue(SidebarToggleEnabledProperty, value);
    }

    public string? SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public bool IsSearchEnabled
    {
        get => GetValue(IsSearchEnabledProperty);
        set => SetValue(IsSearchEnabledProperty, value);
    }

    public static readonly StyledProperty<bool> IsToggleButtonVisibleProperty =
        AvaloniaProperty.Register<SukiSideMenu, bool>(nameof(IsToggleButtonVisible), defaultValue: true);

    public bool IsToggleButtonVisible
    {
        get => GetValue(IsToggleButtonVisibleProperty);
        set => SetValue(IsToggleButtonVisibleProperty, value);
    }

    public static readonly StyledProperty<bool> IsMenuExpandedProperty =
        AvaloniaProperty.Register<SukiSideMenu, bool>(nameof(IsMenuExpanded), defaultValue: true);

    public bool IsMenuExpanded
    {
        get => GetValue(IsMenuExpandedProperty);
        set => SetValue(IsMenuExpandedProperty, value);
    }

    public static readonly StyledProperty<int> OpenPaneLengthProperty =
        AvaloniaProperty.Register<SukiSideMenu, int>(nameof(OpenPaneLength), defaultValue: 220);

    public int OpenPaneLength
    {
        get => GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value switch
        {
            >= 200 => value,
            _ => throw new ArgumentOutOfRangeException($"OpenPaneLength must be greater than or equal to 200, but was {value}")
        });
    }

    public static readonly StyledProperty<HorizontalAlignment> TogglePaneButtonPositionProperty =
        AvaloniaProperty.Register<SukiSideMenu, HorizontalAlignment>(nameof(TogglePaneButtonPosition), defaultValue: HorizontalAlignment.Right);

    public SideMenuTogglePaneButtonPositionOptions TogglePaneButtonPosition
    {
        get => GetValue(TogglePaneButtonPositionProperty) switch
        {
            HorizontalAlignment.Right => SideMenuTogglePaneButtonPositionOptions.Right,
            HorizontalAlignment.Left => SideMenuTogglePaneButtonPositionOptions.Left,
            _ => SideMenuTogglePaneButtonPositionOptions.Right
        };
        set => SetValue(TogglePaneButtonPositionProperty, value switch
        {
            SideMenuTogglePaneButtonPositionOptions.Right => HorizontalAlignment.Right,
            SideMenuTogglePaneButtonPositionOptions.Left => HorizontalAlignment.Left,
            _ => HorizontalAlignment.Right
        });
    }

    public static readonly StyledProperty<bool> IsSelectedItemContentMovableProperty =
        AvaloniaProperty.Register<SukiSideMenu, bool>(nameof(IsSelectedItemContentMovable), defaultValue: true);

    public bool IsSelectedItemContentMovable
    {
        get => GetValue(IsSelectedItemContentMovableProperty);
        set => SetValue(IsSelectedItemContentMovableProperty, value);
    }

    public static readonly StyledProperty<double> HeaderMinHeightProperty =
        AvaloniaProperty.Register<SukiSideMenu, double>(nameof(HeaderMinHeight));

    public double HeaderMinHeight
    {
        get => GetValue(HeaderMinHeightProperty);
        set => SetValue(HeaderMinHeightProperty, value);
    }

    public static readonly StyledProperty<object?> HeaderContentProperty =
        AvaloniaProperty.Register<SukiSideMenu, object?>(nameof(HeaderContent));

    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    public static readonly StyledProperty<object?> FooterContentProperty =
        AvaloniaProperty.Register<SukiSideMenu, object?>(nameof(FooterContent));

    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<SukiSideMenu, object?>(nameof(Content));

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly StyledProperty<bool> UseCustomContentProperty =
        AvaloniaProperty.Register<SukiSideMenu, bool>(nameof(UseCustomContent), defaultValue: false);

    public bool UseCustomContent
    {
        get => GetValue(UseCustomContentProperty);
        set => SetValue(UseCustomContentProperty, value);
    }

    private bool IsSpacerVisible => !IsMenuExpanded;


    private SukiTransitioningContentControl? _contentControl;
    private Grid? _spacer;

    public SukiSideMenu()
    {
        SelectionMode = SelectionMode.Single | SelectionMode.AlwaysSelected;
    }

    private void MenuExpandedClicked()
    {
        IsMenuExpanded = !IsMenuExpanded;

        UpdateMenuItemsExpansion();
    }

    private void UpdateMenuItemsExpansion()
    {
        if(_sideMenuItems.Any())
            foreach (var item in _sideMenuItems)
                item.IsTopMenuExpanded = IsMenuExpanded;

        else if(Items.FirstOrDefault() is SukiSideMenuItem)
            foreach (SukiSideMenuItem? item in Items)
                item!.IsTopMenuExpanded = IsMenuExpanded;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (Items.Any())
        {
            SelectedItem = Items.First();
        }

        e.NameScope.Get<Button>("PART_SidebarToggleButton").Click += (_, _) => MenuExpandedClicked();
        _contentControl = e.NameScope.Get<SukiTransitioningContentControl>("PART_TransitioningContentControl");
        SetContentControlContent();
        _spacer = e.NameScope.Get<Grid>("PART_Spacer");
        if(_spacer != null) _spacer.IsVisible = IsSpacerVisible;


    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        SetContentControlContent();
        UpdateMenuItemsExpansion();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SearchTextProperty)
        {
            FilterItems(change.GetNewValue<string>());
        }

        if (change.Property.Name == nameof(SelectedItem) && !UseCustomContent)
            SetContentControlContent(change.NewValue);
        else if (change.Property.Name == nameof(Content) && UseCustomContent)
            SetContentControlContent(change.NewValue);
        else if (change.Property.Name == nameof(UseCustomContent))
            SetContentControlContent();
        else if (change.Property == IsMenuExpandedProperty && _spacer != null)
            _spacer.IsVisible = IsSpacerVisible;
    }

    protected virtual void FilterItems(string search)
    {
        search = search.ToLower();

        foreach (var item in _sideMenuItems)
        {
            var header = item.Header?.ToLower() ?? "";

            if (header.Contains(search))
            {
                item.Show();
            }
            else
            {
                item.Hide();
            }
        }
    }

    protected virtual void SetContentControlContent(object? newContent)
    {
        if (_contentControl == null) return;
        _contentControl.Content = newContent switch
        {
            SukiSideMenuItem { PageContent: { } sukiMenuPageContent } => sukiMenuPageContent,
            _ => newContent
        };
    }

    protected virtual void SetContentControlContent()
    {
        if (UseCustomContent)
            SetContentControlContent(Content);
        else
            SetContentControlContent(SelectedItem);
    }

    public bool UpdateSelectionFromPointerEvent(Control source) => UpdateSelectionFromEventSource(source);

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var menuItem = (ItemTemplate != null && ItemTemplate.Match(item) &&
                        ItemTemplate.Build(item) is SukiSideMenuItem sukiMenuItem)
                ? sukiMenuItem
                : new SukiSideMenuItem();
        menuItem.IsContentMovable = IsSelectedItemContentMovable;
        _sideMenuItems.Add(menuItem);
        return menuItem;
    }

    private readonly List<SukiSideMenuItem> _sideMenuItems = new();

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<SukiSideMenuItem>(item, out recycleKey);
    }
}



public class WindowBackgroundToCornerRadiusConverter : IValueConverter
{
    public static readonly WindowBackgroundToCornerRadiusConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return new CornerRadius(0);

        if((bool)value == false)
            return new CornerRadius(17);

        return new CornerRadius(0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}


public class WindowBackgroundToMarginConverter : IValueConverter
{
    public static readonly WindowBackgroundToMarginConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return new Thickness(0);

        if((bool)value == false)
            return new Thickness(10,5,0,10);

        return new Thickness(0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}


