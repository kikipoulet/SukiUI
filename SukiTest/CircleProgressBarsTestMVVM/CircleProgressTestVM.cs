using ReactiveUI;

namespace SukiTest.CircleProgressBarsTestMVVM;

public class CircleProgressTestVM : ReactiveObject
{
    private int _value = 0;
    public int Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    public void Add10()
    {
        Value += 10;
    }
}