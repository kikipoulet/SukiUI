using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using SukiUI.Animations;
using SukiUI.Controls.GlassMorphism;
using SukiUI.Demo;
using SukiUI.Demo.Features.ControlsLibrary.DockControls;
using SukiUI.Demo.Features.Helpers;
using SukiUI.Controls;
using SukiUI.Utilities.Effects;
using Xunit;

namespace SukiUI.PerformanceTests;

public sealed class DemoInteractionPerformanceTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public async Task Scrolls_a_dynamic_blur_scene_and_records_frame_costs()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(App));
        var result = await session.Dispatch(MeasureDynamicBlurScroll, CancellationToken.None);

        Assert.True(result.Offset > 0, "The scripted wheel input did not move the demo ScrollViewer.");
        WriteSummary("dynamic-blur-scroll", result.Samples, result.Offset);
    }

    [Fact]
    public async Task Drags_a_squishy_demo_control_and_records_frame_costs()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(App));
        var result = await session.Dispatch(MeasurePullingEffectDrag, CancellationToken.None);

        WriteSummary("pulling-effect-drag", result.Samples, result.Offset);
    }

    [Fact]
    public async Task Dock_solution_explorer_uses_a_bounded_demo_tree()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(App));
        var rootItems = await session.Dispatch(() => new SolutionExplore().FolderContents, CancellationToken.None);

        Assert.Collection(rootItems,
            item => Assert.Equal("SukiUI", item.Name),
            item => Assert.Equal("SukiUI.Demo", item.Name),
            item => Assert.Equal("tests", item.Name),
            item => Assert.Equal("SukiUI.sln", item.Name),
            item => Assert.Equal("Directory.Packages.props", item.Name));
        Assert.All(rootItems.Take(3), item => Assert.True(item.IsDirectory));
        Assert.All(rootItems.Skip(3), item => Assert.False(item.IsDirectory));
        Assert.Equal(14, CountItems(rootItems));
    }

    [Fact]
    public async Task Side_menu_keeps_previous_page_during_transition()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(App));
        await session.Dispatch(() =>
        {
            var firstPage = new Border();
            var secondPage = new Border();
            var firstItem = new SukiSideMenuItem { Header = "First", PageContent = firstPage };
            var secondItem = new SukiSideMenuItem { Header = "Second", PageContent = secondPage };
            var menu = new SukiSideMenu { Items = { firstItem, secondItem } };
            var window = CreateWindow(menu);

            try
            {
                menu.SelectedItem = firstItem;
                Render(window);
                menu.SelectedItem = secondItem;
                Render(window);

                var presentedPages = menu.GetVisualDescendants().OfType<ContentPresenter>()
                    .Select(static presenter => presenter.Content)
                    .Where(content => ReferenceEquals(content, firstPage) || ReferenceEquals(content, secondPage))
                    .ToArray();

                Assert.Contains(firstPage, presentedPages);
                Assert.Contains(secondPage, presentedPages);
            }
            finally
            {
                window.Close();
            }
        }, CancellationToken.None);
    }

    [Theory]
    [InlineData("cells")]
    [InlineData("waves")]
    public void Embedded_background_shader_compiles(string shaderName)
    {
        var effect = SukiEffect.FromEmbeddedResource(shaderName);

        Assert.NotNull(effect.Effect);
    }

    private static ScenarioResult MeasureDynamicBlurScroll()
    {
        var scene = CreateDynamicBlurScene();
        var window = CreateWindow(scene.Root);
        try
        {
            const int warmupFrames = 12;
            const int measuredFrames = 40;
            var cursor = new Point(window.Bounds.Width / 2, 100);

            for (var index = 0; index < warmupFrames; index++)
                DriveFrame(window, () => window.MouseWheel(cursor, new Vector(0, -120)));

            var samples = Enumerable.Range(0, measuredFrames)
                .Select(_ => DriveFrame(window, () => window.MouseWheel(cursor, new Vector(0, -120))))
                .ToArray();

            return new ScenarioResult(samples, scene.ScrollViewer.Offset.Y);
        }
        finally
        {
            window.Close();
        }
    }

    private static DynamicBlurScene CreateDynamicBlurScene()
    {
        var content = new StackPanel { Spacing = 12, Margin = new Thickness(32) };
        for (var index = 0; index < 120; index++)
        {
            content.Children.Add(new Border
            {
                Height = 72,
                CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(index % 2 == 0 ? Color.Parse("#244C6D") : Color.Parse("#805B3884")),
                Child = new TextBlock
                {
                    Margin = new Thickness(20),
                    FontSize = 18,
                    Text = $"Dynamic blur performance row {index + 1}"
                }
            });
        }

        var scrollViewer = new ScrollViewer { Content = content };
        var root = new Grid();
        root.Children.Add(scrollViewer);
        root.Children.Add(new BlurBackground
        {
            Width = 900,
            Height = 430,
            IsDynamic = true,
            IntensityFactor = 1,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            IsHitTestVisible = false
        });

        return new DynamicBlurScene(root, scrollViewer);
    }

    private static ScenarioResult MeasurePullingEffectDrag()
    {
        var view = new PullingEffect();
        var window = CreateWindow(view);
        try
        {
            var target = view.GetVisualDescendants().OfType<Control>()
                .First(static candidate => SquishyBehavior.GetEnable(candidate));
            var origin = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window);

            Assert.True(origin.HasValue, "The scripted control is not attached to the test window.");

            var start = origin.Value;
            window.MouseMove(start);
            window.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
            Render(window);

            const int measuredFrames = 40;
            var samples = Enumerable.Range(0, measuredFrames)
                .Select(index => DriveFrame(window, () => window.MouseMove(start + new Vector(index % 20, index % 12))))
                .ToArray();

            window.MouseUp(start + new Vector(20, 12), MouseButton.Left, RawInputModifiers.None);
            Render(window);

            return new ScenarioResult(samples, 0);
        }
        finally
        {
            window.Close();
        }
    }

    private static Window CreateWindow(Control content)
    {
        var window = new Window
        {
            Width = 1280,
            Height = 900,
            Content = content
        };

        window.Show();
        window.SetRenderScaling(2);
        Render(window, 2);
        return window;
    }

    private static FrameSample DriveFrame(Window window, Action input)
    {
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var startedAt = Stopwatch.GetTimestamp();

        input();
        Render(window);

        return new FrameSample(
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
            GC.GetAllocatedBytesForCurrentThread() - allocatedBefore);
    }

    private static void Render(Window window, int ticks = 1)
    {
        AvaloniaHeadlessPlatform.ForceRenderTimerTick(ticks);
        using var frame = window.CaptureRenderedFrame();
    }

    private void WriteSummary(string scenario, IReadOnlyList<FrameSample> samples, double offset)
    {
        var frameTimes = samples.Select(static sample => sample.Milliseconds).OrderBy(static value => value).ToArray();
        var allocations = samples.Select(static sample => (double)sample.AllocatedBytes).OrderBy(static value => value).ToArray();

        output.WriteLine(
            $"{scenario}: frames={samples.Count}, offset={offset:F1}, " +
            $"frame-ms[p50/p95/max]={Percentile(frameTimes, .50):F3}/{Percentile(frameTimes, .95):F3}/{frameTimes[^1]:F3}, " +
            $"alloc-bytes[p50/p95/max]={Percentile(allocations, .50):F0}/{Percentile(allocations, .95):F0}/{allocations[^1]:F0}");
    }

    private static double Percentile(IReadOnlyList<double> values, double percentile)
    {
        var index = (int)Math.Ceiling(values.Count * percentile) - 1;
        return values[Math.Clamp(index, 0, values.Count - 1)];
    }

    private static int CountItems(IEnumerable<FolderItem> items) =>
        items.Sum(item => 1 + CountItems(item.Children));

    private sealed record FrameSample(double Milliseconds, long AllocatedBytes);

    private sealed record ScenarioResult(IReadOnlyList<FrameSample> Samples, double Offset);

    private sealed record DynamicBlurScene(Grid Root, ScrollViewer ScrollViewer);
}
