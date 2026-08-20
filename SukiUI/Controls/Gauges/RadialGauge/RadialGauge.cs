using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using SukiUI.Extensions;
using Path = Avalonia.Controls.Shapes.Path;

namespace SukiUI.Controls.Gauges;

public class RadialGauge : Panel
{
    private static readonly IBrush DefaultTrailBrush = new ImmutableLinearGradientBrush(new LinearGradientBrush
    {
        EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
        StartPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
        GradientStops =
        {
            new GradientStop(Colors.White, 0),
            new GradientStop(Colors.Transparent, 0.7),
            new GradientStop(Colors.Transparent, 1)
        }
    });
    
    public static readonly StyledProperty<string> SubtitleTextProperty =
        AvaloniaProperty.Register<RadialGauge, string>(nameof(SubtitleText), "");
    
    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<RadialGauge, double>(nameof(Minimum), 0d);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<RadialGauge, double>(nameof(Maximum), 100d);

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<RadialGauge, double>(nameof(Value), 0d);
    
    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<RadialGauge, double>(nameof(StartAngle), -140d);

    public static readonly StyledProperty<double> EndAngleProperty =
        AvaloniaProperty.Register<RadialGauge, double>(nameof(EndAngle), -40d); 

    public static readonly StyledProperty<int> TickCountProperty =
        AvaloniaProperty.Register<RadialGauge, int>(nameof(TickCount), 10,
            coerce: (_, value) => Math.Max(0, value));

    public static readonly StyledProperty<double> TickSizeProperty =
        AvaloniaProperty.Register<RadialGauge, double>(nameof(TickSize), 4d,
            coerce: (_, value) => Math.Max(0, value));

    public static readonly StyledProperty<double> RimThicknessProperty =
        AvaloniaProperty.Register<RadialGauge, double>(nameof(RimThickness), 2d,
            coerce: (_, value) => Math.Max(0, value));

    public static readonly StyledProperty<IBrush?> RimBrushProperty =
        AvaloniaProperty.Register<RadialGauge, IBrush?>(nameof(RimBrush), Brushes.White);

    public static readonly StyledProperty<IBrush?> TickBrushProperty =
        AvaloniaProperty.Register<RadialGauge, IBrush?>(nameof(TickBrush), Brushes.DarkGray);

    public static readonly StyledProperty<IBrush?> NeedleBrushProperty =
        AvaloniaProperty.Register<RadialGauge, IBrush?>(nameof(NeedleBrush), Brushes.White);

    public static readonly StyledProperty<double> NeedleThicknessProperty =
        AvaloniaProperty.Register<RadialGauge, double>(nameof(NeedleThickness), 3d,
            coerce: (_, value) => Math.Max(1, value));

    public static readonly StyledProperty<double> NeedleLengthRatioProperty =
        AvaloniaProperty.Register<RadialGauge, double>(nameof(NeedleLengthRatio), 0.68d,
            coerce: (_, value) => SukiTheme.Clamp(value, 0.2, 0.95));

    public static readonly StyledProperty<IBrush?> BackgroundBrushProperty =
        AvaloniaProperty.Register<RadialGauge, IBrush?>(nameof(BackgroundBrush), Brushes.Transparent);
    
    public static readonly StyledProperty<IBrush?> TrailBrushProperty =
        AvaloniaProperty.Register<RadialGauge, IBrush?>(nameof(TrailBrush), DefaultTrailBrush);

    public static readonly StyledProperty<double> TrailThicknessProperty =
        AvaloniaProperty.Register<RadialGauge, double>(nameof(TrailThickness), 40d,
            coerce: (_, value) => Math.Max(1, value));

    public static readonly StyledProperty<IList<RadialGaugeSegment>?> SegmentsProperty =
        AvaloniaProperty.Register<RadialGauge, IList<RadialGaugeSegment>?>(nameof(Segments));
    
