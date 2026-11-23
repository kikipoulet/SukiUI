using System;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Mini3D.Controls;

/// <summary>
/// Interactive armored PIN-code safe vault:
/// Click any of the 12 3D keypad keys right on the door—each key physically depresses on press!
/// Enter 1337 (hint on the right sticky note) to spin the 3-spoke vault wheel and swing
/// open the heavy armored door, revealing locking bolts, gold bars, and a glowing energy core.
/// </summary>
public sealed class SafeVaultScene : Model3D
{
    private static readonly SKColor ArmorLight = Rgb(220, 227, 240);
    private static readonly SKColor ArmorMid = Rgb(184, 194, 214);
    private static readonly SKColor DoorSlate = Rgb(54, 63, 82);
    private static readonly SKColor InteriorDark = Hex("#1A2030");
    private static readonly SKColor ShelfSteel = Rgb(134, 146, 170);
    private static readonly SKColor Chrome = Rgb(208, 218, 234);
    private static readonly SKColor ScreenBg = Hex("#0D121F");
    private static readonly SKColor Accent = Hex("#5B8CFF");
    private static readonly SKColor AccentSoft = Hex("#8EB2FF");
    private static readonly SKColor Emerald = Hex("#3FD38C");
    private static readonly SKColor Crimson = Hex("#F25C69");
    private static readonly SKColor Gold = Hex("#F2B83B");
    private static readonly SKColor GoldRim = Hex("#C6891B");
    private static readonly SKColor NoteYellow = Hex("#FFE97D");

    public static readonly string[] KeyIds =
    {
        "key_0", "key_1", "key_2", "key_3", "key_4", "key_5",
        "key_6", "key_7", "key_8", "key_9", "key_c", "key_ok"
    };

