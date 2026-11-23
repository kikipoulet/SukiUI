using System;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Mini3D.Controls;

/// <summary>
/// Detailed procedural 3D synthesizer deck built for the HyperMini3D demo.
/// Features fixed tactile pushbuttons on dedicated pedestals, physical drum pads,
/// side cheek guards, rear I/O ports, live OLED spectrum + peak hold, and hinged VU visor.
/// </summary>
public sealed class SynthConsoleScene : Model3D
{
    private static readonly SKColor Chassis = Rgb(232, 238, 248);
    private static readonly SKColor Bezel = Rgb(210, 219, 235);
    private static readonly SKColor Slate = Rgb(48, 56, 74);
    private static readonly SKColor DarkScreen = Hex("#121726");
    private static readonly SKColor Accent = Hex("#5B8CFF");
    private static readonly SKColor AccentSoft = Hex("#88AEFF");
    private static readonly SKColor Emerald = Hex("#3FD38C");
    private static readonly SKColor Amber = Hex("#F2B83B");
    private static readonly SKColor Rubber = Rgb(36, 41, 52);

    protected override void Build(Scene3D s)
    {
        s.Camera(yaw: 32, pitch: 25, intro: -10);
        s.Frame(center: (120, 85, 42), size: (390, 290), screen: (195, 165));
        s.Ground(z: 0).Shadow((-6, -4), (246, 174));
        s.EdgeColor(Rgb(62, 72, 98));

        s.Input("power", ms: 260, initial: 1);
        s.Input("auto", ms: 260, initial: 1);
        s.Input("lid", ms: 750, initial: 1);
        s.Input("dial", ms: 650, initial: 0);
        s.Input("level", ms: 280, initial: 0.65);

        s.Channel("activeLevel", c => c["power"] * c["level"]);

        // Four corner isolator feet
        s.Box((10, 10, 0), (32, 32, 6), Rubber, open: Side.Top);
        s.Box((208, 10, 0), (230, 32, 6), Rubber, open: Side.Top);
        s.Box((10, 138, 0), (32, 160, 6), Rubber, open: Side.Top);
        s.Box((208, 138, 0), (230, 160, 6), Rubber, open: Side.Top);

        // Main aluminum deck
        var deck = s.Box((0, 0, 6), (240, 170, 48), Chassis);

        // Side protective rubber cheek rails
        var leftCheek = s.Box((-5, -2, 8), (0, 172, 50), Rubber);
        leftCheek.Face(Side.Left).Draw(g =>
        {
            for (var i = 0; i < 4; i++)
                g.RoundRect((24 + i * 32, 18), (44 + i * 32, 38), 2).Fill(DarkScreen);
        });
        var rightCheek = s.Box((240, -2, 8), (245, 172, 50), Rubber);
        rightCheek.Face(Side.Right).Draw(g =>
        {
            g.Screws(2.4, (14, 18), (156, 18), (14, 40), (156, 40));
            g.Text("REV 2.4", (68, 26), 6.2).Fill(Bezel);
        });

        // Front panel: speaker grille, label, TRS headphone jack & power status LED
        deck.Face(Side.Front).Draw(g =>
        {
            g.RoundRect((14, 15), (96, 37), 4).Fill(Bezel).Stroke(Rgb(90, 102, 128), 0.7);
            for (var i = 0; i < 7; i++)
                g.Line((22 + i * 10.5, 20), (22 + i * 10.5, 32)).Stroke(Rgb(74, 84, 108), 1.5);

            g.Text("SYNTH-01", (106, 22), 10.5).Fill(Slate);
            // Brass 1/4" TRS + 3.5mm Aux jacks
            g.Circle(186, 26, 5.2, 14).Fill(DarkScreen).Stroke(Rgb(198, 152, 54), 1.6);
            g.Circle(203, 26, 3.6, 12).Fill(DarkScreen).Stroke(Rgb(160, 170, 190), 1.1);
            g.Circle(222, 26, 2.8, 12).FillWith("power", Rgb(86, 94, 114), Emerald).Glow("power", 0.9);
        });

        // Rear I/O panel: USB-C, 12V DC, MIDI DIN, heatsink fins & serial plate
        deck.Face(Side.Back).Draw(g =>
        {
            g.RoundRect((18, 14), (82, 38), 3).Fill(DarkScreen).Stroke(Slate, 0.8);
            g.RoundRect((24, 22), (40, 29), 3).Fill(Rgb(150, 162, 184));
            g.Circle(56, 26, 4.5, 12).Fill(Slate).Stroke(Rgb(180, 190, 206), 0.9);
            g.Circle(72, 26, 2.8, 10).FillWith("power", Rgb(76, 84, 102), Emerald).Glow("power", 0.7);
            for (var i = 0; i < 6; i++)
                g.Rect((152 + i * 11, 15), (158 + i * 11, 38)).Fill(Slate);
            g.RoundRect((96, 18), (138, 34), 2).Fill(Bezel);
            g.Text("SN-0042", (101, 23), 6.2).Fill(Slate);
        });

        // Top deck artwork & live OLED spectrum analyzer
        deck.Face(Side.Top)
            .Draw(g =>
            {
                g.Screws(2.6, (10, 10), (230, 10), (10, 160), (230, 160));

                // Recessed OLED spectrum window
                g.RoundRect((18, 18), (128, 96), 6).Fill(DarkScreen).Stroke(Slate, 1.0);
                for (var y = 34; y <= 76; y += 14)
                    g.Line((24, y), (122, y)).Stroke(Rgb(34, 42, 62), 0.6);
                g.Text("SPECTRUM", (24, 83), 7.2).Fill(AccentSoft);

                // Dial tick ring around pedestal B
                for (var i = 0; i < 12; i++)
                {
                    var ang = (i * 30.0) * Deg;
                    var x1 = 182 + 33 * Math.Cos(ang);
                    var y1 = 58 + 33 * Math.Sin(ang);
                    var x2 = 182 + 37 * Math.Cos(ang);
                    var y2 = 58 + 37 * Math.Sin(ang);
                    g.Line((x1, y1), (x2, y2)).Stroke(Rgb(106, 118, 144), 1.0);
                }
            })
            .DrawWith("activeLevel", (g, lvl) =>
            {
                var db = lvl > 0.02 ? $"{-36 + lvl * 36:0}dB" : "MUTE";
                g.Text(db, (92, 83), 6.8).Fill(lvl > 0.02 ? Emerald : Rgb(120, 130, 150));

                for (var i = 0; i < 6; i++)
                {
                    var x0 = 26 + i * 16;
                    var wave = 0.32 + 0.68 * Math.Abs(Math.Sin(lvl * Math.PI * 1.45 + i * 0.62));
                    var h = 4 + 46 * lvl * wave;
                    g.RoundRect((x0, 26), (x0 + 10, 26 + h), 2).Fill(i >= 4 ? AccentSoft : Accent);
                    // Peak-hold cap dot
                    g.RoundRect((x0, 28 + h), (x0 + 10, 31 + h), 1).Fill(SKColors.White);
                }
            });

        // Four physical backlit drum trigger pads mounted on Front-Right of Top deck
        for (var i = 0; i < 4; i++)
        {
            var x0 = 142 + i * 21;
            var pad = deck.Face(Side.Top).Mount((x0, 18, 48), (x0 + 17, 34, 52), Slate, open: Side.Bottom);
            pad.Face(Side.Top).Draw(g =>
            {
                g.RoundRect((x0 + 2, 20), (x0 + 15, 32), 2)
                 .FillWith("power", Rgb(68, 76, 94), i == 3 ? Amber : Accent)
                 .Glow("power", 0.65);
            });
        }

        // Control Pod Base (pit floor at z=51.5, recessed pocket rims 51.5..55.0 so caps never float or dip past floor)
        var ctrlBase = s.Box((18, 108, 48), (126, 152, 51.5), DarkScreen, open: Side.Bottom);
        var ctrlTop = ctrlBase.Face(Side.Top);

        // Surround bezel strips forming two clean switch wells around (28..66, 118..142) & (76..114, 118..142)
        ctrlTop.Mount((18, 142, 51.5), (126, 152, 55.0), Bezel, open: Side.Bottom);
        ctrlTop.Mount((18, 118, 51.5), (28, 142, 55.0), Bezel, open: Side.Bottom);
        ctrlTop.Mount((66, 118, 51.5), (76, 142, 55.0), Bezel, open: Side.Bottom);
        ctrlTop.Mount((114, 118, 51.5), (126, 142, 55.0), Bezel, open: Side.Bottom);
        ctrlTop.Mount((18, 108, 51.5), (68, 118, 55.0), Bezel, open: Side.Bottom);
        ctrlTop.Mount((68, 108, 51.5), (126, 118, 55.0), Bezel, open: Side.Bottom);

        // Pressable latching PWR button (54.0..62.0 unpressed -> 51.5..59.5 flush inside pocket when pressed)
        var pwrBtn = s.Group().MountTo(ctrlTop).MoveWith("power", (0, 0, -2.5)).Clickable("power");
        var pwrCap = pwrBtn.Box((28.5, 118.5, 54.0), (65.5, 141.5, 62.0), Slate, open: Side.Bottom);
        pwrCap.Face(Side.Top).Draw(g =>
        {
            g.RoundRect((32, 122), (62, 138), 3)
             .FillWith("power", Rgb(70, 78, 96), Emerald)
             .Glow("power", 0.88);
            g.Text("PWR", (36, 126.5), 6.8).Fill(SKColors.White);
        });

        // Pressable latching AUTO button (54.0..62.0 unpressed -> 51.5..59.5 flush inside pocket when pressed)
        var autoBtn = s.Group().MountTo(ctrlTop).MoveWith("auto", (0, 0, -2.5)).Clickable("auto");
        var autoCap = autoBtn.Box((76.5, 118.5, 54.0), (113.5, 141.5, 62.0), Slate, open: Side.Bottom);
        autoCap.Face(Side.Top).Draw(g =>
        {
            g.RoundRect((80, 122), (110, 138), 3)
             .FillWith("auto", Rgb(70, 78, 96), Accent)
             .Glow("auto", 0.88);
            g.Text("AUTO", (82, 126.5), 6.4).Fill(SKColors.White);
        });

        // Dial pedestal (top at z=56)
        s.Box((154, 42, 48), (210, 98, 56), Bezel, open: Side.Bottom);

        // Extruded fluted rotary dial
        var knobPath = new Path2D().Circle(182, 70, 24);
        var dialGroup = s.Group().Pivot((182, 70, 56), Axis.Z).RotateWith("dial", 0, 360).Clickable("dial");
        var dial = dialGroup.Extrude(knobPath, Axis.Z, 56, 73, Rgb(242, 246, 253), rim: Rgb(168, 178, 198));
        dial.Face(Side.Top).Draw(g =>
        {
            g.Circle(182, 70, 18).Fill(DarkScreen);
            g.Circle(182, 70, 12).Stroke(Accent, 1.5).Glow("power", 0.55);
            g.RoundRect((180, 72), (184, 87), 1.5).Fill(AccentSoft).Glow("power", 0.85);
            g.Circle(182, 70, 4.5).Fill(Bezel);
        });

        // Rear hinge pillar + articulated flip-up visor with dual VU meters
        s.Box((146, 146, 48), (218, 158, 58), Slate, open: Side.Bottom);

        var lidGroup = s.Group().Pivot((182, 152, 58), Axis.X).RotateWith("lid", 0, -76).Clickable("lid");
        var visor = lidGroup.Box((142, 104, 58), (222, 158, 67), Slate);
        visor.Face(Side.Top).Draw(g =>
        {
            g.RoundRect((148, 110), (216, 152), 4).Fill(Bezel);
            g.Text("MONITOR", (158, 127), 7.5).Fill(Slate);
        });
        visor.Face(Side.Bottom)
            .Draw(g =>
            {
                g.RoundRect((148, 110), (216, 152), 4).Fill(DarkScreen).Stroke(Rgb(74, 86, 114), 0.7);
                g.RoundRect((153, 115), (210, 123), 2).Fill(Rgb(24, 30, 48));
                g.RoundRect((153, 126), (210, 134), 2).Fill(Rgb(24, 30, 48));
                for (var x = 158; x <= 206; x += 8)
                    g.Line((x, 138), (x, 143)).Stroke(Rgb(78, 92, 122), 0.9);
            })
            .DrawWith("activeLevel", (g, lvl) =>
            {
                var w1 = Math.Max(4, 53 * lvl);
                var w2 = Math.Max(4, 53 * Math.Min(1, lvl * 0.85 + 0.08));
                g.RoundRect((154, 127), (154 + w1, 133), 1.5).Fill(lvl > 0.02 ? Emerald : Rgb(62, 72, 92));
                g.RoundRect((154, 116), (154 + w2, 122), 1.5).Fill(lvl > 0.02 ? Accent : Rgb(62, 72, 92));
            });
    }
}
