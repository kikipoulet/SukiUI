using System;
using Avalonia;
using Avalonia.Data;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Mini3D.Controls;

/// <summary>
/// Interactive CoreXY-style desktop 3D printer backed by <see cref="Printer3DScene"/>.
/// Exposes bindable Avalonia properties for print state, bed/toolhead axes, spool rotation, and progress;
/// 3D scene definition lives in <c>Printer3D.Scene.cs</c>.
/// </summary>
public sealed class Printer3D : Model3DControl<Printer3DScene>
{
    public static readonly StyledProperty<bool> IsPrintingProperty =
        AvaloniaProperty.Register<Printer3D, bool>(nameof(IsPrinting), true, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> BedYProperty =
        AvaloniaProperty.Register<Printer3D, double>(nameof(BedY), 0.5,
            coerce: (_, v) => Math.Clamp(v, 0, 1));

    public static readonly StyledProperty<double> ToolXProperty =
        AvaloniaProperty.Register<Printer3D, double>(nameof(ToolX), 0.5,
            coerce: (_, v) => Math.Clamp(v, 0, 1));

    public static readonly StyledProperty<double> SpoolTurnsProperty =
        AvaloniaProperty.Register<Printer3D, double>(nameof(SpoolTurns), 0.0);

    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<Printer3D, double>(nameof(Progress), 0.35,
            coerce: (_, v) => Math.Clamp(v, 0, 1));

    static Printer3D()
    {
        BindChannel<Printer3D>(IsPrintingProperty, "printing");
        BindChannel<Printer3D>(BedYProperty, "bedY");
        BindChannel<Printer3D>(ToolXProperty, "toolX");
        BindChannel<Printer3D>(SpoolTurnsProperty, "spool");
        BindChannel<Printer3D>(ProgressProperty, "progress");
    }

    public bool IsPrinting
    {
        get => GetValue(IsPrintingProperty);
        set => SetValue(IsPrintingProperty, value);
    }

    public double BedY
    {
        get => GetValue(BedYProperty);
        set => SetValue(BedYProperty, value);
    }

    public double ToolX
    {
        get => GetValue(ToolXProperty);
        set => SetValue(ToolXProperty, value);
    }

    public double SpoolTurns
    {
        get => GetValue(SpoolTurnsProperty);
        set => SetValue(SpoolTurnsProperty, value);
    }

    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    protected override void OnModelClicked(string id)
    {
        if (id == "print")
            SetCurrentValue(IsPrintingProperty, !IsPrinting);

        base.OnModelClicked(id);
    }
}
