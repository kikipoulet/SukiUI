using System;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using SukiUI.Helpers;

namespace SukiUI.Controls;

public class GlassCard : ContentControl
{
    private ContextMenu? _attachedContextMenu;
    private Panel? _rootPanel;
    private Border? _cardBorder;
    private Border? _legacyCardBorder;
    private Border? _clipBorder;
    private bool _animationsEnabled;

    public new static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<GlassCard, CornerRadius>(nameof(CornerRadius), new CornerRadius(20));

    public new CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public new static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<GlassCard, Thickness>(nameof(BorderThickness), new Thickness(1));

    public new Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }
    
 
    public static readonly StyledProperty<bool> IsAnimatedProperty =
        AvaloniaProperty.Register<GlassCard, bool>(nameof(IsAnimated), true);

    public bool IsAnimated
    {
        get => GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsOpaqueProperty =
        AvaloniaProperty.Register<GlassCard, bool>(nameof(IsOpaque), false);

    public bool IsOpaque
    {
        get => GetValue(IsOpaqueProperty);
        set => SetValue(IsOpaqueProperty, value);
    }

    public static readonly StyledProperty<bool> IsInteractiveProperty = AvaloniaProperty.Register<GlassCard, bool>(nameof(IsInteractive));

    public bool IsInteractive
    {
        get => GetValue(IsInteractiveProperty);
        set => SetValue(IsInteractiveProperty, value);
    }

    public static readonly StyledProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<GlassCard, ICommand?>(nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<object?> CommandParameterProperty = AvaloniaProperty.Register<GlassCard, object?>(nameof(CommandParameter));

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachContextMenu(ContextMenu);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        AttachContextMenu(null);
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContextMenuProperty)
            AttachContextMenu(change.NewValue as ContextMenu);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _rootPanel = e.NameScope.Find<Panel>("RootPanel");
        _cardBorder = e.NameScope.Find<Border>("PART_BorderCard") ??
                      e.NameScope.Find<Border>("PART_BorderCardLight");
        _legacyCardBorder = e.NameScope.Find<Border>("PART_BorderCardDark");
        if (ReferenceEquals(_legacyCardBorder, _cardBorder))
            _legacyCardBorder = null;
        _clipBorder = e.NameScope.Find<Border>("PART_ClipBorder");
        _animationsEnabled = IsAnimated;

        if (IsLoaded)
            ConfigureAnimations();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureAnimations();
    }

    private void ConfigureAnimations()
    {
        if (!_animationsEnabled)
            return;

        MakeOpacityAnimated(_rootPanel);
        MakeSizeAnimated(_cardBorder);
        MakeSizeAnimated(_legacyCardBorder);
        MakeSizeAnimated(_clipBorder);
    }

    private static void MakeOpacityAnimated(Control? control)
    {
        if (control is not null && ElementComposition.GetElementVisual(control) is { } visual)
            CompositionAnimationHelper.MakeOpacityAnimated(visual);
    }

    private static void MakeSizeAnimated(Control? control)
    {
        if (control is not null && ElementComposition.GetElementVisual(control) is { } visual)
            CompositionAnimationHelper.MakeSizeAnimated(visual);
    }

    private void AttachContextMenu(ContextMenu? contextMenu)
    {
        if (_attachedContextMenu is not null)
            _attachedContextMenu.Opening -= ContextMenuOnOpening;
        _attachedContextMenu = contextMenu;
        if (_attachedContextMenu is not null)
            _attachedContextMenu.Opening += ContextMenuOnOpening;
    }
    



    private void ContextMenuOnOpening(object? sender, CancelEventArgs e)
    {
        PseudoClasses.Set(":pointerdown", false);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        PseudoClasses.Set(":pointerdown", true);
        if(IsInteractive && Command is not null && Command.CanExecute(CommandParameter))
            Command.Execute(CommandParameter);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        PseudoClasses.Set(":pointerdown", false);
    }
}

/// <summary>
/// Border used by the <see cref="GlassCard"/> template. Renders either <see cref="LightBorderBrush"/>
/// or a lazily built liquid gradient (<see cref="DarkBorderStartColor"/>/<see cref="DarkBorderEndColor"/>)
/// depending on <see cref="UseDarkBorder"/>, so a single border serves both theme variants.
/// </summary>
public sealed class GlassCardBorder : Border
{
    public static readonly StyledProperty<IBrush?> LightBorderBrushProperty =
        AvaloniaProperty.Register<GlassCardBorder, IBrush?>(nameof(LightBorderBrush));

