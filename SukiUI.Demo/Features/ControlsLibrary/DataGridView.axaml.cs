using Avalonia.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class DataGridView : UserControl
{
    public DataGridView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Assigns the row background imperatively, the code path reported broken in issue #404. Also
    /// numbers the row: a grid can only bind one LoadingRow handler, so this one covers both.
    /// </summary>
    private void OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (e.Row.DataContext is DataGridDemoItem item)
            e.Row.Background = item.StatusBrush;
        e.Row.Header = e.Row.Index + 1;
    }

    /// <summary>
    /// DataGrid has no built-in row numbering: the row header shows whatever Header the row is given,
    /// and LoadingRow is the hook for it. Rows are recycled while scrolling, so this fires again with
    /// the new Index and the numbering stays in step.
    /// </summary>
    private void OnRowNumberLoading(object? sender, DataGridRowEventArgs e)
    {
        e.Row.Header = e.Row.Index + 1;
    }
}
