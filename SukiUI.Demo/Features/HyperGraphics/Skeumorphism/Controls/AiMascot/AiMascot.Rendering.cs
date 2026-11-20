using System;
using Avalonia;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public sealed partial class AiMascot
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private const int Moods = 20;
        private const double Blend = 0.15;
        private const double Faint = 0.004;
        private const double GazeTravel = 1.8;
        private const double GazeLag = 0.12;
        private const double WaveX = 37, WaveY = 30;
        private static readonly SKColor ScanColor = new(0x72, 0x2E, 0xD1, 36);

        private HyperLayer _ringA = null!, _ringB = null!, _body = null!, _scan = null!, _shine = null!, _cheeks = null!;
        private HyperLayer _eyes = null!, _eyesDoubt = null!, _eyesHappy = null!, _eyesSleep = null!, _glints = null!, _mouth = null!, _frown = null!;
        private HyperLayer _particle = null!, _sparkA = null!, _sparkB = null!, _zA = null!, _zB = null!;
        private HyperLayer _eyesX = null!, _drop = null!, _question = null!, _heart = null!, _trailA = null!, _trailB = null!, _trailC = null!;
        private HyperLayer _spiralL = null!, _spiralR = null!, _teeth = null!, _chill = null!, _snowA = null!, _snowB = null!, _noteA = null!, _noteB = null!;
        private HyperLayer _shades = null!, _glare = null!, _smirk = null!;
        private HyperLayer[] _conf = null!;
        private HyperPath[] _confPaths = null!;
        private HyperPath _scanPath = null!, _doubtPath = null!, _sparkAPath = null!, _sparkBPath = null!, _sleepPath = null!;
        private HyperPath _snowAPath = null!, _snowBPath = null!, _noteAPath = null!, _noteBPath = null!;
        private Point _home, _orbitCenter, _rest, _pivot;
        private double _orbitRadius;
        private SKPaint _band = null!;
        private readonly HyperSmooth[] _weights = new HyperSmooth[Moods];
        private readonly double[] _start = new double[Moods];
        private int _current = -1;
        private HyperSmooth _gazeX, _gazeY;

        private double _bx, _by, _cos = 1, _sin, _rot, _bs = 1, _bsy = 1;
        private double _scanT;

        public Stage() => TargetFps = 30;

        protected override void OnBind(IHyperBinder ui)
        {
            _ringA = ui.Layer("ringA");
            _ringB = ui.Layer("ringB");
            _body = ui.Layer("body");
            _scan = ui.Layer("scan");
            _shine = ui.Layer("shine");
            _cheeks = ui.Layer("cheeks");
            _eyes = ui.Layer("eyes");
            _eyesDoubt = ui.Layer("eyesDoubt");
            _eyesHappy = ui.Layer("eyesHappy");
            _eyesSleep = ui.Layer("eyesSleep");
            _glints = ui.Layer("glints");
            _mouth = ui.Layer("mouth");
            _frown = ui.Layer("frown");
            _particle = ui.Layer("particle");
            _sparkA = ui.Layer("sparkA");
            _sparkB = ui.Layer("sparkB");
            _zA = ui.Layer("zA");
            _zB = ui.Layer("zB");
            _eyesX = ui.Layer("eyesX");
            _drop = ui.Layer("drop");
            _question = ui.Layer("question");
            _heart = ui.Layer("heart");
            _trailA = ui.Layer("trailA");
            _trailB = ui.Layer("trailB");
            _trailC = ui.Layer("trailC");
            _spiralL = ui.Layer("spiralL");
            _spiralR = ui.Layer("spiralR");
            _teeth = ui.Layer("teeth");
            _chill = ui.Layer("chill");
            _snowA = ui.Layer("snowA");
            _snowB = ui.Layer("snowB");
            _noteA = ui.Layer("noteA");
            _noteB = ui.Layer("noteB");
            _shades = ui.Layer("shades");
            _glare = ui.Layer("glare");
            _smirk = ui.Layer("smirk");
            _snowAPath = ui.Path("snowA");
            _snowBPath = ui.Path("snowB");
            _noteAPath = ui.Path("noteA");
            _noteBPath = ui.Path("noteB");
            _conf = [ui.Layer("confA"), ui.Layer("confB"), ui.Layer("confC"), ui.Layer("confD")];
            _confPaths = [ui.Path("confA"), ui.Path("confB"), ui.Path("confC"), ui.Path("confD")];
            _scanPath = ui.Path("scan");
            _doubtPath = ui.Path("doubt");
            _sparkAPath = ui.Path("sparkA");
            _sparkBPath = ui.Path("sparkB");
            _sleepPath = ui.Path("sleep");

            _home = _particle.Pivot;
            var orbit = ui.Zone("orbit");
            _orbitCenter = orbit.Center;
            _orbitRadius = orbit.Width / 2;
            _rest = ui.Zone("rest").Center;
            _pivot = _body.Pivot;

            _band = ui.Own(new SKPaint { IsAntialias = false, Style = SKPaintStyle.Fill });
            ui.Paint("scan", PaintScan);
        }

        protected override bool OnFrame(in HyperFrame frame, in AiMascotState state)
        {
            var mood = (int)state.Mood;
            if (mood != _current)
            {
                _start[mood] = _current < 0 ? 0 : frame.Seconds;
                _current = mood;
            }

            var total = 0.0;
            for (var i = 0; i < Moods; i++)
            {
                _weights[i].Follow(i == mood ? 1 : 0, in frame, Blend);
                total += _weights[i].Value;
            }

            _gazeX.Follow(state.GazeX, in frame, GazeLag);
            _gazeY.Follow(state.GazeY, in frame, GazeLag);

            var t = frame.Seconds;
            var pose = default(MascotPose);
            for (var i = 0; i < Moods; i++)
            {
                var w = _weights[i].Value;
                if (w <= 0) continue;
                var p = PoseOf((AiMascotMood)i, t, t - _start[i]);
                pose.Add(in p, w / total);
            }
            Apply(in pose);

            return true;
        }

        private MascotPose PoseOf(AiMascotMood mood, double t, double since) => mood switch
        {
            AiMascotMood.Waking => Waking(since),
            AiMascotMood.Curious => Curious(t),
            AiMascotMood.Oops => Oops(t, since),
            AiMascotMood.Celebrate => Celebrate(t),
            AiMascotMood.Petted => Petted(since),
            AiMascotMood.Greeting => Greeting(t),
            AiMascotMood.Showing => Showing(t),
            AiMascotMood.Yawning => Yawning(since),
            AiMascotMood.Dizzy => Dizzy(t),
            AiMascotMood.Shivering => Shivering(t),
            AiMascotMood.Humming => Humming(t),
            AiMascotMood.Cool => Cool(t),
            AiMascotMood.Thinking => Thinking(t),
            AiMascotMood.Analyzing => Analyzing(t),
            AiMascotMood.Talking => Talking(t),
            AiMascotMood.Success => Success(t),
            AiMascotMood.Hesitant => Hesitant(t),
            AiMascotMood.Sleeping => Sleeping(t),
            AiMascotMood.Listening => Listening(t),
            _ => Idle(t)
        };

        private MascotPose Rest()
        {
            var p = default(MascotPose);
            p.BodyScale = p.BodyScaleY = p.EyeOpen = p.MouthOpen = p.PScale = p.POpacity = p.RingAScale = p.RingBScale = 1;
            p.EyeScale = p.MouthScale = p.HeartScale = 1;
            p.PX = _home.X;
            p.PY = _home.Y;
            return p;
        }

        private MascotPose Idle(double t)
        {
            var p = Rest();
            p.BodyY = -1.4 * Ease(Phase(t, 3.2));
            p.EyeOpen = Blink(t, 4);
            p.Pill = 1;
            p.EyeX = GazeTravel * _gazeX.Value;
            p.EyeY = GazeTravel * _gazeY.Value;
            var q = Ease(Phase(t, 2.6));
            p.PX += 0.6 * q;
            p.PY -= 1.8 * q;
            p.PRot = 18 * q;
            return p;
        }

        private MascotPose Thinking(double t)
        {
            var p = Rest();
            var u = Phase(t, 6);
            var a = -Math.PI / 4 + 2 * Math.PI * u;
            var (cos, sin) = (Math.Cos(a), Math.Sin(a));
            p.PX = _orbitCenter.X + _orbitRadius * cos;
            p.PY = _orbitCenter.Y + _orbitRadius * sin;
            p.PRot = 360 * (u < 0.5 ? u : u - 1);
            p.EyeX = 1.5 * cos;
            p.EyeY = 1.5 * sin;
            p.EyeOpen = 0.89 * Blink(t, 5);
            p.Pill = 1;
            return p;
        }

        private MascotPose Analyzing(double t)
        {
            var p = Rest();
            var s = Ease(Phase(t, 3));
            var point = _scanPath.PointAt(s);
            p.PX = point.X;
            p.PY = point.Y;
            p.PScale = 0.85;
            p.EyeX = -1.4 + 2.8 * s;
            p.EyeY = 1.6;
            p.EyeOpen = 0.91;
            p.Pill = 1;
            p.Scan = 1;
            p.ScanT = s;
            return p;
        }

        private MascotPose Talking(double t)
        {
            var p = Rest();
            var beat = Ease(Phase(t, 0.6));
            p.BodyY = -0.5 * beat;
            p.EyeY = -1;
            p.EyeOpen = Blink(t, 5);
            p.Pill = 1;
            p.Mouth = 1;
            p.MouthOpen = (0.55 + 0.9 * beat + 0.25 * Ease(Phase(t, 0.9))) / 1.7;
            p.PY -= 1.2 * Ease(Phase(t, 1.2));
            return p;
        }

        private MascotPose Success(double t)
        {
            var p = Rest();
            var u = Phase(t, 1.8);
            p.BodyY = u < 0.45 ? -3 * Math.Sin(Math.PI * u / 0.45) : 0;
            p.Happy = 1;
            p.Cheeks = 1;
            p.PRot = 360 * (Smooth((u - 0.1) / 0.45) % 1);
            Spark(u - 0.2, 0.8, out p.SparkA, out p.SparkAT);
            Spark(u - 0.3, 0.8, out p.SparkB, out p.SparkBT);
            return p;
        }

        private MascotPose Hesitant(double t)
        {
            var p = Rest();
            var e = Ease(Phase(t, 4));
            p.BodyRot = -7 + 13 * e;
            p.Doubt = 1;
            p.Frown = 1;
            p.EyeX = -0.8 + 1.6 * e;
            p.EyeY = -1;
            var point = _doubtPath.PointAt(e);
            p.PX = point.X;
            p.PY = point.Y;
            return p;
        }

        private MascotPose Sleeping(double t)
        {
            var p = Rest();
            var e = Ease(Phase(t, 4.5));
            p.BodyScale = 1 + 0.03 * e;
            p.BodyScaleY = (1 - 0.04 * e) / p.BodyScale;
            p.Sleep = 1;
            p.PX = _rest.X;
            p.PY = _rest.Y;
            p.PRot = -20;
            p.POpacity = 0.75;
            p.PAttach = 1;
            var k = Phase(t, 3);
            p.ZA = 0.7 * Math.Sin(Math.PI * k);
            p.ZAT = k;
            k = Phase(t + 1.5, 3);
            p.ZB = 0.6 * Math.Sin(Math.PI * k);
            p.ZBT = k;
            return p;
        }

        private MascotPose Listening(double t)
        {
            var p = Rest();
            p.Pill = 1;
            p.EyeOpen = 8 / 7.0;
            p.Glints = 1;
            var k = Phase(t, 2.4);
            p.RingA = 0.55 * (1 - k);
            p.RingAScale = 1 + 0.39 * k;
            k = Phase(t + 1.2, 2.4);
            p.RingB = 0.55 * (1 - k);
            p.RingBScale = 1 + 0.39 * k;
            p.PScale = 1 + 0.12 * Ease(Phase(t, 2.4));
            return p;
        }

        private MascotPose Waking(double s)
        {
            var p = Rest();
            var u = s / 1.6;
            var squash = Smooth(u / 0.3);
            var stretch = Smooth((u - 0.3) / 0.25);
            var settle = Smooth((u - 0.55) / 0.25);
            var sx = Lerp(Lerp(Lerp(1, 1.04, squash), 0.94, stretch), 1, settle);
            var sy = Lerp(Lerp(Lerp(1, 0.88, squash), 1.12, stretch), 1, settle);
            p.BodyScale = sx;
            p.BodyScaleY = sy / sx;
            p.BodyY = -3 * stretch * (1 - settle);
            var open = Smooth((u - 0.3) / 0.15);
            p.Sleep = 1 - open;
            p.Pill = open;
            p.Glints = open;
            p.Mouth = open;
            p.MouthScale = 0.8;
            p.MouthOpen = 0.7;
            var k = Smooth((u - 0.35) / 0.4);
            p.PX = Lerp(_rest.X, _home.X, k);
            p.PY = Lerp(_rest.Y, _home.Y, k) - 5 * Math.Sin(Math.PI * k);
            p.PRot = -20 * (1 - k);
            p.POpacity = 0.75 + 0.25 * k;
            p.PAttach = 1 - k;
            return p;
        }

        private MascotPose Curious(double t)
        {
            var p = Rest();
            p.BodyRot = -12;
            p.BodyY = -1.5 * Ease(Phase(t, 2.4));
            p.Pill = 1;
            p.Glints = 1;
            p.EyeScale = 1.28;
            p.EyeOpen = Blink(t, 3.5);
            p.Mouth = 1;
            p.MouthScale = 0.8;
            p.PY -= 4 * Ease(Phase(t, 1.2));
            var q = Phase(t, 2.4);
            p.Question = Smooth((q - 0.3) / 0.15) * (1 - Smooth((q - 0.85) / 0.15));
            p.QY = -1.2 * Ease(q);
            return p;
        }

        private MascotPose Oops(double t, double s)
        {
            var p = Rest();
            var u = Phase(t, 1.2);
            var shake = u < 0.5 ? Math.Sin(16 * Math.PI * u) * (1 - u / 0.5) : 0;
            p.BodyRot = 5 * shake;
            p.BodyX = 2 * shake;
            p.Xeyes = 1;
            p.Frown = 1;
            p.Drop = 0.9 * Math.Sin(Math.PI * u);
            p.DropY = 5 * u;
            var fall = s < 0.5 ? 35 * (s / 0.5) * (s / 0.5) : s < 0.8 ? 35 - 6 * Math.Sin(Math.PI * (s - 0.5) / 0.3) : 35;
            var k = Math.Min(1, s / 0.8);
            p.PX = _home.X + 6 * k;
            p.PY = _home.Y + fall;
            p.PRot = 120 * k;
            return p;
        }

        private MascotPose Celebrate(double t)
        {
            var p = Rest();
            var u = Phase(t, 1.2);
            var jump = u < 0.6 ? Math.Sin(Math.PI * u / 0.6) : 0;
            var land = u >= 0.6 ? Math.Sin(Math.PI * (u - 0.6) / 0.4) : 0;
            p.BodyY = -7 * jump;
            p.BodyScale = 1 + 0.08 * land - 0.04 * jump;
            p.BodyScaleY = (1 - 0.1 * land + 0.06 * jump) / p.BodyScale;
            p.Happy = 1;
            p.Cheeks = 1;
            p.Mouth = 1;
            p.MouthScale = 1.15;
            p.MouthOpen = 1.35;
            var a = -Math.PI / 4 + 2 * Math.PI * u;
            p.PX = _orbitCenter.X + _orbitRadius * Math.Cos(a);
            p.PY = _orbitCenter.Y + _orbitRadius * Math.Sin(a);
            p.PRot = 360 * (u < 0.5 ? u : u - 1);
            p.Conf = Math.Sin(Math.PI * u);
            p.ConfT = u;
            return p;
        }

        private MascotPose Petted(double s)
        {
            var p = Rest();
            var v = Phase(s, 1.6);
            var k = Math.Sin(Math.PI * Math.Min(1, v / 0.55));
            p.BodyScale = 1 + 0.1 * k;
            p.BodyScaleY = (1 - 0.12 * k) / p.BodyScale;
            p.Happy = 1;
            p.Cheeks = 1;
            p.Mouth = 1;
            p.MouthScale = 0.9;
            p.MouthOpen = 0.6;
            p.POpacity = 0;
            p.Heart = v < 0.3 ? v / 0.3 : (1 - v) / 0.7;
            p.HeartY = 4 - 11 * v;
            p.HeartScale = 0.5 + 0.55 * Smooth(v / 0.3);
            return p;
        }

        private MascotPose Greeting(double t)
        {
            var p = Rest();
            p.BodyRot = 6 * Math.Sin(2 * Math.PI * t / 1.4);
            p.Happy = 1;
            p.Mouth = 1;
            p.MouthOpen = 0.7;
            var a = 25 * Math.PI / 180 * Math.Sin(2 * Math.PI * t / 0.7);
            var (cos, sin) = (Math.Cos(a), Math.Sin(a));
            var (vx, vy) = (_home.X - WaveX, _home.Y - WaveY);
            p.PX = WaveX + vx * cos - vy * sin;
            p.PY = WaveY + vx * sin + vy * cos;
            p.PRot = a * 180 / Math.PI;
            return p;
        }

        private MascotPose Showing(double t)
        {
            var p = Rest();
            var e = Ease(Phase(t, 2));
            p.BodyRot = 6 + 3 * e;
            p.BodyY = -e;
            p.Pill = 1;
            p.Glints = 1;
            p.EyeX = 2;
            p.EyeY = -2.2;
            p.EyeOpen = Blink(t, 4);
            p.Mouth = 1;
            p.MouthOpen = 0.7;
            var q = Phase(t, 1.4);
            var z = Ease(q);
            p.PX = _home.X + 7 * z;
            p.PY = _home.Y - 8 * z;
            p.TrailA = Bump(q);
            p.TrailB = Bump(q - 0.086);
            p.TrailC = Bump(q - 0.17);
            return p;
        }

        private MascotPose Yawning(double s)
        {
            var p = Rest();
            var u = Math.Min(1, s / 2.4);
            var yawn = Smooth((u - 0.1) / 0.3) * (1 - Smooth((u - 0.6) / 0.3));
            var drowsy = Smooth((u - 0.7) / 0.3);
            p.BodyScale = 1 - 0.03 * yawn + 0.02 * drowsy;
            p.BodyScaleY = (1 + 0.08 * yawn - 0.05 * drowsy) / p.BodyScale;
            p.Pill = 1;
            p.EyeOpen = 1 - 0.78 * yawn - 0.5 * drowsy * (1 - yawn);
            p.Mouth = 1;
            p.MouthScale = 0.7 + 0.7 * yawn;
            p.MouthOpen = 1 + 0.86 * yawn;
            p.PX = _home.X - 2 * drowsy;
            p.PY = _home.Y + 3 * Smooth(u / 0.6) + 2 * drowsy;
            return p;
        }

        private MascotPose Dizzy(double t)
        {
            var p = Rest();
            p.BodyRot = 8 * Math.Sin(2 * Math.PI * t / 1.6);
            p.BodyX = p.BodyRot / 8;
            p.Spiral = 1;
            p.SpinAngle = Wrap(360 * Phase(t, 1));
            p.Frown = 1;
            var u = Phase(t, 0.9);
            var a = 2 * Math.PI * u;
            p.PX = 29 + 15 * Math.Cos(a);
            p.PY = 15 + 4 * Math.Sin(a);
            p.PRot = Wrap(360 * u);
            return p;
        }

        private MascotPose Shivering(double t)
        {
            var p = Rest();
            var jitter = Math.Sin(2 * Math.PI * t / 0.09);
            p.BodyX = 0.7 * jitter;
            p.Pill = 1;
            p.EyeOpen = 0.55;
            p.Teeth = 1;
            p.TeethOpen = 0.85 + 0.35 * Math.Sin(2 * Math.PI * t / 0.12);
            p.Chill = 1;
            Flake(Phase(t, 2.4), out p.SnowA, out p.SnowAT);
            Flake(Phase(t + 1.2, 2.4), out p.SnowB, out p.SnowBT);
            p.PX += 0.5 * jitter;
            return p;
        }

        private MascotPose Humming(double t)
        {
            var p = Rest();
            p.BodyRot = 5 * Math.Sin(2 * Math.PI * t / 1.6);
            p.Sleep = 1;
            p.Cheeks = 1;
            p.Mouth = 1;
            p.MouthScale = 0.75;
            p.MouthOpen = 1.1 + 0.15 * Ease(Phase(t, 0.8));
            Flake(Phase(t, 2.4), out p.NoteA, out p.NoteAT);
            Flake(Phase(t + 1.2, 2.4), out p.NoteB, out p.NoteBT);
            p.PY -= 1.5 * Ease(Phase(t, 1.6));
            return p;
        }

        private MascotPose Cool(double t)
        {
            var p = Rest();
            var e = Ease(Phase(t, 1.2));
            p.BodyY = -1.2 * e;
            p.BodyRot = -2 + 4 * e;
            p.Shades = 1;
            p.Smirk = 1;
            var k = Math.Clamp((Phase(t, 2.6) - 0.55) / 0.3, 0, 1);
            p.GlareX = -8 + 16 * k;
            p.Glare = Math.Abs(p.GlareX) < 1 ? 0 : 0.9 * Math.Sin(Math.PI * k);
            var a = -Math.PI / 4 + 2 * Math.PI * Phase(t, 6);
            p.PX = _orbitCenter.X + _orbitRadius * Math.Cos(a);
            p.PY = _orbitCenter.Y + _orbitRadius * Math.Sin(a);
            return p;
        }

        private void Apply(in MascotPose p)
        {
            _bx = p.BodyX;
            _by = p.BodyY;
            _rot = p.BodyRot;
            var rad = p.BodyRot * Math.PI / 180;
            _cos = Math.Cos(rad);
            _sin = Math.Sin(rad);
            _bs = p.BodyScale;
            _bsy = p.BodyScaleY;

            Attach(_ringA, 0, 0, p.RingAScale, 1, p.RingA);
            Attach(_ringB, 0, 0, p.RingBScale, 1, p.RingB);
            Attach(_body, 0, 0, 1, 1, 1);
            _scanT = p.ScanT;
            Attach(_scan, 0, 0, 1, 1, p.Scan);
            Attach(_shine, 0, 0, 1, 1, 1);
            Attach(_cheeks, 0, 0, 1, 1, p.Cheeks);
            Attach(_eyes, p.EyeX, p.EyeY, p.EyeScale, p.EyeOpen, p.Pill);
            Attach(_eyesDoubt, p.EyeX, p.EyeY, 1, 1, p.Doubt);
            Attach(_eyesHappy, p.EyeX, p.EyeY, 1, 1, p.Happy);
            Attach(_eyesSleep, p.EyeX, p.EyeY, 1, 1, p.Sleep);
            Attach(_eyesX, p.EyeX, p.EyeY, 1, 1, p.Xeyes);
            if (Attach(_spiralL, 0, 0, 1, 1, p.Spiral)) _spiralL.Rotation = _rot + p.SpinAngle;
            if (Attach(_spiralR, 0, 0, 1, 1, p.Spiral)) _spiralR.Rotation = _rot - p.SpinAngle;
            Attach(_shades, 0, 0, 1, 1, p.Shades);
            Attach(_glare, p.GlareX, 0, 1, 1, p.Glare);
            Attach(_chill, 0, 0, 1, 1, p.Chill);
            Attach(_teeth, 0, 0, 1, p.TeethOpen, p.Teeth);
            Attach(_smirk, 0, 0, 1, 1, p.Smirk);
            if (Follow(_snowA, _snowAPath, p.SnowAT, p.SnowA)) _snowA.Rotation = 180 * p.SnowAT;
            if (Follow(_snowB, _snowBPath, p.SnowBT, p.SnowB)) _snowB.Rotation = 180 * p.SnowBT;
            Follow(_noteA, _noteAPath, p.NoteAT, p.NoteA);
            Follow(_noteB, _noteBPath, p.NoteBT, p.NoteB);
            Attach(_glints, p.EyeX, p.EyeY, p.EyeScale, 1, p.Glints);
            Attach(_mouth, 0, 0, p.MouthScale, p.MouthOpen, p.Mouth);
            Attach(_frown, 0, 0, 1, 1, p.Frown);
            Attach(_drop, 0, p.DropY, 1, 1, p.Drop);
            if (Show(_question, p.Question)) _question.MoveTo(_question.Pivot.X, _question.Pivot.Y + p.QY);
            if (Show(_heart, p.Heart))
            {
                _heart.MoveTo(_heart.Pivot.X, _heart.Pivot.Y + p.HeartY);
                _heart.Scale = p.HeartScale;
            }
            Show(_trailA, p.TrailA);
            Show(_trailB, p.TrailB);
            Show(_trailC, p.TrailC);
            for (var i = 0; i < _conf.Length; i++)
            {
                var part = _conf[i];
                if (!Show(part, p.Conf)) continue;
                var progress = p.ConfT * (1 - 0.08 * i);
                var point = _confPaths[i].PointAt(progress);
                part.MoveTo(point.X, point.Y);
                part.Rotation = 220 * progress;
            }

            if (Show(_particle, p.POpacity))
            {
                var (ax, ay) = OnBody(p.PX, p.PY);
                var k = p.PAttach;
                _particle.MoveTo(p.PX + (ax - p.PX) * k, p.PY + (ay - p.PY) * k);
                _particle.Rotation = p.PRot + _rot * k;
                _particle.Scale = p.PScale * (1 + (_bs - 1) * k);
            }

            Follow(_sparkA, _sparkAPath, p.SparkAT, p.SparkA);
            Follow(_sparkB, _sparkBPath, p.SparkBT, p.SparkB);
            Follow(_zA, _sleepPath, p.ZAT, p.ZA);
            Follow(_zB, _sleepPath, p.ZBT, p.ZB);
        }

        private bool Attach(HyperLayer part, double dx, double dy, double scale, double scaleY, double opacity)
        {
            if (!Show(part, opacity)) return false;
            var pivot = part.Pivot;
            var (x, y) = OnBody(pivot.X + dx, pivot.Y + dy);
            part.MoveTo(x, y);
            part.Rotation = _rot;
            part.ScaleX = _bs * scale;
            part.ScaleY = _bs * scale * _bsy * scaleY;
            return true;
        }

        private (double X, double Y) OnBody(double x, double y)
        {
            var vx = (x - _pivot.X) * _bs;
            var vy = (y - _pivot.Y) * _bs * _bsy;
            return (_pivot.X + vx * _cos - vy * _sin + _bx, _pivot.Y + vx * _sin + vy * _cos + _by);
        }

        private static bool Follow(HyperLayer part, HyperPath path, double t, double opacity)
        {
            if (!Show(part, opacity)) return false;
            var point = path.PointAt(t);
            part.MoveTo(point.X, point.Y);
            return true;
        }

        private static bool Show(HyperLayer part, double opacity)
        {
            opacity = opacity < Faint ? 0 : opacity > 1 - Faint ? 1 : opacity;
            part.Opacity = opacity;
            return opacity > 0;
        }

        private void PaintScan(SKCanvas canvas, Rect window, double opacity)
        {
            var band = window.Height * 0.1;
            var y = window.Y + _scanT * (window.Height - band);
            _band.Color = ScanColor.WithAlpha((byte)(ScanColor.Alpha * Math.Clamp(opacity, 0, 1)));
            canvas.DrawRect((float)window.X, (float)y, (float)window.Width, (float)band, _band);
        }

        private static double Phase(double t, double period) => t / period % 1;
        private static double Lerp(double a, double b, double k) => a + (b - a) * k;
        private static double Wrap(double degrees) => degrees > 180 ? degrees - 360 : degrees;
        private static void Flake(double u, out double opacity, out double progress)
        {
            progress = u;
            opacity = u < 0.2 ? 0.9 * u / 0.2 : 0.9 * (1 - u) / 0.8;
        }
        private static double Bump(double u) => u is > 0.2 and < 0.8 ? 0.8 * Math.Sin(Math.PI * (u - 0.2) / 0.6) : 0;
        private static double Ease(double u) => 0.5 - 0.5 * Math.Cos(2 * Math.PI * u);
        private static double Smooth(double u)
        {
            u = Math.Clamp(u, 0, 1);
            return u * u * (3 - 2 * u);
        }
        private static double Blink(double t, double period)
        {
            var u = Phase(t, period);
            return u is < 0.9 or > 0.98 ? 1 : 1 - 0.89 * Math.Sin(Math.PI * (u - 0.9) / 0.08);
        }
        private static void Spark(double u, double peak, out double opacity, out double progress)
        {
            progress = Math.Clamp(u / 0.6, 0, 1);
            opacity = u is > 0 and < 0.6 ? peak * Math.Sin(Math.PI * progress) : 0;
        }
    }

    private struct MascotPose
    {
        public double BodyX, BodyY, BodyRot, BodyScale, BodyScaleY;
        public double EyeX, EyeY, EyeOpen, Pill, Doubt, Happy, Sleep, Glints, Cheeks;
        public double Mouth, MouthOpen, Frown;
        public double PX, PY, PRot, PScale, POpacity, PAttach;
        public double Scan, ScanT, RingA, RingAScale, RingB, RingBScale;
        public double SparkA, SparkAT, SparkB, SparkBT, ZA, ZAT, ZB, ZBT;
        public double EyeScale, MouthScale, Xeyes, Drop, DropY, Question, QY, Heart, HeartY, HeartScale;
        public double TrailA, TrailB, TrailC, Conf, ConfT;
        public double Spiral, SpinAngle, Teeth, TeethOpen, Chill, SnowA, SnowAT, SnowB, SnowBT, NoteA, NoteAT, NoteB, NoteBT;
        public double Shades, Glare, GlareX, Smirk;

        public void Add(in MascotPose p, double w)
        {
            BodyX += p.BodyX * w; BodyY += p.BodyY * w; BodyRot += p.BodyRot * w; BodyScale += p.BodyScale * w; BodyScaleY += p.BodyScaleY * w;
            EyeX += p.EyeX * w; EyeY += p.EyeY * w; EyeOpen += p.EyeOpen * w;
            Pill += p.Pill * w; Doubt += p.Doubt * w; Happy += p.Happy * w; Sleep += p.Sleep * w; Glints += p.Glints * w; Cheeks += p.Cheeks * w;
            Mouth += p.Mouth * w; MouthOpen += p.MouthOpen * w; Frown += p.Frown * w;
            PX += p.PX * w; PY += p.PY * w; PRot += p.PRot * w; PScale += p.PScale * w; POpacity += p.POpacity * w; PAttach += p.PAttach * w;
            Scan += p.Scan * w; ScanT += p.ScanT * w;
            RingA += p.RingA * w; RingAScale += p.RingAScale * w; RingB += p.RingB * w; RingBScale += p.RingBScale * w;
            SparkA += p.SparkA * w; SparkAT += p.SparkAT * w; SparkB += p.SparkB * w; SparkBT += p.SparkBT * w;
            ZA += p.ZA * w; ZAT += p.ZAT * w; ZB += p.ZB * w; ZBT += p.ZBT * w;
            EyeScale += p.EyeScale * w; MouthScale += p.MouthScale * w; Xeyes += p.Xeyes * w;
            Drop += p.Drop * w; DropY += p.DropY * w; Question += p.Question * w; QY += p.QY * w;
            Heart += p.Heart * w; HeartY += p.HeartY * w; HeartScale += p.HeartScale * w;
            TrailA += p.TrailA * w; TrailB += p.TrailB * w; TrailC += p.TrailC * w; Conf += p.Conf * w; ConfT += p.ConfT * w;
            Spiral += p.Spiral * w; SpinAngle += p.SpinAngle * w; Teeth += p.Teeth * w; TeethOpen += p.TeethOpen * w; Chill += p.Chill * w;
            SnowA += p.SnowA * w; SnowAT += p.SnowAT * w; SnowB += p.SnowB * w; SnowBT += p.SnowBT * w;
            NoteA += p.NoteA * w; NoteAT += p.NoteAT * w; NoteB += p.NoteB * w; NoteBT += p.NoteBT * w;
            Shades += p.Shades * w; Glare += p.Glare * w; GlareX += p.GlareX * w; Smirk += p.Smirk * w;
        }
    }
}
