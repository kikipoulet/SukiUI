using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SukiUI.Controls;
using SukiUI.Enums;
using SukiUI.Models;
using System.Runtime.CompilerServices;

namespace SukiUI.Demo;

public partial class SukiUIDemoView : SukiWindow
{
    public SukiUIDemoView()
    {
        InitializeComponent();

        if (RuntimeFeature.IsDynamicCodeCompiled == false)
        {
            Title += " (native)";
        }
    }

    private void ThemeMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SukiUIDemoViewModel vm) return;
        if (e.Source is not MenuItem mItem) return;
        if (mItem.DataContext is not SukiColorTheme cTheme) return;
        vm.ChangeTheme(cTheme);
    }

    private void BackgroundMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SukiUIDemoViewModel vm) return;
        if (e.Source is not MenuItem mItem) return;
        if (mItem.DataContext is not SukiBackgroundStyle cStyle) return;
        vm.BackgroundStyle = cStyle;
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        IsMenuVisible = !IsMenuVisible;
    }
}