using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Takes a list of values and returns the first non-null value.
/// </summary>
public class EitherNotNullConverter : IMultiValueConverter
{
    /// <summary>
    /// Gets <see cref="EitherNotNullConverter"/> instance.
    /// </summary>
    public static readonly EitherNotNullConverter Instance = new EitherNotNullConverter();

    /// <inheritdoc/>
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        foreach (var value in values)
        {
            if (value != null)
                return value;
        }

        return values;
    }
}
