using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;

[assembly: AvaloniaTestApplication(typeof(SukiUI.ConditionalXAML.Tests.TestAppBuilder))]

namespace SukiUI.ConditionalXAML.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<Application>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
