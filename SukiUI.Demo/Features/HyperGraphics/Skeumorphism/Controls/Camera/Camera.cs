using Avalonia;
using Avalonia.Layout;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public readonly record struct CameraState(
    double Exposure,
    double Gain,
    double Fps,
    SkeumoPalette Palette);

/// <summary>
/// Skeumo-Suki short industrial camera facing a particle cell.
/// Display only: the exposure sets the light (beam, lens, chamber), the gain brightens and pixelates the particles,
/// the frame rate paces the data pulses on the cable. Drawing and animation live in <c>Camera.Rendering.cs</c>.
/// </summary>
public partial class Camera : HyperControl<CameraState>
{
    /// <summary>Exposure time in microseconds.</summary>
    public static readonly StyledProperty<double> ExposureProperty =
        AvaloniaProperty.Register<Camera, double>(nameof(Exposure), 600);

    /// <summary>Gain multiplier (1 = no gain).</summary>
    public static readonly StyledProperty<double> GainProperty =
        AvaloniaProperty.Register<Camera, double>(nameof(Gain), 2);

    /// <summary>Frame rate in frames per second.</summary>
    public static readonly StyledProperty<double> FpsProperty =
        AvaloniaProperty.Register<Camera, double>(nameof(Fps), 10);

    static Camera() => AffectsRenderState(ExposureProperty, GainProperty, FpsProperty);

    public Camera()
    {
        Width = 268;
        Height = 93;
        HorizontalAlignment = HorizontalAlignment.Center;
        IsHitTestVisible = false;
    }

    public double Exposure
    {
        get => GetValue(ExposureProperty);
        set => SetValue(ExposureProperty, value);
    }

    public double Gain
    {
        get => GetValue(GainProperty);
        set => SetValue(GainProperty, value);
    }

    public double Fps
    {
        get => GetValue(FpsProperty);
        set => SetValue(FpsProperty, value);
    }

    protected override CameraState CaptureState() =>
        new(Exposure, Gain, Fps, SkeumoSkia.CapturePalette(this));
}
