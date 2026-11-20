using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

/// <summary>Insulation colour of a <see cref="Wire"/>.</summary>
public enum WireInsulation
{
    Neutral,
    Black,
    Red
}

public sealed record WireGeometrySamples(
    SKPoint[] Points,
    SKPoint[] Normals,
    float[] CumLengths,
    float TotalLength,
    SKPoint FadeDir)
{
    public void SampleAt(float s, out SKPoint pt, out SKPoint normal)
    {
        if (Points.Length == 0)
        {
            pt = default;
            normal = new SKPoint(0, 1);
            return;
        }
        s = Math.Clamp(s, 0f, TotalLength);
        var i = 1;
        while (i < CumLengths.Length - 1 && CumLengths[i] < s) i++;
        var len = CumLengths[i] - CumLengths[i - 1];
        var t = len > 1e-5f ? (s - CumLengths[i - 1]) / len : 0f;
        var a = Points[i - 1];
        var b = Points[i];
        pt = new SKPoint(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
        normal = Normals[Math.Min(i - 1, Normals.Length - 1)];
    }
}

public readonly record struct WireState(
    WireGeometrySamples? Geometry,
    int GeometryVersion,
    WireInsulation Insulation,
    double Current,
    bool FadeStart,
    int TriggerSeq,
    double BoundsWidth,
    double BoundsHeight,
    SkeumoPalette Palette);

/// <summary>
/// Skeumo-Suki insulated wire linking two parts of a scene (a switch and the device it powers, for instance).
/// Display only: when current flows, a faint highlight drifts slowly along the wire (stronger with the current);
/// a command is carried by a single impulse sent on each <see cref="Trigger"/> change. The start can melt into the card.
/// Drawing and animation live in <c>Wire.Rendering.cs</c>.
/// </summary>
public partial class Wire : HyperControl<WireState>
{
    /// <summary>Path of the wire, from where the current comes from to where it goes.</summary>
    public static readonly StyledProperty<Geometry?> DataProperty =
        AvaloniaProperty.Register<Wire, Geometry?>(nameof(Data));

    public static readonly StyledProperty<WireInsulation> InsulationProperty =
        AvaloniaProperty.Register<Wire, WireInsulation>(nameof(Insulation));

    /// <summary>Current reported by the circuit: 0 = none .. 1 = full load.</summary>
    public static readonly StyledProperty<double> CurrentProperty =
        AvaloniaProperty.Register<Wire, double>(nameof(Current), coerce: (_, v) => Math.Clamp(v, 0, 1));

    /// <summary>When true, the start of the wire fades into the card (a cable coming from outside the scene).</summary>
    public static readonly StyledProperty<bool> FadeStartProperty =
        AvaloniaProperty.Register<Wire, bool>(nameof(FadeStart));

    /// <summary>
    /// Event-driven signal: every time this value changes, one electric impulse travels along the wire.
    /// Bind it to the command the wire carries (a setting, a selected index); it never pulses on its own.
    /// </summary>
    public static readonly StyledProperty<double> TriggerProperty =
        AvaloniaProperty.Register<Wire, double>(nameof(Trigger));

    static Wire() => AffectsRenderState(DataProperty, InsulationProperty, CurrentProperty, FadeStartProperty, BoundsProperty);

    private WireGeometrySamples? _samples;
    private int _geometryVersion;
    private int _triggerSeq;
    private bool _armed;

    public Wire()
    {
        IsHitTestVisible = false;
        AttachedToVisualTree += (_, _) => _armed = true;
        DetachedFromVisualTree += (_, _) => _armed = false;
    }

    public double Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    public Geometry? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public WireInsulation Insulation
    {
        get => GetValue(InsulationProperty);
        set => SetValue(InsulationProperty, value);
    }

    public double Current
    {
        get => GetValue(CurrentProperty);
        set => SetValue(CurrentProperty, value);
    }

    public bool FadeStart
    {
        get => GetValue(FadeStartProperty);
        set => SetValue(FadeStartProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize) => default;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DataProperty)
        {
            _samples = SampleGeometry(change.GetNewValue<Geometry?>());
            _geometryVersion++;
            Refresh();
        }
        else if (change.Property == TriggerProperty && _armed)
        {
            _triggerSeq++;
            Refresh();
        }
    }

    private static WireGeometrySamples? SampleGeometry(Geometry? data)
    {
        if (data == null) return null;
        var length = data.ContourLength;
        if (length <= 0.1) return null;

        var count = Math.Clamp((int)Math.Ceiling(length / 2.0), 16, 512);
        var pts = new List<SKPoint>(count + 1);
        var nrms = new List<SKPoint>(count + 1);
        var cum = new List<float>(count + 1);

        var total = 0f;
        SKPoint prev = default;
        var fadeDir = new SKPoint(1, 0);

        for (var i = 0; i <= count; i++)
        {
            var d = length * i / count;
            if (!data.TryGetPointAndTangentAtDistance(d, out var pt, out var tg)) continue;
            var p = new SKPoint((float)pt.X, (float)pt.Y);
            var tl = (float)Math.Sqrt(tg.X * tg.X + tg.Y * tg.Y);
            var dir = tl > 1e-5f ? new SKPoint((float)(tg.X / tl), (float)(tg.Y / tl)) : new SKPoint(1, 0);
            if (i == 0) fadeDir = dir;
            if (pts.Count > 0)
            {
                var dx = p.X - prev.X;
                var dy = p.Y - prev.Y;
                total += (float)Math.Sqrt(dx * dx + dy * dy);
            }
            pts.Add(p);
            nrms.Add(new SKPoint(-dir.Y, dir.X));
            cum.Add(total);
            prev = p;
        }

        if (pts.Count < 2 || total <= 0.1f) return null;
        return new WireGeometrySamples(pts.ToArray(), nrms.ToArray(), cum.ToArray(), total, fadeDir);
    }

    protected override WireState CaptureState() =>
        new(_samples, _geometryVersion, Insulation, Current, FadeStart, _triggerSeq, Math.Max(1, Bounds.Width), Math.Max(1, Bounds.Height), SkeumoSkia.CapturePalette(this));
}
