using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using SukiUI.Models;
using System;
using System.Collections.ObjectModel;
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
    
    public ISukiToastManager? Manager { get; set; }
    public Action<ISukiToast>? OnDismissed { get; set; }
    public Action<ISukiToast>? OnClicked { get; set; }
    
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
    
    public static readonly StyledProperty<bool> LoadingStateProperty = AvaloniaProperty.Register<SukiToast, bool>(nameof(LoadingState));

    public bool LoadingState
    {
        get => GetValue(LoadingStateProperty);
        set => SetValue(LoadingStateProperty, value);
    }

    public static readonly StyledProperty<bool> CanDismissByClickingProperty = AvaloniaProperty.Register<SukiToast, bool>(nameof(CanDismissByClicking));

    public bool CanDismissByClicking
    {
        get => GetValue(CanDismissByClickingProperty);
        set => SetValue(CanDismissByClickingProperty, value);
    }
    
    public static readonly StyledProperty<ObservableCollection<object>> ActionButtonsProperty = AvaloniaProperty.Register<SukiToast, 
        ObservableCollection<object>>(nameof(ActionButtons));

    public ObservableCollection<object> ActionButtons
    {
        get => GetValue(ActionButtonsProperty);
        set => SetValue(ActionButtonsProperty, value);
    }
    
    public Action<ISukiToast>? DelayDismissAction { get; set; }

    public SukiToast()
    {
        ActionButtons = new ObservableCollection<object>();
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        e.NameScope.Get<Border>("PART_ToastCard").PointerPressed += ToastCardClickedHandler;
    }

    private void ToastCardClickedHandler(object o, PointerPressedEventArgs pointerPressedEventArgs)
    {
        OnClicked?.Invoke(this);
        if (!CanDismissByClicking) return;
        Manager.Dismiss(this);
        OnDismissed?.Invoke(this);
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

    public ISukiToast ResetToDefault()
    {
        Title = string.Empty;
        Content = string.Empty;
        Icon = Icons.InformationOutline;
        Foreground = NotificationColor.InfoIconForeground;
        CanDismissByClicking = false;
        ActionButtons.Clear();
        OnDismissed = null;
        OnClicked = null;
        LoadingState = false;
        DelayDismissAction = null;
        DockPanel.SetDock(this, Dock.Bottom);
        return this;
    }
}