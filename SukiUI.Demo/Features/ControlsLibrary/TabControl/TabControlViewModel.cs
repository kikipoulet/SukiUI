using System.Linq;
using Avalonia.Collections;
using Material.Icons;

namespace SukiUI.Demo.Features.ControlsLibrary.TabControl;

public class TabControlViewModel : DemoPageBase
{
    public AvaloniaList<TabViewModel> Tabs { get; } = [];
        
    public TabControlViewModel() : base("Tabs", MaterialIconKind.Tab)
    {
        Tabs.AddRange(Enumerable.Range(1, 5).Select(x => new TabViewModel(x)));
        Tabs.Add(new TabViewModel(6, false));
    }
}