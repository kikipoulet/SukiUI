using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;

namespace SukiUI.Controls.Gauges.VerticalBarMeter;


public class VerticalBarMeter : Panel
{
    
    
    public static readonly StyledProperty<int> BarCountProperty =
        AvaloniaProperty.Register<VerticalBarMeter, int>(nameof(BarCount), 12);

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<VerticalBarMeter, double>(nameof(Value), 0d);

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<VerticalBarMeter, double>(nameof(Minimum), 0d);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<VerticalBarMeter, double>(nameof(Maximum), 100d);

    public static readonly StyledProperty<double> GapProperty =
        AvaloniaProperty.Register<VerticalBarMeter, double>(nameof(Gap), 6d);

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<VerticalBarMeter, CornerRadius>(nameof(CornerRadius), new CornerRadius(4));

    public static readonly StyledProperty<IBrush?> ActiveBrushProperty =
        AvaloniaProperty.Register<VerticalBarMeter, IBrush?>(nameof(ActiveBrush), Brushes.MediumPurple);

    public static readonly StyledProperty<IBrush?> InactiveBrushProperty =
        AvaloniaProperty.Register<VerticalBarMeter, IBrush?>(nameof(InactiveBrush), Brushes.Gray);
    
    public static readonly StyledProperty<bool> ShowValueProperty =
        AvaloniaProperty.Register<VerticalBarMeter, bool>(nameof(ShowValue), false);

    public static readonly StyledProperty<string?> SuffixProperty =
        AvaloniaProperty.Register<VerticalBarMeter, string?>(nameof(Suffix), string.Empty);

    public static readonly StyledProperty<string?> StartTextProperty =
        AvaloniaProperty.Register<VerticalBarMeter, string?>(nameof(StartText), string.Empty);

