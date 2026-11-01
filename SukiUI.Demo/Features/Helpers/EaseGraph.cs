using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SukiUI.Motion;

namespace SukiUI.Demo.Features.Helpers
{
    /// <summary>The graph of an ease: the curve over the duration, the preset behind it as a ghost,
    /// and a dot that travels along the curve while it plays.</summary>
    public sealed class EaseGraph : Control
    {
        private static readonly Typeface Face = new("Inter, Segoe UI, sans-serif");

        public SukiEase? Ease { get; set; }
        public SukiEase? Ghost { get; set; }
        public int Milliseconds { get; set; } = 600;

        /// <summary>0 to 1 while playing; null to hide the dot.</summary>
        public double? Progress { get; set; }

        public EaseGraph()
        {
            MinHeight = 240;
            Height = 300;
        }

        public void Refresh() => InvalidateVisual();

        private IBrush Resource(string key, IBrush fallback) =>
            this.TryFindResource(key, ActualThemeVariant, out var value)
                ? value switch { IBrush brush => brush, Color color => new SolidColorBrush(color), _ => fallback }
                : fallback;

        private static Color ColorOf(IBrush brush) => brush is ISolidColorBrush solid ? solid.Color : Colors.DodgerBlue;

        private static (double Low, double High) Range(params SukiEase?[] eases)
        {
            double low = 0.0, high = 1.0;
            foreach (var ease in eases)
            {
                if (ease is null) continue;
                foreach (double v in ease.Trace(160))
                {
                    low = Math.Min(low, v);
                    high = Math.Max(high, v);
                }
            }
            return (low - 0.12, high + 0.12);
        }

        public override void Render(DrawingContext context)
        {
            var size = Bounds.Size;
            if (Ease is null || size.Width < 120 || size.Height < 80) return;

            var primary = Resource("SukiPrimaryColor", Brushes.DodgerBlue);
            var low = Resource("SukiLowText", Brushes.Gray);
            var border = Resource("SukiControlBorderBrush", Brushes.Gray);
            var accent = ColorOf(primary);

            var plot = new Rect(54, 14, size.Width - 54 - 20, size.Height - 14 - 34);
            var (lo, hi) = Range(Ease, Ghost);
            double X(double t) => plot.Left + t * plot.Width;
            double Y(double v) => plot.Bottom - (v - lo) / (hi - lo) * plot.Height;

            var gridPen = new Pen(border, 1) { DashStyle = new DashStyle(new double[] { 3, 4 }, 0) };
            var axisPen = new Pen(low, 1) { DashStyle = new DashStyle(new double[] { 5, 4 }, 0) };

            // time ticks, in the milliseconds of the chosen duration
            foreach (int step in new[] { 50, 100, 200, 250, 500, 1000, 2000 })
            {
                if (Milliseconds / (double)step > 6.5) continue;
                for (int ms = 0; ms <= Milliseconds; ms += step)
                {
                    double x = X(ms / (double)Milliseconds);
                    if (ms > 0) context.DrawLine(gridPen, new Point(x, plot.Top), new Point(x, plot.Bottom));
                    Text(context, ms == 0 ? "0" : $"{ms} ms", low, 11, new Point(x, plot.Bottom + 8), 0.5);
                }
                break;
            }

            // the start and the target
            context.DrawLine(axisPen, new Point(plot.Left, Y(0)), new Point(plot.Right, Y(0)));
            context.DrawLine(axisPen, new Point(plot.Left, Y(1)), new Point(plot.Right, Y(1)));
            Text(context, "start", low, 11, new Point(plot.Left - 8, Y(0)), 1.0, middle: true);
            Text(context, "target", low, 11, new Point(plot.Left - 8, Y(1)), 1.0, middle: true);

            int count = Math.Max(24, (int)plot.Width);
            Geometry Line(SukiEase ease, bool closed)
            {
                var geometry = new StreamGeometry();
                using var g = geometry.Open();
                var points = ease.Trace(count);
                g.BeginFigure(new Point(X(0), Y(points[0])), closed);
                for (int i = 1; i < count; i++)
                    g.LineTo(new Point(X((double)i / (count - 1)), Y(points[i])));
                if (closed)
                {
                    g.LineTo(new Point(X(1), Y(0)));
                    g.LineTo(new Point(X(0), Y(0)));
                    g.EndFigure(true);
                }
                else
                    g.EndFigure(false);
                return geometry;
            }

            if (Ghost is not null && !ReferenceEquals(Ghost, Ease) && !Ghost.Equals(Ease))
                context.DrawGeometry(null, new Pen(new SolidColorBrush(accent, 0.35), 2, lineCap: PenLineCap.Round), Line(Ghost, false));

            var fill = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.FromArgb(70, accent.R, accent.G, accent.B), 0),
                    new GradientStop(Color.FromArgb(0, accent.R, accent.G, accent.B), 1),
                },
            };
            context.DrawGeometry(fill, null, Line(Ease, true));

            context.DrawGeometry(null, new Pen(primary, 3.5, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round), Line(Ease, false));

            if (Progress is { } progress)
            {
                double t = Math.Clamp(progress, 0.0, 1.0);
                var dot = new Point(X(t), Y(Ease.Ease(t)));
                context.DrawLine(new Pen(new SolidColorBrush(accent, 0.35), 1), new Point(dot.X, plot.Top), new Point(dot.X, plot.Bottom));
                context.DrawEllipse(new SolidColorBrush(accent, 0.25), null, dot, 13, 13);
                context.DrawEllipse(primary, new Pen(Brushes.White, 2), dot, 6.5, 6.5);
            }
        }

        private static void Text(DrawingContext context, string text, IBrush brush, double size, Point at, double anchor, bool middle = false)
        {
            var formatted = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Face, size, brush);
            context.DrawText(formatted, new Point(at.X - formatted.Width * anchor, middle ? at.Y - formatted.Height / 2 : at.Y));
        }
    }

    /// <summary>A small thumbnail of an ease, to recognise a preset by its shape.</summary>
    public sealed class EaseThumbnail : Control
    {
        private readonly SukiEase _ease;
        private Geometry? _geometry;

        public bool Highlighted { get; set; }

        public EaseThumbnail(SukiEase ease)
        {
            _ease = ease;
            Width = 58;
            Height = 28;
        }

        public override void Render(DrawingContext context)
        {
            const double low = -0.35, high = 1.4;
            if (_geometry is null)
            {
                var geometry = new StreamGeometry();
                using (var g = geometry.Open())
                {
                    var points = _ease.Trace(48);
                    for (int i = 0; i < points.Length; i++)
                    {
                        var point = new Point(3 + (Bounds.Width - 6) * i / (points.Length - 1),
                            Bounds.Height - 3 - (points[i] - low) / (high - low) * (Bounds.Height - 6));
                        if (i == 0) g.BeginFigure(point, false); else g.LineTo(point);
                    }
                    g.EndFigure(false);
                }
                _geometry = geometry;
            }

            var key = Highlighted ? "SukiPrimaryColor" : "SukiLowText";
            IBrush brush = this.TryFindResource(key, ActualThemeVariant, out var value)
                ? value switch { IBrush b => b, Color c => new SolidColorBrush(c), _ => Brushes.Gray }
                : Brushes.Gray;
            context.DrawGeometry(null, new Pen(brush, Highlighted ? 2.2 : 1.6, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round), _geometry);
        }
    }
}
