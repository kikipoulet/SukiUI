using Avalonia.Media;
using Material.Icons;
using SukiUI.Content;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Demo.Services;
using SukiUI.Toasts;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class IconsViewModel : DemoPageBase
{
    public Dictionary<string, StreamGeometry> AllIcons { get; }

    private readonly ClipboardService _clipboardService;
    private readonly ISukiToastManager _toastManager;
    
    public IconsViewModel(ClipboardService clipboardService, ISukiToastManager toastManager) : base("Icons", MaterialIconKind.AlphaICircleOutline, int.MaxValue)
    {
        _clipboardService = clipboardService;
        _toastManager = toastManager;
        AllIcons = typeof(Icons)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.FieldType == typeof(StreamGeometry))
            .OrderBy(x => x.Name)
            .ToDictionary(x => x.Name, y => (StreamGeometry)y.GetValue(null)!);
    }

    [RelayCommand]
    private void OnIconClicked(string iconName)
    {
        _clipboardService.CopyToClipboard($"<PathIcon Data=\"{{x:Static content:Icons.{iconName}}}\" />");
        _toastManager.CreateSimpleInfoToast()
            .WithTitle("Copied To Clipboard")
            .WithContent($"Copied the XAML for {iconName} to your clipboard.")
            .Queue();
    }
}