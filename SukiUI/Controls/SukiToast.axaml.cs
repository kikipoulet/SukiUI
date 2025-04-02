using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SukiUI.ColorTheme;
using SukiUI.Content;
using SukiUI.Toasts;

namespace SukiUI.Controls;

[TemplatePart("PART_ToastCard", typeof(Border))]
[TemplatePart("PART_DismissProgressBar", typeof(ProgressBar))]
public class SukiToast : ContentControl, ISukiToast
{
    private bool _wasDismissTimerInterrupted;
    private readonly Stopwatch _dismissStopwatch = new();
    private Border? _toastCard;

    public ISukiToastManager? Manager { get; set; }
    public Action<ISukiToast, SukiToastDismissSource>? OnDismissed { get; set; }
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

    public static readonly StyledProperty<bool> CanDismissByTimeProperty = AvaloniaProperty.Register<SukiToast, bool>(nameof(CanDismissByTime));

    public bool CanDismissByTime
    {
        get => GetValue(CanDismissByTimeProperty);
        set => SetValue(CanDismissByTimeProperty, value);
    }

    public static readonly DirectProperty<SukiToast, TimeSpan> DismissTimeoutProperty =
        AvaloniaProperty.RegisterDirect<SukiToast, TimeSpan>(nameof(DismissTimeout), o => o.DismissTimeout, (o, value) => o.DismissTimeout = value);

    private TimeSpan _dismissTimeout = TimeSpan.FromSeconds(5);
    public TimeSpan DismissTimeout
    {
        get => _dismissTimeout;
        set => SetAndRaise(DismissTimeoutProperty, ref _dismissTimeout, value);
    }

    public static readonly StyledProperty<bool> InterruptDismissTimerWhileHoverProperty =
        AvaloniaProperty.Register<SukiToast, bool>(nameof(InterruptDismissTimerWhileHover), true);

    public bool InterruptDismissTimerWhileHover
    {
        get => GetValue(InterruptDismissTimerWhileHoverProperty);
        set => SetValue(InterruptDismissTimerWhileHoverProperty, value);
    }

    public static readonly DirectProperty<SukiToast, double> DismissProgressValueProperty =
        AvaloniaProperty.RegisterDirect<SukiToast, double>(nameof(DismissProgressValue), o => o.DismissProgressValue);

    private double _dismissProgressValue = 100;
    public double DismissProgressValue
    {
        get => _dismissProgressValue;
        private set => SetAndRaise(DismissProgressValueProperty, ref _dismissProgressValue, value);
    }

    public static readonly StyledProperty<ObservableCollection<object>> ActionButtonsProperty = AvaloniaProperty.Register<SukiToast,
        ObservableCollection<object>>(nameof(ActionButtons), new ObservableCollection<object>());

    public ObservableCollection<object> ActionButtons
    {
        get => GetValue(ActionButtonsProperty);
        set => SetValue(ActionButtonsProperty, value);
    }

    private readonly DispatcherTimer _dismissTimer = new DispatcherTimer()
    {
        Interval = TimeSpan.FromSeconds(5)
    };
    private readonly DispatcherTimer _dismissProgressValueTimer = new DispatcherTimer()
    {
        Interval = TimeSpan.FromMilliseconds(50)
    };

    public SukiToast()
    {
        _dismissTimer.Tick += DismissTimerOnTick;
        _dismissProgressValueTimer.Tick += DismissProgressValueTimerOnTick;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_toastCard is not null)
        {
            _toastCard.PointerPressed -= ToastCardClickedHandler;
        }

        _toastCard = e.NameScope.Get<Border>("PART_ToastCard");
        _toastCard.PointerPressed += ToastCardClickedHandler;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (CanDismissByTime && _dismissTimer.Interval.TotalMilliseconds > 0)
        {
            StartDismissTimer();
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        StopDismissTimer();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (ReferenceEquals(e.Property, CanDismissByTimeProperty))
        {
            if (IsLoaded)
            {
                if (CanDismissByTime)
                {
                    StartDismissTimer();
                }
                else
                {
                    StopDismissTimer();
                }
            }
        }
        else if (ReferenceEquals(e.Property, DismissTimeoutProperty))
        {
            _dismissTimer.Interval = DismissTimeout;
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (InterruptDismissTimerWhileHover && _dismissTimer.IsEnabled)
        {
            _wasDismissTimerInterrupted = true;
            StopDismissTimer();
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        if (_wasDismissTimerInterrupted)
        {
            _wasDismissTimerInterrupted = false;
            if (IsLoaded && CanDismissByTime) // Need to check for IsLoaded as this method will still trigger after Unloaded!
                StartDismissTimer();
        }
    }

    private void ToastCardClickedHandler(object o, PointerPressedEventArgs pointerPressedEventArgs)
    {
        OnClicked?.Invoke(this);
        if (!CanDismissByClicking) return;
        Manager.Dismiss(this);
        OnDismissed?.Invoke(this, SukiToastDismissSource.Click);
    }

    private void DismissTimerOnTick(object sender, EventArgs e)
    {
        StopDismissTimer(0);
        Manager.Dismiss(this);
        OnDismissed?.Invoke(this, SukiToastDismissSource.Timeout);
    }

    private void DismissProgressValueTimerOnTick(object sender, EventArgs e)
    {
        DismissProgressValue = Math.Min(Math.Max(100 - _dismissStopwatch.ElapsedMilliseconds / DismissTimeout.TotalMilliseconds * 100, 0), 100);
    }

    public void AnimateShow()
    {
        this.Animate(OpacityProperty, 0d, 1d, TimeSpan.FromMilliseconds(500));
        this.Animate<double>(MaxHeightProperty, 0, 500, TimeSpan.FromMilliseconds(500));
        this.Animate(MarginProperty, new Thickness(0, 10, 0, -10), new Thickness(), TimeSpan.FromMilliseconds(500));
    }

    public void AnimateDismiss()
    {
        this.Animate(OpacityProperty, 1d, 0d, TimeSpan.FromMilliseconds(300));
        this.Animate(MarginProperty, new Thickness(), new Thickness(0, 0, 0, 10), TimeSpan.FromMilliseconds(300));
    }

    private void StartDismissTimer()
    {
        if (_dismissTimer.IsEnabled) return;
        _dismissStopwatch.Restart();
        _dismissTimer.Start();
        _dismissProgressValueTimer.Start();
    }

    private void StopDismissTimer(double dismissProgressValue = 100)
    {
        _dismissTimer.Stop();
        _dismissProgressValueTimer.Stop();
        _dismissStopwatch.Stop();
        DismissProgressValue = dismissProgressValue;
    }

    public ISukiToast ResetToDefault()
    {
        _wasDismissTimerInterrupted = false;

        Title = string.Empty;
        Content = string.Empty;
        Icon = Icons.InformationOutline;
        Foreground = NotificationColor.InfoIconForeground;
        CanDismissByClicking = false;
        CanDismissByTime = false;
        DismissTimeout = TimeSpan.FromSeconds(5);
        InterruptDismissTimerWhileHover = true;
        DismissProgressValue = 100;

        ActionButtons.Clear();
        OnDismissed = null;
        OnClicked = null;
        LoadingState = false;
        DockPanel.SetDock(this, Dock.Bottom);
        return this;
    }
}