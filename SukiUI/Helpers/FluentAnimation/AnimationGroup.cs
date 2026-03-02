using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace SukiUI.Helpers;

public class AnimationGroup
{
    private readonly Animatable _control;
    private readonly AnimationSequence? _parentSequence;
    private readonly List<Func<Task>> _animations = new();
    private Action? _onCompleted;

    internal AnimationGroup(Animatable control, AnimationSequence? parentSequence = null)
    {
        _control = control;
        _parentSequence = parentSequence;
    }

    internal void AddAnimation(Func<Task> animation)
    {
        _animations.Add(animation);
    }

    internal void SetOnCompleted(Action callback)
    {
        _onCompleted = callback;
    }

    public FluentAnimator<T> Animate<T>(AvaloniaProperty<T> property)
    {
        return new FluentAnimator<T>(_control, property, this, _parentSequence);
    }

    public AnimationSequence Then()
    {
        if (_parentSequence != null)
        {
            return _parentSequence;
        }
        return new AnimationSequence(_control, this);
    }

    public async Task StartAsync()
    {
        if (_animations.Count == 0)
            return;

        var tasks = _animations.Select(a => a()).ToArray();
        await Task.WhenAll(tasks);
        _onCompleted?.Invoke();
    }

    public void Start() => _ = StartAsync();

    internal async Task StartAsyncLoop(CancellationToken token, int? maxIterations = null)
    {
        var iteration = 0;
        while (!token.IsCancellationRequested && (maxIterations == null || iteration < maxIterations))
        {
            await StartAsync();
            iteration++;
        }
    }
}
