using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using SukiUI.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class TextViewModel() : DemoPageBase("Text", MaterialIconKind.Text)
{
    private const string DefaultText = "Hello, World!";
    [ObservableProperty] private string _textBoxValue = DefaultText;
    [ObservableProperty] private string _textBlockValue = DefaultText;

    public void HyperlinkClicked()
    {
        SukiHost.ShowToast("Clicked a hyperlink", "You clicked the hyperlink on the Text page.");
    }
}