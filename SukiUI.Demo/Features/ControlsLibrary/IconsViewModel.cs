using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Media;
using Material.Icons;
using SukiUI.Content;

namespace SukiUI.Demo.Features.ControlsLibrary;

public class IconsViewModel : DemoPageBase
{
    public Dictionary<string, StreamGeometry> AllIcons { get; }

    public IconsViewModel() : base("Icons", MaterialIconKind.SimpleIcons, int.MaxValue)
    {
        AllIcons = typeof(Icons)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.FieldType == typeof(StreamGeometry))
            .OrderBy(x => x.Name)
            .ToDictionary(x => x.Name, y => (StreamGeometry)y.GetValue(null)!);
    }
}