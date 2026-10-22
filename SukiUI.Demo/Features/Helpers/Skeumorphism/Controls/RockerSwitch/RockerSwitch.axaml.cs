using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Skeumo-Suki wall rocker switch (a light switch): the paddle rocks to "I" or "O" and a neon pilot glows when on.
/// A click, Space or Enter flips it. Two terminals at the bottom take the wires. It only reports the chosen state.
/// Drawing and animation live in <c>RockerSwitch.Rendering.cs</c>.
/// </summary>
public partial class RockerSwitch : UserControl
{
    public static readonly StyledProperty<bool> IsOnProperty =
        AvaloniaProperty.Register<RockerSwitch, bool>(nameof(IsOn), defaultBindingMode: BindingMode.TwoWay);

    public bool IsOn
    {
        get => GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

    public RockerSwitch()
    {
        InitializeComponent();
        InitializeRendering();
    }
}
