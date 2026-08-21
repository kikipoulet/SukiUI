using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Toasts;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class ContextMenusViewModel(ISukiToastManager toastManager) : DemoPageBase("Context Menus", MaterialIconKind.Menu)
{
    /// <summary>
    /// Long enough to overflow the work area of a normal display, so the menu templates fall back to
    /// their scroll viewer and the up/down arrow <c>RepeatButton</c>s become visible.
    /// </summary>
    public string[] LongMenuItems { get; } = Enumerable.Range(1, 60)
        .Select(i => $"Item {i:00}")
        .ToArray();

    [RelayCommand]
    private void OptionClicked(bool withIcon)
    {
        toastManager.CreateSimpleInfoToast()
            .WithTitle("Clicked Context Menu")
            .WithContent(withIcon ? "You clicked the option with the icon." : "You clicked the option without the icon.")
            .Queue();
    }

    [RelayCommand]
    private void MenuActionClicked(string action)
    {
        toastManager.CreateSimpleInfoToast()
            .WithTitle("Clicked Context Menu")
            .WithContent($"You selected \"{action}\".")
            .Queue();
    }

    [RelayCommand]
    private void NestedOptionClicked()
    {
        toastManager.CreateSimpleInfoToast()
            .WithTitle("Clicked Context Menu")
            .WithContent("You clicked the nested option.")
            .Queue();
    }
}