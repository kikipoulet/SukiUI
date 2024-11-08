using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using SukiUI.Dialogs;

namespace SukiUI.Controls;

public partial class PropertyGridTemplateSelector : ResourceDictionary, IDataTemplate
{
    public SukiDialogHost? SukiDialogHost { get; set; }

    public bool UseSukiHost { get; set; } = true;

    public PropertyGridTemplateSelector()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

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

        return ContainsKey(key) != false;
    }

    private static void ShowSukiHostDialog(ISukiDialogManager manager, ComplexTypeViewModel viewModel)
    {
        manager
            .CreateDialog()
            .WithContent(new PropertyGridDialog()
            {
                DataContext = viewModel.Value
            })
            .WithTitle(viewModel.DisplayName)
            .Dismiss().ByClickingBackground()
            .TryShow();
    }

    private static async Task ShowWindowDialogAsync(Control control)
    {
        var root = control.GetVisualRoot();
        if (root is not Window parentWindow || control.DataContext is not ComplexTypeViewModel childViewModel || childViewModel.Value is null)
        {
            return;
        }

        var window = new PropertyGridWindow()
        {
            DataContext = childViewModel.Value,
            Title = childViewModel.DisplayName,
        };

        await window.ShowDialog(parentWindow);
    }

    protected virtual async void OnMoreInfoClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        var sukiDialogHost = SukiDialogHost;
        if (UseSukiHost)
        {
            if (sukiDialogHost is not null)
            {
                if (control.DataContext is not ComplexTypeViewModel childViewModel || childViewModel.Value is null)
                {
                    return;
                }

                ShowSukiHostDialog(sukiDialogHost.Manager, childViewModel);
            }
            else
            {
                var root = control.GetVisualRoot();
                if (root is not SukiWindow parentWindow || control.DataContext is not ComplexTypeViewModel childViewModel || childViewModel.Value is null)
                {
                    return;
                }

                sukiDialogHost = parentWindow.Hosts.Where(p => p is SukiDialogHost).Cast<SukiDialogHost>().FirstOrDefault();
                if (sukiDialogHost is not null)
                {
                    ShowSukiHostDialog(sukiDialogHost.Manager, childViewModel);
                }
                else
                {
                    await ShowWindowDialogAsync(control);
                }
            }
        }
        else
        {
            await ShowWindowDialogAsync(control);
        }
    }
}