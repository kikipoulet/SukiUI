
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SukiUI.Controls.Gauges.HorizontalBarMeter;

public class HorizontalBarMeter : Panel
{

    public static readonly StyledProperty<int> BarCountProperty =
        AvaloniaProperty.Register<HorizontalBarMeter, int>(nameof(BarCount), 12);

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<HorizontalBarMeter, double>(nameof(Value), 0d);

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<HorizontalBarMeter, double>(nameof(Minimum), 0d);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<HorizontalBarMeter, double>(nameof(Maximum), 100d);

    public static readonly StyledProperty<double> GapProperty =
        AvaloniaProperty.Register<HorizontalBarMeter, double>(nameof(Gap), 6d);

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<HorizontalBarMeter, CornerRadius>(nameof(CornerRadius), new CornerRadius(4));

    public static readonly StyledProperty<IBrush?> ActiveBrushProperty =
        AvaloniaProperty.Register<HorizontalBarMeter, IBrush?>(nameof(ActiveBrush), Brushes.MediumPurple);

    public static readonly StyledProperty<IBrush?> InactiveBrushProperty =
        AvaloniaProperty.Register<HorizontalBarMeter, IBrush?>(nameof(InactiveBrush), Brushes.Gray);
    
    public int BarCount { get => GetValue(BarCountProperty); set => SetValue(BarCountProperty, Math.Max(1, value)); }
    public double Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public double Minimum { get => GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public double Gap { get => GetValue(GapProperty); set => SetValue(GapProperty, Math.Max(0, value)); }
    public CornerRadius CornerRadius { get => GetValue(CornerRadiusProperty); set => SetValue(CornerRadiusProperty, value); }
    public IBrush? ActiveBrush { get => GetValue(ActiveBrushProperty); set => SetValue(ActiveBrushProperty, value); }
    public IBrush? InactiveBrush { get => GetValue(InactiveBrushProperty); set => SetValue(InactiveBrushProperty, value); }

    private readonly List<Border> _bars = new();

    static HorizontalBarMeter()
    {
        BarCountProperty.Changed.AddClassHandler<HorizontalBarMeter>((s, _) => s.RebuildBars());
        GapProperty.Changed.AddClassHandler<HorizontalBarMeter>((s, _) => s.InvalidateMeasure());
        ValueProperty.Changed.AddClassHandler<HorizontalBarMeter>((s, _) => s.InvalidateArrange());
        MinimumProperty.Changed.AddClassHandler<HorizontalBarMeter>((s, _) => s.InvalidateArrange());
        MaximumProperty.Changed.AddClassHandler<HorizontalBarMeter>((s, _) => s.InvalidateArrange());
        CornerRadiusProperty.Changed.AddClassHandler<HorizontalBarMeter>((s, _) => s.UpdateBarCorners());
        ActiveBrushProperty.Changed.AddClassHandler<HorizontalBarMeter>((s, _) => s.UpdateBrushes());
        InactiveBrushProperty.Changed.AddClassHandler<HorizontalBarMeter>((s, _) => s.UpdateBrushes());
    }

    public HorizontalBarMeter() => RebuildBars();

    private void RebuildBars()
    {
        Children.Clear();
        _bars.Clear();

        var count = Math.Max(1, BarCount);
        for (int i = 0; i < count; i++)
        {
            var bar = new Border
            {
                CornerRadius = CornerRadius,
                Classes = { "hbm-segment" }, 
            };
            
            var fill = new Border
            {
                CornerRadius = CornerRadius,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                Width = 0,
                Classes = { "hbm-fill" }
            };

            bar.Child = fill;
            _bars.Add(bar);
            Children.Add(bar);
        }

        InvalidateMeasure();
        InvalidateArrange();
    }

    private void UpdateBarCorners()
    {
        foreach (var b in _bars)
        {
            b.CornerRadius = CornerRadius;
            if (b.Child is Border fill)
                fill.CornerRadius = CornerRadius;
        }
        InvalidateArrange();
    }

    private void UpdateBrushes()
    {
        foreach (var b in _bars)
        {
            b.Background = InactiveBrush;
            if (b.Child is Border fill)
                fill.Background = ActiveBrush;
        }
        InvalidateVisual();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var bars = Math.Max(1, BarCount);
        var totalGap = Gap * (bars - 1);
        var width = double.IsInfinity(availableSize.Width) ? bars * 12 + totalGap : availableSize.Width;
        var height = double.IsInfinity(availableSize.Height) ? 24 : availableSize.Height;

        var barW = Math.Max(0, (width - totalGap) / bars);
        var barSize = new Size(barW, height);

        foreach (var b in _bars)
            b.Measure(barSize);

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var bars = Math.Max(1, BarCount);
        var totalGap = Gap * (bars - 1);
        var barW = Math.Max(0, (finalSize.Width - totalGap) / bars);
        var barH = finalSize.Height;
        
        var min = Minimum;
        var max = Maximum <= min ? min + 1 : Maximum;
        var t = SukiTheme.Clamp((Value - min) / (max - min), 0.0, 1.0);
        var filled = t * bars;
        var fullBars = (int)Math.Floor(filled);
        var partial = filled - fullBars;

        double x = 0;
        for (int i = 0; i < bars; i++)
        {
            var rect = new Rect(x, 0, barW, barH);
            _bars[i].Arrange(rect);

            if (_bars[i].Child is Border fill)
            {
                double w = 0;
                if (i < fullBars) w = barW;
                else if (i == fullBars && partial > 0) w = barW * partial;
                
                fill.CornerRadius = (w >= barW - 0.5)
                    ? CornerRadius
                    : new CornerRadius(CornerRadius.TopLeft, 0, 0, CornerRadius.BottomLeft);

                fill.Width = Math.Max(0, w);
            }

            x += barW + Gap;
        }

        return finalSize;
    }
}
