
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
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
    
    public static readonly StyledProperty<bool> ShowValueProperty =
        AvaloniaProperty.Register<HorizontalBarMeter, bool>(nameof(ShowValue), false);

    public static readonly StyledProperty<string?> SuffixProperty =
        AvaloniaProperty.Register<HorizontalBarMeter, string?>(nameof(Suffix), string.Empty);

    public static readonly StyledProperty<string?> StartTextProperty =
        AvaloniaProperty.Register<HorizontalBarMeter, string?>(nameof(StartText), string.Empty);

    public static readonly StyledProperty<string?> EndTextProperty =
        AvaloniaProperty.Register<HorizontalBarMeter, string?>(nameof(EndText), string.Empty);
    
    public int BarCount { get => GetValue(BarCountProperty); set => SetValue(BarCountProperty, Math.Max(1, value)); }
    public double Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public double Minimum { get => GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public double Gap { get => GetValue(GapProperty); set => SetValue(GapProperty, Math.Max(0, value)); }
    public CornerRadius CornerRadius { get => GetValue(CornerRadiusProperty); set => SetValue(CornerRadiusProperty, value); }
    public IBrush? ActiveBrush { get => GetValue(ActiveBrushProperty); set => SetValue(ActiveBrushProperty, value); }
    public IBrush? InactiveBrush { get => GetValue(InactiveBrushProperty); set => SetValue(InactiveBrushProperty, value); }

    public bool ShowValue { get => GetValue(ShowValueProperty); set => SetValue(ShowValueProperty, value); }
    public string? Suffix { get => GetValue(SuffixProperty); set => SetValue(SuffixProperty, value); }
    public string? StartText { get => GetValue(StartTextProperty); set => SetValue(StartTextProperty, value); }
    public string? EndText { get => GetValue(EndTextProperty); set => SetValue(EndTextProperty, value); }

    /// 

    private readonly List<Border> _bars = new();
    
    private readonly Border _marker = new()
    {
        [!Border.BackgroundProperty] = new DynamicResourceExtension("SukiNeedleBrush"), Margin = new Thickness(0,25),
        CornerRadius = new CornerRadius(6),
        IsHitTestVisible = false,
        Classes = { "hbm-marker" }
    };

    private readonly TextBlock _valueText = new()
    {
        [!TextBlock.ForegroundProperty] = new DynamicResourceExtension("SukiText"),
        FontSize = 17,
        FontWeight = Avalonia.Media.FontWeight.Bold,
        TextAlignment = TextAlignment.Center,
        IsHitTestVisible = false,
        Classes = { "hbm-value" }
    };

    private readonly TextBlock _startText = new()
    {
        [!TextBlock.ForegroundProperty] = new DynamicResourceExtension("SukiLowText"),
        FontSize = 15, Margin = new Thickness(0,-8), VerticalAlignment = VerticalAlignment.Bottom, FontWeight = FontWeight.DemiBold,
        HorizontalAlignment = HorizontalAlignment.Left,
        TextAlignment = TextAlignment.Left,
        IsHitTestVisible = false,
        Classes = { "hbm-start" }
    };

    private readonly TextBlock _endText = new()
    {
        [!TextBlock.ForegroundProperty] = new DynamicResourceExtension("SukiLowText"),
        FontSize = 15, Margin = new Thickness(0,-8),
        VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Right, FontWeight = FontWeight.DemiBold,
        TextAlignment = TextAlignment.Right,
        IsHitTestVisible = false,
        Classes = { "hbm-end" }
    };

    /// 

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
        ShowValueProperty.Changed.AddClassHandler<HorizontalBarMeter>((s, _) => s.OnShowValueChanged());
        SuffixProperty.Changed.AddClassHandler<HorizontalBarMeter>((s, _) => s.InvalidateArrange());
        StartTextProperty.Changed.AddClassHandler<HorizontalBarMeter>((s, _) => s.InvalidateArrange());
        EndTextProperty.Changed.AddClassHandler<HorizontalBarMeter>((s, _) => s.InvalidateArrange());
    }

    public HorizontalBarMeter()
    {
        RebuildBars();

        Children.Add(_marker);
        Children.Add(_valueText);
        Children.Add(_startText);
        Children.Add(_endText);
        
        _marker.IsVisible = false;
        _valueText.IsVisible = false;

        _marker.ZIndex = 1000;
        _valueText.ZIndex = 1001;
    }

    private void OnShowValueChanged()
    {
        var visible = ShowValue;
        _marker.IsVisible = visible;
        _valueText.IsVisible = visible;
        InvalidateArrange();
    }

    private void RebuildBars()
    {
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            if (Children[i] is Border b && (b == _marker || b == _valueText.Parent)) continue;
        }

        for (int i = Children.Count - 1; i >= 0; i--)
        {
            if (Children[i] is Border b && (b != _marker))
            {
                if (!ReferenceEquals(b, _marker) && b != _valueText.Parent)
                    Children.RemoveAt(i);
            }
        }

        _bars.Clear();

        var count = Math.Max(1, BarCount);
        for (int i = 0; i < count; i++)
        {
            var bar = new Border
            {
                Background = InactiveBrush,
                Margin = new Thickness(0,20),
                CornerRadius = CornerRadius,
                Classes = { "hbm-segment" },
                ClipToBounds = true
            };

            var fill = new Border
            {
                Background = ActiveBrush,
                CornerRadius = CornerRadius,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 0,
                Classes = { "hbm-fill" }
            };

            bar.Child = fill;
            _bars.Add(bar);
            Children.Insert(i, bar);
        }
        
        UpdateBrushes();
        InvalidateMeasure();
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

    private void UpdateBarCorners()
    {
        foreach (var b in _bars)
        {
            b.CornerRadius = CornerRadius;
            if (b.Child is Border fill)
                fill.CornerRadius = CornerRadius;
        }
        _marker.CornerRadius = new CornerRadius(Math.Max(6, CornerRadius.TopLeft + 2));
        InvalidateArrange();
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
        
        _marker.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        _valueText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        _startText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        _endText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

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
        var t = Clamp((Value - min) / (max - min), 0.0, 1.0);
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


        if (ShowValue && bars > 0 && (fullBars > 0 || partial > 0))
        {
            int lastActiveIndex = partial > 0 ? fullBars : Math.Max(0, fullBars - 1);
            
            double anchorX = lastActiveIndex * (barW + Gap);
            
            double markerW = barW * 1.15;    
            double markerH = barH * 1.35;     
            double markerLeft = anchorX - (markerW - barW) / 2;
            
            markerLeft = Math.Max(0, Math.Min(markerLeft, finalSize.Width - markerW));

            double markerTop = -(markerH - barH) / 2; 

            _marker.IsVisible = true;
            _marker.Arrange(new Rect(markerLeft, markerTop, markerW, markerH));
            
            string text = FormatValue(Math.Round(Value)) + (string.IsNullOrEmpty(Suffix) ? "" : Suffix);
            _valueText.Text = text;

            _valueText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var textSize = _valueText.DesiredSize;

            double centerX = markerLeft + markerW / 2;
            double textLeft = centerX - textSize.Width / 2;
            textLeft = Math.Max(0, Math.Min(textLeft, finalSize.Width - textSize.Width));

            double textTop = markerTop + (textSize.Height ) -30; 

            _valueText.IsVisible = true;
            _valueText.Arrange(new Rect(textLeft, textTop, textSize.Width, textSize.Height));
        }
        else
        {
            _marker.IsVisible = false;
            _valueText.IsVisible = false;
        }

        _startText.Text = StartText;
        _endText.Text = EndText;

        var startSize = _startText.DesiredSize;
        var endSize = _endText.DesiredSize;

        _startText.Arrange(new Rect(0, finalSize.Height - startSize.Height, startSize.Width, startSize.Height));
        _endText.Arrange(new Rect(finalSize.Width - endSize.Width, finalSize.Height - endSize.Height, endSize.Width, endSize.Height));

        return finalSize;
    }


    private static double Clamp(double v, double min, double max) => v < min ? min : (v > max ? max : v);


    private static string FormatValue(double v)
    {
        return Math.Abs(v % 1.0) < 1e-9 ? v.ToString("0") : v.ToString("0.##");
    }
}