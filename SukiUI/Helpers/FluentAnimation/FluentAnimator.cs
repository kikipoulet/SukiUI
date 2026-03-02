using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Styling;

namespace SukiUI.Helpers;

public static class FluentAnimatorExtensions
{
    public static FluentAnimator<T> Animate<T>(this Animatable control, AvaloniaProperty<T> property) => new(control, property);
}

public class FluentAnimator<T>
{
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(.5);
    private static readonly Easing DefaultEasing = new CubicEaseInOut();

    private readonly Animatable _control;
    private readonly AvaloniaProperty<T> _property;
    private readonly AnimationGroup? _parentGroup;
    private readonly AnimationSequence? _parentSequence;

    private bool _fromSet;
    private T? _from;
    private T? _to;
    private TimeSpan? _duration;
    private TimeSpan? _delay;
    private Easing? _easing;
    private CancellationToken _cancellation;
    private Action? _onCompleted;

    public FluentAnimator(Animatable control, AvaloniaProperty<T> property, AnimationGroup? parentGroup = null, AnimationSequence? parentSequence = null)
    {
        _control = control;
        _property = property;
        _parentGroup = parentGroup;
        _parentSequence = parentSequence;
    }

    public FluentAnimator<T> From(T value)
    {
        _from = value;
        _fromSet = true;
        return this;
    }

    public FluentAnimator<T> To(T value)
    {
        _to = value;
        return this;
    }

    public FluentAnimator<T> WithDuration(TimeSpan duration)
    {
        _duration = duration;
        return this;
    }

    public FluentAnimator<T> WithEasing(Easing easing)
    {
        _easing = easing;
        return this;
    }

    public FluentAnimator<T> WithCancellationToken(CancellationToken cancellation)
    {
        _cancellation = cancellation;
        return this;
    }

    public FluentAnimator<T> WithDelay(TimeSpan delay)
    {
        _delay = delay;
        return this;
    }

    public FluentAnimator<T> ContinueWith(Action callback)
    {
        _onCompleted = callback;
        return this;
    }

    public void Start()
    {
        if (_parentSequence != null)
        {
            var group = _parentGroup ?? _parentSequence.GetCurrentGroup();
            group.AddAnimation(RunAsync);
            if (_onCompleted != null)
                _parentSequence.SetOnCompleted(_onCompleted);
            _parentSequence.Start();
        }
        else if (_parentGroup != null)
        {
            _parentGroup.AddAnimation(RunAsync);
            if (_onCompleted != null)
                _parentGroup.SetOnCompleted(_onCompleted);
            _parentGroup.Start();
        }
        else
        {
            _ = RunAsyncAndNotify();
        }
    }

    public async Task RunAsync()
    {
        if (_to == null)
            throw new InvalidOperationException("The 'To' value must be set before starting the animation.");

        if (_delay.HasValue)
            await Task.Delay(_delay.Value);

        var duration = _duration ?? DefaultDuration;
        var easing = _easing ?? DefaultEasing;

        var animation = new Animation
        {
            Duration = duration,
            Easing = easing,
            FillMode = FillMode.Forward,
        };

        var fromValue = _fromSet ? _from! : _control.GetValue(_property);

        animation.Children.Add(new KeyFrame
        {
            Cue = new Cue(0),
            Setters = { new Setter { Property = _property, Value = fromValue } }
        });

        animation.Children.Add(new KeyFrame
        {
            Cue = new Cue(1),
            Setters = { new Setter { Property = _property, Value = _to } }
        });

        await animation.RunAsync(_control, _cancellation);
    }

    private async Task RunAsyncAndNotify()
    {
        await RunAsync();
        _onCompleted?.Invoke();
    }

    public CancellationTokenSource RepeatForever()
    {
        var cts = new CancellationTokenSource();

        if (_parentSequence != null)
        {
            var group = _parentGroup ?? _parentSequence.GetCurrentGroup();
            group.AddAnimation(RunAsync);
            if (_onCompleted != null)
                _parentSequence.SetOnCompleted(_onCompleted);
            _ = _parentSequence.StartAsyncLoop(cts.Token);
        }
        else if (_parentGroup != null)
        {
            _parentGroup.AddAnimation(RunAsync);
            if (_onCompleted != null)
                _parentGroup.SetOnCompleted(_onCompleted);
            _ = _parentGroup.StartAsyncLoop(cts.Token);
        }
        else
        {
            _ = RunAsyncLoop(cts.Token);
        }

        return cts;
    }

    public CancellationTokenSource Repeat(int count)
    {
        var cts = new CancellationTokenSource();

        if (_parentSequence != null)
        {
            var group = _parentGroup ?? _parentSequence.GetCurrentGroup();
            group.AddAnimation(RunAsync);
            if (_onCompleted != null)
                _parentSequence.SetOnCompleted(_onCompleted);
            _ = _parentSequence.StartAsyncLoop(cts.Token, count);
        }
        else if (_parentGroup != null)
        {
            _parentGroup.AddAnimation(RunAsync);
            if (_onCompleted != null)
                _parentGroup.SetOnCompleted(_onCompleted);
            _ = _parentGroup.StartAsyncLoop(cts.Token, count);
        }
        else
        {
            _ = RunAsyncLoop(cts.Token, count);
        }

        return cts;
    }

    private async Task RunAsyncLoop(CancellationToken token, int? maxIterations = null)
    {
        var iteration = 0;
        while (!token.IsCancellationRequested && (maxIterations == null || iteration < maxIterations))
        {
            await RunAsync();
            iteration++;
        }
        _onCompleted?.Invoke();
    }

    public AnimationGroup And()
    {
        if (_parentSequence != null)
        {
            var group = _parentGroup ?? _parentSequence.GetCurrentGroup();
            group.AddAnimation(RunAsync);
            return group;
        }

        var newGroup = _parentGroup ?? new AnimationGroup(_control);
        newGroup.AddAnimation(RunAsync);
        return newGroup;
    }

    public AnimationSequence Then()
    {
        if (_parentSequence != null)
        {
            var currentGroup = _parentGroup ?? _parentSequence.GetCurrentGroup();
            currentGroup.AddAnimation(RunAsync);
            return _parentSequence;
        }

        var group = _parentGroup ?? new AnimationGroup(_control);
        group.AddAnimation(RunAsync);
        return new AnimationSequence(_control, group);
    }
}