    public static readonly StyledProperty<bool> UseDarkBorderProperty =
        AvaloniaProperty.Register<GlassCardBorder, bool>(nameof(UseDarkBorder));

    public static readonly StyledProperty<Color> DarkBorderStartColorProperty =
        AvaloniaProperty.Register<GlassCardBorder, Color>(nameof(DarkBorderStartColor));

    public static readonly StyledProperty<Color> DarkBorderEndColorProperty =
        AvaloniaProperty.Register<GlassCardBorder, Color>(nameof(DarkBorderEndColor));

    private LinearGradientBrush? _darkBorderBrush;
    private GradientStop? _darkBorderStart;
    private GradientStop? _darkBorderEnd;
    private Size _gradientSize = new(double.NaN, double.NaN);

    // Selectors match on StyleKey, so this keeps the "/template/ Border.GlassCardBorderPartCard"
    // styles in GlassCard.axaml (.Accent, .Primary, IsOpaque, ...) matching this control.
    // Removing it silently disables all of them.
    protected override Type StyleKeyOverride => typeof(Border);

    public IBrush? LightBorderBrush
    {
        get => GetValue(LightBorderBrushProperty);
        set => SetValue(LightBorderBrushProperty, value);
    }

    public bool UseDarkBorder
    {
        get => GetValue(UseDarkBorderProperty);
        set => SetValue(UseDarkBorderProperty, value);
    }

    public Color DarkBorderStartColor
    {
        get => GetValue(DarkBorderStartColorProperty);
        set => SetValue(DarkBorderStartColorProperty, value);
    }

    public Color DarkBorderEndColor
    {
        get => GetValue(DarkBorderEndColorProperty);
        set => SetValue(DarkBorderEndColorProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == UseDarkBorderProperty)
        {
            UpdateBorderBrush(suppressTransition: true);
        }
        else if (change.Property == LightBorderBrushProperty && !UseDarkBorder)
        {
            UpdateBorderBrush(suppressTransition: false);
        }
        else if (change.Property == DarkBorderStartColorProperty && _darkBorderStart is not null)
        {
            _darkBorderStart.Color = DarkBorderStartColor;
        }
        else if (change.Property == DarkBorderEndColorProperty && _darkBorderEnd is not null)
        {
            _darkBorderEnd.Color = DarkBorderEndColor;
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (UseDarkBorder)
            UpdateGradientGeometry(finalSize);
        return base.ArrangeOverride(finalSize);
    }

    private void UpdateBorderBrush(bool suppressTransition)
    {
        var suspendTransitions = suppressTransition && IsLoaded && Transitions is not null;
        var transitions = suspendTransitions ? Transitions : null;
        if (suspendTransitions)
            Transitions = null;

        try
        {
            if (UseDarkBorder)
            {
                SetCurrentValue(BorderBrushProperty, GetDarkBorderBrush());
                UpdateGradientGeometry(Bounds.Size);
            }
            else
            {
                SetCurrentValue(BorderBrushProperty, LightBorderBrush);
            }
        }
        finally
        {
            if (suspendTransitions)
                Transitions = transitions;
        }
    }

    private LinearGradientBrush GetDarkBorderBrush()
    {
        if (_darkBorderBrush is not null)
            return _darkBorderBrush;

        _darkBorderStart = new GradientStop
        {
            Offset = 0,
            Color = DarkBorderStartColor
        };
        _darkBorderEnd = new GradientStop
        {
            Offset = 1,
            Color = DarkBorderEndColor
        };
        _darkBorderBrush = new LinearGradientBrush
        {
            GradientStops =
            [
                _darkBorderStart,
                new GradientStop { Offset = 0.5, Color = Colors.Transparent },
                _darkBorderEnd
            ]
        };
        return _darkBorderBrush;
    }

    private void UpdateGradientGeometry(Size size)
    {
        if (_darkBorderBrush is null || size == _gradientSize)
            return;

        var min = Math.Min(size.Width, size.Height);
        var max = Math.Max(size.Width, size.Height);
        if (!double.IsFinite(min) || !double.IsFinite(max) || min <= 0 || max <= 0)
            return;

        _gradientSize = size;
        var factor = Math.Abs(min / max);
        var y = 1 / (1.75 * factor);
        _darkBorderBrush.StartPoint = new RelativePoint(0.2, -y, RelativeUnit.Relative);
        _darkBorderBrush.EndPoint = new RelativePoint(0.8, 1 + y, RelativeUnit.Relative);
    }
}
