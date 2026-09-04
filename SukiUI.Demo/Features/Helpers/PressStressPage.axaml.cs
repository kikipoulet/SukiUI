using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SukiUI.ControlsAnimation;

namespace SukiUI.Demo.Features.Helpers
{
    /// <summary>
    /// Benchmark harness comparing the Suki press physics system (shared SukiTicker loop,
    /// real SukiPressPhysics state machines driven programmatically) against plain XAML
    /// transitions (:checked -&gt; scale(0.96) on a ToggleButton, native
    /// TransformOperationsTransition). One grid of buttons; every button is "clicked"
    /// constantly (120ms press every 800ms, uniformly staggered so gestures flow
    /// continuously); each run lasts 60 seconds and ends with a results line that is kept
    /// in a history so both systems can be compared run by run.
    /// The frame monitor uses its OWN RequestAnimationFrame loop (not SukiTicker), so the
    /// measurement is identical whichever engine is under test.
    /// CPU is probed at 1 Hz (whole process + UI thread, the thread both engines run on)
    /// and dispatcher pressure by the delay of a Background-priority heartbeat (100ms
    /// nominal); probes run at the lowest priority so they never compete with the
    /// Render-priority work being measured.
    /// </summary>
    public partial class PressStressPage : UserControl
    {
        private const double CycleMs = 800;   // one full click cycle per button
        private const double PressMs = 120;   // how long each "press" is held
        private const double RunSeconds = 60; // benchmark duration

        private sealed class BenchButton
        {
            public Button? PhysicsButton;
            public SukiPressPhysics? Engine;  // real press state machine (Suki mode)
            public ToggleButton? XamlButton;  // native transition carrier (XAML mode)
            public TimeSpan Origin;           // cycle origin (staggered per button)
            public bool Engaged;
        }

        private readonly List<BenchButton> _buttons = new();
        private readonly List<string> _history = new();

        private bool _running;
        private bool _rafLoop;
        private bool _xamlMode;
        private int _count = 50;

        // Run state & stats (all on the UI thread).
        private TimeSpan _runStart, _lastFrame, _lastUiRefresh;
        private long _frames;
        private double _sumMs, _sumSqMs, _minMs = double.MaxValue, _maxMs;
        private long _allocBase;
        private int _gc0, _gc1, _gc2;
        private long _clicks;

        // Engine cost (suki mode): deltas of SukiTicker's instrumentation around the run.
        private double _engMsBase;

        // CPU probes: 1 Hz samples of process-wide and UI-thread TotalProcessorTime.
        private Process? _process;
        private DispatcherTimer? _cpuSampler;
        private TimeSpan _lastProcCpu, _lastUiCpu, _lastCpuSampleAt;
        private double _procCpuSumPct, _uiCpuSumPct;
        private int _cpuSamples, _uiCpuSamples;
        private uint _uiTid; // Win32 thread id of the UI thread (0 = not captured / not Windows)

        // Dispatcher pressure: delay of a 100ms Background-priority heartbeat.
        private DispatcherTimer? _hb;
        private TimeSpan _hbLast;
        private double _hbLagSum, _hbLagMax;
        private long _hbSamples;

        private static class Native
        {
            [DllImport("kernel32.dll")]
            internal static extern uint GetCurrentThreadId();
        }

        public PressStressPage()
        {
            InitializeComponent();
            UpdateModeLabel();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            AbortRun("page left");
            base.OnDetachedFromVisualTree(e);
        }

        // ---- UI handlers -----------------------------------------------------------

        private void OnModeToggled(object? sender, RoutedEventArgs e)
        {
            _xamlMode = ModeToggle.IsChecked is true;
            UpdateModeLabel();
            if (!_running)
                GenerateGrid();
        }

        private void OnGenerate(object? sender, RoutedEventArgs e) => GenerateGrid();

        private void OnStartStop(object? sender, RoutedEventArgs e)
        {
            if (_running)
                AbortRun("manual stop");
            else
                StartRun();
        }

