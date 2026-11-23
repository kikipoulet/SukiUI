using System;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Mini3D.Controls;

/// <summary>
/// Mid-engine GT supercar sculpted in pure C# via doubly-curved <see cref="Container3D.SmoothLoft"/>,
/// <see cref="Path2D.Quadratic"/>, <see cref="Path2D.Cubic"/>, <see cref="Container3D.Lathe"/>,
/// and <see cref="Container3D.RoundedBox"/> — partitioned into clean non-overlapping aerodynamic
/// zones so <see cref="Model3D.Quality"/> (0..100) refines curvature cleanly without painter flashing.
/// </summary>
public sealed class SportsCarScene : Model3D
{
    private static readonly SKColor BodyPaint = Hex("#3A6CF4");
    private static readonly SKColor BodyDeep = Hex("#2750C4");
    private static readonly SKColor Carbon = Rgb(24, 28, 38);
    private static readonly SKColor CarbonMid = Rgb(44, 50, 64);
    private static readonly SKColor DarkGlass = Hex("#111726");
    private static readonly SKColor TireBlack = Rgb(24, 27, 34);
    private static readonly SKColor Alloy = Rgb(218, 226, 238);
    private static readonly SKColor CaliperRed = Hex("#F24C5E");
    private static readonly SKColor HeadlightOn = Hex("#E0F6FF");
    private static readonly SKColor TaillightOn = Hex("#FF3352");
    private static readonly SKColor LightOff = Rgb(86, 94, 114);

    private const double MidY = 60.0;
    private const double InnerHalfW = 34.0; // Y = 26..94 inner chassis spine width
    private const double WheelYOut = 12.2;  // Outer face of left wheel (mirrored 107.8 on right)
    private const double WheelYIn = 23.4;   // Inner face of left wheel (mirrored 96.6 on right)
    private const double WheelR = 17.5;     // Tire outer radius
    private const double ArchR = 19.0;      // Wheel-arch clearance radius (> WheelR)

    protected override void Build(Scene3D s)
    {
        s.Camera(yaw: 38, pitch: 19, intro: -8);
        s.Frame(center: (136, MidY, 31), size: (420, 275), screen: (220, 165));
        s.Ground(z: 0).Shadow((8, 12), (266, 108));
        s.EdgeColor(Rgb(34, 42, 60));

        s.Input("lights", ms: 260, initial: 1);
        s.Input("aero", ms: 550, initial: 1);
        s.Input("wheel", ms: 240, initial: 0);

        BuildLowerAero(s);
        BuildSculptedBodyShell(s);
        BuildGreenhouse(s);
        BuildLightsAndTrim(s);
        BuildActiveRearWing(s);
        BuildWheels(s);
    }

    private static double ArchTopZ(double x, double cx)
    {
        var dx = x - cx;
        var sq = ArchR * ArchR - dx * dx;
        return sq > 0 ? WheelR + Math.Sqrt(sq) : WheelR;
    }

    // Continuous full-width fuselage cross-section: flat rocker bottom -> beveled hip ->
    // sculpted left/right fender shoulders framing a gently recessed central hood/deck channel.
    private static Path2D FullBodyCrossSection(double botW, double hipW, double shW,
        double zBot, double zHip, double zShoulder, double zCenter) =>
        new Path2D()
            .Line(MidY - botW, zBot)
            .Line(MidY + botW, zBot)
            .Line(MidY + hipW, zHip)
            .Quadratic(MidY + (hipW + shW) * 0.52, zShoulder + 0.5, MidY + shW, zShoulder)
            .Quadratic(MidY, zCenter, MidY - shW, zShoulder)
            .Quadratic(MidY - (hipW + shW) * 0.52, zShoulder + 0.5, MidY - hipW, zHip);

    // Matching central spine cross-section (Y = 26..94) across wheel-arch bays so nose/hood/doors/deck join seamlessly.
    private static Path2D CenterChannelSection(double zBot, double zEdge, double zCenter) =>
        new Path2D()
            .Line(MidY - InnerHalfW, zBot)
            .Line(MidY + InnerHalfW, zBot)
            .Line(MidY + InnerHalfW, zEdge)
            .Quadratic(MidY, zCenter, MidY - InnerHalfW, zEdge);

