using System.Collections.Generic;
using System.IO;
using SkiaSharp;

namespace SukiUI.HyperGraphics;

/// <summary>
///     Rendu hors ecran d'un modele en PNG, sans Avalonia : pour verifier un modele (un LLM peut regarder la planche).
///     Depuis un projet console jetable qui inclut Mini3D/Core/*.cs, le modele et le paquet SkiaSharp.
/// </summary>
public static class Model3DPreview
{
    private static readonly (string Label, double? Yaw, double? Pitch, bool Wire)[] Views =
    {
        ("Vue par defaut", null, null, false),
        ("Face (yaw 0)", 0, 8, false),
        ("Gauche (yaw 90)", 90, 8, false),
        ("Droite (yaw -90)", -90, 8, false),
        ("Arriere (yaw 200)", 200, 24, false),
        ("Filaire", null, null, true),
    };

    /// <summary>Une vue en PNG. yaw / pitch en degres (defaut : camera du modele) ; channels : valeurs des entrees (s.Input).</summary>
    public static byte[] Png(Model3D model, int width = 680, int height = 400, double? yaw = null, double? pitch = null,
        IReadOnlyDictionary<string, double>? channels = null, bool dark = false, bool wire = false)
    {
        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        surface.Canvas.Clear(Background(dark));
        Render(surface.Canvas, model, new SKRect(0, 0, width, height), yaw, pitch, channels, dark, wire);
        return Encode(surface);
    }

    /// <summary>Planche de 6 vues (defaut, face, gauche, droite, arriere, filaire) ecrite en PNG dans path.</summary>
    public static void SaveSheet(Model3D model, string path, IReadOnlyDictionary<string, double>? channels = null, bool dark = false)
    {
        const int cellW = 510, cellH = 300, label = 26, cols = 3;
        var rows = (Views.Length + cols - 1) / cols;
        using var surface = SKSurface.Create(new SKImageInfo(cellW * cols, (cellH + label) * rows, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(Background(dark));
        using var font = new SKFont(SKTypeface.Default, 15);
        using var ink = new SKPaint { IsAntialias = true, Color = dark ? new SKColor(200, 205, 215) : new SKColor(60, 66, 80) };
        using var line = new SKPaint { Color = dark ? new SKColor(60, 64, 74) : new SKColor(220, 224, 232), StrokeWidth = 1 };
        for (var i = 0; i < Views.Length; i++)
        {
            var (text, yaw, pitch, wire) = Views[i];
            float x = i % cols * cellW, y = i / cols * (cellH + label);
            canvas.DrawText(text, x + 10, y + 18, SKTextAlign.Left, font, ink);
            Render(canvas, model, new SKRect(x, y + label, x + cellW, y + label + cellH), yaw, pitch, channels, dark, wire);
            canvas.DrawLine(x, y, x, y + cellH + label, line);
            canvas.DrawLine(x, y, x + cellW, y, line);
        }
        File.WriteAllBytes(path, Encode(surface));
    }

    internal static void Render(SKCanvas canvas, Model3D model, SKRect cell, double? yaw, double? pitch,
        IReadOnlyDictionary<string, double>? channels, bool dark, bool wire)
    {
        using var renderer = new Renderer(model);
        var info = renderer.Info;
        if (channels != null)
            foreach (var (name, value) in channels) renderer.Channels.Values[info.Inputs[info.InputSlot(name)].Index] = value;
        renderer.Dark = dark;
        renderer.Wire = wire;
        renderer.Yaw = (yaw ?? info.Yaw) * M.Deg;
        renderer.Pitch = (pitch ?? info.Pitch) * M.Deg;
        renderer.Layout(cell.Width, cell.Height);
        canvas.Save();
        canvas.ClipRect(cell);
        canvas.Translate(cell.Left + renderer.Offset.X, cell.Top + renderer.Offset.Y);
        canvas.Scale((float)renderer.Scale);
        renderer.DrawFrame(canvas);
        canvas.Restore();
    }

    private static SKColor Background(bool dark) => dark ? new SKColor(28, 30, 36) : new SKColor(247, 248, 250);

    private static byte[] Encode(SKSurface surface)
    {
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
