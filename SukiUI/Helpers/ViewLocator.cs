using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace SukiUI.Helpers;

internal static class ViewLocator
{
    private static IDataTemplate? _locator;

    /// <summary>
    /// Tries to build a suitable control using an appropriate DataTemplate provided by the App.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>A valid control provided by a suitable ViewLocator if available, otherwise returns a TextBlock.</returns>
    internal static Control TryBuild(object? data)
    {
        _locator ??= Application.Current?.DataTemplates.FirstOrDefault();
        return _locator?.Build(data) ?? new TextBlock() { Text= $"Unable to find suitable view for {data?.GetType().Name}"};
    }
}