    public string SubtitleText { get => GetValue(SubtitleTextProperty); set => SetValue(SubtitleTextProperty, value); }
    public double Minimum { get => GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public double Value   { get => GetValue(ValueProperty);   set => SetValue(ValueProperty, value); }
    public double StartAngle { get => GetValue(StartAngleProperty); set => SetValue(StartAngleProperty, value); }
    public double EndAngle   { get => GetValue(EndAngleProperty);   set => SetValue(EndAngleProperty, value); }
    public int    TickCount  { get => GetValue(TickCountProperty);  set => SetValue(TickCountProperty, Math.Max(0, value)); }
    public double TickSize   { get => GetValue(TickSizeProperty);   set => SetValue(TickSizeProperty, Math.Max(0, value)); }
    public double RimThickness { get => GetValue(RimThicknessProperty); set => SetValue(RimThicknessProperty, Math.Max(0, value)); }
    public IBrush? RimBrush  { get => GetValue(RimBrushProperty);  set => SetValue(RimBrushProperty, value); }
    public IBrush? TickBrush { get => GetValue(TickBrushProperty); set => SetValue(TickBrushProperty, value); }
    public IBrush? NeedleBrush { get => GetValue(NeedleBrushProperty); set => SetValue(NeedleBrushProperty, value); }
    public double NeedleThickness { get => GetValue(NeedleThicknessProperty); set => SetValue(NeedleThicknessProperty, Math.Max(1, value)); }
    public double NeedleLengthRatio { get => GetValue(NeedleLengthRatioProperty); set => SetValue(NeedleLengthRatioProperty, SukiTheme.Clamp(value, 0.2, 0.95)); }
    public IBrush? BackgroundBrush { get => GetValue(BackgroundBrushProperty); set => SetValue(BackgroundBrushProperty, value); }

    public IBrush? TrailBrush { get => GetValue(TrailBrushProperty); set => SetValue(TrailBrushProperty, value); }
    public double  TrailThickness { get => GetValue(TrailThicknessProperty); set => SetValue(TrailThicknessProperty, Math.Max(1, value)); }
    public IList<RadialGaugeSegment>? Segments { get => GetValue(SegmentsProperty); set => SetValue(SegmentsProperty, value); }


    private Border _rim = null!;
    private Border _dial = null!;
    private List<Border> _ticks = new();
    private Grid _gridPath = null!;
    private Path _trailPath = null!;
    private Border _needle = null!;
    private TextBlock _valueText = null!;
    private TextBlock _subtitleText = null!;
    private StackPanel _stackText = null!;
    private List<Path> _segmentPaths = new();
    private Grid _segmentsGrid = null!;
    private readonly RotateTransform _needleTransform = new(0, 0, 0);
    private readonly ArcSegment _trailArc = new() { SweepDirection = SweepDirection.Clockwise };
    private readonly PathFigure _trailFigure;
    private readonly PathGeometry _trailGeometry;
    private LinearGradientBrush? _segmentTrailBrush;
    private Color _segmentTrailBrushColor;
    private double? _lastDisplayValue;
    private readonly HashSet<RadialGaugeSegment> _subscribedSegments = new();
    private bool _segmentsGeometryDirty = true;
    private Rect? _segmentsGeometryBounds;
    private bool _isAttached;
          

    static RadialGauge()
    {
        AffectsMeasure<RadialGauge>(TickCountProperty, RimThicknessProperty);
        AffectsArrange<RadialGauge>(MinimumProperty, MaximumProperty, ValueProperty,
                                    StartAngleProperty, EndAngleProperty,
                                    TickCountProperty, TickSizeProperty,
                                    NeedleThicknessProperty, NeedleLengthRatioProperty,
                                    TrailThicknessProperty, SegmentsProperty);

        RimBrushProperty.Changed.AddClassHandler<RadialGauge>((s, _) => s.UpdateColors());
        TickBrushProperty.Changed.AddClassHandler<RadialGauge>((s, _) => s.UpdateColors());
        NeedleBrushProperty.Changed.AddClassHandler<RadialGauge>((s, _) => s.UpdateColors());
        BackgroundBrushProperty.Changed.AddClassHandler<RadialGauge>((s, _) => s.UpdateColors());
        TrailBrushProperty.Changed.AddClassHandler<RadialGauge>((s, _) => s.UpdateColors());
        SegmentsProperty.Changed.AddClassHandler<RadialGauge>((s, e) => s.OnSegmentsChanged(e));
        SubtitleTextProperty.Changed.AddClassHandler<RadialGauge>((s, _) => s.UpdateSubtitle());
        RimThicknessProperty.Changed.AddClassHandler<RadialGauge>((s, _) => s.UpdateRimThickness());
        MinimumProperty.Changed.AddClassHandler<RadialGauge>((s, _) => s.InvalidateSegmentGeometry());
        MaximumProperty.Changed.AddClassHandler<RadialGauge>((s, _) => s.InvalidateSegmentGeometry());
        StartAngleProperty.Changed.AddClassHandler<RadialGauge>((s, _) => s.InvalidateSegmentGeometry());
        EndAngleProperty.Changed.AddClassHandler<RadialGauge>((s, _) => s.InvalidateSegmentGeometry());
    }

    public RadialGauge()
    {
        _trailFigure = new PathFigure
        {
            IsClosed = false,
            IsFilled = false,
            Segments = new PathSegments { _trailArc }
        };
        _trailGeometry = new PathGeometry { Figures = new PathFigures { _trailFigure } };
    }

    private void LoadControls()
    {
        
        
        _dial = new Border
        {
            Background = BackgroundBrush,
            BorderBrush = null,
            BorderThickness = new Thickness(0),
        };

       
        _rim = new Border
        {
            Opacity = 0.4,
            BorderBrush = RimBrush,
            BorderThickness = new Thickness(RimThickness),
            Background = null,
            IsHitTestVisible = false
        };


        _trailPath = new Path
        {
            Stroke = TrailBrush,
            StrokeThickness = TrailThickness,
            StrokeLineCap = PenLineCap.Flat,
            IsHitTestVisible = false
        };

        _gridPath = new Grid() {Opacity = 0.8, Effect = new BlurEffect() { Radius = 60 } };

        _segmentsGrid = new Grid() { IsHitTestVisible = false };

     
        _needle = new Border
        {
            Background = NeedleBrush,
            CornerRadius = new CornerRadius(32),
            RenderTransformOrigin = new RelativePoint(0, 0.5, RelativeUnit.Relative), 
            RenderTransform = _needleTransform,
            IsHitTestVisible = false,
            Classes = { "rg-needle" }
        };

        _valueText = new TextBlock()
        {
            Text = "",
            FontSize =SubtitleText != "" ? 28 : 32, 
            Margin = new Thickness(0, 0, 0, SubtitleText != "" ? 0 : 5),
            FontWeight = FontWeight.DemiBold,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom
        };
        
        _subtitleText = new TextBlock()
        {
            [!TextBlock.ForegroundProperty] = new DynamicResourceExtension("SukiMuteText"),
            FontSize = 13,
           Text = SubtitleText,
           IsVisible = SubtitleText != "",
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom
        };

        _stackText = new StackPanel()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom, Width = 100, 
            Spacing = 0, Margin = new Thickness(0, 0, 0, 10),
        };
        
        _stackText.Children.Add(_valueText);
        _stackText.Children.Add(_subtitleText);

    
        Children.Add(_dial);
        
        Children.Add(_rim); 
        _gridPath.Children.Add(_trailPath);
        Children.Add(_gridPath);
        Children.Add(_segmentsGrid);
        Children.Add(_needle);
        Children.Add(_stackText);

        RebuildTicks();
        RebuildSegments();
    }

