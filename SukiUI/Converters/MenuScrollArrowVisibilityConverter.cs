using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;

namespace SukiUI.Converters;

/// <summary>
/// Decides whether a menu's scroll-up / scroll-down arrow should be shown, given
/// <c>[VerticalScrollBarVisibility, Offset.Y, Extent.Height, Viewport.Height]</c>.
/// </summary>
/// <remarks>
/// <para>
/// Replaces Avalonia's <c>MenuScrollingVisibilityConverter</c>, which only accounts for
/// <c>extent == viewport</c> and <c>extent &gt; viewport</c>. When the content is slightly *smaller*
/// than the viewport (say extent 161.6 against viewport 162.4, which ordinary layout rounding
/// produces) it is neither close enough to be treated as equal nor an overflow, so the percentage
/// test computes <c>0 / -0.8 = 0</c>, fails the "at the end" check, and falls through to true. The
/// result is an arrow on a menu that has nothing to scroll and does not respond to clicks.
/// </para>
/// <para>
/// It also treats <see cref="ScrollBarVisibility.Visible"/> as "show only when there is something to
/// scroll" rather than always, for the same reason: an arrow that cannot move is worse than no arrow.
/// </para>
/// </remarks>
public sealed class MenuScrollArrowVisibilityConverter : IMultiValueConverter
{
    /// <summary>
    /// Sub-pixel slack. Layout rounding routinely leaves extent and viewport a fraction of a pixel
    /// apart, and an overflow that small is not something the user can scroll to anyway.
    /// </summary>
    private const double Tolerance = 0.5;

    /// <summary>Shown while the content can still be scrolled up (offset is off the top).</summary>
    public static readonly MenuScrollArrowVisibilityConverter Up = new(towardsStart: true);

    /// <summary>Shown while the content can still be scrolled down (offset is off the bottom).</summary>
    public static readonly MenuScrollArrowVisibilityConverter Down = new(towardsStart: false);

    private readonly bool _towardsStart;

    private MenuScrollArrowVisibilityConverter(bool towardsStart) => _towardsStart = towardsStart;

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        // Anything unresolved means "not known to be scrollable", so stay hidden. Returning
        // UnsetValue here would leave IsVisible at its own default of true.
        if (values.Count != 4 ||
            values[0] is not ScrollBarVisibility visibility ||
            values[1] is not double offset ||
            values[2] is not double extent ||
            values[3] is not double viewport)
        {
            return false;
        }

        if (visibility is ScrollBarVisibility.Disabled or ScrollBarVisibility.Hidden)
            return false;

        if (!double.IsFinite(offset) || !double.IsFinite(extent) || !double.IsFinite(viewport))
            return false;

        // The guard Avalonia's converter is missing: no meaningful overflow, including the case where
        // the content measures a fraction of a pixel shorter than the viewport.
        var scrollable = extent - viewport;
        if (scrollable <= Tolerance)
            return false;

        return _towardsStart
            ? offset > Tolerance
            : offset < scrollable - Tolerance;
    }
}
