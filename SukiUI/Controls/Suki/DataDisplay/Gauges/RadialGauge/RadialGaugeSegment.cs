using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;

namespace SukiUI.Controls.Gauges;

/// <summary>
/// Represents a colored segment in a RadialGauge between two values.
/// </summary>
public class RadialGaugeSegment : INotifyPropertyChanged
{
    private double _fromValue;
    private double _toValue;
    private Color _color;
    private double _thickness = 4.0;
    private double _opacity = 1;

    /// <summary>
    /// The starting value of the segment.
    /// </summary>
    public double FromValue { get => _fromValue; set => SetField(ref _fromValue, value); }
    
    /// <summary>
    /// The ending value of the segment.
    /// </summary>
    public double ToValue { get => _toValue; set => SetField(ref _toValue, value); }
    
    /// <summary>
    /// The color of the segment.
    /// </summary>
    public Color Color { get => _color; set => SetField(ref _color, value); }
    
    /// <summary>
    /// The thickness of the segment arc in pixels. Default is 4.
    /// </summary>
    public double Thickness { get => _thickness; set => SetField(ref _thickness, Math.Max(0, value)); }
    public double Opacity { get => _opacity; set => SetField(ref _opacity, Math.Clamp(value, 0, 1)); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

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
