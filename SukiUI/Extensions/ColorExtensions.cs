using Avalonia.Media;

namespace SukiUI.Extensions;

public static class ColorExtensions
{
    public static Color WithAlpha(this Color c, byte alpha) =>
        new(alpha, c.R, c.G, c.B);

    public static Color WithAlpha(this Color c, double alpha) =>
        WithAlpha(c, (byte)(255 * alpha));

    /// <summary>
    /// Used primarily for SukiEffect runtime effect uniforms, converts a standard Color to an RGB float array in the range 0-1
    /// </summary>
    public static float[] ToFloatArray(this Color c) => 
        new[] { c.R / 255f, c.G / 255f, c.B / 255f };

    /// <summary>
    /// Used primarily for SukiEffect runtime effect uniforms, converts a standard Color to an RGB float array in the range 0-1.
    /// Allows recycling of an array for performance.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="array"></param>
    public static void ToFloatArrayNonAlloc(this Color c, float[] array)
    {
        array[0] = c.R / 255f;
        array[1] = c.G / 255f;
        array[2] = c.B / 255f;
    }
}