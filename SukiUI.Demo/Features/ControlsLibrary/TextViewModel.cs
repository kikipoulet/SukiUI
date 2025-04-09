using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using System.Threading.Tasks;
using SukiUI.Toasts;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class TextViewModel(ISukiToastManager toastManager) : DemoPageBase("Text", MaterialIconKind.Text)
{
    private const string DefaultText = "Hello, World!";

    [ObservableProperty] private string _textBoxValue = DefaultText;
    [ObservableProperty] private string _textBlockValue = DefaultText;
    [ObservableProperty] private string _SelectableTextBlockValue = DefaultText;
    [ObservableProperty] private bool _hyperlinkVisited;

    [RelayCommand]
    private void HyperlinkClicked()
    {
        HyperlinkVisited = true;
        toastManager.CreateToast()
            .WithTitle("Clicked Hyperlink")
            .WithContent("You clicked the hyperlink on the Text page.")
            .Dismiss().After(TimeSpan.FromSeconds(5))
            .Dismiss().ByClicking()
            .Queue();
    }
}