using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

/// <summary>Expression or activity currently displayed by <see cref="AiMascot"/>.</summary>
public enum AiMascotMood
{
    Idle,
    Thinking,
    Analyzing,
    Talking,
    Success,
    Hesitant,
    Sleeping,
    Listening,
    Waking,
    Curious,
    Oops,
    Celebrate,
    Petted,
    Greeting,
    Showing,
    Yawning,
    Dizzy,
    Shivering,
    Humming,
    Cool
}

public readonly record struct AiMascotState(
    AiMascotMood Mood,
    double GazeX,
    double GazeY);

/// <summary>
/// Animated AI mascot built on <see cref="HyperControl{TState}"/> with smooth mood blending
/// and optional cursor gaze tracking.
/// </summary>
public sealed partial class AiMascot : HyperControl<AiMascotState>
{
    private const double GazeReach = 260.0;

    public static readonly StyledProperty<AiMascotMood> MoodProperty =
        AvaloniaProperty.Register<AiMascot, AiMascotMood>(nameof(Mood));

    public static readonly StyledProperty<Vector> GazeProperty =
        AvaloniaProperty.Register<AiMascot, Vector>(nameof(Gaze));

    public static readonly StyledProperty<bool> FollowPointerProperty =
        AvaloniaProperty.Register<AiMascot, bool>(nameof(FollowPointer));

    private TopLevel? _tracked;

    static AiMascot() => AffectsRenderState(MoodProperty, GazeProperty);

    public AiMascotMood Mood
    {
        get => GetValue(MoodProperty);
        set => SetValue(MoodProperty, value);
    }

    public Vector Gaze
    {
        get => GetValue(GazeProperty);
        set => SetValue(GazeProperty, value);
    }

    public bool FollowPointer
    {
        get => GetValue(FollowPointerProperty);
        set => SetValue(FollowPointerProperty, value);
    }

    /// <summary>Aims the mascot's gaze toward <paramref name="pointInWindow"/> (or resets on <c>null</c>).</summary>
    public void LookAt(Point? pointInWindow)
    {
        var top = TopLevel.GetTopLevel(this);
        if (pointInWindow is not { } p || top == null ||
            this.TranslatePoint(new Point(Bounds.Width / 2, Bounds.Height / 2), top) is not { } center)
        {
            Gaze = default;
            return;
        }
        Vector v = p - center;
        var length = v.Length;
        Gaze = length < 1e-6 ? default : v / length * Math.Min(1, length / GazeReach);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Track(FollowPointer);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Track(false);
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FollowPointerProperty && VisualRoot != null)
            Track(FollowPointer);
    }

    private void Track(bool on)
    {
        if (_tracked != null)
        {
            _tracked.RemoveHandler(PointerMovedEvent, OnWindowPointerMoved);
            _tracked.PointerExited -= OnWindowPointerExited;
            _tracked = null;
        }
        if (!on || TopLevel.GetTopLevel(this) is not { } top) return;
        _tracked = top;
        top.AddHandler(PointerMovedEvent, OnWindowPointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        top.PointerExited += OnWindowPointerExited;
    }

    private void OnWindowPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_tracked != null) LookAt(e.GetPosition(_tracked));
    }

    private void OnWindowPointerExited(object? sender, PointerEventArgs e) => LookAt(null);

    protected override AiMascotState CaptureState() => new(Mood, Gaze.X, Gaze.Y);
}
