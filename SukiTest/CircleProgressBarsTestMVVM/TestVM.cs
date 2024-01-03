using ReactiveUI;

namespace SukiTest.CircleProgressBarsTestMVVM;

public class TestVM : ReactiveObject
{
    private int progress = 0;

    public int Progress
    {
        get => progress;
        set => this.RaiseAndSetIfChanged(ref progress, value);
    }

    public void Increase10()
    {
        Progress += 10;
    }
}