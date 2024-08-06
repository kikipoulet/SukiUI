using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using System.Threading.Tasks;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class TextViewModel() : DemoPageBase("Text", MaterialIconKind.Text)
{
    private const string DefaultText = "Hello, World!";

    [ObservableProperty] private string _textBoxValue = DefaultText;
    [ObservableProperty] private string _textBlockValue = DefaultText;
    [ObservableProperty] private bool _hyperlinkVisited;

    [RelayCommand]
    private Task HyperlinkClicked()
    {
        HyperlinkVisited = true;
        return SukiHost.ShowToast("Clicked a hyperlink", "You clicked the hyperlink on the Text page.");
    }
}