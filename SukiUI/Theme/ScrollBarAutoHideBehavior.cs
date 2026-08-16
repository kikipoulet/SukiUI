using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace SukiUI.Theme;

public static class ScrollBarAutoHideBehavior
{
    private const string ExpandedClass = "suki-scrollbar-expanded";

    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, bool>(
            "IsEnabled",
            typeof(ScrollBarAutoHideBehavior));

    public static readonly AttachedProperty<TimeSpan> HideDelayProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, TimeSpan>(
            "HideDelay",
            typeof(ScrollBarAutoHideBehavior),
            TimeSpan.FromMilliseconds(500));

    private static readonly AttachedProperty<State?> StateProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, State?>(
            "State",
            typeof(ScrollBarAutoHideBehavior));

    static ScrollBarAutoHideBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<ScrollViewer>(OnIsEnabledChanged);
    }

    public static bool GetIsEnabled(ScrollViewer scrollViewer) =>
        scrollViewer.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(ScrollViewer scrollViewer, bool value) =>
        scrollViewer.SetValue(IsEnabledProperty, value);

    public static TimeSpan GetHideDelay(ScrollViewer scrollViewer) =>
        scrollViewer.GetValue(HideDelayProperty);

    public static void SetHideDelay(ScrollViewer scrollViewer, TimeSpan value) =>
        scrollViewer.SetValue(HideDelayProperty, value);

    private static void OnIsEnabledChanged(ScrollViewer scrollViewer, AvaloniaPropertyChangedEventArgs e)
    {
        scrollViewer.GetValue(StateProperty)?.Detach();
        scrollViewer.SetValue(StateProperty, null);
        scrollViewer.Classes.Remove(ExpandedClass);

        if (e.NewValue is true)
        {
            var state = new State(scrollViewer);
            scrollViewer.SetValue(StateProperty, state);
            state.Attach();
        }
    }

    private sealed class State(ScrollViewer scrollViewer)
    {
        private DispatcherTimer? _hideTimer;
        private bool _isDragging;

        public void Attach()
        {
            scrollViewer.PointerEntered += OnPointerEntered;
            scrollViewer.PointerExited += OnPointerExited;
            scrollViewer.AddHandler(
                Thumb.DragStartedEvent,
                OnDragStarted,
                RoutingStrategies.Bubble,
                handledEventsToo: true);
            scrollViewer.AddHandler(
                Thumb.DragCompletedEvent,
                OnDragCompleted,
                RoutingStrategies.Bubble,
                handledEventsToo: true);
        }

        public void Detach()
        {
            CancelHide();
            scrollViewer.PointerEntered -= OnPointerEntered;
            scrollViewer.PointerExited -= OnPointerExited;
            scrollViewer.RemoveHandler(Thumb.DragStartedEvent, OnDragStarted);
            scrollViewer.RemoveHandler(Thumb.DragCompletedEvent, OnDragCompleted);
        }

        private void OnPointerEntered(object? sender, PointerEventArgs e)
        {
            Show();
        }

        private void OnPointerExited(object? sender, PointerEventArgs e)
        {
            if (!_isDragging)
            {
                ScheduleHide();
            }
        }

        private void OnDragStarted(object? sender, VectorEventArgs e)
        {
            _isDragging = true;
            Show();
        }

        private void OnDragCompleted(object? sender, VectorEventArgs e)
        {
            _isDragging = false;

            if (scrollViewer.IsPointerOver)
            {
                Show();
            }
            else
            {
                ScheduleHide();
            }
        }

        private void Show()
        {
            CancelHide();
            scrollViewer.Classes.Add(ExpandedClass);
        }

        private void ScheduleHide()
        {
            CancelHide();

            _hideTimer = new DispatcherTimer
            {
                Interval = GetHideDelay(scrollViewer)
            };
            _hideTimer.Tick += OnHideTimerTick;
            _hideTimer.Start();
        }

        private void OnHideTimerTick(object? sender, EventArgs e)
        {
            CancelHide();

            if (!_isDragging && !scrollViewer.IsPointerOver)
            {
                scrollViewer.Classes.Remove(ExpandedClass);
            }
        }

        private void CancelHide()
        {
            if (_hideTimer is null)
            {
                return;
            }

            _hideTimer.Stop();
            _hideTimer.Tick -= OnHideTimerTick;
            _hideTimer = null;
        }
    }
}
