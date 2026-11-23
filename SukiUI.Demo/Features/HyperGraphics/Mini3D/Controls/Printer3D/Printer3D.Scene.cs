using System;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Mini3D.Controls;

/// <summary>
/// Detailed CoreXY-style desktop 3D printer: dual Z-towers with vertical leadscrews,
/// stepper motors &amp; GT2 belts, leveling wheels beneath a sliding heated PEI bed with a
/// 3-tier printed totem, traversing hotend with fan duct &amp; brass nozzle, and rotating spool.
/// </summary>
public sealed class Printer3DScene : Model3D
{
    private static readonly SKColor FrameLight = Rgb(232, 238, 248);
    private static readonly SKColor Trim = Rgb(204, 214, 232);
    private static readonly SKColor DarkSlate = Rgb(44, 51, 66);
    private static readonly SKColor StepperBlack = Rgb(30, 35, 46);
    private static readonly SKColor ScreenBg = Hex("#121726");
    private static readonly SKColor Steel = Rgb(186, 197, 216);
    private static readonly SKColor Brass = Hex("#E5A93C");
    private static readonly SKColor Silicone = Hex("#E86A38");
    private static readonly SKColor Accent = Hex("#5B8CFF");
    private static readonly SKColor AccentSoft = Hex("#8BB0FF");
    private static readonly SKColor Emerald = Hex("#3FD38C");