    protected override void Build(Scene3D s)
    {
        s.Camera(yaw: 28, pitch: 22, intro: -10);
        s.Frame(center: (82, 66, 98), size: (410, 308), screen: (205, 155));
        s.Ground(z: 0).Shadow((-6, -6), (186, 166));
        s.EdgeColor(Rgb(56, 66, 90));

        s.Input("door", ms: 1050, initial: 0);
        s.Input("wheelRot", ms: 520, initial: 0);
        s.Input("unlocked", ms: 320, initial: 0);
        s.Input("error", ms: 240, initial: 0);
        s.Input("pinState", ms: 0, initial: 0);

        foreach (var k in KeyIds)
            s.Input(k, ms: 105, initial: 0);

        s.Channel("openGlow", c => Math.Max(c["unlocked"], Math.Clamp(c["door"] * 2.0, 0, 1)));

        // Four heavy corner steel feet
        s.Box((8, 8, 0), (34, 34, 8), DoorSlate, open: Side.Top);
        s.Box((146, 8, 0), (172, 34, 8), DoorSlate, open: Side.Top);
        s.Box((8, 126, 0), (34, 152, 8), DoorSlate, open: Side.Top);
        s.Box((146, 126, 0), (172, 152, 8), DoorSlate, open: Side.Top);

        // Outer armored shell (5 non-overlapping slabs around cavity 16..164, 16..144, 24..176)
        s.Box((0, 0, 8), (180, 160, 24), ArmorLight);
        var topSlab = s.Box((0, 0, 176), (180, 160, 192), ArmorLight);
        topSlab.Face(Side.Top).Draw(g =>
        {
            g.Screws(3.2, (12, 12), (168, 12), (12, 148), (168, 148));
            g.RoundRect((34, 38), (146, 122), 6).Fill(ArmorMid).Stroke(Rgb(96, 108, 134), 0.7);
            g.RoundRect((56, 56), (124, 104), 4).Stroke(Rgb(120, 132, 158), 0.6);
        });

        var leftWall = s.Box((0, 0, 24), (16, 160, 176), ArmorLight, open: Side.Top | Side.Bottom);
        leftWall.Face(Side.Left).Draw(g =>
        {
            g.RoundRect((30, 46), (130, 78), 3).Fill(DoorSlate).Stroke(Rgb(96, 108, 134), 0.7);
            // Side.Left advances toward decreasing Y (right = -1): start near rear Y=120 so it centers inside (30..130)
            g.Text("CLASS IV // GRADE A", (118, 59), 6.2).Fill(ArmorLight);
            g.Screws(2.2, (16, 36), (144, 36), (16, 164), (144, 164));
        });

        var rightWall = s.Box((164, 0, 24), (180, 160, 176), ArmorLight, open: Side.Top | Side.Bottom);
        rightWall.Face(Side.Right).Draw(g =>
        {
            // Sticky note taped to the right side of the safe
            g.RoundRect((48, 104), (104, 150), 2).Fill(NoteYellow).Stroke(Rgb(196, 168, 72), 0.7);
            g.Rect((68, 146), (84, 152)).Fill(Rgb(225, 232, 244));
            g.Text("PIN:", (58, 130), 7.5).Fill(DoorSlate);
            g.Text("1337", (58, 115), 10.5).Fill(ScreenBg);
            g.Screws(2.2, (16, 36), (144, 36), (16, 164), (144, 164));
        });

        s.Box((16, 144, 24), (164, 160, 176), ArmorMid, open: Side.Top | Side.Bottom | Side.Left | Side.Right);

        // Armored cavity + shelf + gold bullion + glowing core inside DepthGroup
        var cavity = s.DepthGroup((90, 82, 100));
        cavity.Wall(InteriorDark, (16.2, 143.8, 24.2), (163.8, 143.8, 24.2), (163.8, 143.8, 175.8), (16.2, 143.8, 175.8))
              .Draw(g =>
              {
                  g.RoundRect((28, 108), (152, 160), 4).Fill(ScreenBg);
                  g.RoundRect((42, 128), (138, 140), 2).Fill(Accent).Glow("openGlow", 0.75);
                  g.RoundRect((42, 52), (138, 60), 2).Fill(Emerald).Glow("openGlow", 0.45);
              });
        cavity.Wall(Rgb(36, 42, 60), (16.2, 16.2, 24.2), (163.8, 16.2, 24.2), (163.8, 143.8, 24.2), (16.2, 143.8, 24.2));
        cavity.Wall(Rgb(24, 29, 44), (16.2, 143.8, 175.8), (163.8, 143.8, 175.8), (163.8, 16.2, 175.8), (16.2, 16.2, 175.8));
        cavity.Wall(Rgb(34, 40, 58), (16.2, 16.2, 24.2), (16.2, 143.8, 24.2), (16.2, 143.8, 175.8), (16.2, 16.2, 175.8));
        cavity.Wall(Rgb(34, 40, 58), (163.8, 143.8, 24.2), (163.8, 16.2, 24.2), (163.8, 16.2, 175.8), (163.8, 143.8, 175.8));

        // Gold bullion stack (pyramid on floor)
        var g1 = cavity.Box((30, 44, 24.5), (76, 114, 42), Gold);
        g1.Face(Side.Top).Draw(g => g.Text("999.9", (42, 74), 6.5).Fill(GoldRim));
        var g2 = cavity.Box((94, 46, 24.5), (140, 116, 42), Gold);
        g2.Face(Side.Top).Draw(g => g.Text("999.9", (106, 76), 6.5).Fill(GoldRim));
        var g3 = cavity.Box((62, 52, 42), (108, 110, 58), Gold);
        g3.Face(Side.Top).Draw(g => g.Text("GOLD", (75, 76), 6.5).Fill(GoldRim));

        // Steel shelf dividing cavity
        cavity.Box((17, 30, 92), (163, 143, 98), ShelfSteel);

        // Glowing octahedral energy relic on top shelf (explicit 8-sided crystal)
        var relic = cavity.Extrude(new Path2D().Circle(90, 86, 22, segments: 8), Axis.Z, 98, 144, Accent, rim: AccentSoft);
        relic.Face(Side.Top).Draw(g => g.Circle(90, 86, 13, 8).Fill(SKColors.White).Glow("openGlow", 0.95));
        cavity.Box((72, 68, 144), (108, 104, 152), Gold, open: Side.Bottom);

        // Exterior cylindrical hinge barrels on left edge
        s.Extrude(new Path2D().Circle(12, 0, 5), Axis.Z, 40, 66, ArmorMid, rim: DoorSlate);
        s.Extrude(new Path2D().Circle(12, 0, 5), Axis.Z, 134, 160, ArmorMid, rim: DoorSlate);

        // Swinging armored vault door (pivots outward & left around (10, 0, 100))
        var doorGroup = s.Group().Pivot((10, 0, 100), Axis.Z).RotateWith("door", 0, -106);
        var door = doorGroup.Box((16.5, 0, 24.5), (163.5, 14, 175.5), DoorSlate);

        // Chrome locking bolts along right edge of door (visible when door swings open!)
        doorGroup.Box((163.5, 5, 48), (169, 9, 58), Chrome, open: Side.Left);
        doorGroup.Box((163.5, 5, 95), (169, 9, 105), Chrome, open: Side.Left);
        doorGroup.Box((163.5, 5, 142), (169, 9, 152), Chrome, open: Side.Left);

        var front = door.Face(Side.Front);
        front.Draw(g =>
        {
            g.RoundRect((21, 29), (159, 171), 5).Stroke(Rgb(98, 112, 140), 1.1);
            g.Screws(2.2, (26, 35), (154, 35), (26, 165), (154, 165));
            g.Text("VAULT // 04", (25, 157), 7.5).Fill(ArmorMid);

            // Keypad backing plate + OLED recess
            g.RoundRect((77, 36), (153, 164), 4).Fill(Rgb(36, 43, 60)).Stroke(Rgb(94, 108, 136), 0.8);
            g.RoundRect((82, 132), (148, 157), 3).Fill(ScreenBg).Stroke(Rgb(80, 92, 118), 0.7);

            // Wheel recess backing on left side
            g.Circle(49, 91, 25, 18).Fill(Rgb(36, 43, 60)).Stroke(Rgb(90, 104, 132), 0.8);

            // Lock status LEDs
            g.Circle(88, 165, 2.3, 10).FillWith("unlocked", Rgb(62, 70, 88), Emerald).Glow("unlocked", 0.95);
            g.Circle(98, 165, 2.3, 10).FillWith("error", Rgb(62, 70, 88), Crimson).Glow("error", 0.95);
        })
        .DrawWith("pinState", (g, raw) =>
        {
            var code = (int)Math.Round(raw);
            var status = code / 1_000_000;
            var rem = code % 1_000_000;
            var len = rem / 10_000;
            var d = rem % 10_000;

            string txt;
            SKColor col;
            if (status == 2) { txt = "OPEN"; col = Emerald; }
            else if (status == 1) { txt = "ERR!"; col = Crimson; }
            else if (len == 0) { txt = "_ _ _ _"; col = AccentSoft; }
            else
            {
                var d0 = d / 1000 % 10;
                var d1 = d / 100 % 10;
                var d2 = d / 10 % 10;
                var d3 = d % 10;
                txt = $"{(len >= 1 ? d0.ToString() : "_")} {(len >= 2 ? d1.ToString() : "_")} {(len >= 3 ? d2.ToString() : "_")} {(len >= 4 ? d3.ToString() : "_")}";
                col = SKColors.White;
            }
            g.Text(txt, (90, 141), 11.5).Fill(col);
        });

        // Spinning 3-spoke vault wheel (mounted on door.Front so it always sorts cleanly above the door!)
        var wheelGroup = doorGroup.Group().MountTo(front).Pivot((49, -4, 91), Axis.Y).RotateWith("wheelRot", 0, 140).Clickable("wheel");
        var wheel = wheelGroup.Extrude(new Path2D().Circle(49, 91, 22), Axis.Y, -6, -0.5, ArmorLight, rim: ArmorMid);
        wheel.Face(Side.Front).Draw(g =>
        {
            g.Circle(49, 91, 17).Fill(DoorSlate).Stroke(ArmorMid, 0.9);
            for (var i = 0; i < 3; i++)
            {
                var a = (i * 120.0 + 30.0) * Deg;
                g.Line((49, 91), (49 + 16 * Math.Cos(a), 91 + 16 * Math.Sin(a))).Stroke(ArmorLight, 3.0);
            }
            g.Circle(49, 91, 5.5).Fill(ArmorLight);
            g.Circle(49, 91, 2.5).FillWith("unlocked", Crimson, Emerald).Glow("unlocked", 0.9);
        });

        // 12 Physically animated 3D keypad buttons (mounted on door.Front so ALL 12 always sort cleanly above the door!)
        string[,] labels =
        {
            { "1", "2", "3" },
            { "4", "5", "6" },
            { "7", "8", "9" },
            { "C", "0", "OK" }
        };
        string[,] ids =
        {
            { "key_1", "key_2", "key_3" },
            { "key_4", "key_5", "key_6" },
            { "key_7", "key_8", "key_9" },
            { "key_c", "key_0", "key_ok" }
        };

        for (var r = 0; r < 4; r++)
        {
            var z0 = 108 - r * 22;
            var z1 = z0 + 16;
            for (var c = 0; c < 3; c++)
            {
                var x0 = 83 + c * 22;
                var x1 = x0 + 18;
                var label = labels[r, c];
                var id = ids[r, c];
                var capColor = id == "key_ok" ? Accent : id == "key_c" ? Rgb(140, 56, 70) : Rgb(74, 86, 112);

                // Recessed switch socket outline drawn statically on front
                front.Draw(g => g.RoundRect((x0 - 0.8, z0 - 0.8), (x1 + 0.8, z1 + 0.8), 2).Fill(ScreenBg));

                // Moving 3D keycap flush with door front (depresses from -3.8..0 to -2.0..+1.8 when clicked)
                var keyGroup = doorGroup.Group().MountTo(front).MoveWith(id, (0, 1.8, 0)).Clickable(id);
                var key = keyGroup.Box((x0, -3.8, z0), (x1, 0, z1), capColor, open: Side.Back);
                key.Face(Side.Front).Draw(g =>
                {
                    var tx = label.Length > 1 ? x0 + 2.5 : x0 + 5.5;
                    g.Text(label, (tx, z0 + 4.5), label.Length > 1 ? 7.0 : 8.5).Fill(SKColors.White);
                });
            }
        }
    }
}