    // Matching outer fender eyebrow cross-section (strictly above ArchTopZ(x) and outboard of InnerHalfW).
    private static Path2D EyebrowSection(double yInner, double yOuter, double yHip, double yPeak,
        double zArch, double zHip, double zPeak, double zInnerTop) =>
        new Path2D()
            .Line(yInner, zArch)
            .Line(yOuter, zArch)
            .Line(yHip, zHip)
            .Quadratic((yHip + yPeak) * 0.5, zPeak + 0.4, yPeak, zPeak)
            .Line(yInner, zInnerTop);

    private static void BuildLowerAero(Scene3D s)
    {
        Path2D BladeSection(double botW, double topW, double z0, double z1) =>
            new Path2D()
                .Line(MidY - botW, z0)
                .Line(MidY + botW, z0)
                .Line(MidY + topW, z1)
                .Line(MidY - topW, z1);

        // Front aero splitter sitting strictly beneath nose bottom (z <= 9.4)
        s.SmoothLoft(Axis.X, Carbon,
            (2,  BladeSection(22, 24, 6.4, 8.6)),
            (12, BladeSection(39, 41, 6.6, 9.2)),
            (26, BladeSection(47, 48, 6.8, 9.4)),
            (38, BladeSection(45, 45, 7.0, 9.4)));

        // Central undertray & side aero blades strictly between wheels (x = 79..191, z <= 9.5)
        s.Box((79, 27, 7.0), (191, 93, 9.5), Carbon, open: Side.Top);
        s.Box((80, 10.5, 7.2), (190, 27, 9.4), Carbon);
        s.Box((80, 93, 7.2), (190, 109.5, 9.4), Carbon);

        // Rear diffuser & lower bumper apron sitting strictly behind rear wheels (x = 233..269)
        s.SmoothLoft(Axis.X, Carbon,
            (233, BladeSection(36, 44, 7.8, 13.5)),
            (252, BladeSection(35, 43, 8.8, 14.5)),
            (268, BladeSection(29, 36, 10.2, 16.0)));

        // Vertical diffuser strakes & quad titanium exhaust tips attached cleanly behind diffuser (x >= 268)
        for (var i = 0; i < 4; i++)
        {
            var y = 39.0 + i * 14.0;
            s.Box((268, y - 0.7, 9.8), (271.5, y + 0.7, 15.8), CarbonMid);
        }
        foreach (var ey in new[] { 38.0, 44.5, 75.5, 82.0 })
            s.Extrude(new Path2D().Circle(ey, 15.8, 2.1), Axis.X, 268.0, 273.5, Alloy, rim: CarbonMid);
    }

