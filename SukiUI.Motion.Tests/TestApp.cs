using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using SukiUI.Motion.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]
// The virtual clock is a process-wide static seam: tests must not run concurrently.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace SukiUI.Motion.Tests;

public sealed class TestApp : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApp>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = true });
}
