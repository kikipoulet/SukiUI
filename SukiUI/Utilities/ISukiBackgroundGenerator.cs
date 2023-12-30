using System;
using Avalonia.Styling;
using SkiaSharp;
using SukiUI.Models;

namespace SukiUI.Utilities;

public interface ISukiBackgroundGenerator : IDisposable
{
    public void Draw(SKImageInfo info, SKCanvas canvas, SukiColorTheme colorTheme, ThemeVariant baseTheme);
}