using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace SukiUI.Demo.Common;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        var fullName = data?.GetType().FullName;
        if (fullName is null) 
            return new TextBlock {Text = "Data is null or has no name."};
        var name = fullName.Replace("ViewModel", "View");
        var type = Type.GetType(name);
        if (type is null) 
            return new TextBlock { Text = $"No View For {name}." };
        
        if (data is not IViewAware viewAware) 
            return (Control)Activator.CreateInstance(type)!;
        viewAware.View ??= (Control)Activator.CreateInstance(type)!;
        return viewAware.View;
    }

    public bool Match(object? data) => data is INotifyPropertyChanged;
}