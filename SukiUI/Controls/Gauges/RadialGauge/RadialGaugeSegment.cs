using System;
using Avalonia.Media;

namespace SukiUI.Controls.Gauges;

/// <summary>
/// Represents a colored segment in a RadialGauge between two values.
/// </summary>
public class RadialGaugeSegment
{
    /// <summary>
    /// The starting value of the segment.
    /// </summary>
    public double FromValue { get; set; }
    
    /// <summary>
    /// The ending value of the segment.
    /// </summary>
    public double ToValue { get; set; }
    
    /// <summary>
    /// The color of the segment.
    /// </summary>
    public Color Color { get; set; }
    
    /// <summary>
    /// The thickness of the segment arc in pixels. Default is 4.
    /// </summary>
    public double Thickness { get; set; } = 4.0;
    public double Opacity { get; set; } = 1;

    public RadialGaugeSegment()
    {
    }

    public RadialGaugeSegment(double fromValue, double toValue, Color color, double thickness = 4.0, double opacity = 1)
    {
        FromValue = fromValue;
        ToValue = toValue;
        Color = color;
        Thickness = thickness;
        Opacity = opacity;
    }
}
