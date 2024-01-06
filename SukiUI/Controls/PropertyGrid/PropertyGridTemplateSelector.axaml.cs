using Avalonia.Controls;
using Avalonia.Controls.Templates;

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
}