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

namespace SukiUI.Controls;

[TemplatePart("PART_VisualLayerManager", typeof(VisualLayerManager))]
[TemplatePart("PART_Root", typeof(Panel))]
[TemplatePart("PART_Background", typeof(SukiBackground))]
[TemplatePart("PART_LayoutTransform", typeof(LayoutTransformControl))]
[TemplatePart("PART_TitleBarBackground", typeof(GlassCard))]
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
    public enum TitleBarVisibilityMode
    {
        Unchanged,
        Visible,
        Hidden,
        AutoHidden
    }
    #endregion

    #region Template
    protected override Type StyleKeyOverride => typeof(SukiWindow);
    #endregion

    #region Members
    private bool _isDisposed;

    private bool _canMaximize;
    private bool _canMove;
    private bool _canResize;
    private bool _wasTitleBarVisibleBeforeFullScreen = true;

    private const int DefaultAutoHideDelay = 1000;
    private const int DefaultAutoShowDelay = 300;
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
    public static readonly StyledProperty<double> TitleFontSizeProperty =
        AvaloniaProperty.Register<SukiWindow, double>(nameof(TitleFontSize), defaultValue: 13);

    public double TitleFontSize
    {
        get => GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    public static readonly StyledProperty<ContextMenu> TitleBarContextMenuProperty =
        AvaloniaProperty.Register<SukiWindow, ContextMenu>(nameof(TitleBarContextMenu));

    public ContextMenu TitleBarContextMenu
    {
        get => GetValue(TitleBarContextMenuProperty);
        set => SetValue(TitleBarContextMenuProperty, value);
    }

    public static readonly StyledProperty<FontWeight> TitleFontWeightProperty =
        AvaloniaProperty.Register<SukiWindow, FontWeight>(nameof(TitleFontWeight), defaultValue: FontWeight.Bold);

    public FontWeight TitleFontWeight
    {
        get => GetValue(TitleFontWeightProperty);
        set => SetValue(TitleFontWeightProperty, value);
    }

    public static readonly StyledProperty<Control?> LogoContentProperty =
        AvaloniaProperty.Register<SukiWindow, Control?>(nameof(LogoContent));

    public Control? LogoContent
    {
        get => GetValue(LogoContentProperty);
        set => SetValue(LogoContentProperty, value);
    }

    public static readonly StyledProperty<bool> ShowBottomBorderProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(ShowBottomBorder), defaultValue: true);

    public bool ShowBottomBorder
    {
        get => GetValue(ShowBottomBorderProperty);
        set => SetValue(ShowBottomBorderProperty, value);
    }

    public static readonly StyledProperty<bool> IsTitleBarVisibleProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(IsTitleBarVisible), defaultValue: true);

    public bool IsTitleBarVisible
    {
        get => GetValue(IsTitleBarVisibleProperty);
        set => SetValue(IsTitleBarVisibleProperty, value);
    }


    public static readonly StyledProperty<TitleBarVisibilityMode> TitleBarVisibilityOnFullScreenProperty =
        AvaloniaProperty.Register<SukiWindow, TitleBarVisibilityMode>(nameof(TitleBarVisibilityOnFullScreen), TitleBarVisibilityMode.AutoHidden);

    public TitleBarVisibilityMode TitleBarVisibilityOnFullScreen
    {
        get => GetValue(TitleBarVisibilityOnFullScreenProperty);
        set => SetValue(TitleBarVisibilityOnFullScreenProperty, value);
    }

    public static readonly StyledProperty<int> TitleBarAutoHideDelayProperty =
        AvaloniaProperty.Register<SukiWindow, int>(nameof(TitleBarAutoHideDelay), DefaultAutoHideDelay);

    public int TitleBarAutoHideDelay
    {
        get => GetValue(TitleBarAutoHideDelayProperty);
        set => SetValue(TitleBarAutoHideDelayProperty, value);
    }

    public static readonly StyledProperty<int> TitleBarAutoShowDelayProperty =
        AvaloniaProperty.Register<SukiWindow, int>(nameof(TitleBarAutoShowDelay), DefaultAutoShowDelay);

    public int TitleBarAutoShowDelay
    {
        get => GetValue(TitleBarAutoShowDelayProperty);
        set => SetValue(TitleBarAutoShowDelayProperty, value);
    }

    public static readonly StyledProperty<bool> TitleBarAnimationEnabledProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(TitleBarAnimationEnabled), defaultValue: true);


    public bool TitleBarAnimationEnabled
    {
        get => GetValue(TitleBarAnimationEnabledProperty);
        set => SetValue(TitleBarAnimationEnabledProperty, value);
    }

    public static readonly StyledProperty<bool> IsMenuVisibleProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(IsMenuVisible), defaultValue: false);

    public bool IsMenuVisible
    {
        get => GetValue(IsMenuVisibleProperty);
        set => SetValue(IsMenuVisibleProperty, value);
    }

    public static readonly StyledProperty<AvaloniaList<MenuItem>?> MenuItemsProperty =
        AvaloniaProperty.Register<SukiWindow, AvaloniaList<MenuItem>?>(nameof(MenuItems));

    public AvaloniaList<MenuItem>? MenuItems
    {
        get => GetValue(MenuItemsProperty);
        set => SetValue(MenuItemsProperty, value);
    }

    public static readonly StyledProperty<CornerRadius> RootCornerRadiusProperty =
        AvaloniaProperty.Register<Border, CornerRadius>(nameof(RootCornerRadius), defaultValue: default);

    public CornerRadius RootCornerRadius
    {
        get => GetValue(RootCornerRadiusProperty);
        set => SetValue(RootCornerRadiusProperty, value);
    }

    public static readonly StyledProperty<bool> CanMinimizeProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(CanMinimize), defaultValue: true);

    public bool CanMinimize
    {
        get => GetValue(CanMinimizeProperty);
        set => SetValue(CanMinimizeProperty, value);
    }

    public static readonly StyledProperty<bool> ShowTitlebarBackgroundProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(ShowTitlebarBackground), defaultValue: true);
    public bool ShowTitlebarBackground
    {
        get => GetValue(ShowTitlebarBackgroundProperty);
        set => SetValue(ShowTitlebarBackgroundProperty, value);
    }

    public static readonly StyledProperty<bool> CanFullScreenProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(CanFullScreen));
    public bool CanFullScreen
    {
        get => GetValue(CanFullScreenProperty);
        set => SetValue(CanFullScreenProperty, value);
    }

    public static readonly StyledProperty<bool> CanPinProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(CanPin));
    public bool CanPin
    {
        get => GetValue(CanPinProperty);
        set => SetValue(CanPinProperty, value);
    }

    public static readonly StyledProperty<bool> CanMaximizeProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(CanMaximize), defaultValue: true);
    public bool CanMaximize
    {
        get => GetValue(CanMaximizeProperty);
        set => SetValue(CanMaximizeProperty, value);
    }

    public static readonly StyledProperty<bool> CanMoveProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(CanMove), defaultValue: true);

    public bool CanMove
    {
        get => GetValue(CanMoveProperty);
        set => SetValue(CanMoveProperty, value);
    }

    // Background properties
    public static readonly StyledProperty<bool> BackgroundAnimationEnabledProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(BackgroundAnimationEnabled), defaultValue: false);

    /// <inheritdoc cref="SukiBackground.AnimationEnabled"/>
    public bool BackgroundAnimationEnabled
    {
        get => GetValue(BackgroundAnimationEnabledProperty);
        set => SetValue(BackgroundAnimationEnabledProperty, value);
    }

    public static readonly StyledProperty<SukiBackgroundStyle> BackgroundStyleProperty =
        AvaloniaProperty.Register<SukiWindow, SukiBackgroundStyle>(nameof(BackgroundStyle),
            defaultValue: SukiBackgroundStyle.GradientSoft);

    /// <inheritdoc cref="SukiBackground.Style"/>
    public SukiBackgroundStyle BackgroundStyle
    {
        get => GetValue(BackgroundStyleProperty);
        set => SetValue(BackgroundStyleProperty, value);
    }

    public static readonly StyledProperty<string?> BackgroundShaderFileProperty =
        AvaloniaProperty.Register<SukiWindow, string?>(nameof(BackgroundShaderFile));

    /// <inheritdoc cref="SukiBackground.ShaderFile"/>
    public string? BackgroundShaderFile
    {
        get => GetValue(BackgroundShaderFileProperty);
        set => SetValue(BackgroundShaderFileProperty, value);
    }

    public static readonly StyledProperty<string?> BackgroundShaderCodeProperty =
        AvaloniaProperty.Register<SukiWindow, string?>(nameof(BackgroundShaderCode));

    /// <inheritdoc cref="SukiBackground.ShaderCode"/>
    public string? BackgroundShaderCode
    {
        get => GetValue(BackgroundShaderCodeProperty);
        set => SetValue(BackgroundShaderCodeProperty, value);
    }

    public static readonly StyledProperty<bool> BackgroundTransitionsEnabledProperty =
        AvaloniaProperty.Register<SukiBackground, bool>(nameof(BackgroundTransitionsEnabled), defaultValue: false);

    /// <inheritdoc cref="SukiBackground.TransitionsEnabled"/>
    public bool BackgroundTransitionsEnabled
    {
        get => GetValue(BackgroundTransitionsEnabledProperty);
        set => SetValue(BackgroundTransitionsEnabledProperty, value);
    }

    public static readonly StyledProperty<double> BackgroundTransitionTimeProperty =
        AvaloniaProperty.Register<SukiBackground, double>(nameof(BackgroundTransitionTime), defaultValue: 1.0);

    /// <inheritdoc cref="SukiBackground.TransitionTime"/>
    public double BackgroundTransitionTime
    {
        get => GetValue(BackgroundTransitionTimeProperty);
        set => SetValue(BackgroundTransitionTimeProperty, value);
    }

    public static readonly StyledProperty<Avalonia.Controls.Controls> RightWindowTitleBarControlsProperty =
        AvaloniaProperty.Register<SukiWindow, Avalonia.Controls.Controls>(nameof(RightWindowTitleBarControls),
            defaultValue: new Avalonia.Controls.Controls());

    public static readonly StyledProperty<bool> BackgroundForceSoftwareRenderingProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(BackgroundForceSoftwareRendering));

    /// <summary>
    /// Forces the background of the window to utilise software rendering.
    /// This prevents use of any advanced effects or animations and provides only a flat background colour that changes with the theme.
    /// </summary>
    public bool BackgroundForceSoftwareRendering
    {
        get => GetValue(BackgroundForceSoftwareRenderingProperty);
        set => SetValue(BackgroundForceSoftwareRenderingProperty, value);
    }

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
        AvaloniaProperty.Register<SukiWindow, Avalonia.Controls.Controls>(nameof(Hosts),
            defaultValue: new Avalonia.Controls.Controls());

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
    public WindowState PreviousVisibleWindowState
    {
        get => _previousVisibleWindowState;
        private set => SetAndRaise(PreviousVisibleWindowStateProperty, ref _previousVisibleWindowState, value);
    }
    #endregion

    #region Constructor
    public SukiWindow()
    {
        MenuItems = new AvaloniaList<MenuItem>();
        RightWindowTitleBarControls = new Avalonia.Controls.Controls();
        Hosts = new Avalonia.Controls.Controls();

        _hideTitleBarTimer.Tick += HideTitleBarTimerOnTick;
        _showTitleBarTimer.Tick += ShowTitleBarTimerOnTick;
    }
    #endregion

    #region Overrides
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // save the initial values of CanMaximize and CanMove
        if (_canMaximize == false)
            _canMaximize = CanMaximize;
        if (_canMove == false)
            _canMove = CanMove;
        if (_canResize == false)
            _canResize = CanResize;

        OnWindowStateChanged(WindowState);

        // Create handlers for buttons
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

        if (e.NameScope.Find<GlassCard>("PART_TitleBarBackground") is { } titleBar)
        {
            titleBar.PointerPressed += OnTitleBarPointerPressed;
            titleBar.DoubleTapped += OnMaximizeButtonClicked;
            _disposeActions.Add(() =>
            {
                titleBar.PointerPressed -= OnTitleBarPointerPressed;
                titleBar.DoubleTapped -= OnMaximizeButtonClicked;
            });
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

    protected override void OnClosed(EventArgs e)
    {
        Dispose();
        base.OnClosed(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WindowStateProperty && change.NewValue is WindowState windowState)
        {
            if (change.OldValue is WindowState oldWindowState)
            {
                if (oldWindowState != WindowState.Minimized)
                    PreviousVisibleWindowState = oldWindowState;
            }

            OnWindowStateChanged(windowState);
        }
        else if (change.Property == TitleBarVisibilityOnFullScreenProperty)
        {
            if (WindowState != WindowState.FullScreen) return;

            if (change.NewValue is TitleBarVisibilityMode mode)
            {
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
        /*else if (change.Property == IsTitleBarVisibleProperty) // Debug
        {
            var value = IsTitleBarVisible;
        }*/
    }
    #endregion

    #region Events
    private void OnWindowStateChanged(WindowState state)
    {
        PointerMoved -= AutoHideTitleBarOnPointerMoved;
        _showTitleBarTimer.Stop();
        _hideTitleBarTimer.Stop();
        var titleBarVisibilityOnFullScreen = TitleBarVisibilityOnFullScreen;
        if (state == WindowState.FullScreen)
        {
            // Disable window control capabilities
            _canMaximize = CanMaximize;
            CanMaximize = false;
            _canMove = CanMove;
            CanMove = false;
            _canResize = CanResize;
            CanResize = false;

            _wasTitleBarVisibleBeforeFullScreen = IsTitleBarVisible;
            switch (titleBarVisibilityOnFullScreen)
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
        else
        {
            // Restore window control capabilities
            CanMaximize = _canMaximize;
            CanMove = _canMove;
            CanResize = _canResize;

            if (titleBarVisibilityOnFullScreen != TitleBarVisibilityMode.Unchanged)
            {
                IsTitleBarVisible = _wasTitleBarVisibleBeforeFullScreen;
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) // only for windows platform
        {
            if (state == WindowState.Maximized)
                Margin = new Thickness(7);
            else
                Margin = new Thickness(0);
        }
    }

    private void OnFullScreenButtonClicked(object? sender, RoutedEventArgs args)
    {
        if (!CanFullScreen) return;
        ToggleFullScreen();
    }

    private void OnPinButtonClicked(object? sender, RoutedEventArgs args)
    {
        if (!CanPin) return;
        Topmost = !Topmost;
    }

    private void OnMinimizeButtonClicked(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void OnMaximizeButtonClicked(object? sender, RoutedEventArgs args)
    {
        if (!CanMaximize) return;
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void OnCloseButtonClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void AutoHideTitleBarOnPointerMoved(object sender, PointerEventArgs e)
    {
        var position = e.GetPosition(this);

        if (position.Y <= 3)
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

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!CanMove)
            return;
        base.OnPointerPressed(e);
        BeginMoveDrag(e);
    }

    private void RaiseResize(object? sender, PointerPressedEventArgs e)
    {
        if (!CanResize) return;
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
        IsTitleBarVisible = false;
    }

    private void ShowTitleBarTimerOnTick(object sender, EventArgs e)
    {
        IsTitleBarVisible = true;
    }
    #endregion

    #region Methods
    private void EnableWindowsSnapLayout(Button maximize)
    {
        var pointerOnMaxButton = false;
        var setter = typeof(Button).GetProperty("IsPointerOver");
        var proc = (IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
        {
            switch (msg)
            {
                case 533:
                    if (!pointerOnMaxButton) break;
                    if (!CanMaximize) break;
                    WindowState = WindowState == WindowState.Maximized
                        ? WindowState.Normal
                        : WindowState.Maximized;
                    break;
                case 0x0084:
                    var point = new PixelPoint(
                        (short)(ToInt32(lParam) & 0xffff),
                        (short)(ToInt32(lParam) >> 16));
                    var desiredSize = maximize.DesiredSize;
                    var buttonLeftTop = maximize.PointToScreen(FlowDirection == FlowDirection.LeftToRight
                                                               ? new Point(desiredSize.Width, 0)
                                                               : new Point(0, 0));
                    var x = (buttonLeftTop.X - point.X) / RenderScaling;
                    var y = (point.Y - buttonLeftTop.Y) / RenderScaling;
                    if (new Rect(0, 0,
                            desiredSize.Width,
                            desiredSize.Height)
                        .Contains(new Point(x, y)))
                    {
                        setter?.SetValue(maximize, true);
                        pointerOnMaxButton = true;
                        handled = true;
                        return (IntPtr)9;
                    }

                    pointerOnMaxButton = false;
                    setter?.SetValue(maximize, false);
                    break;
            }

            return IntPtr.Zero;

            static int ToInt32(IntPtr ptr) => IntPtr.Size == 4
                ? ptr.ToInt32()
                : (int)(ptr.ToInt64() & 0xffffffff);
        };

        var wndProcHookCallback = new Win32Properties.CustomWndProcHookCallback(proc);
        Win32Properties.AddWndProcHookCallback(this, wndProcHookCallback);

        _disposeActions.Add(() => Win32Properties.RemoveWndProcHookCallback(this, wndProcHookCallback));
    }

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
                Cursor = new Cursor(config.Cursor)
            };

            if (config.IsCorner)
            {
                border.Width = 8;
                border.Height = 8;
                border.VerticalAlignment = config.VerticalAlignment;
                border.HorizontalAlignment = config.HorizontalAlignment;
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
                border.VerticalAlignment = config.VerticalAlignment;
                border.HorizontalAlignment = config.HorizontalAlignment;
            }

            border.PointerPressed += RaiseResize;
            _disposeActions.Add(() => border.PointerPressed -= RaiseResize);
            rootPanel.Children.Add(border);
        }
    }

    public void ToggleFullScreen()
    {
        WindowState = WindowState == WindowState.FullScreen
            ? PreviousVisibleWindowState
            : WindowState.FullScreen;
    }

    #endregion

    #region Dispose
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Stop timers if running
        _hideTitleBarTimer.Stop();
        _showTitleBarTimer.Stop();

        // Release all events
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