        private void UpdateModeLabel()
        {
            ModeToggle.Content = _xamlMode ? "Mode: XAML transition" : "Mode: SUKI physics";
            ModeDesc.Text = _xamlMode
                ? "Every click toggles a :checked ToggleButton → scale(0.96), animated by a native XAML TransformOperationsTransition (150 ms). The animation engine is Avalonia's own style engine."
                : "Every click drives the real SukiPressPhysics state machine (elastic press + release spring), advanced by the shared SukiTicker loop — exactly what a real click triggers.";
        }

        private void GenerateGrid()
        {
            _count = CountSelector.SelectedIndex switch
            {
                0 => 10,
                2 => 100,
                3 => 200,
                _ => 50
            };
            GridButtons.Children.Clear();
            _buttons.Clear();
            for (int i = 0; i < _count; i++)
            {
                var bench = new BenchButton();
                if (_xamlMode)
                {
                    bench.XamlButton = new ToggleButton { Content = $"X{i}" };
                    bench.XamlButton.Classes.Add("bench");
                    GridButtons.Children.Add(bench.XamlButton);
                }
                else
                {
                    bench.PhysicsButton = new Button { Content = $"B{i}" };
                    bench.PhysicsButton.Classes.Add("stressBtn");
                    GridButtons.Children.Add(bench.PhysicsButton);
                    // The very instance the style's Enable wiring would use — no duplicate.
                    bench.Engine = SukiPress.EnsurePhysicsInternal(bench.PhysicsButton);
                }
                _buttons.Add(bench);
            }
        }

        // ---- Run lifecycle ---------------------------------------------------------

        private void StartRun()
        {
            if (_buttons.Count == 0)
                GenerateGrid();
            if (TopLevel.GetTopLevel(this) is not { } topLevel)
                return;

            _running = true;
            StartStopButton.Content = "Stop";
            ModeToggle.IsEnabled = false;
            CountSelector.IsEnabled = false;
            GenerateButton.IsEnabled = false;

            _runStart = SukiTicker.Now;
            _lastFrame = _runStart;
            _lastUiRefresh = _runStart;
            _frames = 0;
            _sumMs = _sumSqMs = 0;
            _minMs = double.MaxValue;
            _maxMs = 0;
            _clicks = 0;
            _allocBase = GC.GetAllocatedBytesForCurrentThread();
            _gc0 = GC.CollectionCount(0);
            _gc1 = GC.CollectionCount(1);
            _gc2 = GC.CollectionCount(2);
            _engMsBase = SukiTicker.TotalDispatchMs;

            // Uniform stagger: with N buttons, a click starts every CycleMs/N ms.
            double stride = CycleMs / _buttons.Count;
            for (int i = 0; i < _buttons.Count; i++)
                _buttons[i].Origin = _runStart + TimeSpan.FromMilliseconds(i * stride);

            StartProbes();

            // Kick-start: engaging the first buttons invalidates the scene, which
            // schedules the frame the RAF monitor (and the animation) will ride on.
            Drive(SukiTicker.Now);
            LiveText.Text = "run in progress…";

            _rafLoop = true;
            topLevel.RequestAnimationFrame(OnFrame);
        }

