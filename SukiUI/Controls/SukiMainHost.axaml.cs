using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using SukiUI.Enums;

namespace SukiUI.Controls;

[TemplatePart("PART_Root", typeof(Panel))]
[TemplatePart("PART_VisualLayerManager", typeof(VisualLayerManager))]
[TemplatePart("PART_Background", typeof(SukiBackground))]
public class SukiMainHost : ContentControl
{
    // Background properties
    public static readonly StyledProperty<bool> BackgroundAnimationEnabledProperty =
        AvaloniaProperty.Register<SukiMainHost, bool>(nameof(BackgroundAnimationEnabled), defaultValue: false);

    /// <inheritdoc cref="SukiBackground.AnimationEnabled"/>
    public bool BackgroundAnimationEnabled
    {
        get => GetValue(BackgroundAnimationEnabledProperty);
        set => SetValue(BackgroundAnimationEnabledProperty, value);
    }

    public static readonly StyledProperty<SukiBackgroundStyle> BackgroundStyleProperty =
        AvaloniaProperty.Register<SukiMainHost, SukiBackgroundStyle>(nameof(BackgroundStyle),
            defaultValue: SukiBackgroundStyle.GradientSoft);

    /// <inheritdoc cref="SukiBackground.Style"/>
    public SukiBackgroundStyle BackgroundStyle
    {
        get => GetValue(BackgroundStyleProperty);
        set => SetValue(BackgroundStyleProperty, value);
    }

    public static readonly StyledProperty<string?> BackgroundShaderFileProperty =
        AvaloniaProperty.Register<SukiMainHost, string?>(nameof(BackgroundShaderFile));

    /// <inheritdoc cref="SukiBackground.ShaderFile"/>
    public string? BackgroundShaderFile
    {
        get => GetValue(BackgroundShaderFileProperty);
        set => SetValue(BackgroundShaderFileProperty, value);
    }

    public static readonly StyledProperty<string?> BackgroundShaderCodeProperty =
        AvaloniaProperty.Register<SukiMainHost, string?>(nameof(BackgroundShaderCode));

    /// <inheritdoc cref="SukiBackground.ShaderCode"/>
    public string? BackgroundShaderCode
    {
        get => GetValue(BackgroundShaderCodeProperty);
        set => SetValue(BackgroundShaderCodeProperty, value);
    }

    public static readonly StyledProperty<bool> BackgroundTransitionsEnabledProperty =
        AvaloniaProperty.Register<SukiMainHost, bool>(nameof(BackgroundTransitionsEnabled), defaultValue: false);

    /// <inheritdoc cref="SukiBackground.TransitionsEnabled"/>
    public bool BackgroundTransitionsEnabled
    {
        get => GetValue(BackgroundTransitionsEnabledProperty);
        set => SetValue(BackgroundTransitionsEnabledProperty, value);
    }

    public static readonly StyledProperty<double> BackgroundTransitionTimeProperty =
        AvaloniaProperty.Register<SukiMainHost, double>(nameof(BackgroundTransitionTime), defaultValue: 1.0);

    /// <inheritdoc cref="SukiBackground.TransitionTime"/>
    public double BackgroundTransitionTime
    {
        get => GetValue(BackgroundTransitionTimeProperty);
        set => SetValue(BackgroundTransitionTimeProperty, value);
    }

    public static readonly StyledProperty<bool> BackgroundForceSoftwareRenderingProperty =
        AvaloniaProperty.Register<SukiMainHost, bool>(nameof(BackgroundForceSoftwareRendering));

    /// <summary>
    /// Forces the background of the window to utilise software rendering.
    /// This prevents use of any advanced effects or animations and provides only a flat background colour that changes with the theme.
    /// </summary>
    public bool BackgroundForceSoftwareRendering
    {
        get => GetValue(BackgroundForceSoftwareRenderingProperty);
        set => SetValue(BackgroundForceSoftwareRenderingProperty, value);
    }

    public static readonly StyledProperty<Avalonia.Controls.Controls> HostsProperty =
        AvaloniaProperty.Register<SukiMainHost, Avalonia.Controls.Controls>(nameof(Hosts));

    /// <summary>
    /// These controls are displayed above all others and fill the entire window.
    /// You can include <see cref="SukiDialogHost"/> and <see cref="SukiToastHost"/> or create your own custom implementations.
    /// </summary>
    public Avalonia.Controls.Controls Hosts
    {
        get => GetValue(HostsProperty);
        set => SetValue(HostsProperty, value);
    }

    public SukiMainHost()
    {
        Hosts = [];
    }
}