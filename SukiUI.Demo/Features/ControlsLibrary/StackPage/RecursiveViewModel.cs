using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary.StackPage;

public partial class RecursiveViewModel(int index, Action<RecursiveViewModel> onRecurseClicked)
    : ObservableObject, ISukiStackPageTitleProvider
{
    public string Title { get; } = $"Recursive {index}";

    [RelayCommand]
    private void Recurse() => 
        onRecurseClicked.Invoke(new RecursiveViewModel(index + 1, onRecurseClicked));
}