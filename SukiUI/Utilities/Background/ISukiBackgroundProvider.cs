using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using SukiUI.Models;

namespace SukiUI.Utilities.Background;

public interface ISukiBackgroundProvider
{
    /// <summary>
    /// Updates the values from the main thread, allowing the generator to keep drawing in the background.
    /// </summary>
    /// <param name="colorTheme"></param>
    /// <param name="baseTheme"></param>
    public void UpdateValues(SukiColorTheme colorTheme, ThemeVariant baseTheme);
    
    /// <summary>
    /// Called every time the Background requests a draw.
    /// If animation is enabled this will be called per-frame at 60fps.
    /// </summary>
    /// <param name="bitmap">Bitmap to draw the background to.</param>
    /// <returns></returns>
    public Task Draw(WriteableBitmap bitmap);
}