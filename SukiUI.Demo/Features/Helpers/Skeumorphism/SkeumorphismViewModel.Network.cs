using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism;

// Network card: the protocol buttons choose a scenario, the NetworkMap plays it and reports its progress.
public partial class SkeumorphismViewModel
{
    private IReadOnlyList<NetworkProtocol>? _networkProtocols;

    public IReadOnlyList<NetworkProtocol> NetworkProtocols => _networkProtocols ??= NetworkScenarios.Create(RunProtocol);

    /// <summary>Scenario played by the map. Null stops it.</summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(NetworkCaption), nameof(NetworkBadge))] private NetworkScript? _networkScript;

    /// <summary>Step being played, reported by the map.</summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(NetworkCaption), nameof(NetworkBadge))] private int _networkStepIndex = -1;

    /// <summary>True while the map plays, reported by the map.</summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(NetworkBadge))] private bool _isNetworkPlaying;

    public string NetworkCaption =>
        NetworkScript is { } script && NetworkStepIndex >= 0 && NetworkStepIndex < script.Steps.Count
            ? script.Steps[NetworkStepIndex].Caption
            : "Pick a protocol to see what it does on the local network.";

    public string NetworkBadge =>
        NetworkScript is not { } script ? "Idle"
        : IsNetworkPlaying ? $"{script.Name} · {NetworkStepIndex + 1}/{script.Steps.Count}"
        : $"{script.Name} · done";

    // A playing protocol is stopped by its own button; nothing else starts during an animation.
    private void RunProtocol(NetworkProtocol protocol)
    {
        if (IsNetworkPlaying)
        {
            if (protocol.IsActive) NetworkScript = null;
            return;
        }

        protocol.IsActive = true;
        NetworkScript = protocol.CreateScript();
    }

    partial void OnIsNetworkPlayingChanged(bool value)
    {
        foreach (var protocol in NetworkProtocols)
        {
            if (!value) protocol.IsActive = false;
            protocol.IsBlocked = value && !protocol.IsActive;
        }
    }
}
