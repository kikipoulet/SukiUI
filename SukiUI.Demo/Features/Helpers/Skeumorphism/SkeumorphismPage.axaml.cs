using System;
using Avalonia.Controls;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism;

public partial class SkeumorphismPage : UserControl
{
    public SkeumorphismPage()
    {
        InitializeComponent();
    }

    private SkeumorphismViewModel? Vm => DataContext as SkeumorphismViewModel;

    private void Lever_OnReleased(object? sender, EventArgs e) => Vm?.CloseValves();

    private void Syringe_OnPositionRequested(object? sender, double position) => Vm?.RequestSyringePosition(position);

    private void Syringe_OnValveToggleRequested(object? sender, EventArgs e) => Vm?.ToggleValve();
}
