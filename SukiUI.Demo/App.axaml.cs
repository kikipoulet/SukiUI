using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Demo.Common;
using SukiUI.Demo.Features;
using SukiUI.Demo.Services;
using System;
using System.Linq;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace SukiUI.Demo;

public class App : Application
{
    private IServiceProvider? _provider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        _provider = ConfigureServices();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewLocator = _provider?.GetRequiredService<IDataTemplate>();
            var mainViewModel = _provider?.GetRequiredService<SukiUIDemoViewModel>();
            var mainView = _provider?.GetRequiredService<SukiUIDemoView>();
            mainView.DataContext = mainViewModel;
            desktop.MainWindow = mainView;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices()
    {
        var viewLocator = Current?.DataTemplates.First(x => x is ViewLocator);
        var services = new ServiceCollection();

        // Views
        services.AddSingleton<SukiUIDemoView>();
        
        // Services
        if (viewLocator is not null)
            services.AddSingleton(viewLocator);
        services.AddSingleton<PageNavigationService>();
        services.AddSingleton<ClipboardService>();
        services.AddSingleton<ISukiToastManager, SukiToastManager>();
        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();

        // ViewModels
        services.AddSingleton<SukiUIDemoViewModel>();
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => !p.IsAbstract && typeof(DemoPageBase).IsAssignableFrom(p));
        foreach (var type in types)
            services.AddSingleton(typeof(DemoPageBase), type);

        return services.BuildServiceProvider();
    }
}