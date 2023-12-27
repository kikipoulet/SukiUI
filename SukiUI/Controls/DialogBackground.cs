using Avalonia.Controls;
using Avalonia.Input;

namespace SukiUI.Controls;

public class DialogBackground : Border
{
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        InteractiveContainer.BackgroundRequestClose();
    }
}