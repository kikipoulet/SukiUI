using Avalonia;
using Avalonia.Controls;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Skeumo-Suki short industrial camera facing a particle cell.
/// Display only: the exposure sets the light (beam, lens, chamber), the gain brightens and pixelates the particles,
/// the frame rate paces the data pulses on the cable. Drawing and animation live in <c>Camera.Rendering.cs</c>.
/// </summary>
public partial class Camera : UserControl
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

    public Camera()
    {
        InitializeComponent();
        InitializeRendering();
    }
}
