using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Controls;
using SukiUI.Demo.Common;
using SukiUI.Demo.Features.ControlsLibrary;
using SukiUI.Demo.Features.ControlsLibrary.Colors;
using SukiUI.Demo.Features.ControlsLibrary.Dialogs;
using SukiUI.Demo.Features.ControlsLibrary.DockControls;
using SukiUI.Demo.Features.ControlsLibrary.StackPage;
using SukiUI.Demo.Features.ControlsLibrary.TabControl;
using SukiUI.Demo.Features.ControlsLibrary.Toasts;
using SukiUI.Demo.Features.CustomTheme;
using SukiUI.Demo.Features.Dashboard;
using SukiUI.Demo.Features.Effects;
using SukiUI.Demo.Features.Helpers;
using SukiUI.Demo.Features.Playground;
using SukiUI.Demo.Features.Splash;
using SukiUI.Demo.Features.Theming;
using SukiUI.Demo.Services;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace SukiUI.Demo;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

    }

    public override void OnFrameworkInitializationCompleted()
    {

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            services.AddSingleton(desktop);
            var views = ConfigureViews(services);
            var provider = ConfigureServices(services);
            DataTemplates.Add(new ViewLocator(views));
            desktop.MainWindow = views.CreateView<SukiUIDemoViewModel>(provider) as Window;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            var services = new ServiceCollection();
            services.AddSingleton(singleView);
            var views = ConfigureViews(services);
            var provider = ConfigureServices(services);
            DataTemplates.Add(new ViewLocator(views));


            // Ideally, we want to create a MainView that host app content
            // and use it for both IClassicDesktopStyleApplicationLifetime and ISingleViewApplicationLifetime
            singleView.MainView = new SukiMainHost()
            {
                Hosts = [
                    new SukiDialogHost
                    {
                        Manager = new SukiDialogManager()
                    }
                ],
                Content = views.CreateView<DialogViewModel>(provider)
            };
        }

        base.OnFrameworkInitializationCompleted();

    //    Shadcn.Configure(Application.Current, ThemeVariant.Dark);
    }

    private static SukiViews ConfigureViews(ServiceCollection services)
    {
        return new SukiViews()

            // Add main view
            .AddView<SukiUIDemoView, SukiUIDemoViewModel>(services)

            // Add pages
            .AddView<SplashView, SplashViewModel>(services)
            .AddView<ThemingView, ThemingViewModel>(services)
            .AddView<PlaygroundView, PlaygroundViewModel>(services)
            .AddView<EffectsView, EffectsViewModel>(services)
            .AddView<DashboardView, DashboardViewModel>(services)
            .AddView<ButtonsView, ButtonsViewModel>(services)
            .AddView<CardsView, CardsViewModel>(services)
            .AddView<CollectionsView, CollectionsViewModel>(services)
            .AddView<ContextMenusView, ContextMenusViewModel>(services)
            .AddView<DockView, DockViewModel>(services)
            .AddView<DockMvvmView, DockMvvmViewModel>(services)
            .AddView<ExpanderView, ExpanderViewModel>(services)
            .AddView<IconsView, IconsViewModel>(services)
            .AddView<InfoBarView, InfoBarViewModel>(services)
            .AddView<MiscView, MiscViewModel>(services)
            .AddView<ProgressView, ProgressViewModel>(services)
            .AddView<PropertyGridView, PropertyGridViewModel>(services)
            .AddView<TextView, TextViewModel>(services)
            .AddView<TogglesView, TogglesViewModel>(services)
            .AddView<ToastsView, ToastsViewModel>(services)
            .AddView<TabControlView, TabControlViewModel>(services)
            .AddView<StackPageView, StackPageViewModel>(services)
            .AddView<DialogsView, DialogsViewModel>(services)
            .AddView<HelpersView, HelpersViewModel>(services)
            .AddView<ColorsView, ColorsViewModel>(services)
            .AddView<ExperimentalView, ExperimentalViewModel>(services)

            // Add docks view for DockMvvvm
            .AddView<DocumentText, DocumentTextViewModel>(services)
            .AddView<ErrorList, ErrorListViewModel>(services)
            .AddView<OutputView, OutputViewModel>(services)
            .AddView<PropertiesView, PropertiesViewModel>(services)
            .AddView<SolutionExplore, SolutionExploreViewModel>(services)

            // Add additional views
            .AddView<DialogView, DialogViewModel>(services)
            .AddView<VmDialogView, VmDialogViewModel>(services)
            .AddView<RecursiveView, RecursiveViewModel>(services)
            .AddView<CustomThemeDialogView, CustomThemeDialogViewModel>(services);
    }

    private static ServiceProvider ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<ClipboardService>();
        services.AddSingleton<PageNavigationService>();
        services.AddSingleton<ISukiToastManager, SukiToastManager>();
        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();

        return services.BuildServiceProvider();
    }
}