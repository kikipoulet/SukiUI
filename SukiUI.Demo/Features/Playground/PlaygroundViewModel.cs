using System;
using System.Collections.ObjectModel;
using Avalonia.Controls.Notifications;
using Material.Icons;
using SukiUI.Demo.Utilities;
using SukiUI.Toasts;

namespace SukiUI.Demo.Features.Playground;

internal class PlaygroundViewModel(ISukiToastManager toastManager) : DemoPageBase("Playground", MaterialIconKind.Pencil, -150)
{
    public ObservableCollection<string> ButtonsElements { get; init; } =
    [
        XamlData.Buttons["ButtonFlat"],
        XamlData.Buttons["ButtonFlatRounded"],
        XamlData.Buttons["ButtonFlatLarge"],
        XamlData.Buttons["ButtonBasic"],
        XamlData.Buttons["ButtonBasicAccent"],
        XamlData.Buttons["ButtonNeutral"],
        XamlData.Buttons["ButtonOutlined"]
    ];

    public ObservableCollection<string> InputsElements { get; init; } =
    [
        XamlData.Inputs["TextBox"],
        XamlData.Inputs["ToggleSwitch"],
        XamlData.Inputs["ToggleButton"],
        XamlData.Inputs["Slider"],
        XamlData.Inputs["ComboBox"],
        XamlData.Inputs["CheckBox"],
        XamlData.Inputs["RadioButton"],
        XamlData.Inputs["NumericUpDown"],
        XamlData.Inputs["DatePicker"]
    ];

    public ObservableCollection<string> ProgressElements { get; init; } =
    [
        XamlData.Progress["WaveProgress"],
        XamlData.Progress["Stepper"],
        XamlData.Progress["CircleProgressBar"],
        XamlData.Progress["StepperAlternativeStyle"],
        XamlData.Progress["CircleProgressBarIndeterminate"],
        XamlData.Progress["Loading"],
        XamlData.Progress["ProgressBar60"],
        XamlData.Progress["ProgressBar50WithProgressText"],
        XamlData.Progress["ProgressBarIndeterminate"]
    ];

    public ObservableCollection<string> ListsElements { get; init; } =
    [
        XamlData.Lists["ListBox"],
        XamlData.Lists["TreeView"]
    ];

    public ObservableCollection<string> LayoutElements { get; init; } =
    [
        XamlData.Layout["GlassCard"],
        XamlData.Layout["GroupBox"],
        XamlData.Layout["Expander"],
        XamlData.Layout["TabControl"]
    ];

    public void DisplayError(string message)
    {
        toastManager.CreateToast()
            .OfType(NotificationType.Error)
            .WithTitle("Playground Error")
            .WithContent(message)
            .Dismiss().After(TimeSpan.FromSeconds(5))
            .Dismiss().ByClicking()
            .Queue();
    }
}