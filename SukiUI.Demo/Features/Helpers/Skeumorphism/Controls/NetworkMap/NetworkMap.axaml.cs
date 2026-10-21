using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Skeumo-Suki local network: a router, a switch, three computers and a printer, all plugged into the switch.
/// Display only: it plays the <see cref="NetworkScript"/> it is given (packets glide along the cables, the switch lights
/// the ports they use, a broadcast spreads like a ripple) and reports its progress. It knows nothing about protocols.
/// Drawing and animation live in <c>NetworkMap.Rendering.cs</c>.
/// </summary>
public partial class NetworkMap : UserControl
{
    /// <summary>Scenario to play. Setting a new one restarts the map, setting null stops it and clears the scene.</summary>
    public static readonly StyledProperty<NetworkScript?> ScriptProperty =
        AvaloniaProperty.Register<NetworkMap, NetworkScript?>(nameof(Script));

    /// <summary>Index of the step being played (-1 when nothing is playing). Reported by the map.</summary>
    public static readonly StyledProperty<int> StepIndexProperty =
        AvaloniaProperty.Register<NetworkMap, int>(nameof(StepIndex), -1, defaultBindingMode: BindingMode.OneWayToSource);

    /// <summary>True while a scenario is playing. Reported by the map.</summary>
    public static readonly StyledProperty<bool> IsPlayingProperty =
        AvaloniaProperty.Register<NetworkMap, bool>(nameof(IsPlaying), defaultBindingMode: BindingMode.OneWayToSource);

    public NetworkScript? Script
    {
        get => GetValue(ScriptProperty);
        set => SetValue(ScriptProperty, value);
    }

    public int StepIndex
    {
        get => GetValue(StepIndexProperty);
        set => SetValue(StepIndexProperty, value);
    }

    public bool IsPlaying
    {
        get => GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    public NetworkMap()
    {
        InitializeComponent();
        InitializeRendering();
    }
}
