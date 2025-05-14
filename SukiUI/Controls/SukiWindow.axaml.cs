using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Collections;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using SukiUI.Enums;
using System.Runtime.InteropServices;
using Avalonia.Controls.Metadata;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.Controls.Presenters;
using SukiUI.Extensions;

namespace SukiUI.Controls;

[TemplatePart("PART_Root", typeof(Panel))]
[TemplatePart("PART_LayoutTransform", typeof(LayoutTransformControl))]
[TemplatePart("PART_TitleBarBackground", typeof(GlassCard))]
[TemplatePart("PART_Logo", typeof(ContentPresenter))]
[TemplatePart("PART_FullScreenButton", typeof(Button))]
[TemplatePart("PART_PinButton", typeof(Button))]
[TemplatePart("PART_MinimizeButton", typeof(Button))]
[TemplatePart("PART_MaximizeButton", typeof(Button))]
[TemplatePart("PART_CloseButton", typeof(Button))]
[TemplatePart("PART_Menu", typeof(Menu))]
[TemplatePart("PART_BottomBorder", typeof(Border))]
[TemplatePart("PART_ContentPresenter", typeof(ContentPresenter))]
public class SukiWindow : Window, IDisposable
{
    #region Enums
    /// <summary>
    /// Specifies the visibility mode of the title bar.
    /// </summary>
    public enum TitleBarVisibilityMode
    {
        [Description("Unchanged: The title bar visibility will be kept unchanged during diferent states.")]
        Unchanged,

        [Description("Visible: The title bar is visible.")]
        Visible,

        [Description("Hidden: The title bar is hidden.")]
        Hidden,

        [Description("Auto Hidden: The title bar is auto hidden when cursor is far from it.")]
        AutoHidden
    }
    #endregion

    #region Template

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(SukiWindow);
    #endregion

    #region Members
    private const int DefaultAutoHideDelay = 1000;
    private const int DefaultAutoShowDelay = 300;

    private bool _isDisposed;
    private bool _wasTitleBarVisibleBeforeFullScreen = true;

    private readonly DispatcherTimer _hideTitleBarTimer = new DispatcherTimer()
    {
        Interval = TimeSpan.FromMilliseconds(DefaultAutoHideDelay)
    };
    private readonly DispatcherTimer _showTitleBarTimer = new DispatcherTimer()
    {
        Interval = TimeSpan.FromMilliseconds(DefaultAutoShowDelay)
    };

    private readonly List<Action> _disposeActions = new List<Action>();
    #endregion

    #region Properties
    public static readonly StyledProperty<double> MaxWidthScreenRatioProperty =
        AvaloniaProperty.Register<SukiWindow, double>(nameof(MaxWidthScreenRatio), double.NaN);

    /// <summary>
    /// Gets or sets the maximum width of the window as a ratio of the host screen width.
    /// </summary>
    public double MaxWidthScreenRatio
    {
        get => GetValue(MaxWidthScreenRatioProperty);
        set => SetValue(MaxWidthScreenRatioProperty, value);
    }

    public static readonly StyledProperty<double> MaxHeightScreenRatioProperty =
        AvaloniaProperty.Register<SukiWindow, double>(nameof(MaxHeightScreenRatio), double.NaN);

    /// <summary>
    /// Gets or sets the maximum height of the window as a ratio of the host screen height.
    /// </summary>
    public double MaxHeightScreenRatio
    {
        get => GetValue(MaxHeightScreenRatioProperty);
        set => SetValue(MaxHeightScreenRatioProperty, value);
    }

