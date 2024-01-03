using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.IO;

namespace SukiUI.Demo.Utilities;

public static class BitmapUtilities
{
    public static Bitmap? CreateBitmap(StreamGeometry streamGeometry, IBrush brush)
    {
        var internalImage = new DrawingImage
        {
            Drawing = new GeometryDrawing
            {
                Geometry = streamGeometry,
                Brush = brush,
            }
        };
        var pixelSize = new PixelSize((int)internalImage.Size.Width, (int)internalImage.Size.Height);
        using MemoryStream memoryStream = new();
        using (RenderTargetBitmap bitmap = new(pixelSize, new Vector(96, 96)))
        {
            using (var ctx = bitmap.CreateDrawingContext())
            {
                internalImage.Drawing.Draw(ctx);
            }
            bitmap.Save(memoryStream);
        }
        memoryStream.Seek(0, SeekOrigin.Begin);
        var returnImage = new Bitmap(memoryStream);
        return returnImage;
    }
}