        private void StartProbes()
        {
            _procCpuSumPct = _uiCpuSumPct = 0;
            _cpuSamples = _uiCpuSamples = 0;
            _hbLagSum = _hbLagMax = 0;
            _hbSamples = 0;

            // We are on the UI thread here (click handler) — safe to capture its OS id.
            if (_uiTid == 0 && OperatingSystem.IsWindows())
                _uiTid = Native.GetCurrentThreadId();

            _process ??= Process.GetCurrentProcess();
            try
            {
                _process.Refresh();
                _lastProcCpu = _process.TotalProcessorTime;
                _lastUiCpu = ReadUiThreadCpu() ?? default;
                _lastCpuSampleAt = SukiTicker.Now;

                _cpuSampler = new DispatcherTimer(DispatcherPriority.Background)
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                _cpuSampler.Tick += (_, _) => SampleCpu();
                _cpuSampler.Start();
            }
            catch
            {
                // CPU probing unavailable — the run stays valid, cpu columns show n/a.
                _cpuSampler = null;
            }

            _hbLast = SukiTicker.Now;
            _hb = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _hb.Tick += (_, _) =>
            {
                var t = SukiTicker.Now;
                double lag = Math.Max((t - _hbLast).TotalMilliseconds - 100.0, 0.0);
                _hbLagSum += lag;
                if (lag > _hbLagMax) _hbLagMax = lag;
                _hbSamples++;
                _hbLast = t;
            };
            _hb.Start();
        }

        private void SampleCpu()
        {
            if (_process is null)
                return;
            try
            {
                _process.Refresh();
                var now = SukiTicker.Now;
                double elapsedMs = (now - _lastCpuSampleAt).TotalMilliseconds;
                if (elapsedMs <= 0)
                    return;
                var procCpu = _process.TotalProcessorTime;
                _procCpuSumPct += Math.Min((procCpu - _lastProcCpu).TotalMilliseconds / elapsedMs, 1.0) * 100.0;
                _lastProcCpu = procCpu;
                _cpuSamples++;
                if (ReadUiThreadCpu() is { } uiCpu)
                {
                    double uiMs = (uiCpu - _lastUiCpu).TotalMilliseconds;
                    if (uiMs >= 0)
                    {
                        _uiCpuSumPct += Math.Min(uiMs / elapsedMs, 1.0) * 100.0;
                        _uiCpuSamples++;
                    }
                    _lastUiCpu = uiCpu;
                }
                _lastCpuSampleAt = now;
            }
            catch
            {
                // Thread/process enumeration can transiently fail — skip this sample.
            }
        }

        private TimeSpan? ReadUiThreadCpu()
        {
            if (!OperatingSystem.IsWindows() || _uiTid == 0)
                return null;
            try
            {
                foreach (ProcessThread t in _process!.Threads)
                    if (t.Id == _uiTid)
                        return t.TotalProcessorTime;
            }
            catch
            {
                // Collection mutated during enumeration — caller skips this sample.
            }
            return null;
        }

        private void StopProbes()
        {
            _cpuSampler?.Stop();
            _cpuSampler = null;
            _hb?.Stop();
            _hb = null;
        }

        private void OnFrame(TimeSpan _)
        {
            if (!_rafLoop)
                return;
            var now = SukiTicker.Now;

            // Frame-pacing stats (impartial loop — same measurement for both engines).
            double dMs = (now - _lastFrame).TotalMilliseconds;
            _lastFrame = now;
            _frames++;
            _sumMs += dMs;
            _sumSqMs += dMs * dMs;
            if (dMs < _minMs) _minMs = dMs;
            if (dMs > _maxMs) _maxMs = dMs;

            Drive(now);

            if ((now - _runStart).TotalSeconds >= RunSeconds)
            {
                FinishRun(now);
                return;
            }

            if ((now - _lastUiRefresh).TotalMilliseconds >= 500)
            {
                _lastUiRefresh = now;
                LiveText.Text = FormatLine(live: true, now);
            }

            if (TopLevel.GetTopLevel(this) is { } topLevel)
                topLevel.RequestAnimationFrame(OnFrame);
            else
                AbortRun("window closed");
        }

        private void Drive(TimeSpan now)
        {
            foreach (var bench in _buttons)
            {
                double t = (now - bench.Origin).TotalMilliseconds % CycleMs;
                bool pressNow = t < PressMs;
                if (pressNow && !bench.Engaged)
                {
                    bench.Engaged = true;
                    _clicks++;
                    if (bench.Engine is { } engine)
                        engine.Press();
                    else if (bench.XamlButton is { } xaml)
                        xaml.IsChecked = true;
                }
                else if (!pressNow && bench.Engaged)
                {
                    bench.Engaged = false;
                    if (bench.Engine is { } engine)
                        engine.Release();
                    else if (bench.XamlButton is { } xaml)
                        xaml.IsChecked = false;
                }
            }
        }

