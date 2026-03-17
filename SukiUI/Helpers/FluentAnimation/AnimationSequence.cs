using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace SukiUI.Helpers;

public class AnimationSequence
{
    private readonly Animatable _control;
    private readonly List<AnimationGroup> _groups = new();
    private AnimationGroup _currentGroup;
    private Action? _onCompleted;

    internal AnimationSequence(Animatable control, AnimationGroup firstGroup)
    {
        _control = control;
        _currentGroup = firstGroup;
        _groups.Add(firstGroup);
    }

    internal void AddGroup(AnimationGroup group)
    {
        _currentGroup = group;
        _groups.Add(group);
    }

    internal AnimationGroup GetCurrentGroup()
    {
        return _currentGroup;
    }

    internal AnimationGroup CreateNewGroup()
    {
        var group = new AnimationGroup(_control, this);
        _currentGroup = group;
        _groups.Add(group);
        return group;
    }

    internal void SetOnCompleted(Action callback)
    {
        _onCompleted = callback;
    }

    public FluentAnimator<T> Animate<T>(AvaloniaProperty<T> property)
    {
        var group = CreateNewGroup();
        return new FluentAnimator<T>(_control, property, group, this);
    }

    public AnimationSequence Then()
    {
        return this;
    }

    public AnimationSequence ContinueWith(Action callback)
    {
        _onCompleted = callback;
        return this;
    }

    public async Task StartAsync()
    {
        foreach (var group in _groups)
        {
            await group.StartAsync();
        }
        _onCompleted?.Invoke();
    }

    public void Start() => _ = StartAsync();

    public CancellationTokenSource RepeatForever()
    {
        var cts = new CancellationTokenSource();
        _ = StartAsyncLoop(cts.Token);
        return cts;
    }

    public CancellationTokenSource Repeat(int count)
    {
        var cts = new CancellationTokenSource();
        _ = StartAsyncLoop(cts.Token, count);
        return cts;
    }

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
