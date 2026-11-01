using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Skeumo-Suki industrial toggle switch with a safety cover. With <see cref="RequiresCover"/>, the first click lifts
/// the cover and only the next one throws the lever; the cover drops back by itself after a few idle seconds.
/// A click or Space acts; it only reports the chosen state. Drawing and animation live in <c>GuardedToggle.Rendering.cs</c>.
/// </summary>
public partial class GuardedToggle : UserControl
{
    /// <summary>Switch state: lever up = on, lever down = off.</summary>
    public static readonly StyledProperty<bool> IsOnProperty =
        AvaloniaProperty.Register<GuardedToggle, bool>(nameof(IsOn), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>When true, the safety cover must be lifted before the lever can be thrown.</summary>
    public static readonly StyledProperty<bool> RequiresCoverProperty =
        AvaloniaProperty.Register<GuardedToggle, bool>(nameof(RequiresCover), true);

    public static readonly StyledProperty<string?> OnLabelProperty =
        AvaloniaProperty.Register<GuardedToggle, string?>(nameof(OnLabel), "ON");

    public static readonly StyledProperty<string?> OffLabelProperty =
        AvaloniaProperty.Register<GuardedToggle, string?>(nameof(OffLabel), "OFF");

    public bool IsOn
    {
        get => GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

    public bool RequiresCover
    {
        get => GetValue(RequiresCoverProperty);
        set => SetValue(RequiresCoverProperty, value);
    }

    public string? OnLabel
    {
        get => GetValue(OnLabelProperty);
        set => SetValue(OnLabelProperty, value);
    }

    public string? OffLabel
    {
        get => GetValue(OffLabelProperty);
        set => SetValue(OffLabelProperty, value);
    }

    public GuardedToggle()
    {
        InitializeComponent();
        InitializeRendering();
    }
}
