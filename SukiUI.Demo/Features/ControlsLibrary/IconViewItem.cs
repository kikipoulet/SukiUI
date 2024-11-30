using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Demo.Services;
using SukiUI.Toasts;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class IconItemViewModel(ClipboardService clipboard, ISukiToastManager toastManager) : ObservableObject
{
    public required string Name { get; init; }

    public required Geometry Geometry { get; init; }

    [RelayCommand]
    public void OnClick()
    {
        clipboard.CopyToClipboard($"<PathIcon Data=\"{{x:Static content:Icons.{Name}}}\" />");

        toastManager
            .CreateSimpleInfoToast()
            .WithTitle("Copied To Clipboard")
            .WithContent($"Copied the XAML for {Name} to your clipboard.")
            .Queue();
    }
}