    private void BuildSculptedBodyShell(Scene3D s)
    {
        // Zone 1 (x = 4..39): Tapered aerodynamic front nose & bumper shell (capEnd closes front of front wheel well)
        s.SmoothLoft(Axis.X, BodyPaint,
            (4,  FullBodyCrossSection(20, 23, 18,  9.4, 13.0, 15.5, 14.8)),
            (14, FullBodyCrossSection(34, 41, 31,  9.5, 18.5, 24.5, 22.8)),
            (26, FullBodyCrossSection(43, 48, 38,  9.5, 24.0, 32.0, 28.8)),
            (39, FullBodyCrossSection(45, 49, 41,  9.5, 27.5, 36.2, 33.2)));

        // Zone 2A (x = 39..77): Center hood spanning between front wheel wells
        s.SmoothLoft(Axis.X, BodyPaint, rim: null, opacity: 1, capStart: false, capEnd: false, stepsPerSpan: null,
            (39, CenterChannelSection(9.5, 34.4, 33.2)),
            (58, CenterChannelSection(9.5, 37.0, 35.6)),
            (77, CenterChannelSection(9.5, 37.2, 36.0)));

        // Zone 2B (x = 39..77): Front wheel-arch eyebrows smoothly bridging Zone 1 -> Zone 3 above r = ArchR
        BuildWheelArchShell(s, cx: 58.0, x0: 39.0, x1: 77.0,
            zHip0: 27.5, zHip1: 28.5, zPeak0: 36.2, zPeak1: 37.2, zIn0: 34.4, zIn1: 37.2);

        // Compact dark inner wheel-well liners behind front wheels (inside InnerHalfW border y = 24.5..26, 94..95.5)
        s.Extrude(new Path2D().Line(39, 10.5).Arc(58, WheelR, 18.2, 180, 0).Line(77, 10.5), Axis.Y, 24.5, 26.0, Carbon);
        s.Extrude(new Path2D().Line(39, 10.5).Arc(58, WheelR, 18.2, 180, 0).Line(77, 10.5), Axis.Y, 94.0, 95.5, Carbon);

        // Zone 3 (x = 77..193): Continuous sculpted doors, Coke-bottle waist & rising rear haunches (caps seal wheel wells)
        s.SmoothLoft(Axis.X, BodyPaint,
            (77,  FullBodyCrossSection(45, 49, 41,  9.5, 28.5, 37.2, 36.0)),
            (108, FullBodyCrossSection(44, 47, 40,  9.5, 27.5, 36.5, 36.4)),
            (146, FullBodyCrossSection(44, 48, 41,  9.5, 28.5, 37.5, 37.2)),
            (193, FullBodyCrossSection(45, 50, 42,  9.5, 30.0, 39.8, 38.8)));

        // Zone 4A (x = 193..231): Rear center engine deck spanning between rear wheel wells
        s.SmoothLoft(Axis.X, BodyPaint, rim: null, opacity: 1, capStart: false, capEnd: false, stepsPerSpan: null,
            (193, CenterChannelSection(9.5, 39.0, 38.8)),
            (212, CenterChannelSection(9.5, 39.4, 39.2)),
            (231, CenterChannelSection(13.5, 39.2, 39.0)));

        // Zone 4B (x = 193..231): Muscular rear haunch arches over rear wheels
        BuildWheelArchShell(s, cx: 212.0, x0: 193.0, x1: 231.0,
            zHip0: 30.0, zHip1: 29.5, zPeak0: 39.8, zPeak1: 39.4, zIn0: 39.0, zIn1: 39.2);

        // Compact rear inner wheel-well liners
        s.Extrude(new Path2D().Line(193, 10.5).Arc(212, WheelR, 18.2, 180, 0).Line(231, 10.5), Axis.Y, 24.5, 26.0, Carbon);
        s.Extrude(new Path2D().Line(193, 10.5).Arc(212, WheelR, 18.2, 180, 0).Line(231, 10.5), Axis.Y, 94.0, 95.5, Carbon);

        // Zone 5 (x = 231..268): Swept boat-tail rear quarters & Kammback tail (capStart closes rear of rear wheel well)
        s.SmoothLoft(Axis.X, BodyPaint,
            (231, FullBodyCrossSection(45, 50, 42, 13.5, 29.5, 39.4, 39.0)),
            (250, FullBodyCrossSection(41, 46, 39, 16.0, 29.5, 38.2, 38.0)),
            (268, FullBodyCrossSection(35, 38, 33, 18.2, 28.5, 36.2, 36.2)));

        // Flush side air intake vents sitting cleanly on door waist
        var intakeL = s.Box((136, 10.3, 17.5), (172, 12.0, 24.8), Carbon).Clickable("lights");
        intakeL.Face(Side.Front).Draw(g =>
        {
            g.RoundRect((138, 18.2), (170, 24.0), 1.0).Fill(Rgb(16, 19, 26));
            g.Line((139, 21.1), (169, 21.1)).Stroke(BodyPaint, 1.5);
        });
        var intakeR = s.Box((136, 108.0, 17.5), (172, 109.7, 24.8), Carbon).Clickable("lights");
        intakeR.Face(Side.Back).Draw(g =>
        {
            g.RoundRect((138, 18.2), (170, 24.0), 1.0).Fill(Rgb(16, 19, 26));
            g.Line((139, 21.1), (169, 21.1)).Stroke(BodyPaint, 1.5);
        });

        // Sleek wing mirrors anchored cleanly into door shoulder
        s.Box((106, 10.5, 35.5), (110, 20.5, 39.0), Carbon);
        s.RoundedBox((103, 5.5, 38.8), (114, 13.0, 42.8), 1.4, BodyPaint);
        s.Box((106, 99.5, 35.5), (110, 109.5, 39.0), Carbon);
        s.RoundedBox((103, 107.0, 38.8), (114, 114.5, 42.8), 1.4, BodyPaint);
    }

