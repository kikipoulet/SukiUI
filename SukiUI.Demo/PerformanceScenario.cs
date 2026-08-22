using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SukiUI.Animations;
using SukiUI.Controls;
using SukiUI.Controls.GlassMorphism;
using SukiUI.Demo.Features.ControlsLibrary;
using SukiUI.Demo.Features.Helpers;
using SukiUI.Enums;

namespace SukiUI.Demo;

internal static class PerformanceScenario
{
    private const string Argument = "--performance-scenario";
    private const string ProgressArgument = "--performance-progress-scenario";
    private const string FullDemoArgument = "--performance-full-demo-scenario";
    private const string FullDemoNavigationArgument = "--performance-full-demo-navigation-scenario";
    private const int WarmupFrames = 90;
    private const int MeasuredFrames = 300;
    private const string WatchdogSecondsVariable = "SUKIUI_PERF_WATCHDOG_SECONDS";
    private const string SweepPageLimitVariable = "SUKIUI_PERF_SWEEP_PAGE_LIMIT";
    private const string SweepPageStartIndexVariable = "SUKIUI_PERF_SWEEP_PAGE_START_INDEX";
    private const string SweepRepetitionsVariable = "SUKIUI_PERF_SWEEP_REPETITIONS";
    private const string SweepIntervalMillisecondsVariable = "SUKIUI_PERF_SWEEP_INTERVAL_MS";
    private const string BackgroundStyleVariable = "SUKIUI_PERF_BACKGROUND_STYLE";
    private const string BackgroundShaderFileVariable = "SUKIUI_PERF_BACKGROUND_SHADER_FILE";
    private const string BackgroundAnimationsVariable = "SUKIUI_PERF_BACKGROUND_ANIMATIONS";

    public static bool IsRequested => Environment.GetCommandLineArgs().Contains(Argument, StringComparer.Ordinal) ||
                                      Environment.GetCommandLineArgs().Contains(ProgressArgument, StringComparer.Ordinal) ||
                                      IsFullDemoRequested;

    public static bool IsFullDemoRequested =>
        Environment.GetCommandLineArgs().Contains(FullDemoArgument, StringComparer.Ordinal) ||
        IsFullDemoNavigationRequested;

    private static bool IsFullDemoNavigationRequested =>
        Environment.GetCommandLineArgs().Contains(FullDemoNavigationArgument, StringComparer.Ordinal);

    public static void StartFullDemoScenario(IClassicDesktopStyleApplicationLifetime lifetime, SukiUIDemoView window)
    {
        window.Opened += (_, _) => StartOnLargestDisplay(window, () =>
        {
            if (window.DataContext is not SukiUIDemoViewModel viewModel)
            {
                Console.Error.WriteLine("The full Demo window has no SukiUIDemoViewModel.");
                lifetime.Shutdown(3);
                return;
            }

            ConfigureBackground(viewModel);

            if (IsFullDemoNavigationRequested)
            {
                StartFullDemoNavigationSweep(lifetime, window, viewModel);
                return;
            }

            viewModel.ActivePage = viewModel.DemoPages.OfType<ProgressViewModel>().FirstOrDefault();
            DispatcherTimer.RunOnce(() =>
            {
                var progressView = window.GetVisualDescendants().OfType<ProgressView>().FirstOrDefault();
                var scrollViewer = progressView?.GetVisualDescendants().OfType<ScrollViewer>()
                    .OrderByDescending(static candidate => candidate.Bounds.Height)
                    .FirstOrDefault();
                if (scrollViewer is null)
                {
                    Console.Error.WriteLine("The full Demo Progress page did not create a ScrollViewer.");
                    lifetime.Shutdown(3);
                    return;
                }

                Console.WriteLine(
                    $"full-demo-progress-layout: window={window.Bounds.Size}, view={progressView!.Bounds.Size}, " +
                    $"scroll={scrollViewer.Bounds.Size}, extent={scrollViewer.Extent}, viewport={scrollViewer.Viewport}");
                StartScrollScript(window, scrollViewer, lifetime, "full-demo-progress-scroll",
                    () => StartFullDemoSquishyScenario(lifetime, window, viewModel));
            }, TimeSpan.FromMilliseconds(750), DispatcherPriority.Render);
        });
    }

