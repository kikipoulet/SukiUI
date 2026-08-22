using Avalonia.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class DataGridView : UserControl
{
    public DataGridView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Assigns the row background imperatively, the code path reported broken in issue #404.
    /// </summary>
    private void OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (e.Row.DataContext is DataGridDemoItem item)
            e.Row.Background = item.StatusBrush;
    }
}