    private void BuildWheelArchShell(Scene3D s, double cx, double x0, double x1,
        double zHip0, double zHip1, double zPeak0, double zPeak1, double zIn0, double zIn1)
    {
        var steps = Math.Clamp(Quality <= 20 ? 3 : Quality <= 65 ? 5 : 7, 3, 7);
        var leftSections = new (double At, Path2D Profile)[steps + 1];
        var rightSections = new (double At, Path2D Profile)[steps + 1];
        for (var i = 0; i <= steps; i++)
        {
            var u = (double)i / steps;
            var x = x0 + (x1 - x0) * u;
            var zArch = ArchTopZ(x, cx);
            var zHip = Math.Max(zArch + 0.6, zHip0 + (zHip1 - zHip0) * u);
            var zIn = Math.Max(zArch + 0.4, zIn0 + (zIn1 - zIn0) * u);
            var zPeak = Math.Max(zHip + 0.8, (zPeak0 + (zPeak1 - zPeak0) * u) + Math.Sin(u * Math.PI) * 0.9);

            leftSections[i] = (x, EyebrowSection(MidY - InnerHalfW, 13.5, 10.6, 18.0, zArch, zHip, zPeak, zIn));
            rightSections[i] = (x, EyebrowSection(MidY + InnerHalfW, 106.5, 109.4, 102.0, zArch, zHip, zPeak, zIn));
        }
        s.Loft(Axis.X, BodyPaint, leftSections);
        s.Loft(Axis.X, BodyPaint, rightSections);
    }

    private static void BuildGreenhouse(Scene3D s)
    {
        // Strictly non-overlapping greenhouse perched cleanly atop the lower hull (z >= baseZ).
        static Path2D Band(double yL0, double yL1, double yR1, double yR0, double zBot, double zTop, double crown) =>
            new Path2D()
                .Line(yL0, zBot)
                .Line(yR0, zBot)
                .Line(yR1, zTop)
                .Quadratic(MidY, zTop + crown, yL1, zTop);

        static Path2D Strip(double y0Bot, double y1Bot, double y0Top, double y1Top, double zBot, double zTop) =>
            new Path2D()
                .Line(y0Bot, zBot)
                .Line(y1Bot, zBot)
                .Line(y1Top, zTop)
                .Line(y0Top, zTop);

        // 1. Curved wrap-around windshield (x = 78..118, strictly above hood z >= 37.8 & inside A-pillars)
        s.SmoothLoft(Axis.X, DarkGlass,
            (78,  Band(33.0, 34.8, 85.2, 87.0, 37.8, 39.4, 0.5)),
            (98,  Band(35.0, 37.4, 82.6, 85.0, 47.6, 49.4, 0.7)),
            (118, Band(37.5, 39.5, 80.5, 82.5, 57.0, 57.4, 0.3))).Clickable("lights");

        // Left & Right painted A-pillars sitting strictly OUTBOARD of windshield (0.0 overlap)
        s.SmoothLoft(Axis.X, BodyPaint,
            (78,  Strip(29.8, 33.0, 31.6, 34.8, 37.8, 39.8)),
            (98,  Strip(31.8, 35.0, 34.2, 37.4, 47.6, 50.0)),
            (118, Strip(34.5, 37.5, 37.0, 39.5, 57.0, 59.5)));
        s.SmoothLoft(Axis.X, BodyPaint,
            (78,  Strip(87.0, 90.2, 85.2, 88.4, 37.8, 39.8)),
            (98,  Strip(85.0, 88.2, 82.6, 85.8, 47.6, 50.0)),
            (118, Strip(82.5, 85.5, 80.5, 83.0, 57.0, 59.5)));

        // 2. Side cabin glazing & painted roof strictly disjoint vertically (0.0 overlap with Zone 3 below or Roof above)
        // Quarter-window taper from x = 96 fills the triangle cleanly under A-pillars.
        s.SmoothLoft(Axis.X, DarkGlass,
            (96,  Strip(33.2, 86.8, 34.2, 85.8, 39.4, 46.6)),
            (118, Strip(35.0, 85.0, 38.5, 81.5, 39.5, 57.4)),
            (156, Strip(34.8, 85.2, 38.5, 81.5, 40.3, 57.4)),
            (193, Strip(35.2, 84.8, 39.5, 80.5, 41.6, 53.8))).Clickable("lights");

        s.SmoothLoft(Axis.X, BodyPaint,
            (118, Band(38.0, 41.2, 78.8, 82.0, 57.7, 61.0, 1.8)),
            (156, Band(38.0, 41.2, 78.8, 82.0, 57.7, 62.6, 2.0)),
            (193, Band(39.0, 42.2, 77.8, 81.0, 54.2, 56.8, 1.5)));

        // 3. Center recessed rear engine louver glass (x = 193..230, strictly inside y = 42.5..77.5, z >= 40.4)
        s.SmoothLoft(Axis.X, DarkGlass,
            (193, Band(42.5, 43.5, 76.5, 77.5, 40.4, 53.8, 0.5)),
            (212, Band(42.5, 43.5, 76.5, 77.5, 40.4, 46.2, 0.4)),
            (230, Band(42.5, 43.5, 76.5, 77.5, 40.4, 41.2, 0.2))).Clickable("lights");

        // Engine bay louver bars sitting cleanly above rear glass deck
        var louverFrame = s.Box((195, 44.5, 41.3), (229, 75.5, 41.8), Carbon);
        louverFrame.Face(Side.Top).Draw(g =>
        {
            for (var x = 198.0; x <= 225.0; x += 5.4)
                g.Line((x, 46), (x + 1.5, 74)).Stroke(CarbonMid, 1.6);
        });

        // 4. Swept painted C-pillar flying buttresses strictly OUTBOARD of rear glass (y = 35.0..42.5 & 77.5..85.0)
        s.SmoothLoft(Axis.X, BodyPaint,
            (193, Strip(35.2, 42.5, 38.5, 42.5, 40.4, 56.8)),
            (212, Strip(35.2, 42.5, 37.5, 42.5, 40.4, 48.2)),
            (231, Strip(35.2, 42.5, 36.5, 42.5, 40.4, 41.8)));
        s.SmoothLoft(Axis.X, BodyPaint,
            (193, Strip(77.5, 84.8, 77.5, 81.5, 40.4, 56.8)),
            (212, Strip(77.5, 84.8, 77.5, 82.5, 40.4, 48.2)),
            (231, Strip(77.5, 84.8, 77.5, 83.5, 40.4, 41.8)));
    }

