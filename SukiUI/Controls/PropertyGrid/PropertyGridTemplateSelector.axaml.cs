using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace SukiUI.Controls;

public partial class PropertyGridTemplateSelector : ResourceDictionary, IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
        {
            return null;
        }

        var type = param.GetType();
        var key = type.Name;

        if (TryGetResource(key, null, out var resource))
        {
            if (resource is IDataTemplate template)
            {
                return template.Build(param);
            }
        }

        return null;
    }

    public bool Match(object? data)
    {
        if (data is null)
        {
            return false;
        }

        var type = data.GetType();
        var key = type.Name;

        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        if (ContainsKey(key) == false)
        {
            return false;
        }

        return true;
    }

    private static async void OnMoreInfoClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        var root = control.GetVisualRoot();
        if (root is not Window parentWindow || control.DataContext is not ComplexTypeViewModel childViewModel || childViewModel.Value is null)
        {
            return;
        }

        var window = new PropertyGridWindow()
        {
            DataContext = new InstanceViewModel(childViewModel.Value)
        };

        await window.ShowDialog(parentWindow);
    }
}