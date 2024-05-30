using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// The <see cref="GripMode"/> enum value converters.
/// </summary>
public static class GripModeConverters
{
    /// <summary>
    /// The <see cref="GripMode.AutoHide"/> to <see cref="Grid.RowProperty"/> value converter.
    /// </summary>
    public static readonly IValueConverter GridRowAutoHideConverter =
        new FuncValueConverter<GripMode, int>(x => x == GripMode.AutoHide ? 1 : 0);

    /// <summary>
    /// The <see cref="GripMode"/> to <see cref="Visual.IsVisible"/> value converter.
    /// </summary>
    public static readonly IValueConverter IsVisibleVisibleConverter =
        new FuncValueConverter<GripMode, bool>(x => x == GripMode.Visible);

    /// <summary>
    /// The <see cref="GripMode"/> to <see cref="Visual.IsVisible"/> value converter.
    /// </summary>
    public static readonly IValueConverter IsVisibleVisibleOrHiddenConverter =
        new FuncValueConverter<GripMode, bool>(x => x == GripMode.Hidden || x == GripMode.Visible);

    /// <summary>
    /// The <see cref="GripMode"/> to <see cref="Visual.IsVisible"/> value converter.
    /// </summary>
    public static readonly IValueConverter IsVisibleAutoHideOrVisibleConverter =
        new FuncValueConverter<GripMode, bool>(x => x == GripMode.AutoHide || x == GripMode.Visible);

    /// <summary>
    /// The <see cref="GripMode"/> to <see cref="Visual.IsVisible"/> value converter.
    /// </summary>
    public static readonly IValueConverter IsVisibleAutoHideOrHiddenConverter =
        new FuncValueConverter<GripMode, bool>(x => x == GripMode.AutoHide || x == GripMode.Hidden);
}