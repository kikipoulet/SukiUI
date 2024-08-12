using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using SukiUI.Models;
using System;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Media;
using SukiUI.ColorTheme;
using SukiUI.Content;
using SukiUI.Enums;
using SukiUI.Toasts;

namespace SukiUI.Controls;

public class SukiToast : ContentControl, ISukiToast
{
    protected override Type StyleKeyOverride => typeof(SukiToast);
    
    public ISukiToastManager Manager { get; set; }

    private Action? _onClickedCallback;
    private Action? _onActionCallback;

    public SukiToast()
    {
    }
    
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<SukiToast, object?>(nameof(Icon));

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SukiToast, string>(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public static readonly StyledProperty<bool> ShowActionButtonProperty =
        AvaloniaProperty.Register<SukiToast, bool>(nameof(ShowActionButton));

    public bool ShowActionButton
    {
        get => GetValue(ShowActionButtonProperty);
        set => SetValue(ShowActionButtonProperty, value);
    }
    
    public static readonly StyledProperty<string> ActionButtonContentProperty =
        AvaloniaProperty.Register<SukiToast, string>(nameof(ActionButtonContent));

    public string ActionButtonContent
    {
        get => GetValue(ActionButtonContentProperty);
        set => SetValue(ActionButtonContentProperty, value);
    }

    public static readonly StyledProperty<bool> CanDismissByClickingProperty = AvaloniaProperty.Register<SukiToast, bool>(nameof(CanDismissByClicking));

    public bool CanDismissByClicking
    {
        get => GetValue(CanDismissByClickingProperty);
        set => SetValue(CanDismissByClickingProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        e.NameScope.Get<Border>("PART_ToastCard").PointerPressed += ToastCardClickedHandler;
        e.NameScope.Get<Button>("ButtonAction").Click += ButtonActionClicked;
    }

    private void ButtonActionClicked(object sender, RoutedEventArgs e)
    {
        _onActionCallback?.Invoke();
    }

    private void ToastCardClickedHandler(object o, PointerPressedEventArgs pointerPressedEventArgs)
    {
        if (!CanDismissByClicking) return;
        Manager.Dismiss(this);
        _onClickedCallback?.Invoke();
        _onClickedCallback = null;
    }

    public void AnimateShow()
    {
        this.Animate(OpacityProperty, 0d, 1d, TimeSpan.FromMilliseconds(500));
        this.Animate(MarginProperty, new Thickness(0, 10, 0, -10), new Thickness(), TimeSpan.FromMilliseconds(500));
    }

    public void AnimateDismiss()
    {
        this.Animate(OpacityProperty, 1d, 0d, TimeSpan.FromMilliseconds(300));
        this.Animate(MarginProperty, new Thickness(), new Thickness(0, 50, 0, -50), TimeSpan.FromMilliseconds(300));
    }

    public SukiToast ResetToDefault()
    {
        Title = "Information";
        Content = "Toast Information";
        ShowActionButton = false;
        Icon = Icons.InformationOutline;
        Foreground = NotificationColor.InfoIconForeground;
        CanDismissByClicking = false;
        DockPanel.SetDock(this, Dock.Bottom);
        return this;
    }
}