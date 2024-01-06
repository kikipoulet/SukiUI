using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;

namespace SukiUI.Demo.Features.ControlsLibrary
{
    public partial class FormViewModel : ObservableObject
    {
        [ObservableProperty][property: Category("Category 1")][property: DisplayName("Nullable name")] private string? _nullableName = null;
        [ObservableProperty][property: Category("Category 1")][property: DisplayName("Nullable description")] private string? _nullableDescription = null;
        [ObservableProperty][property: Category("Category 1")][property: DisplayName("Nullable boolean")] private bool? _nullableBoolean = null;
        [ObservableProperty][property: Category("Category 1")][property: DisplayName("Nullable dateTime")] private DateTime? _nullableDateTime = null;
        [ObservableProperty][property: Category("Category 1")][property: DisplayName("Nullable dateTimeOffset")] private DateTimeOffset? _nullableDateTimeOffset = null;

        [ObservableProperty][property: Category("Nullable numbers")][property: DisplayName("Nullable integer")] private int? _nullableInteger = null;
        [ObservableProperty][property: Category("Nullable numbers")][property: DisplayName("Nullable long")] private long? _nullableLong = null;
        [ObservableProperty][property: Category("Nullable numbers")][property: DisplayName("Nullable decimal")] private decimal? _nullableDecimal = null;
        [ObservableProperty][property: Category("Nullable numbers")][property: DisplayName("Nullable float")] private float? _nullableFloat = null;
        [ObservableProperty][property: Category("Nullable numbers")][property: DisplayName("Nullable double")] private double? _nullableDouble = null;

        [ObservableProperty][property: Category("Category 2")][property: DisplayName("Name")] private string _name = string.Empty;
        [ObservableProperty][property: Category("Category 2")][property: DisplayName("Description")] private string _description = string.Empty;
        [ObservableProperty][property: Category("Numbers")][property: DisplayName("Integer")] private int _integer;
        [ObservableProperty][property: Category("Numbers")][property: DisplayName("Decimal")] private decimal _decimal;
        [ObservableProperty][property: Category("Category 2")][property: DisplayName("Boolean")] private bool _boolean;
        [ObservableProperty][property: Category("Category 2")][property: DisplayName("DateTime")] private DateTime _dateTime = DateTime.MinValue;
        [ObservableProperty][property: Category("Category 2")][property: DisplayName("DateTimeOffset")] private DateTimeOffset _dateTimeOffset = DateTimeOffset.MinValue;
        [ObservableProperty][property: Category("Category 2")][property: DisplayName("Child form")] private ChildFormViewModel _childForm = new ChildFormViewModel();
        [ObservableProperty][property: Category("Category 2")][property: DisplayName("Orientation")] private Orientation _orientation;
    }

    public partial class ChildFormViewModel : ObservableObject
    {
        [ObservableProperty][property: Category("Text")][property: DisplayName("Name")] private string _name = string.Empty;
        [ObservableProperty][property: Category("Text")][property: DisplayName("Description")] private string _description = string.Empty;
        [ObservableProperty][property: Category("Numbers")][property: DisplayName("Integer")] private int _integer;
        [ObservableProperty][property: Category("Numbers")][property: DisplayName("Decimal")] private decimal _decimal;
        [ObservableProperty][property: Category("Misc")][property: DisplayName("Boolean")] private bool _boolean;
        [ObservableProperty][property: Category("Misc")][property: DisplayName("DateTimeOffset")] private DateTimeOffset _dateTimeOffset = DateTimeOffset.MinValue;
    }
}