    private static void BuildLightsAndTrim(Scene3D s)
    {
        // Front central radiator intake & GT badge strictly in front of Nose x = 4 (x = 1.5..4.0)
        var grille = s.Box((1.5, 40.5, 9.4), (4.0, 79.5, 14.2), Carbon).Clickable("lights");
        grille.Face(Side.Left).Draw(g =>
        {
            g.RoundRect((42, 10.0), (78, 13.6), 0.9).Fill(Rgb(14, 17, 24));
            g.Text("GT", (65, 10.3), 3.4).Fill(Alloy);
        });

        // Sleek LED headlight pods tucked flush onto front nose corners beside the grille
        var hlLeft = s.Box((3.4, 30.2, 12.6), (6.8, 38.6, 15.6), Carbon).Clickable("lights");
        hlLeft.Face(Side.Left).Draw(g =>
            g.RoundRect((30.9, 13.2), (37.9, 15.0), 0.6).FillWith("lights", LightOff, HeadlightOn).Glow("lights", 0.95));
        hlLeft.Face(Side.Front).Draw(g =>
            g.RoundRect((3.9, 13.2), (6.3, 15.0), 0.5).FillWith("lights", LightOff, HeadlightOn).Glow("lights", 0.95));

        var hlRight = s.Box((3.4, 81.4, 12.6), (6.8, 89.8, 15.6), Carbon).Clickable("lights");
        hlRight.Face(Side.Left).Draw(g =>
            g.RoundRect((82.1, 13.2), (89.1, 15.0), 0.6).FillWith("lights", LightOff, HeadlightOn).Glow("lights", 0.95));
        hlRight.Face(Side.Back).Draw(g =>
            g.RoundRect((3.9, 13.2), (6.3, 15.0), 0.5).FillWith("lights", LightOff, HeadlightOn).Glow("lights", 0.95));

        // Full-width recessed carbon rear fascia & LED light bar attached strictly behind x = 268
        var tail = s.Box((268.0, 22.0, 24.5), (270.0, 98.0, 32.5), Carbon).Clickable("lights");
        tail.Face(Side.Right).Draw(g =>
        {
            g.RoundRect((23.5, 25.5), (96.5, 31.5), 1.0).Fill(Rgb(16, 19, 26));
            g.RoundRect((25.0, 27.4), (95.0, 29.6), 0.7).FillWith("lights", LightOff, TaillightOn).Glow("lights", 0.95);
            g.Circle(30.5, 28.5, 2.2).FillWith("lights", LightOff, TaillightOn).Glow("lights", 1.0);
            g.Circle(89.5, 28.5, 2.2).FillWith("lights", LightOff, TaillightOn).Glow("lights", 1.0);
        });
    }