    private static void ConfigureBackground(SukiUIDemoViewModel viewModel)
    {
        var requestedStyle = Environment.GetEnvironmentVariable(BackgroundStyleVariable);
        if (!string.IsNullOrWhiteSpace(requestedStyle))
        {
            if (Enum.TryParse<SukiBackgroundStyle>(requestedStyle, ignoreCase: true, out var style))
                viewModel.BackgroundStyle = style;
            else
                Console.Error.WriteLine($"Ignoring unknown {BackgroundStyleVariable} value: {requestedStyle}.");
        }

        var requestedShaderFile = Environment.GetEnvironmentVariable(BackgroundShaderFileVariable);
        if (!string.IsNullOrWhiteSpace(requestedShaderFile))
            viewModel.CustomShaderFile = requestedShaderFile;

        if (bool.TryParse(Environment.GetEnvironmentVariable(BackgroundAnimationsVariable), out var animationsEnabled))
            viewModel.AnimationsEnabled = animationsEnabled;

        Console.WriteLine($"full-demo-background: style={viewModel.BackgroundStyle}, shader={viewModel.CustomShaderFile ?? "none"}, " +
                          $"animations={viewModel.AnimationsEnabled}");
    }

    private static void StartFullDemoNavigationSweep(IClassicDesktopStyleApplicationLifetime lifetime,
        SukiUIDemoView window, SukiUIDemoViewModel viewModel)
    {
        var watchdog = CreateNavigationWatchdog();
        var requestedPageLimit = int.TryParse(Environment.GetEnvironmentVariable(SweepPageLimitVariable),
            out var limit)
            ? Math.Max(1, limit)
            : int.MaxValue;
        var requestedPageStartIndex = int.TryParse(Environment.GetEnvironmentVariable(SweepPageStartIndexVariable),
            out var startIndex)
            ? Math.Max(0, startIndex)
            : 0;
        var requestedRepetitions = int.TryParse(Environment.GetEnvironmentVariable(SweepRepetitionsVariable),
            out var repetitions)
            ? Math.Max(1, repetitions)
            : 1;
        var sweepIntervalMilliseconds = int.TryParse(Environment.GetEnvironmentVariable(SweepIntervalMillisecondsVariable),
            out var interval)
            ? Math.Clamp(interval, 100, 10_000)
            : 250;
        var pages = viewModel.DemoPages.Skip(requestedPageStartIndex).Take(requestedPageLimit).ToArray();
        if (pages.Length == 0)
        {
            watchdog?.Dispose();
            Console.Error.WriteLine("The full Demo navigation sweep did not select any pages.");
            lifetime.Shutdown(3);
            return;
        }

        var selectionCount = checked(pages.Length * requestedRepetitions);
        var frameIntervals = new List<double>(MeasuredFrames);
        var navigationFrameIntervals = new List<double>();
        var pageIndex = 0;
        var steadyFrame = 0;
        var navigationCompleted = false;
        var startedAt = Stopwatch.GetTimestamp();
        var previousFrame = Stopwatch.GetTimestamp();
        var pageTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(sweepIntervalMilliseconds)
        };
        var frameTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        pageTimer.Tick += (_, _) =>
        {
            if (pageIndex >= selectionCount)
            {
                pageTimer.Stop();
                navigationCompleted = true;
                steadyFrame = 0;
                return;
            }

            var page = pages[pageIndex % pages.Length];
            pageIndex++;
            var activationStartedAt = Stopwatch.GetTimestamp();
            var allocatedBeforeActivation = GC.GetAllocatedBytesForCurrentThread();
            Console.WriteLine(
                $"full-demo-navigation-sweep: selecting {pageIndex}/{selectionCount} {page.DisplayName} " +
                $"at={Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds:F1}ms");
            viewModel.ActivePage = page;
            window.InvalidateVisual();
            Console.WriteLine(
                $"full-demo-navigation-sweep: selected {page.DisplayName} " +
                $"in={Stopwatch.GetElapsedTime(activationStartedAt).TotalMilliseconds:F1}ms " +
                $"alloc-bytes={GC.GetAllocatedBytesForCurrentThread() - allocatedBeforeActivation}");
        };
        frameTimer.Tick += (_, _) =>
        {
            var now = Stopwatch.GetTimestamp();
            var frameInterval = Stopwatch.GetElapsedTime(previousFrame, now).TotalMilliseconds;
            if (frameInterval > 25)
                Console.WriteLine(
                    $"full-demo-navigation-sweep: long-frame={frameInterval:F1}ms " +
                    $"active={viewModel.ActivePage?.DisplayName ?? "none"}");
            if (!navigationCompleted)
                navigationFrameIntervals.Add(frameInterval);
            else if (steadyFrame >= WarmupFrames)
                frameIntervals.Add(frameInterval);

            previousFrame = now;
            if (navigationCompleted)
                steadyFrame++;
            if (!navigationCompleted || steadyFrame < WarmupFrames + MeasuredFrames)
                return;

            frameTimer.Stop();
            WriteSummary(
                $"full-demo-navigation-sweep-navigation(pages={pages.Length}, repetitions={requestedRepetitions}, interval-ms={sweepIntervalMilliseconds})",
                navigationFrameIntervals, 0, 0);
            WriteSummary($"full-demo-navigation-sweep(pages={pages.Length}, repetitions={requestedRepetitions}, interval-ms={sweepIntervalMilliseconds})",
                frameIntervals, 0, 0);
            watchdog?.Dispose();
            lifetime.Shutdown(BlurBackground.IsGpuBlurAvailable ? 0 : 2);
        };