        private void ReleaseAll()
        {
            foreach (var bench in _buttons)
            {
                if (!bench.Engaged)
                    continue;
                bench.Engaged = false;
                bench.Engine?.Release();
                if (bench.XamlButton is { } xaml)
                    xaml.IsChecked = false;
            }
        }

        private void FinishRun(TimeSpan now)
        {
            ReleaseAll();
            _rafLoop = false;
            _running = false;
            StopProbes();
            ResetControls();

            var line = FormatLine(live: false, now);
            _history.Insert(0, line);
            if (_history.Count > 8)
                _history.RemoveAt(_history.Count - 1);
            HistoryText.Text = string.Join(Environment.NewLine, _history);
            LiveText.Text = line + "   ← final result";
        }

        private void AbortRun(string reason)
        {
            if (!_running)
                return;
            ReleaseAll();
            _rafLoop = false;
            _running = false;
            StopProbes();
            ResetControls();
            LiveText.Text = $"run aborted ({reason})";
        }

        private void ResetControls()
        {
            StartStopButton.Content = $"Start run ({RunSeconds:0}s)";
            ModeToggle.IsEnabled = true;
            CountSelector.IsEnabled = true;
            GenerateButton.IsEnabled = true;
        }

        // ---- Reporting ---------------------------------------------------------

        private string FormatLine(bool live, TimeSpan now)
        {
            double seconds = Math.Clamp((now - _runStart).TotalSeconds, 0.001, RunSeconds);
            double fps = _frames / seconds;
            double avgMs = _sumMs / Math.Max(_frames, 1);
            double variance = Math.Max(_sumSqMs / Math.Max(_frames, 1) - avgMs * avgMs, 0);
            double jitter = Math.Sqrt(variance);
            double minMs = _frames > 0 ? _minMs : 0;
            double allocMb = Math.Max(0, GC.GetAllocatedBytesForCurrentThread() - _allocBase) / (1024.0 * 1024.0);
            int g0 = GC.CollectionCount(0) - _gc0;
            int g1 = GC.CollectionCount(1) - _gc1;
            int g2 = GC.CollectionCount(2) - _gc2;
            string mode = _xamlMode ? "xaml" : "suki";

            // Engine cost per rendered frame — only observable in suki mode (the native
            // XAML engine runs inside Avalonia's own clock).
            string engine = !_xamlMode && _frames > 1
                ? $"eng {(SukiTicker.TotalDispatchMs - _engMsBase) / _frames,4:0.00}ms/f"
                : "eng  —";

            double uiPct = _uiCpuSamples > 0 ? _uiCpuSumPct / _uiCpuSamples : double.NaN;
            double procPct = _cpuSamples > 0 ? _procCpuSumPct / _cpuSamples : double.NaN;
            string cpu = $"cpu ui {FmtPct(uiPct)} proc {FmtPct(procPct)}";

            string disp = _hbSamples > 0
                ? $"disp {_hbLagSum / _hbSamples,4:0.0}/{_hbLagMax,4:0.0}ms"
                : "disp n/a";

            string tail = live
                ? $"{RunSeconds - seconds,3:0}s left"
                : $"frame avg {avgMs,5:0.0}ms  min {minMs,4:0.0}  max {_maxMs,5:0.0}";
            return $"[{mode} N={_buttons.Count,3}] fps {fps,6:0.0} | jit {jitter,4:0.0}ms | {engine} | {cpu} | {disp} | +{allocMb,7:0.00}MB | GC {g0}/{g1}/{g2} | clicks {_clicks,6} | {tail}";
        }

        private static string FmtPct(double v) => double.IsNaN(v) ? "n/a" : $"{v,4:0.0}%";
    }
}
