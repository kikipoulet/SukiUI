using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Demo.Common;
using SukiUI.Demo.Features;
using SukiUI.Demo.Services;

namespace SukiUI.Demo;

public partial class App : Application
{
    private IServiceProvider _provider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        _provider = ConfigureServices();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewLocator = _provider.GetService<IDataTemplate>();
            var mainVm = _provider.GetService<SukiUIDemoViewModel>();

            desktop.MainWindow = (Window)viewLocator.Build(mainVm);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton(Current.DataTemplates.First(x => x is ViewLocator));
        services.AddSingleton<PageNavigationService>();
        
        // Viewmodels
        services.AddSingleton<SukiUIDemoViewModel>();
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => !p.IsAbstract && typeof(DemoPageBase).IsAssignableFrom(p));
        foreach (var type in types)
            services.AddSingleton(typeof(DemoPageBase), type);

        return services.BuildServiceProvider();
    }
}