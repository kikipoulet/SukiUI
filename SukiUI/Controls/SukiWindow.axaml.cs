using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using System;
using Avalonia.Collections;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using SukiUI.Enums;
using System.Runtime.InteropServices;
using Avalonia.Layout;

namespace SukiUI.Controls;

public class SukiWindow : Window
{
    protected override Type StyleKeyOverride => typeof(SukiWindow);

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
        AvaloniaProperty.Register<Border, CornerRadius>(nameof(RootCornerRadius));

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
    private int _canMinimize = 0; // 0: uninitialized/notset, 1: true, 2: false

    public static readonly StyledProperty<bool> ShowTitlebarBackgroundProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(ShowTitlebarBackground), defaultValue: true);
    public bool ShowTitlebarBackground
    {
        get => GetValue(ShowTitlebarBackgroundProperty);
        set => SetValue(ShowTitlebarBackgroundProperty, value);
    }

    public static readonly StyledProperty<bool> CanMaximizeProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(CanMaximize), defaultValue: true);
    public bool CanMaximize
    {
        get => GetValue(CanMaximizeProperty);
        set => SetValue(CanMaximizeProperty, value);
    }
    private int _canMaximize = 0; // 0: uninitialized/notset, 1: true, 2: false

    public static readonly StyledProperty<bool> CanMoveProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(CanMove), defaultValue: true);

    public bool CanMove
    {
        get => GetValue(CanMoveProperty);
        set => SetValue(CanMoveProperty, value);
    }
    private int _canMove = 0; // 0: uninitialized/notset, 1: true, 2: false

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

    public SukiWindow()
    {
        MenuItems = new AvaloniaList<MenuItem>();
        RightWindowTitleBarControls = new Avalonia.Controls.Controls();
        Hosts = new Avalonia.Controls.Controls();
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WindowStateProperty && change.NewValue is WindowState windowState)
            OnWindowStateChanged(windowState);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // save the initial values of CanMaximize, CanMinimize, and CanMove
        if (_canMaximize == 0)
            _canMaximize = CanMaximize == true ? 1 : 2;
        if (_canMinimize == 0)
            _canMinimize = CanMinimize == true ? 1 : 2;
        if (_canMove == 0)
            _canMove = CanMove == true ? 1 : 2;

        OnWindowStateChanged(WindowState);
        try
        {
            // Create handlers for buttons
            if (e.NameScope.Get<Button>("PART_MaximizeButton") is { } maximize)
            {
                maximize.Click += OnMaximizeButtonClicked;
                EnableWindowsSnapLayout(maximize);
            }

            if (e.NameScope.Get<Button>("PART_MinimizeButton") is { } minimize)
                minimize.Click += (_, _) => WindowState = WindowState.Minimized;

            if (e.NameScope.Get<Button>("PART_CloseButton") is { } close)
                close.Click += (_, _) => Close();

            if (e.NameScope.Get<GlassCard>("PART_TitleBarBackground") is { } titleBar)
            {
                titleBar.PointerPressed += OnTitleBarPointerPressed;
                titleBar.DoubleTapped += OnMaximizeButtonClicked;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (CanResize && e.NameScope.Get<Panel>("PART_Root") is { } rootPanel)
                {
                    AddResizeGripForLinux(rootPanel);
                }
                if (RootCornerRadius == null)
                {
                    RootCornerRadius = new CornerRadius(10);
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private void OnMaximizeButtonClicked(object? sender, RoutedEventArgs args)
    {
        if (!CanMaximize) return;
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

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

        Win32Properties.AddWndProcHookCallback(this, new Win32Properties.CustomWndProcHookCallback(proc));
    }

    private void OnWindowStateChanged(WindowState state)
    {
        if (state == WindowState.FullScreen)
        {
            // Disable window control capabilities
            _canMaximize = CanMaximize == true ? 1 : 2;
            CanMaximize = false;
            _canMinimize = CanMinimize == true ? 1 : 2;
            CanMinimize = false;
            _canMove = CanMove == true ? 1 : 2;
            CanMove = false;
        }
        else
        {
            // Restore window control capabilities
            CanMaximize = _canMaximize == 1 ? true : false;
            CanMinimize = _canMinimize == 1 ? true : false;
            CanMove = _canMove == 1 ? true : false;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) // only for windows platform
        {
            if (state == WindowState.Maximized)
                Margin = new Thickness(7);
            else
                Margin = new Thickness(0);
        }
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!CanMove)
            return;
        base.OnPointerPressed(e);
        BeginMoveDrag(e);
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
            rootPanel.Children.Add(border);
        }
    }

    private void RaiseResize(object? sender, PointerPressedEventArgs e)
    {
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

        window?.BeginResizeDrag(windowEdge, e);
        e.Handled = true;
    }
}