    public static readonly StyledProperty<bool> IsTitleBarVisibleProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(IsTitleBarVisible), defaultValue: true);

    /// <summary>
    /// Gets or sets a value indicating whether the title bar is visible.
    /// </summary>
    public bool IsTitleBarVisible
    {
        get => GetValue(IsTitleBarVisibleProperty);
        set => SetValue(IsTitleBarVisibleProperty, value);
    }

    public static readonly StyledProperty<TitleBarVisibilityMode> TitleBarVisibilityOnFullScreenProperty =
        AvaloniaProperty.Register<SukiWindow, TitleBarVisibilityMode>(nameof(TitleBarVisibilityOnFullScreen), TitleBarVisibilityMode.AutoHidden);

    /// <summary>
    /// Gets or sets the visibility mode of the title bar when the window is in full screen mode.
    /// </summary>
    public TitleBarVisibilityMode TitleBarVisibilityOnFullScreen
    {
        get => GetValue(TitleBarVisibilityOnFullScreenProperty);
        set => SetValue(TitleBarVisibilityOnFullScreenProperty, value);
    }

    public static readonly StyledProperty<int> TitleBarAutoHideDelayProperty =
        AvaloniaProperty.Register<SukiWindow, int>(nameof(TitleBarAutoHideDelay), DefaultAutoHideDelay);

    /// <summary>
    /// Gets or sets the delay in milliseconds before the title bar is automatically hidden.
    /// </summary>
    public int TitleBarAutoHideDelay
    {
        get => GetValue(TitleBarAutoHideDelayProperty);
        set => SetValue(TitleBarAutoHideDelayProperty, value);
    }

    public static readonly StyledProperty<int> TitleBarAutoShowDelayProperty =
        AvaloniaProperty.Register<SukiWindow, int>(nameof(TitleBarAutoShowDelay), DefaultAutoShowDelay);

    /// <summary>
    /// Gets or sets the delay in milliseconds before the title bar is automatically shown.
    /// </summary>
    public int TitleBarAutoShowDelay
    {
        get => GetValue(TitleBarAutoShowDelayProperty);
        set => SetValue(TitleBarAutoShowDelayProperty, value);
    }

    public static readonly StyledProperty<bool> TitleBarAnimationEnabledProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(TitleBarAnimationEnabled), defaultValue: true);


    /// <summary>
    /// Gets or sets a value indicating whether the title bar animations are enabled.
    /// </summary>
    public bool TitleBarAnimationEnabled
    {
        get => GetValue(TitleBarAnimationEnabledProperty);
        set => SetValue(TitleBarAnimationEnabledProperty, value);
    }

    public static readonly StyledProperty<double> TitleFontSizeProperty =
        AvaloniaProperty.Register<SukiWindow, double>(nameof(TitleFontSize), defaultValue: 13);

    /// <summary>
    /// Gets or sets the font size of the title bar.
    /// </summary>
    public double TitleFontSize
    {
        get => GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    public static readonly StyledProperty<FontWeight> TitleFontWeightProperty =
        AvaloniaProperty.Register<SukiWindow, FontWeight>(nameof(TitleFontWeight), defaultValue: FontWeight.Bold);

    /// <summary>
    /// Gets or sets the font weight of the title bar.
    /// </summary>
    public FontWeight TitleFontWeight
    {
        get => GetValue(TitleFontWeightProperty);
        set => SetValue(TitleFontWeightProperty, value);
    }

    public static readonly StyledProperty<ContextMenu> TitleBarContextMenuProperty =
        AvaloniaProperty.Register<SukiWindow, ContextMenu>(nameof(TitleBarContextMenu));

    /// <summary>
    /// Gets or sets the context menu that appears when the title bar is right-clicked.
    /// </summary>
    public ContextMenu TitleBarContextMenu
    {
        get => GetValue(TitleBarContextMenuProperty);
        set => SetValue(TitleBarContextMenuProperty, value);
    }

    public static readonly StyledProperty<Control?> LogoContentProperty =
        AvaloniaProperty.Register<SukiWindow, Control?>(nameof(LogoContent));

    /// <summary>
    /// Gets or sets the content of the logo displayed in the title bar.
    /// </summary>
    public Control? LogoContent
    {
        get => GetValue(LogoContentProperty);
        set => SetValue(LogoContentProperty, value);
    }

    public static readonly StyledProperty<bool> LogoDoubleTapClosesWindowProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(LogoDoubleTapClosesWindow));

    /// <summary>
    /// Gets or sets a value indicating whether the bottom border of the window is visible.
    /// </summary>
    public bool LogoDoubleTapClosesWindow
    {
        get => GetValue(LogoDoubleTapClosesWindowProperty);
        set => SetValue(LogoDoubleTapClosesWindowProperty, value);
    }

    public static readonly StyledProperty<bool> ShowBottomBorderProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(ShowBottomBorder), defaultValue: true);

    /// <summary>
    /// Gets or sets a value indicating whether the bottom border of the window is visible.
    /// </summary>
    public bool ShowBottomBorder
    {
        get => GetValue(ShowBottomBorderProperty);
        set => SetValue(ShowBottomBorderProperty, value);
    }

    public static readonly StyledProperty<bool> IsMenuVisibleProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(IsMenuVisible), defaultValue: false);

    /// <summary>
    /// Gets or sets a value indicating whether the menu is visible.
    /// </summary>
    public bool IsMenuVisible
    {
        get => GetValue(IsMenuVisibleProperty);
        set => SetValue(IsMenuVisibleProperty, value);
    }

    public static readonly StyledProperty<AvaloniaList<MenuItem>?> MenuItemsProperty =
        AvaloniaProperty.Register<SukiWindow, AvaloniaList<MenuItem>?>(nameof(MenuItems));

    /// <summary>
    /// Gets or sets the menu items that are displayed in the menu.
    /// </summary>
    public AvaloniaList<MenuItem>? MenuItems
    {
        get => GetValue(MenuItemsProperty);
        set => SetValue(MenuItemsProperty, value);
    }

    public static readonly StyledProperty<CornerRadius> RootCornerRadiusProperty =
        AvaloniaProperty.Register<SukiWindow, CornerRadius>(nameof(RootCornerRadius));

    /// <summary>
    /// Gets or sets the corner radius of the window.
    /// </summary>
    public CornerRadius RootCornerRadius
    {
        get => GetValue(RootCornerRadiusProperty);
        set => SetValue(RootCornerRadiusProperty, value);
    }

    public static readonly StyledProperty<bool> CanMinimizeProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(CanMinimize), defaultValue: true);

    /// <summary>
    /// Gets or sets a value indicating whether the window can be minimized.
    /// </summary>
    public bool CanMinimize
    {
        get => GetValue(CanMinimizeProperty);
        set => SetValue(CanMinimizeProperty, value);
    }

    public static readonly StyledProperty<bool> ShowTitlebarBackgroundProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(ShowTitlebarBackground), defaultValue: true);

    /// <summary>
    /// Gets or sets a value indicating whether the title bar background is visible.
    /// </summary>
    public bool ShowTitlebarBackground
    {
        get => GetValue(ShowTitlebarBackgroundProperty);
        set => SetValue(ShowTitlebarBackgroundProperty, value);
    }

    public static readonly StyledProperty<bool> CanFullScreenProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(CanFullScreen));

    /// <summary>
    /// Gets or sets a value indicating whether the window can be full-screened.
    /// </summary>
    public bool CanFullScreen
    {
        get => GetValue(CanFullScreenProperty);
        set => SetValue(CanFullScreenProperty, value);
    }

    public static readonly StyledProperty<bool> CanPinProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(CanPin));

    /// <summary>
    /// Gets or sets a value indicating whether the window can be pinned.
    /// </summary>
    public bool CanPin
    {
        get => GetValue(CanPinProperty);
        set => SetValue(CanPinProperty, value);
    }

    public static readonly StyledProperty<bool> CanMaximizeProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(CanMaximize), defaultValue: true);

    /// <summary>
    /// Gets or sets a value indicating whether the window can be maximized.
    /// </summary>
    public bool CanMaximize
    {
        get => GetValue(CanMaximizeProperty);
        set => SetValue(CanMaximizeProperty, value);
    }

    public static readonly StyledProperty<bool> CanMoveProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(CanMove), defaultValue: true);

    /// <summary>
    /// Gets or sets a value indicating whether the window can be moved.
    /// </summary>
    public bool CanMove
    {
        get => GetValue(CanMoveProperty);
        set => SetValue(CanMoveProperty, value);
    }

    // Background properties
    public static readonly StyledProperty<bool> BackgroundAnimationEnabledProperty =
        SukiMainHost.BackgroundAnimationEnabledProperty.AddOwner<SukiWindow>();

    /// <inheritdoc cref="SukiBackground.AnimationEnabled"/>
    public bool BackgroundAnimationEnabled
    {
        get => GetValue(BackgroundAnimationEnabledProperty);
        set => SetValue(BackgroundAnimationEnabledProperty, value);
    }

    public static readonly StyledProperty<SukiBackgroundStyle> BackgroundStyleProperty =
        SukiMainHost.BackgroundStyleProperty.AddOwner<SukiWindow>();

    /// <inheritdoc cref="SukiBackground.Style"/>
    public SukiBackgroundStyle BackgroundStyle
    {
        get => GetValue(BackgroundStyleProperty);
        set => SetValue(BackgroundStyleProperty, value);
    }

    public static readonly StyledProperty<string?> BackgroundShaderFileProperty =
        SukiMainHost.BackgroundShaderFileProperty.AddOwner<SukiWindow>();

    /// <inheritdoc cref="SukiBackground.ShaderFile"/>
    public string? BackgroundShaderFile
    {
        get => GetValue(BackgroundShaderFileProperty);
        set => SetValue(BackgroundShaderFileProperty, value);
    }

    public static readonly StyledProperty<string?> BackgroundShaderCodeProperty =
        SukiMainHost.BackgroundShaderCodeProperty.AddOwner<SukiWindow>();

    /// <inheritdoc cref="SukiBackground.ShaderCode"/>
    public string? BackgroundShaderCode
    {
        get => GetValue(BackgroundShaderCodeProperty);
        set => SetValue(BackgroundShaderCodeProperty, value);
    }

    public static readonly StyledProperty<bool> BackgroundTransitionsEnabledProperty =
        SukiMainHost.BackgroundTransitionsEnabledProperty.AddOwner<SukiWindow>();

    /// <inheritdoc cref="SukiBackground.TransitionsEnabled"/>
    public bool BackgroundTransitionsEnabled
    {
        get => GetValue(BackgroundTransitionsEnabledProperty);
        set => SetValue(BackgroundTransitionsEnabledProperty, value);
    }

    public static readonly StyledProperty<double> BackgroundTransitionTimeProperty =
        SukiMainHost.BackgroundTransitionTimeProperty.AddOwner<SukiWindow>();

    /// <inheritdoc cref="SukiBackground.TransitionTime"/>
    public double BackgroundTransitionTime
    {
        get => GetValue(BackgroundTransitionTimeProperty);
        set => SetValue(BackgroundTransitionTimeProperty, value);
    }

    public static readonly StyledProperty<bool> BackgroundForceSoftwareRenderingProperty =
        SukiMainHost.BackgroundForceSoftwareRenderingProperty.AddOwner<SukiWindow>();

    /// <summary>
    /// Forces the background of the window to utilise software rendering.
    /// This prevents use of any advanced effects or animations and provides only a flat background colour that changes with the theme.
    /// </summary>
    public bool BackgroundForceSoftwareRendering
    {
        get => GetValue(BackgroundForceSoftwareRenderingProperty);
        set => SetValue(BackgroundForceSoftwareRenderingProperty, value);
    }

    public static readonly StyledProperty<Avalonia.Controls.Controls> RightWindowTitleBarControlsProperty =
        AvaloniaProperty.Register<SukiWindow, Avalonia.Controls.Controls>(nameof(RightWindowTitleBarControls));


    /// <summary>
    /// Controls that are displayed on the right side of the title bar,
    /// to the left of the normal window control buttons. (Displays provided controls right-to-left)
    /// </summary>
    public Avalonia.Controls.Controls RightWindowTitleBarControls
    {
        get => GetValue(RightWindowTitleBarControlsProperty);
        set => SetValue(RightWindowTitleBarControlsProperty, value);
    }

    public static readonly StyledProperty<Avalonia.Controls.Controls> HostsProperty =
        SukiMainHost.HostsProperty.AddOwner<SukiWindow>();

    /// <summary>
    /// These controls are displayed above all others and fill the entire window.
    /// You can include <see cref="SukiDialogHost"/> and <see cref="SukiToastHost"/> or create your own custom implementations.
    /// </summary>
    public Avalonia.Controls.Controls Hosts
    {
        get => GetValue(HostsProperty);
        set => SetValue(HostsProperty, value);
    }

    public static readonly DirectProperty<SukiWindow, WindowState> PreviousVisibleWindowStateProperty =
        AvaloniaProperty.RegisterDirect<SukiWindow, WindowState>(
            nameof(PreviousVisibleWindowState),
            o => o.PreviousVisibleWindowState);

    private WindowState _previousVisibleWindowState = WindowState.Normal;
    /// <summary>
    /// Gets the previous visible window state.
    /// </summary>
    public WindowState PreviousVisibleWindowState
    {
        get => _previousVisibleWindowState;
        private set => SetAndRaise(PreviousVisibleWindowStateProperty, ref _previousVisibleWindowState, value);
    }
    #endregion

    #region Constructor
    public SukiWindow()
    {
        Hosts = [];
        RightWindowTitleBarControls = [];
        MenuItems = [];
        ScalingChanged += OnScalingChanged;

        _hideTitleBarTimer.Tick += HideTitleBarTimerOnTick;
        _showTitleBarTimer.Tick += ShowTitleBarTimerOnTick;
    }

    #endregion

    #region Overrides

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Dispose all previous event in a case of re-applying the template
        foreach (var disposeAction in _disposeActions)
        {
            disposeAction.Invoke();
        }
        _disposeActions.Clear();

        // save the initial values
        _wasTitleBarVisibleBeforeFullScreen = IsTitleBarVisible;

        // Create handlers for buttons
        if (e.NameScope.Find<GlassCard>("PART_TitleBarBackground") is { } titleBar)
        {
            titleBar.PointerPressed += OnTitleBarPointerPressed;
            titleBar.PointerReleased += OnTitleBarPointerReleased;
            titleBar.DoubleTapped += OnMaximizeButtonClicked;
            _disposeActions.Add(() =>
            {
                titleBar.PointerPressed -= OnTitleBarPointerPressed;
                titleBar.PointerReleased -= OnTitleBarPointerReleased;
                titleBar.DoubleTapped -= OnMaximizeButtonClicked;
            });
        }

        if (e.NameScope.Find<ContentPresenter>("PART_Logo") is { } logo)
        {
            logo.DoubleTapped += LogoOnDoubleTapped;
            _disposeActions.Add(() => logo.DoubleTapped -= LogoOnDoubleTapped);
        }

        if (e.NameScope.Find<Button>("PART_FullScreenButton") is { } fullscreen)
        {
            fullscreen.Click += OnFullScreenButtonClicked;
            _disposeActions.Add(() => fullscreen.Click -= OnFullScreenButtonClicked);
        }

        if (e.NameScope.Find<Button>("PART_PinButton") is { } pin)
        {
            pin.Click += OnPinButtonClicked;
            _disposeActions.Add(() => pin.Click -= OnPinButtonClicked);
        }

        if (e.NameScope.Find<Button>("PART_MinimizeButton") is { } minimize)
        {
            minimize.Click += OnMinimizeButtonClicked;
            _disposeActions.Add(() => minimize.Click -= OnMinimizeButtonClicked);
        }

        if (e.NameScope.Find<Button>("PART_MaximizeButton") is { } maximize)
        {
            maximize.Click += OnMaximizeButtonClicked;
            _disposeActions.Add(() => maximize.Click -= OnMaximizeButtonClicked);
            EnableWindowsSnapLayout(maximize);
        }

        if (e.NameScope.Find<Button>("PART_CloseButton") is { } close)
        {
            close.Click += OnCloseButtonClicked;
            _disposeActions.Add(() => close.Click -= OnCloseButtonClicked);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (e.NameScope.Find<Panel>("PART_Root") is { } rootPanel)
            {
                AddResizeGripForLinux(rootPanel);
            }
            if (RootCornerRadius == default)
            {
                RootCornerRadius = new CornerRadius(10);
            }
        }
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;
        if (desktop.MainWindow is SukiWindow window && window != this)
        {
            Icon ??= window.Icon;
            // This would be nice to do, but obviously LogoContent is a control and you can't attach it twice.
            // if (LogoContent is null) LogoContent = s.LogoContent;
        }
    }

    /// <inheritdoc />
    protected override void OnClosed(EventArgs e)
    {
        Dispose();
        base.OnClosed(e);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MaxWidthScreenRatioProperty)
        {
            this.ConstrainMaxSizeToScreenRatio(MaxWidthScreenRatio, double.NaN);
        }
        else if (change.Property == MaxHeightScreenRatioProperty)
        {
            this.ConstrainMaxSizeToScreenRatio(double.NaN, MaxHeightScreenRatio);
        }
        else if (change.Property == WindowStateProperty)
        {
            if (change.OldValue is not WindowState oldWindowState
            || change.NewValue is not WindowState newWindowState) return;

            OnWindowStateChanged(oldWindowState, newWindowState);
        }
        else if (change.Property == TitleBarVisibilityOnFullScreenProperty)
        {
            if (WindowState == WindowState.FullScreen)
            {
                if (change.NewValue is not TitleBarVisibilityMode mode) return;

                IsTitleBarVisible = mode switch
                {
                    TitleBarVisibilityMode.Unchanged => _wasTitleBarVisibleBeforeFullScreen,
                    TitleBarVisibilityMode.Visible => true,
                    TitleBarVisibilityMode.Hidden or TitleBarVisibilityMode.AutoHidden => false,
                    _ => IsTitleBarVisible
                };

                PointerMoved -= AutoHideTitleBarOnPointerMoved;
                if (mode == TitleBarVisibilityMode.AutoHidden)
                {
                    PointerMoved += AutoHideTitleBarOnPointerMoved;
                }
            }
        }
        else if (change.Property == TitleBarAutoHideDelayProperty)
        {
            _hideTitleBarTimer.Interval = TimeSpan.FromMilliseconds(TitleBarAutoHideDelay);
        }
        else if (change.Property == TitleBarAutoShowDelayProperty)
        {
            _showTitleBarTimer.Interval = TimeSpan.FromMilliseconds(TitleBarAutoShowDelay);
        }
    }
    #endregion

    #region Events

    /// <summary>
    /// Occurs when the scaling of the window changes.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnScalingChanged(object sender, EventArgs e)
    {
        this.ConstrainMaxSizeToScreenRatio(MaxWidthScreenRatio, MaxHeightScreenRatio);
    }

    /// <summary>
    /// Occurs when the window newState changes.
    /// </summary>
    /// <param name="oldState"></param>
    /// <param name="newState"></param>
    private void OnWindowStateChanged(WindowState oldState, WindowState newState)
    {
        PointerMoved -= AutoHideTitleBarOnPointerMoved;
        _showTitleBarTimer.Stop();
        _hideTitleBarTimer.Stop();

        if (oldState != WindowState.Minimized)
        {
            PreviousVisibleWindowState = oldState;
        }

        if (newState == WindowState.Minimized) return;
        if (newState == WindowState.FullScreen)
        {
            _wasTitleBarVisibleBeforeFullScreen = IsTitleBarVisible;
            switch (TitleBarVisibilityOnFullScreen)
            {
                case TitleBarVisibilityMode.Visible:
                    IsTitleBarVisible = true;
                    break;
                case TitleBarVisibilityMode.Hidden:
                    IsTitleBarVisible = false;
                    break;
                case TitleBarVisibilityMode.AutoHidden:
                    if (IsTitleBarVisible) _hideTitleBarTimer.Start();
                    PointerMoved += AutoHideTitleBarOnPointerMoved;
                    break;
            }
        }
        else if (oldState == WindowState.FullScreen)
        {
            // Restore window control capabilities from a state before the fullscreen
            if (TitleBarVisibilityOnFullScreen != TitleBarVisibilityMode.Unchanged)
            {
                IsTitleBarVisible = _wasTitleBarVisibleBeforeFullScreen;
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) // only for windows platform
        {
            Margin = new Thickness(newState == WindowState.Maximized
                ? 7
                : 0);
        }

        this.ConstrainMaxSizeToScreenRatio(MaxWidthScreenRatio, MaxHeightScreenRatio);
    }

    /// <summary>
    /// Occurs when the logo is double-tapped.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LogoOnDoubleTapped(object sender, TappedEventArgs e)
    {
        if (!LogoDoubleTapClosesWindow) return;
        Close();
    }

    /// <summary>
    /// Occurs when the full screen button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnFullScreenButtonClicked(object? sender, RoutedEventArgs args)
    {
        if (!CanFullScreen) return;
        ToggleFullScreen();
    }

    /// <summary>
    /// Occurs when the pin button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnPinButtonClicked(object? sender, RoutedEventArgs args)
    {
        if (!CanPin) return;
        Topmost = !Topmost;
    }

    /// <summary>
    /// Occurs when the minimize button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMinimizeButtonClicked(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// Occurs when the maximize button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnMaximizeButtonClicked(object? sender, RoutedEventArgs args)
    {
        var windowState = WindowState;
        if (!CanMaximize || windowState == WindowState.FullScreen) return;
        WindowState = windowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    /// <summary>
    /// Occurs when the close button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCloseButtonClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Occurs when the cursor is moved when the application is in fullscreen mode and title bar visibility is set to <see cref="TitleBarVisibilityMode.AutoHidden"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AutoHideTitleBarOnPointerMoved(object sender, PointerEventArgs e)
    {
        var position = e.GetPosition(this);

        if (position.Y <= 10)
        {
            _hideTitleBarTimer.Stop();
            if (!IsTitleBarVisible)
            {
                _showTitleBarTimer.Start();
            }
        }
        else if (position.Y >= 50)
        {
            _showTitleBarTimer.Stop();
            if (IsTitleBarVisible)
            {
                _hideTitleBarTimer.Start();
            }
        }
    }

    /// <summary>
    /// Occurs when the title bar is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!CanMove || WindowState == WindowState.FullScreen)
            return;
        BeginMoveDrag(e);
    }

    /// <summary>
    /// Occurs when the title bar is released
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnTitleBarPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        // Ensure correct window max size if dropped on other screen/resolution while using max size ratio
        if (!CanMove || e.InitialPressMouseButton != MouseButton.Left) return;
        this.ConstrainMaxSizeToScreenRatio(MaxWidthScreenRatio, MaxHeightScreenRatio);
    }

    /// <summary>
    /// Occurs when the resize grip is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void RaiseResize(object? sender, PointerPressedEventArgs e)
    {
        if (!CanResize || WindowState != WindowState.Normal) return;
        if (sender is not Border border || border.Tag is not string edge) return;
        if (VisualRoot is not Window window)
            return;

        var windowEdge = edge switch
        {
            "North" => WindowEdge.North,
            "South" => WindowEdge.South,
            "West" => WindowEdge.West,
            "East" => WindowEdge.East,
            "NW" => WindowEdge.NorthWest,
            "NE" => WindowEdge.NorthEast,
            "SW" => WindowEdge.SouthWest,
            "SE" => WindowEdge.SouthEast,
            _ => throw new ArgumentOutOfRangeException()
        };

        window.BeginResizeDrag(windowEdge, e);
        e.Handled = true;
    }

    private void HideTitleBarTimerOnTick(object sender, EventArgs e)
    {
        _hideTitleBarTimer.Stop();
        IsTitleBarVisible = false;
    }

    private void ShowTitleBarTimerOnTick(object sender, EventArgs e)
    {
        _showTitleBarTimer.Stop();
        IsTitleBarVisible = true;
    }
    #endregion

    #region Methods
    private void EnableWindowsSnapLayout(Button maximize)
    {
        const int HTMAXBUTTON = 9;
        const uint WM_NCHITTEST = 0x0084;
        const uint WM_CAPTURECHANGED = 0x0215;

        var pointerOnButton = false;
        var pointerOverSetter = typeof(Button).GetProperty(nameof(IsPointerOver));
        if (pointerOverSetter is null) throw new NullReferenceException($"Unable to find Button.{nameof(IsPointerOver)} property.");

        nint ProcHookCallback(nint hWnd, uint msg, nint wParam, nint lParam, ref bool handled)
        {
            if (!maximize.IsVisible) return 0;

            if (msg == WM_NCHITTEST)
            {
                var point = new PixelPoint((short)(ToInt32(lParam) & 0xffff), (short)(ToInt32(lParam) >> 16));

                var buttonSize = maximize.DesiredSize;

                var buttonLeftTop = maximize.PointToScreen(FlowDirection == FlowDirection.LeftToRight
                                                           ? new Point(buttonSize.Width, 0)
                                                           : new Point(0, 0));

                var x = (buttonLeftTop.X - point.X) / RenderScaling;
                var y = (point.Y - buttonLeftTop.Y) / RenderScaling;

                if (new Rect(default, buttonSize).Contains(new Point(x, y)))
                {
                    handled = true;

                    if (pointerOnButton == false)
                    {
                        pointerOnButton = true;
                        pointerOverSetter.SetValue(maximize, true);
                    }

                    return HTMAXBUTTON;
                }
                else
                {
                    if (pointerOnButton)
                    {
                        pointerOnButton = false;
                        pointerOverSetter.SetValue(maximize, false);
                    }
                }
            }
            else if (msg == WM_CAPTURECHANGED)
            {
                if (pointerOnButton && CanMaximize)
                {
                    WindowState = WindowState == WindowState.Maximized
                                  ? WindowState.Normal
                                  : WindowState.Maximized;

                    pointerOverSetter.SetValue(maximize, false);
                }
            }

            return 0;
        }

        static int ToInt32(IntPtr ptr) => IntPtr.Size == 4 ? ptr.ToInt32() : (int)(ptr.ToInt64() & 0xffffffff);


        var wndProcHookCallback = new Win32Properties.CustomWndProcHookCallback(ProcHookCallback);
        Win32Properties.AddWndProcHookCallback(this, wndProcHookCallback);

        _disposeActions.Add(() => Win32Properties.RemoveWndProcHookCallback(this, wndProcHookCallback));
    }

    /// <summary>
    /// Adds resize grips to the window for Linux system.
    /// </summary>
    /// <param name="rootPanel"></param>
    private void AddResizeGripForLinux(Panel rootPanel)
    {
        var resizeBorders = new[]
        {
            new {
                Tag = "North",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Cursor = StandardCursorType.SizeNorthSouth,
                IsCorner = false
            },
            new {
                Tag = "South",
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Cursor = StandardCursorType.SizeNorthSouth,
                IsCorner = false
            },
            new {
                Tag = "West",
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Left,
                Cursor = StandardCursorType.SizeWestEast,
                IsCorner = false
            },
            new {
                Tag = "East",
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Right,
                Cursor = StandardCursorType.SizeWestEast,
                IsCorner = false
            },

            new {
                Tag = "NW",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Cursor = StandardCursorType.TopLeftCorner,
                IsCorner = true
            },
            new {
                Tag = "NE",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Cursor = StandardCursorType.TopRightCorner,
                IsCorner = true
            },
            new {
                Tag = "SW",
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left,
                Cursor = StandardCursorType.BottomLeftCorner,
                IsCorner = true
            },
            new {
                Tag = "SE",
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
                Cursor = StandardCursorType.BottomRightCorner,
                IsCorner = true
            }
        };

        foreach (var config in resizeBorders)
        {
            var border = new Border
            {
                Tag = config.Tag,
                Background = Brushes.Transparent,
                VerticalAlignment = config.VerticalAlignment,
                HorizontalAlignment = config.HorizontalAlignment,
                Cursor = new Cursor(config.Cursor)
            };

            if (config.IsCorner)
            {
                border.Width = 8;
                border.Height = 8;
            }
            else
            {
                if (config.VerticalAlignment == VerticalAlignment.Stretch)
                {
                    border.Width = 6;
                }
                if (config.HorizontalAlignment == HorizontalAlignment.Stretch)
                {
                    border.Height = 6;
                }
            }

            border.PointerPressed += RaiseResize;
            _disposeActions.Add(() => border.PointerPressed -= RaiseResize);
            rootPanel.Children.Add(border);
        }
    }

    /// <summary>
    /// Toggles the full screen mode.
    /// </summary>
    public void ToggleFullScreen()
    {
        WindowState = WindowState == WindowState.FullScreen
            ? PreviousVisibleWindowState
            : WindowState.FullScreen;
    }

    #endregion

    #region Dispose

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Stop timers if running
        _hideTitleBarTimer.Stop();
        _showTitleBarTimer.Stop();

        // Release all events
        ScalingChanged -= OnScalingChanged;
        PointerMoved -= AutoHideTitleBarOnPointerMoved;
        _hideTitleBarTimer.Tick -= HideTitleBarTimerOnTick;
        _showTitleBarTimer.Tick -= ShowTitleBarTimerOnTick;
        foreach (var disposeAction in _disposeActions)
        {
            disposeAction.Invoke();
        }
    }
    #endregion
}