        pageTimer.Start();
        frameTimer.Start();
    }

    private static Timer? CreateNavigationWatchdog()
    {
        if (!int.TryParse(Environment.GetEnvironmentVariable(WatchdogSecondsVariable), out var seconds) || seconds <= 0)
            return null;

        return new Timer(_ =>
        {
            Console.Error.WriteLine($"full-demo-navigation-sweep timed out after {seconds} seconds.");
            Environment.Exit(4);
        }, null, TimeSpan.FromSeconds(seconds), Timeout.InfiniteTimeSpan);
    }

    private static void StartFullDemoSquishyScenario(IClassicDesktopStyleApplicationLifetime lifetime,
        SukiUIDemoView window, SukiUIDemoViewModel viewModel)
    {
        viewModel.ActivePage = viewModel.DemoPages.OfType<HelpersViewModel>().FirstOrDefault();
        DispatcherTimer.RunOnce(() =>
        {
            var helpersView = window.GetVisualDescendants().OfType<HelpersView>().FirstOrDefault();
            var helpersMenu = helpersView?.GetVisualDescendants().OfType<SukiSideMenu>().FirstOrDefault();
            var squishItem = helpersMenu?.Items.OfType<SukiSideMenuItem>()
                .FirstOrDefault(static item => string.Equals(item.Header?.ToString(), "Squish Effect", StringComparison.Ordinal));
            if (helpersMenu is null || squishItem is null)
            {
                Console.Error.WriteLine("The full Demo Helpers page did not create the Squish Effect navigation item.");
                lifetime.Shutdown(3);
                return;
            }

            helpersMenu.SelectedItem = squishItem;
            DispatcherTimer.RunOnce(() =>
            {
                var pullingEffect = window.GetVisualDescendants().OfType<PullingEffect>().FirstOrDefault();
                var target = pullingEffect?.GetVisualDescendants().OfType<Control>()
                    .FirstOrDefault(static candidate => SquishyBehavior.GetEnable(candidate));
                if (target is null)
                {
                    Console.Error.WriteLine("The full Demo Squish Effect page did not create a draggable control.");
                    lifetime.Shutdown(3);
                    return;
                }

                var origin = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window);
                if (!origin.HasValue)
                {
                    Console.Error.WriteLine("The full Demo draggable control is not attached to the window.");
                    lifetime.Shutdown(3);
                    return;
                }

                StartSquishyDragScript(window, target, origin.Value, lifetime);
            }, TimeSpan.FromMilliseconds(750), DispatcherPriority.Render);
        }, TimeSpan.FromMilliseconds(750), DispatcherPriority.Render);
    }

    public static Window CreateWindow(IClassicDesktopStyleApplicationLifetime lifetime)
    {
        if (Environment.GetCommandLineArgs().Contains(ProgressArgument, StringComparer.Ordinal))
            return CreateProgressWindow(lifetime);

        var scrollViewer = new ScrollViewer { Content = CreateScrollContent() };
        var root = new Grid();
        root.Children.Add(scrollViewer);
        root.Children.Add(CreateDynamicBlurOverlay());

        var window = new Window
        {
            Title = "SukiUI performance scenario",
            Width = 1280,
            Height = 900,
            Content = root
        };

        window.Opened += (_, _) => StartOnLargestDisplay(window,
            () => StartScrollScript(window, scrollViewer, lifetime));
        return window;
    }

    private static Window CreateProgressWindow(IClassicDesktopStyleApplicationLifetime lifetime)
    {
        var view = new ProgressView { DataContext = new ProgressViewModel() };
        var window = new Window
        {
            Title = "SukiUI ProgressView performance scenario",
            Width = 1280,
            Height = 900,
            Content = view
        };

        window.Opened += (_, _) => StartOnLargestDisplay(window, () => Dispatcher.UIThread.Post(() =>
        {
            var scrollViewer = view.GetVisualDescendants().OfType<ScrollViewer>()
                .OrderByDescending(static candidate => candidate.Bounds.Height)
                .FirstOrDefault();
            if (scrollViewer is null)
            {
                Console.Error.WriteLine("ProgressView does not contain a ScrollViewer.");
                lifetime.Shutdown(3);
                return;
            }

            StartScrollScript(window, scrollViewer, lifetime, "progress-view-scroll");
        }, DispatcherPriority.Render));

        return window;
    }

    private static void StartOnLargestDisplay(Window window, Action action)
    {
        var screen = window.Screens?.All
            .OrderByDescending(static candidate => (long)candidate.Bounds.Width * candidate.Bounds.Height)
            .FirstOrDefault();
        if (screen is not null)
            window.Position = screen.WorkingArea.Position + new PixelPoint(24, 24);

        DispatcherTimer.RunOnce(() =>
        {
            Console.WriteLine($"display: bounds={screen?.Bounds}, scaling={window.RenderScaling:F2}");
            action();
        }, TimeSpan.FromMilliseconds(500), DispatcherPriority.Render);
    }

    private static Control CreateScrollContent()
    {
        var content = new StackPanel { Spacing = 12, Margin = new Thickness(32) };
        for (var index = 0; index < 160; index++)
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

        return content;
    }

    private static Control CreateDynamicBlurOverlay()
    {
        var overlay = new Panel();
        overlay.Children.Add(new SukiBlurBackground
        {
            IsDynamic = true,
            IntensityFactor = .2,
            Margin = new Thickness(-200),
            IsHitTestVisible = false
        });
        overlay.Children.Add(new TextBlock
        {
            Margin = new Thickness(24),
            FontSize = 22,
            FontWeight = FontWeight.DemiBold,
            Text = "Scripted dynamic blur scroll"
        });

        return new GlassCard
        {
            Width = 860,
            Height = 280,
            Margin = new Thickness(0, 0, 0, 48),
            Padding = new Thickness(0),
            ClipToBounds = true,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Content = overlay
        };
    }

    private static void StartScrollScript(Window window, ScrollViewer scrollViewer,
        IClassicDesktopStyleApplicationLifetime lifetime, string scenario = "dynamic-blur-scroll", Action? completed = null)
    {
        var frameIntervals = new List<double>(MeasuredFrames);
        var previousFrame = Stopwatch.GetTimestamp();
        var frame = 0;
        var offset = 0d;
        var peakOffset = 0d;
        var direction = 1d;
        var timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        timer.Tick += (_, _) =>
        {
            var now = Stopwatch.GetTimestamp();
            offset += 84 * direction;
            var maximum = Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height);
            if (offset >= maximum || offset <= 0)
            {
                direction = -direction;
                offset = Math.Clamp(offset, 0, maximum);
            }

            scrollViewer.Offset = new Vector(0, offset);
            peakOffset = Math.Max(peakOffset, offset);
            window.InvalidateVisual();

            if (frame >= WarmupFrames)
                frameIntervals.Add(Stopwatch.GetElapsedTime(previousFrame, now).TotalMilliseconds);

            previousFrame = now;
            frame++;
            if (frame < WarmupFrames + MeasuredFrames)
                return;

            timer.Stop();
            WriteSummary(scenario, frameIntervals, offset, peakOffset);
            if (completed is not null)
            {
                completed();
                return;
            }

            lifetime.Shutdown(BlurBackground.IsGpuBlurAvailable ? 0 : 2);
        };

        timer.Start();
    }

    private static void StartSquishyDragScript(Window window, Control target, Point origin,
        IClassicDesktopStyleApplicationLifetime lifetime)
    {
        var pointer = new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, isPrimary: true);
        RaisePointerPressed(target, window, pointer, origin);

        var frameIntervals = new List<double>(MeasuredFrames);
        var previousFrame = Stopwatch.GetTimestamp();
        var frame = 0;
        var timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        timer.Tick += (_, _) =>
        {
            var now = Stopwatch.GetTimestamp();
            var offset = new Vector((frame % 31) - 15, (frame % 19) - 9);
            RaisePointerMoved(target, window, pointer, origin + offset);
            window.InvalidateVisual();

            if (frame >= WarmupFrames)
                frameIntervals.Add(Stopwatch.GetElapsedTime(previousFrame, now).TotalMilliseconds);

            previousFrame = now;
            frame++;
            if (frame < WarmupFrames + MeasuredFrames)
                return;

            timer.Stop();
            RaisePointerReleased(target, window, pointer, origin + offset);
            pointer.Dispose();
            WriteSummary("full-demo-squishy-drag", frameIntervals, 0, 0);
            lifetime.Shutdown(BlurBackground.IsGpuBlurAvailable ? 0 : 2);
        };

        timer.Start();
    }

    private static void RaisePointerPressed(Control target, Window window, Pointer pointer, Point position) =>
        target.RaiseEvent(new PointerPressedEventArgs(target, pointer, window, position, Timestamp(),
            new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));

    private static void RaisePointerMoved(Control target, Window window, Pointer pointer, Point position) =>
        target.RaiseEvent(new PointerEventArgs(InputElement.PointerMovedEvent, target, pointer, window, position, Timestamp(),
            new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.Other), KeyModifiers.None));

    private static void RaisePointerReleased(Control target, Window window, Pointer pointer, Point position) =>
        target.RaiseEvent(new PointerReleasedEventArgs(target, pointer, window, position, Timestamp(),
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased), KeyModifiers.None,
            MouseButton.Left));

    private static ulong Timestamp() => unchecked((ulong)Environment.TickCount64);

    private static void WriteSummary(string scenario, IReadOnlyList<double> frameIntervals, double offset, double peakOffset)
    {
        var sorted = frameIntervals.Order().ToArray();
        var target = TimeSpan.FromSeconds(1d / 60).TotalMilliseconds;
        var missedFrames = frameIntervals.Count(interval => interval > target * 1.5);
        Console.WriteLine(
            $"{scenario}: frames={frameIntervals.Count}, offset={offset:F1}, peak-offset={peakOffset:F1}, " +
            $"frame-interval-ms[p50/p95/max]={Percentile(sorted, .50):F3}/{Percentile(sorted, .95):F3}/{sorted[^1]:F3}, " +
            $"missed-60hz={missedFrames}, gpu-blur={BlurBackground.IsGpuBlurAvailable}");
    }

    private static double Percentile(IReadOnlyList<double> values, double percentile)
    {
        var index = (int)Math.Ceiling(values.Count * percentile) - 1;
        return values[Math.Clamp(index, 0, values.Count - 1)];
    }
}