    protected override void Build(Scene3D s)
    {
        s.Camera(yaw: 34, pitch: 23, intro: -8);
        s.Frame(center: (114, 108, 126), size: (450, 338), screen: (226, 165));
        s.Ground(z: 0).Shadow((-6, -6), (226, 216));
        s.EdgeColor(Rgb(62, 72, 98));

        s.Input("printing", ms: 260, initial: 1);
        s.Input("bedY", ms: 260, initial: 0.5);
        s.Input("toolX", ms: 240, initial: 0.5);
        s.Input("spool", ms: 260, initial: 0);
        s.Input("progress", ms: 260, initial: 0.35);

        // Four rubber isolator feet
        s.Box((10, 10, 0), (32, 32, 6), StepperBlack, open: Side.Top);
        s.Box((188, 10, 0), (210, 32, 6), StepperBlack, open: Side.Top);
        s.Box((10, 178, 0), (32, 200, 6), StepperBlack, open: Side.Top);
        s.Box((188, 178, 0), (210, 200, 6), StepperBlack, open: Side.Top);

        // Main base plinth
        var baseBox = s.Box((0, 0, 6), (220, 210, 42), FrameLight);

        baseBox.Face(Side.Front)
            .Draw(g =>
            {
                g.RoundRect((14, 11), (144, 37), 4).Fill(ScreenBg).Stroke(DarkSlate, 0.9);
                g.Text("CORE-3D // MK2", (20, 29), 6.2).Fill(AccentSoft);
                g.Circle(152, 24, 2.5, 12).FillWith("printing", Rgb(86, 94, 114), Emerald).Glow("printing", 0.9);
                g.Rect((196, 14), (206, 34)).Fill(StepperBlack);
                g.RoundRect((161, 13), (191, 35), 3).Fill(Trim).Stroke(DarkSlate, 0.7);
            })
            .DrawWith("progress", (g, p) =>
            {
                var pct = Math.Clamp(p, 0, 1);
                g.RoundRect((20, 16), (138, 22), 2).Fill(Rgb(30, 38, 56));
                var w = Math.Max(4, 118 * pct);
                g.RoundRect((20, 16), (20 + w, 22), 2).Fill(Accent);
                g.Text($"{pct * 100:0}%  215C", (88, 29), 6.0).Fill(SKColors.White);
            });

        baseBox.Face(Side.Back).Draw(g =>
        {
            // Rear PSU AC inlet & exhaust grille
            g.RoundRect((20, 12), (82, 36), 3).Fill(StepperBlack);
            g.Circle(51, 24, 9, 14).Stroke(DarkSlate, 1.0);
            g.RoundRect((146, 14), (196, 34), 3).Fill(DarkSlate);
            g.Rect((154, 19), (168, 29)).Fill(Hex("#D64550"));
        });

        baseBox.Face(Side.Left).Draw(g =>
        {
            for (var i = 0; i < 5; i++)
                g.RoundRect((26 + i * 32, 16), (44 + i * 32, 30), 2).Fill(DarkSlate);
        });

        baseBox.Face(Side.Right).Draw(g =>
        {
            g.Screws(2.6, (14, 14), (196, 14), (14, 34), (196, 34));
            g.RoundRect((90, 16), (126, 32), 2).Fill(DarkSlate);
        });

        baseBox.Face(Side.Top).Draw(g =>
        {
            g.Screws(2.6, (10, 10), (210, 10), (10, 200), (210, 200));
            g.RoundRect((38, 16), (182, 194), 4).Fill(Rgb(218, 226, 240));
            // GT2 centerline belt track
            g.Line((108, 26), (108, 184)).Stroke(StepperBlack, 2.4);
            g.Line((112, 26), (112, 184)).Stroke(StepperBlack, 2.4);
        });

        // Y-axis rails + slim front idler pulley (rear space kept completely clear for bed travel)
        s.Box((58, 18, 42), (66, 192, 46), Steel, open: Side.Bottom);
        s.Box((154, 18, 42), (162, 192, 46), Steel, open: Side.Bottom);
        s.Box((98, 12, 42), (122, 24, 46), DarkSlate, open: Side.Bottom);

        // Pressable front Start/Pause button mounted directly on Front face (never floats or clips)
        var baseFront = baseBox.Face(Side.Front);
        var pwrBtn = s.Group().MountTo(baseFront).MoveWith("printing", (0, 1.8, 0)).Clickable("print");
        var pwrCap = pwrBtn.Box((164, -4.0, 16), (188, 0, 32), StepperBlack, open: Side.Back);
        pwrCap.Face(Side.Front).Draw(g =>
        {
            g.RoundRect((168, 19), (184, 29), 2)
             .FillWith("printing", Rgb(80, 90, 110), Emerald)
             .Glow("printing", 0.9);
        });

        // Sliding heated print bed with 4 leveling thumbwheels & 3-tier extruded totem
        var bedGroup = s.Group().MoveWith("bedY", (0, -24, 0));
        bedGroup.Extrude(new Path2D().Circle(58, 56, 7), Axis.Z, 46, 50, DarkSlate);
        bedGroup.Extrude(new Path2D().Circle(162, 56, 7), Axis.Z, 46, 50, DarkSlate);
        bedGroup.Extrude(new Path2D().Circle(58, 152, 7), Axis.Z, 46, 50, DarkSlate);
        bedGroup.Extrude(new Path2D().Circle(162, 152, 7), Axis.Z, 46, 50, DarkSlate);

        var bed = bedGroup.Box((48, 46, 50), (172, 162, 58), DarkSlate);
        bed.Face(Side.Top).Draw(g =>
        {
            g.RoundRect((52, 50), (168, 158), 3).Fill(Rgb(38, 44, 60)).Stroke(Rgb(96, 112, 144), 0.8);
            for (var x = 68; x <= 152; x += 28)
                g.Line((x, 52), (x, 156)).Stroke(Rgb(66, 78, 102), 0.5);
            for (var y = 64; y <= 148; y += 28)
                g.Line((54, y), (166, y)).Stroke(Rgb(66, 78, 102), 0.5);
            g.Circle(60, 58, 2.2, 10).FillWith("printing", Rgb(80, 90, 110), Brass).Glow("printing", 0.75);
            g.Text("PEI", (148, 56), 5.8).Fill(Trim);
        });

        // Multi-tier 3D printed calibration prism on the bed (explicit 6-sided hexagon)
        bedGroup.Extrude(new Path2D().Circle(110, 104, 19, segments: 6), Axis.Z, 58, 70, Accent, rim: AccentSoft);
        bedGroup.Extrude(new Path2D().Circle(110, 104, 13, segments: 6), Axis.Z, 70, 79, AccentSoft, rim: Accent);
        bedGroup.Extrude(new Path2D().Circle(110, 104, 6.5, segments: 6), Axis.Z, 79, 85, Accent, rim: AccentSoft);

        // Rigid Z-towers + Dual Z-stepper motors + Z-leadscrews
        var leftTower = s.Box((12, 114, 42), (32, 142, 232), FrameLight, open: Side.Bottom);
        leftTower.Face(Side.Front).Draw(g => g.Rect((19, 48), (25, 226)).Fill(Trim));
        var rightTower = s.Box((188, 114, 42), (208, 142, 232), FrameLight, open: Side.Bottom);
        rightTower.Face(Side.Front).Draw(g => g.Rect((195, 48), (201, 226)).Fill(Trim));

        // Dual Z stepper motors behind towers + brass couplers + darker steel leadscrew rods
        var screwSteel = Rgb(126, 140, 168);
        s.Box((12, 144, 42), (32, 166, 64), StepperBlack, open: Side.Bottom);
        s.Box((188, 144, 42), (208, 166, 64), StepperBlack, open: Side.Bottom);
        s.Extrude(new Path2D().Circle(36, 128, 3.8), Axis.Z, 42, 54, Brass);
        s.Extrude(new Path2D().Circle(36, 128, 2.5), Axis.Z, 54, 232, screwSteel);
        s.Extrude(new Path2D().Circle(184, 128, 3.8), Axis.Z, 42, 54, Brass);
        s.Extrude(new Path2D().Circle(184, 128, 2.5), Axis.Z, 54, 232, screwSteel);

        // X-bridge + X-stepper motor
        s.Box((4, 118, 118), (11.5, 138, 138), StepperBlack);
        var bridge = s.Box((38.5, 114, 118), (181.5, 136, 136), DarkSlate);
        bridge.Face(Side.Front).Draw(g =>
        {
            g.Rect((40, 125), (180, 128)).Fill(Steel);
        });

        // Top arch beam
        var beam = s.Box((8, 110, 232), (212, 146, 248), Trim);
        beam.Face(Side.Front).Draw(g =>
        {
            g.Text("HYPER-PRINT", (74, 236), 8.2).Fill(DarkSlate);
        });
        beam.Face(Side.Top).Draw(g =>
        {
            g.Screws(2.4, (18, 128), (202, 128));
            g.RoundRect((98, 122), (122, 134), 3).Fill(DarkSlate);
        });

        // Sliding extruder printhead along X
        var toolGroup = s.Group().MoveWith("toolX", (84, 0, 0)).Clickable("print");
        var tool = toolGroup.Box((48, 84, 106), (88, 113.5, 150), DarkSlate);
        tool.Face(Side.Front).Draw(g =>
        {
            g.RoundRect((54, 114), (82, 142), 4).Fill(ScreenBg).Stroke(Steel, 0.8);
            g.Circle(68, 128, 10, 14).Fill(Rgb(30, 38, 56)).Stroke(Accent, 1.3);
            g.Circle(68, 128, 3.2, 10).Fill(AccentSoft).Glow("printing", 0.9);
            g.RoundRect((54, 109), (82, 112), 1).FillWith("printing", Rgb(80, 88, 106), Emerald).Glow("printing", 0.85);
        });
        tool.Face(Side.Left).Draw(g =>
        {
            for (var z = 114; z <= 138; z += 6)
                g.Line((88, z), (108, z)).Stroke(Steel, 1.0);
        });

        // Part-cooling fan duct + orange silicone heater block + brass nozzle (clears print height 83)
        toolGroup.Box((48, 86, 94), (56, 106, 106), Trim, open: Side.Top);
        toolGroup.Box((60, 92, 96), (76, 106, 106), Silicone, open: Side.Top);
        toolGroup.Extrude(new Path2D().Circle(68, 99, 3.8), Axis.Z, 86, 96, Brass, rim: Rgb(186, 130, 30));

        // Rotating side spool holder + extruded filament spool
        s.Box((208, 122, 178), (216.5, 134, 190), DarkSlate, open: Side.Left);
        var spoolGroup = s.Group().Pivot((232, 128, 184), Axis.X).RotateWith("spool", 0, 360);
        var flangeShape = new Path2D().Circle(128, 184, 24);
        var coreShape = new Path2D().Circle(128, 184, 19);
        spoolGroup.Extrude(flangeShape, Axis.X, 217, 220, DarkSlate, rim: Steel);
        spoolGroup.Extrude(coreShape, Axis.X, 220, 234, Accent, rim: AccentSoft);
        spoolGroup.Extrude(flangeShape, Axis.X, 234, 237, DarkSlate, rim: Steel);
    }
}