    private bool IsInitialize = false;
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        AttachSegments(Segments);
        if (!IsInitialize)
        {
            IsInitialize = true;
            LoadControls();
        }
        
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        DetachSegments(Segments);
        base.OnDetachedFromVisualTree(e);
    }

    private void UpdateColors()
    {
        if (_rim == null)
            return; 
        
        _rim.BorderBrush = RimBrush;
        _dial.Background = BackgroundBrush;
        _needle.Background = NeedleBrush;
        _trailPath.Stroke = TrailBrush;
        _trailPath.StrokeThickness = TrailThickness;

        foreach (var t in _ticks) t.Background = TickBrush;
        InvalidateVisual();
    }

    private void UpdateSubtitle()
    {
        if (!IsInitialize) return;
        var hasSubtitle = !string.IsNullOrEmpty(SubtitleText);
        _subtitleText.Text = SubtitleText;
        _subtitleText.IsVisible = hasSubtitle;
        _valueText.FontSize = hasSubtitle ? 28 : 32;
        _valueText.Margin = new Thickness(0, 0, 0, hasSubtitle ? 0 : 5);
        InvalidateMeasure();
    }

    private void UpdateRimThickness()
    {
        if (!IsInitialize) return;
        _rim.BorderThickness = new Thickness(Math.Max(0, RimThickness));
        InvalidateSegmentGeometry();
        InvalidateArrange();
    }

    private void RebuildTicks()
    {
        foreach (var t in _ticks) Children.Remove(t);
        _ticks.Clear();

        var count = Math.Max(0, TickCount);
        for (int i = 0; i < count; i++)
        {
            var tick = new Border
            {
                Background = TickBrush,
                CornerRadius = new CornerRadius(3),
                IsHitTestVisible = false,
                Classes = { "rg-tick" }, 
                RenderTransform = new RotateTransform(0,0.5,0.5)
            };
            _ticks.Add(tick);
           
            Children.Insert(2, tick); 
        }

        InvalidateMeasure();
        InvalidateArrange();
    }

    private void RebuildSegments()
    {
        foreach (var path in _segmentPaths) _segmentsGrid.Children.Remove(path);
        _segmentPaths.Clear();

        if (Segments == null)
        {
            _segmentsGeometryDirty = false;
            return;
        }

        InvalidateSegmentGeometry();

        for (int i = 0; i < Segments.Count; i++)
        {
            var segment = Segments[i];
            var path = new Path
            {
                Stroke = new SolidColorBrush(segment.Color),
                StrokeThickness = segment.Thickness,
                StrokeLineCap = PenLineCap.Round,
                IsHitTestVisible = false
            };
            _segmentPaths.Add(path);

            _segmentsGrid?.Children.Add(path);
        }

        InvalidateMeasure();
        InvalidateArrange();
    }

    private void OnSegmentsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is IList<RadialGaugeSegment> oldCollection)
            DetachSegments(oldCollection);
        if (_isAttached && e.NewValue is IList<RadialGaugeSegment> newCollection)
            AttachSegments(newCollection);

        if (IsInitialize)
            RebuildSegments();
    }

    private void AttachSegments(IList<RadialGaugeSegment>? segments)
    {
        if (segments is null)
            return;
        if (segments is INotifyCollectionChanged notifyCollection)
        {
            notifyCollection.CollectionChanged -= OnSegmentsCollectionChanged;
            notifyCollection.CollectionChanged += OnSegmentsCollectionChanged;
        }
        foreach (var segment in segments)
            AttachSegment(segment);
    }

    private void DetachSegments(IList<RadialGaugeSegment>? segments)
    {
        if (segments is INotifyCollectionChanged notifyCollection)
            notifyCollection.CollectionChanged -= OnSegmentsCollectionChanged;
        foreach (var segment in _subscribedSegments)
            segment.PropertyChanged -= OnSegmentPropertyChanged;
        _subscribedSegments.Clear();
    }

    private void AttachSegment(RadialGaugeSegment segment)
    {
        if (!_subscribedSegments.Add(segment))
            return;
        segment.PropertyChanged += OnSegmentPropertyChanged;
    }

    private void OnSegmentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateSegmentGeometry();
        InvalidateArrange();
    }

    private void OnSegmentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var segment in _subscribedSegments)
                segment.PropertyChanged -= OnSegmentPropertyChanged;
            _subscribedSegments.Clear();
            if (Segments is not null)
                foreach (var segment in Segments)
                    AttachSegment(segment);
        }
        else
        {
            if (e.OldItems is not null)
                foreach (RadialGaugeSegment segment in e.OldItems)
                {
                    segment.PropertyChanged -= OnSegmentPropertyChanged;
                    _subscribedSegments.Remove(segment);
                }
            if (e.NewItems is not null)
                foreach (RadialGaugeSegment segment in e.NewItems)
                    AttachSegment(segment);
        }
        RebuildSegments();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double size = Math.Min(
            double.IsInfinity(availableSize.Width) ? 160 : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? 160 : availableSize.Height);

        var s = new Size(size, size);
        foreach (var c in Children)
            c.Measure(s);
        return s;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double size = Math.Min(finalSize.Width, finalSize.Height);
        var rect = new Rect((finalSize.Width - size) / 2, (finalSize.Height - size) / 2, size, size);

        _stackText.Arrange(rect);
        UpdateValueText();

        
        
    
        _dial.CornerRadius = new CornerRadius(size / 2);
        _dial.Arrange(rect);

        _rim.CornerRadius = new CornerRadius(size / 2);
        _rim.Arrange(rect);
        
        _segmentsGrid.Arrange(rect);
        UpdateSegmentsGeometry(rect);

        var center = rect.Center;
        double radius = size / 2.0;

    
        double tickRadius = radius - Math.Max(RimThickness, 2) - 8;
        double tickLen = TickSize;
        double tickWidth = Math.Max(2, 2);

    
        double sweep = NormalizeSweep(StartAngle, EndAngle);

      
        if (_ticks.Count != TickCount) RebuildTicks();
        for (int i = 0; i < _ticks.Count; i++)
        {
            double u = _ticks.Count <= 1 ? 0 : (double)i / (_ticks.Count - 1);
            double ang = Deg2Rad(StartAngle - u * sweep); 
            
            if (_ticks[i].RenderTransform is RotateTransform rotateTransform)
                rotateTransform.Angle = -(StartAngle - (u * sweep)) - 90;

            double cos = Math.Cos(ang);
            double sin = Math.Sin(ang);

            var cx = center.X + cos * tickRadius;
            var cy = center.Y - sin * tickRadius;

            var tickRect = new Rect(cx - tickWidth / 2, cy - tickLen, tickWidth, tickLen);
            _ticks[i].Arrange(tickRect);
        }

        double needleLen = radius * SukiTheme.Clamp(NeedleLengthRatio, 0.2, 0.95);
        double needleTh = NeedleThickness;


        double t = Normalize01(Value, Minimum, Maximum);
        double angle = StartAngle - t * sweep; 

        var needleRect = new Rect(center.X, center.Y - needleTh / 2, needleLen, needleTh);
        _needle.CornerRadius = new CornerRadius(needleTh / 2);
        _needle.Arrange(needleRect);
        _needleTransform.Angle = -angle;


        
        angle += 35;
        
        if (angle > StartAngle)
            angle = StartAngle - 2;
        
        double trailRadius = needleLen * 0.55;

   
        double deltaDeg = angleDistanceCW(StartAngle, angle );
        if (deltaDeg <= 0.01)
        {
            _trailPath.Data = null;
            return finalSize;
        }

        Point Pt(double angDeg)
        {
            double a = angDeg * Math.PI / 180.0;
            return new Point(
                center.X + trailRadius * Math.Cos(a),
                center.Y - trailRadius * Math.Sin(a)); 
        }

        var startPt = Pt(StartAngle);
        var endPt   = Pt(angle );

        bool isLarge = deltaDeg >= 180.0;

        _trailFigure.StartPoint = startPt;
        _trailArc.Point = endPt;
        _trailArc.Size = new Size(trailRadius, trailRadius);
        _trailArc.IsLargeArc = isLarge;

        _trailPath.Data = _trailGeometry;
        _trailPath.StrokeThickness = TrailThickness;
        
        IBrush trailBrush = GetTrailBrush();
        _trailPath.Stroke = trailBrush;
        _trailPath.StrokeLineCap = PenLineCap.Round;

        return finalSize;
    }

    private void UpdateSegmentsGeometry(Rect rect)
    {
        if (_segmentsGeometryBounds != rect)
        {
            _segmentsGeometryBounds = rect;
            _segmentsGeometryDirty = true;
        }

        if (_segmentPaths.Count != (Segments?.Count ?? 0))
        {
            RebuildSegments();
            return;
        }

        if (Segments == null || !_segmentsGeometryDirty)
            return;

        var center = rect.Center;
        double radius = rect.Width / 2.0;
        double sweep = NormalizeSweep(StartAngle, EndAngle);
        
        double segmentRadius = radius - Math.Max(RimThickness, 2) - 6;

        for (int i = 0; i < Segments.Count; i++)
        {
            var segment = Segments[i];
            var path = _segmentPaths[i];
            
            double fromT = Normalize01(segment.FromValue, Minimum, Maximum);
            double toT = Normalize01(segment.ToValue, Minimum, Maximum);
            
            double fromAngle = StartAngle - fromT * sweep;
            double toAngle = StartAngle - toT * sweep;
            
            double deltaDeg = angleDistanceCW(fromAngle, toAngle);
            
            if (deltaDeg <= 0.01)
            {
                path.Data = null;
                continue;
            }
            
            Point Pt(double angDeg)
            {
                double a = angDeg * Math.PI / 180.0;
                return new Point(
                    center.X + segmentRadius * Math.Cos(a),
                    center.Y - segmentRadius * Math.Sin(a));
            }

            var startPt = Pt(fromAngle);
            var endPt = Pt(toAngle);

            bool isLarge = deltaDeg >= 180.0;

            var fig = new PathFigure
            {
                StartPoint = startPt,
                IsClosed = false,
                IsFilled = false,
                Segments = new PathSegments
                {
                    new ArcSegment
                    {
                        Point = endPt,
                        Size = new Size(segmentRadius, segmentRadius),
                        IsLargeArc = isLarge,
                        SweepDirection = SweepDirection.Clockwise
                    }
                }
            };

            var geom = new PathGeometry();
            geom.Figures = new PathFigures { fig };

            path.Data = geom;
            path.StrokeThickness = segment.Thickness;
            path.Stroke = new SolidColorBrush(segment.Color);
            path.Opacity = segment.Opacity;
            path.StrokeLineCap = PenLineCap.Round;
        }

        _segmentsGeometryDirty = false;
    }

    private void InvalidateSegmentGeometry() => _segmentsGeometryDirty = true;

    private void UpdateValueText()
    {
        var displayValue = Math.Round(Value, MidpointRounding.AwayFromZero);
        if (_lastDisplayValue is { } last && last.Equals(displayValue))
            return;

        _lastDisplayValue = displayValue;
        _valueText.Text = displayValue.ToString("F0");
    }

    private static double Normalize01(double v, double min, double max)
    {
        if (max <= min) max = min + 1;
        var t = (v - min) / (max - min);
        return SukiTheme.Clamp(t, 0.0, 1.0);
    }

    private static double NormalizeSweep(double startDeg, double endDeg)
    {
        double sweep = startDeg - endDeg;
        if (sweep < 0) sweep += 360;
        return SukiTheme.Clamp(sweep, 5, 359); 
    }

    private static double Deg2Rad(double d) => Math.PI * d / 180.0;

    private static double angleDistanceCW(double a, double b)
    {
        double d = a - b;
        if (d < 0) d += 360;
        return d;
    }

    private IBrush GetTrailBrush()
    {
        if (Segments == null || Segments.Count == 0)
            return TrailBrush ?? Brushes.Transparent;
        
        foreach (var segment in Segments)
        {
            if (Value >= segment.FromValue && Value <= segment.ToValue)
            {
                var color = segment.Color.WithAlpha(0.75);
                if (_segmentTrailBrush is null || _segmentTrailBrushColor != color)
                {
                    _segmentTrailBrushColor = color;
                    _segmentTrailBrush = new LinearGradientBrush()
                    {
                        StartPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                        EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                        GradientStops = new GradientStops()
                        {
                            new GradientStop()
                            {
                                Offset = 0,
                                Color = color
                            },

                            new GradientStop()
                            {
                                Offset = 0.7,
                                Color = Colors.Transparent
                            },
                            new GradientStop()
                            {
                                Offset = 1,
                                Color = Colors.Transparent
                            },
                        }
                    };
                }

                return _segmentTrailBrush;
            }
        }

        return TrailBrush ?? Brushes.Transparent;
    }
}