    public static readonly StyledProperty<string?> EndTextProperty =
        AvaloniaProperty.Register<VerticalBarMeter, string?>(nameof(EndText), string.Empty);
    
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
        [!Border.BackgroundProperty] = new DynamicResourceExtension("SukiNeedleBrush"), Margin = new Thickness(25,0),
        CornerRadius = new CornerRadius(6),
        IsHitTestVisible = false,
        Classes = { "vbm-marker" }
    };

    private readonly TextBlock _valueText = new()
    {
        [!TextBlock.ForegroundProperty] = new DynamicResourceExtension("SukiText"),
        FontSize = 17, HorizontalAlignment = HorizontalAlignment.Right,
        FontWeight = Avalonia.Media.FontWeight.Bold,
        TextAlignment = TextAlignment.Center,
        IsHitTestVisible = false,
        Classes = { "vbm-value" }
    };

    private readonly TextBlock _startText = new()
    {
        [!TextBlock.ForegroundProperty] = new DynamicResourceExtension("SukiLowText"),
        FontSize = 15, Margin = new Thickness(-25,-5), HorizontalAlignment = HorizontalAlignment.Left, FontWeight = FontWeight.DemiBold,
        VerticalAlignment = VerticalAlignment.Bottom,
        TextAlignment = TextAlignment.Left,
        IsHitTestVisible = false,
        Classes = { "vbm-start" }
    };

    private readonly TextBlock _endText = new()
    {
        [!TextBlock.ForegroundProperty] = new DynamicResourceExtension("SukiLowText"),
        FontSize = 15, Margin = new Thickness(-25,-5),
        HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, FontWeight = FontWeight.DemiBold,
        TextAlignment = TextAlignment.Left,
        IsHitTestVisible = false,
        Classes = { "vbm-end" }
    };

    /// 

    static VerticalBarMeter()
    {
        BarCountProperty.Changed.AddClassHandler<VerticalBarMeter>((s, _) => s.RebuildBars());
        GapProperty.Changed.AddClassHandler<VerticalBarMeter>((s, _) => s.InvalidateMeasure());
        ValueProperty.Changed.AddClassHandler<VerticalBarMeter>((s, _) => s.InvalidateArrange());
        MinimumProperty.Changed.AddClassHandler<VerticalBarMeter>((s, _) => s.InvalidateArrange());
        MaximumProperty.Changed.AddClassHandler<VerticalBarMeter>((s, _) => s.InvalidateArrange());
        CornerRadiusProperty.Changed.AddClassHandler<VerticalBarMeter>((s, _) => s.UpdateBarCorners());
        ActiveBrushProperty.Changed.AddClassHandler<VerticalBarMeter>((s, _) => s.UpdateBrushes());
        InactiveBrushProperty.Changed.AddClassHandler<VerticalBarMeter>((s, _) => s.UpdateBrushes());
        ShowValueProperty.Changed.AddClassHandler<VerticalBarMeter>((s, _) => s.OnShowValueChanged());
        SuffixProperty.Changed.AddClassHandler<VerticalBarMeter>((s, _) => s.InvalidateArrange());
        StartTextProperty.Changed.AddClassHandler<VerticalBarMeter>((s, _) => s.InvalidateArrange());
        EndTextProperty.Changed.AddClassHandler<VerticalBarMeter>((s, _) => s.InvalidateArrange());
    }

    public VerticalBarMeter()
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
                Margin = new Thickness(20,0),
                CornerRadius = CornerRadius,
                Classes = { "vbm-segment" },
                ClipToBounds = true
            };

            var fill = new Border
            {
                Background = ActiveBrush,
                CornerRadius = CornerRadius,
                VerticalAlignment = VerticalAlignment.Bottom,
                Height = 0,
                Classes = { "vbm-fill" }
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
        var width = double.IsInfinity(availableSize.Width) ? 24 : availableSize.Width;
        var height = double.IsInfinity(availableSize.Height) ? bars * 12 + totalGap : availableSize.Height;

        var barH = Math.Max(0, (height - totalGap) / bars);
        var barSize = new Size(width, barH);

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
        var barW = finalSize.Width;
        var barH = Math.Max(0, (finalSize.Height - totalGap) / bars);

        var min = Minimum;
        var max = Maximum <= min ? min + 1 : Maximum;
        var t = Clamp((Value - min) / (max - min), 0.0, 1.0);
        var filled = t * bars;
        var fullBars = (int)Math.Floor(filled);
        var partial = filled - fullBars;

        double y = 0;
        for (int i = 0; i < bars; i++)
        {
            var rect = new Rect(0, y, barW, barH);
            _bars[i].Arrange(rect);

            if (_bars[i].Child is Border fill)
            {
                double h = 0;
                // Inverser la logique : les barres du bas se remplissent en premier
                int reversedIndex = bars - 1 - i;
                if (reversedIndex < fullBars) h = barH;
                else if (reversedIndex == fullBars && partial > 0) h = barH * partial;

                fill.CornerRadius = (h >= barH - 0.5)
                    ? CornerRadius
                    : new CornerRadius(0, 0, CornerRadius.BottomLeft, CornerRadius.BottomRight);

                fill.Height = Math.Max(0, h);
            }

            y += barH + Gap;
        }


        if (ShowValue && bars > 0 && (fullBars > 0 || partial > 0))
        {
            int lastActiveIndex = partial > 0 ? fullBars : Math.Max(0, fullBars - 1);
            // Inverser l'index pour le positionnement du marqueur (bas en haut)
            int reversedIndex = bars - 1 - lastActiveIndex;
            double anchorY = reversedIndex * (barH + Gap);
            
            double markerW = barW * 1.35;    
            double markerH = barH * 1.15;     
            double markerTop = anchorY - (markerH - barH) / 2;
            
            markerTop = Math.Max(0, Math.Min(markerTop, finalSize.Height - markerH));

            double markerLeft = (finalSize.Width /2) - (markerW/2); // Position à droite

            _marker.IsVisible = true;
            _marker.Arrange(new Rect(markerLeft, markerTop, markerW, markerH));
            
            string text = FormatValue(Math.Round(Value)) + (string.IsNullOrEmpty(Suffix) ? "" : Suffix);
            _valueText.Text = text;

            _valueText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var textSize = _valueText.DesiredSize;

            double centerY = markerTop + markerH / 2;
            double textTop = centerY - textSize.Height / 2;
            textTop = Math.Max(0, Math.Min(textTop, finalSize.Height - textSize.Height));

            double textRight = markerLeft + (markerW *0.85);

            _valueText.IsVisible = true;
            _valueText.Arrange(new Rect(textRight, textTop, textSize.Width, textSize.Height));
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

       
        _endText.Arrange(new Rect(0, 0, startSize.Width, startSize.Height));
        _startText.Arrange(new Rect(0, finalSize.Height - endSize.Height, endSize.Width, endSize.Height));

        return finalSize;
    }


    private static double Clamp(double v, double min, double max) => v < min ? min : (v > max ? max : v);


    private static string FormatValue(double v)
    {
        return Math.Abs(v % 1.0) < 1e-9 ? v.ToString("0") : v.ToString("0.##");
    }
}
