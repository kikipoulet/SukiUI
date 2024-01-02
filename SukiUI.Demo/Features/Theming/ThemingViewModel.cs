using System.Collections.ObjectModel;
using Avalonia.Collections;
using Material.Icons;
using SukiUI.Models;

namespace SukiUI.Demo.Features.Theming;

public class ThemingViewModel() : DemoPageBase("Theming", MaterialIconKind.PaletteOutline, -200)
{
    public IAvaloniaReadOnlyList<SukiColorTheme> AvailablesColors { get; } =  SukiTheme.GetInstance().ColorThemes;

}