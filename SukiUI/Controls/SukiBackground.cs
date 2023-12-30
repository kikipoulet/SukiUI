using System;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SukiUI.Utilities.Background;

namespace SukiUI.Controls;

public class SukiBackground : Image, IDisposable
{
    private const int ImageWidth = 100;
    private const int ImageHeight = 100;

    private readonly WriteableBitmap _bmp = new(new PixelSize(ImageWidth, ImageHeight), new Vector(96, 96),
        PixelFormats.Bgra8888);

    /// <summary>
    /// Quickly and easily assign a generator either for testing, or in future allow dev-defined generators...
    /// </summary>
    private readonly ISukiBackgroundRenderer _renderer = new FastNoiseBackgroundRenderer();
    
    private static readonly Timer _animationTick = new(16.7) { AutoReset = true };

    private bool _animationEnabled = false;

    public SukiBackground()
    {
        Source = _bmp;
        Stretch = Stretch.UniformToFill;
        _animationTick.Elapsed += (_, _) => _renderer.Render(_bmp);
    }

    public override void EndInit()
    {
        base.EndInit();

        SukiTheme.OnColorThemeChanged += theme =>
        {
            _renderer.UpdateValues(theme, Dispatcher.UIThread.Invoke(() => Application.Current!.ActualThemeVariant));
            if (!_animationEnabled) _renderer.Render(_bmp);
        };
        SukiTheme.OnBaseThemeChanged += baseTheme =>
        {
            _renderer.UpdateValues(SukiTheme.ActiveColorTheme, baseTheme);
            if (!_animationEnabled) _renderer.Render(_bmp);
        };

        _renderer.UpdateValues(SukiTheme.ActiveColorTheme, Application.Current!.RequestedThemeVariant!);
        _renderer.Render(_bmp);
        
        if(_animationEnabled) _animationTick.Start();
    }

    public void SetAnimationEnabled(bool value)
    {
        if (_animationEnabled == value) return;
        _animationEnabled = value;
        if(_animationEnabled) _animationTick.Start();
        else _animationTick.Stop();
    }

    public void Dispose()
    {
        _bmp.Dispose();
    }
}