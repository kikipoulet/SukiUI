using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;

namespace SukiUI.Controls;

public class SukiSideMenu : ContentControl
{
    public static readonly StyledProperty<bool> IsMenuExpandedProperty = 
        AvaloniaProperty.Register<SukiSideMenu, bool>(nameof(IsMenuExpanded), defaultValue: true);

    public bool IsMenuExpanded
    {
        get => GetValue(IsMenuExpandedProperty);
        set => SetValue(IsMenuExpandedProperty, value);
    }

    public static readonly StyledProperty<bool> HeaderContentOverlapsToggleButtonProperty = 
        AvaloniaProperty.Register<SukiSideMenu, bool>(nameof(HeaderContentOverlapsToggleButton), defaultValue: false);

    public bool HeaderContentOverlapsToggleButton
    {
        get => GetValue(HeaderContentOverlapsToggleButtonProperty);
        set => SetValue(HeaderContentOverlapsToggleButtonProperty, value);
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

    public static readonly StyledProperty<object?> FooterContentProperty = AvaloniaProperty.Register<SukiSideMenu, object?>(nameof(FooterContent));

    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    public static readonly StyledProperty<AvaloniaList<object>> MenuItemsProperty = 
        AvaloniaProperty.Register<SukiSideMenu, AvaloniaList<object>>(nameof(MenuItems), defaultValue: new AvaloniaList<object>());

    public AvaloniaList<object> MenuItems
    {
        get => GetValue(MenuItemsProperty);
        set => SetValue(MenuItemsProperty, value);
    }

    public static readonly StyledProperty<object> SelectedPageProperty = 
        AvaloniaProperty.Register<SukiSideMenu, object>(nameof(SelectedPage));

    public object SelectedPage
    {
        get => GetValue(SelectedPageProperty);
        set => SetValue(SelectedPageProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        if (MenuItems.Any())
            SelectedPage = MenuItems.First();
        
        e.NameScope.Get<Button>("PART_SidebarToggleButton").Click += (_, _) => 
            IsMenuExpanded = !IsMenuExpanded;
        e.NameScope.Get<Button>("PART_SidebarToggleButtonOverlay").Click += (_, _) => 
            IsMenuExpanded = !IsMenuExpanded;
        
        if (e.NameScope.Find<Grid>("PART_Spacer") is { } spacer)
        {
            spacer.IsVisible = IsSpacerVisible;
            var menuObservable = this.GetObservable(IsMenuExpandedProperty)
                .Select(_ => Unit.Default);
            var headerContentObservable = this.GetObservable(HeaderContentOverlapsToggleButtonProperty)
                .Select(_ => Unit.Default);
            menuObservable
                .Merge(headerContentObservable)
                .ObserveOn(new AvaloniaSynchronizationContext())
                .Subscribe(_ => spacer.IsVisible = IsSpacerVisible);
        }
    }
    
    private bool IsSpacerVisible => HeaderContentOverlapsToggleButton && !IsMenuExpanded;
}