    private static void BuildActiveRearWing(Scene3D s)
    {
        // Active swan-neck GT rear wing resting strictly at z >= 39.2 (elevates & pitches on "aero")
        var wingGroup = s.Group().Pivot((256, MidY, 46), Axis.Y).RotateWith("aero", 0, 11).MoveWith("aero", (0, 0, 6.5)).Clickable("aero");
        wingGroup.Box((246, 44, 39.2), (255, 46.8, 47.0), Carbon);
        wingGroup.Box((246, 73.2, 39.2), (255, 76.0, 47.0), Carbon);

        var wingBlade = new Path2D()
            .Line(244, 46.8)
            .Quadratic(255, 49.5, 268, 47.4)
            .Line(268, 49.6)
            .Quadratic(255, 51.8, 244, 48.6);
        wingGroup.Extrude(wingBlade, Axis.Y, 17, 103, Carbon);

        wingGroup.Box((242, 15.2, 43.5), (270, 17.0, 52.5), BodyDeep);
        wingGroup.Box((242, 103.0, 43.5), (270, 104.8, 52.5), BodyDeep);
    }

    private static void BuildWheels(Scene3D s)
    {
        AddWheel(s, cx: 58,  y0: WheelYOut, y1: WheelYIn, outerLeft: true);
        AddWheel(s, cx: 212, y0: WheelYOut, y1: WheelYIn, outerLeft: true);
        AddWheel(s, cx: 58,  y0: 120 - WheelYIn, y1: 120 - WheelYOut, outerLeft: false);
        AddWheel(s, cx: 212, y0: 120 - WheelYIn, y1: 120 - WheelYOut, outerLeft: false);
    }

    private static void AddWheel(Scene3D s, double cx, double y0, double y1, bool outerLeft)
    {
        var yMid = (y0 + y1) * 0.5;
        var yOuter = outerLeft ? y0 : y1;
        var yInner = outerLeft ? y1 : y0;
        var sign = outerLeft ? 1.0 : -1.0;

        var wheelGroup = s.Group().Pivot((cx, yMid, WheelR), Axis.Y).RotateWith("wheel", 0, 360).Clickable("drive");

        var tireProfile = new Path2D()
            .Line(12.2, yOuter + sign * 0.5)
            .Line(16.2, yOuter)
            .Line(WheelR, yOuter + sign * 1.4)
            .Line(WheelR, yInner - sign * 1.4)
            .Line(16.2, yInner)
            .Line(12.2, yInner - sign * 0.5);
        wheelGroup.Lathe(tireProfile, Axis.Y, (cx, WheelR), TireBlack);

        var rimProfile = new Path2D()
            .Line(10.5, yOuter + sign * 1.6)
            .Line(10.5, yOuter + sign * 0.4)
            .Line(12.2, yOuter + sign * 0.4)
            .Line(12.2, yOuter + sign * 1.6);
        wheelGroup.Lathe(rimProfile, Axis.Y, (cx, WheelR), Alloy);

        var rotor = wheelGroup.Extrude(new Path2D().Circle(cx, WheelR, 10.5), Axis.Y,
            outerLeft ? yOuter + 1.6 : yOuter - 2.6,
            outerLeft ? yOuter + 2.6 : yOuter - 1.6,
            Carbon);
        var outerFace = rotor.Face(outerLeft ? Side.Front : Side.Back);
        outerFace.Draw(g =>
        {
            g.Circle(cx + (outerLeft ? -3.4 : 3.4), WheelR + 3.5, 2.9).Fill(CaliperRed);
            for (var i = 0; i < 5; i++)
            {
                var baseAng = (i * 72.0) * Deg;
                var a1 = baseAng - 10.0 * Deg;
                var a2 = baseAng + 10.0 * Deg;
                var mx = cx + 3.2 * Math.Cos(baseAng);
                var mz = WheelR + 3.2 * Math.Sin(baseAng);
                g.Line((mx, mz), (cx + 9.8 * Math.Cos(a1), WheelR + 9.8 * Math.Sin(a1))).Stroke(Alloy, 1.5);
                g.Line((mx, mz), (cx + 9.8 * Math.Cos(a2), WheelR + 9.8 * Math.Sin(a2))).Stroke(Alloy, 1.5);
            }
            g.Circle(cx, WheelR, 2.5).Fill(CarbonMid);
            g.Circle(cx, WheelR, 1.2).Fill(CaliperRed);
        });
    }
}
