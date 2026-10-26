using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SukiUI.Controls
{
    /// <summary>
    /// A check mark drawn as a stroke. <see cref="Progress"/> is how much of the mark is drawn
    /// (the head); <see cref="Trail"/> follows it: the span between the two is drawn wider and
    /// translucent — a halo behind the head while drawing, an afterimage while erasing. At
    /// rest the two are equal and the trail is empty. Designed on a 12x12 box, scaled to its bounds.
    /// </summary>
    public sealed class CheckStroke : Control
    {
        public static readonly StyledProperty<double> ProgressProperty =
            AvaloniaProperty.Register<CheckStroke, double>(nameof(Progress));

        public static readonly StyledProperty<double> TrailProperty =
            AvaloniaProperty.Register<CheckStroke, double>(nameof(Trail));

        public static readonly StyledProperty<IBrush?> StrokeProperty =
            AvaloniaProperty.Register<CheckStroke, IBrush?>(nameof(Stroke));

        public static readonly StyledProperty<double> ThicknessProperty =
            AvaloniaProperty.Register<CheckStroke, double>(nameof(Thickness), 1.8);

        public static readonly StyledProperty<double> TrailOpacityProperty =
            AvaloniaProperty.Register<CheckStroke, double>(nameof(TrailOpacity), 0.35);

        static CheckStroke() =>
            AffectsRender<CheckStroke>(ProgressProperty, TrailProperty, StrokeProperty,
                ThicknessProperty, TrailOpacityProperty);

        public double Progress { get => GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }
        public double Trail { get => GetValue(TrailProperty); set => SetValue(TrailProperty, value); }
        public IBrush? Stroke { get => GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }
        public double Thickness { get => GetValue(ThicknessProperty); set => SetValue(ThicknessProperty, value); }
        public double TrailOpacity { get => GetValue(TrailOpacityProperty); set => SetValue(TrailOpacityProperty, value); }

        // Gap (fraction of the mark) below which the trail is not drawn, and above which it
        // is at full opacity.
        private const double MinTrailGap = 0.04, FullTrailGap = 0.3;

        // Head position below which the stroke is not drawn, and above which it is opaque.
        private const double MinHead = 0.03, FullHead = 0.2;

        private static readonly Point Start = new(1.6, 6.4), Corner = new(4.7, 9.5), End = new(10.4, 2.9);
        private static readonly double ShortLeg = Distance(Start, Corner);
        private static readonly double Length = ShortLeg + Distance(Corner, End);

        private static double Distance(Point a, Point b) =>
            Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));

        public override void Render(DrawingContext context)
        {
            if (Stroke is not { } brush)
                return;

            double head = Math.Clamp(Progress, 0.0, 1.0);
            double trail = Math.Clamp(Trail, 0.0, 1.0);
            double scale = Math.Min(Bounds.Width, Bounds.Height) / 12.0;

            using (context.PushTransform(Matrix.CreateScale(scale, scale)))
            {
                // The trail fades with its own length: it never lingers as a dot at the end
                // of the stroke while it catches up with the head.
                double gap = Math.Abs(head - trail);
                if (gap > MinTrailGap)
                {
                    using (context.PushOpacity(TrailOpacity * Math.Min(1.0, gap / FullTrailGap)))
                        DrawSpan(context, brush, Thickness * 1.5, Math.Min(head, trail), Math.Max(head, trail));
                }
                // Same for the stroke itself: a head about to vanish (or just born) is a round
                // cap with no length, a bright dot — it fades out before it gets there.
                if (head > MinHead)
                {
                    using (context.PushOpacity(Math.Min(1.0, head / FullHead)))
                        DrawSpan(context, brush, Thickness, 0.0, head);
                }
            }
        }

        private static void DrawSpan(DrawingContext context, IBrush brush, double thickness, double from, double to)
        {
            if (to - from < 0.001)
                return;

            var geometry = new StreamGeometry();
            using (var g = geometry.Open())
            {
                g.BeginFigure(PointAt(from), false);
                if (from < ShortLeg / Length && to > ShortLeg / Length)
                    g.LineTo(Corner);
                g.LineTo(PointAt(to));
                g.EndFigure(false);
            }

            context.DrawGeometry(null,
                new Pen(brush, thickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round), geometry);
        }

        private static Point PointAt(double t)
        {
            double d = t * Length;
            return d <= ShortLeg
                ? Start + (Corner - Start) * (d / ShortLeg)
                : Corner + (End - Corner) * ((d - ShortLeg) / (Length - ShortLeg));
        }
    }
}
