using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class DataGridViewModel : DemoPageBase
{
    public ObservableCollection<DataGridDemoItem> Items { get; } = [];

    /// <summary>Grouped projection of <see cref="Items"/>, used by the grouping sample.</summary>
    public DataGridCollectionView GroupedItems { get; }

    public IReadOnlyList<DataGridGridLinesVisibility> GridLinesOptions { get; } =
        Enum.GetValues<DataGridGridLinesVisibility>();

    public IReadOnlyList<DataGridHeadersVisibility> HeadersOptions { get; } =
        Enum.GetValues<DataGridHeadersVisibility>();

    public IReadOnlyList<DataGridSelectionMode> SelectionModes { get; } =
        Enum.GetValues<DataGridSelectionMode>();

    public IReadOnlyList<DataGridRowDetailsVisibilityMode> RowDetailsModes { get; } =
        Enum.GetValues<DataGridRowDetailsVisibilityMode>();

    public IReadOnlyList<ScrollBarVisibility> ScrollBarVisibilities { get; } =
        Enum.GetValues<ScrollBarVisibility>();

    [ObservableProperty] private DataGridGridLinesVisibility _gridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
    [ObservableProperty] private DataGridHeadersVisibility _headersVisibility = DataGridHeadersVisibility.Column;
    [ObservableProperty] private DataGridSelectionMode _selectionMode = DataGridSelectionMode.Extended;
    [ObservableProperty] private DataGridRowDetailsVisibilityMode _rowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
    [ObservableProperty] private bool _canUserResizeColumns = true;
    [ObservableProperty] private bool _canUserReorderColumns = true;
    [ObservableProperty] private bool _canUserSortColumns = true;
    [ObservableProperty] private bool _isReadOnly;
    [ObservableProperty] private bool _isGridEnabled = true;
    [ObservableProperty] private int _frozenColumnCount;
    [ObservableProperty] private ScrollBarVisibility _horizontalScrollBarVisibility = ScrollBarVisibility.Auto;
    [ObservableProperty] private ScrollBarVisibility _verticalScrollBarVisibility = ScrollBarVisibility.Auto;

    /// <summary>
    /// Raising this past the available width is what forces the star columns to overflow, which is
    /// the only way to exercise horizontal scrolling and frozen columns in the sample.
    /// </summary>
    [ObservableProperty] private double _minColumnWidth = 20;

    public DataGridViewModel() : base("DataGrid", MaterialIconKind.Table)
    {
        foreach (var item in Enumerable.Range(1, 60).Select(x => new DataGridDemoItem(x)))
        {
            item.PropertyChanged += OnItemPropertyChanged;
            Items.Add(item);
        }

        GroupedItems = new DataGridCollectionView(Items);
        GroupedItems.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(DataGridDemoItem.Category)));
    }

    /// <summary>
    /// DataGridCollectionView does not watch its items for changes, so picking a new category from
    /// the grouped grid's ComboBox would leave the row filed under its old group header until the
    /// view is refreshed. The refresh is posted so it does not run while the ComboBox is still
    /// closing its popup.
    /// </summary>
    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(DataGridDemoItem.Category)) return;
        Dispatcher.UIThread.Post(() => GroupedItems.Refresh());
    }
}

public partial class DataGridDemoItem : ObservableObject
{
    /// <summary>Options for the Category template column's ComboBox, bound through x:Static.</summary>
    public static readonly string[] Categories = ["Hardware", "Software", "Services"];

    public DataGridDemoItem(int index)
    {
        _id = index;
        _name = $"Item {index}";
        _category = Categories[index % Categories.Length];
        _quantity = index * 17 % 500;
        _isActive = index % 3 != 0;
        _updated = DateTime.Today.AddDays(-index);
        _notes = $"Detail row for item {index}. Row details are part of the DataGrid feature set.";
    }

    [ObservableProperty] private int _id;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _category;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(StatusBrush))] private int _quantity;
    [ObservableProperty] private bool _isActive;
    [ObservableProperty] private DateTime _updated;
    [ObservableProperty] private string _notes;

    /// <summary>
    /// Backs the per-row background sample. Issue #404 was that a brush bound this way only reached
    /// every other row, so the sample is deliberately data driven rather than a static colour.
    /// </summary>
    public IBrush StatusBrush => Quantity switch
    {
        < 50 => new SolidColorBrush(Color.FromArgb(60, 220, 60, 60)),
        < 150 => new SolidColorBrush(Color.FromArgb(60, 230, 160, 40)),
        _ => Brushes.Transparent
    };
}
