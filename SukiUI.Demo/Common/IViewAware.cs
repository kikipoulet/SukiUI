using Avalonia.Controls;

namespace SukiUI.Demo.Common;

public interface IViewAware
{
    public Control? View { get; set; }
}