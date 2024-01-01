using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SukiUI.Demo.Common;

public abstract class ViewAwareObservableObject : ObservableObject, IViewAware
{
    public Control? View { get; set; }
}