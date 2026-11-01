using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using Path = Avalonia.Controls.Shapes.Path;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the Battery: four flat pill segments filled bottom to top.
// Colour by level (green / amber / red); the segment being charged breathes gently; a readout and the bolt on the lid.
public partial class Battery
{
    private const int CellCount = 4;
    private const double CellTop = 68, CellHeight = 17, CellGap = 5, BodyLeft = 31, BodyWidth = 58, LowCharge = 0.1;

    private static readonly Led Green = new("#FF8EF0B8", "#FF46D38B", "#FF248F58");
    private static readonly Led Amber = new("#FFFFD887", "#FFF2B33D", "#FFAE7612");
    private static readonly Led Red = new("#FFFF8A8A", "#FFE5484D", "#FF9E2328");
    private static readonly Color BoltCharging = Color.Parse("#FFFFD24A");

    private readonly List<Cell> _cells = new();
    private DispatcherTimer _timer = null!;
    private double _shown = -1;
    private long _tick;
    private Led? _led;
    private BoltState _bolt = (BoltState)(-1);

    private enum BoltState { Off, Charging, Full }

    private void InitializeRendering()
    {
        BuildCells();
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) => Tick());
        AttachedToVisualTree      += (_, _) => { _led = null; _bolt = (BoltState)(-1); _timer.Start(); };
        DetachedFromVisualTree    += (_, _) => _timer.Stop();
    }

    // Cell 0 is the bottom one. A flat pill: a soft track and a fill growing inside it.
    private void BuildCells()
    {
        var canvas = this.Get<Canvas>("Cells");
        for (var i = 0; i < CellCount; i++)
        {
            var top = CellTop + (CellCount - 1 - i) * (CellHeight + CellGap);
            var track = new Border { Width = BodyWidth, Height = CellHeight, CornerRadius = new CornerRadius(CellHeight / 2) };
            track.BindBrush(Border.BackgroundProperty, this, "SkTrack");
            Canvas.SetLeft(track, BodyLeft);
            Canvas.SetTop(track, top);
            canvas.Children.Add(track);

            var fill = new Border { Height = CellHeight, Width = 0, CornerRadius = new CornerRadius(CellHeight / 2) };
            Canvas.SetLeft(fill, BodyLeft);
            Canvas.SetTop(fill, top);
            canvas.Children.Add(fill);
            _cells.Add(new Cell { Fill = fill, Top = top });
        }
    }

    private void Tick()
    {
        _tick++;
        if (_shown < 0) _shown = Charge;
        _shown += (Charge - _shown) * 0.08;

        var led = _shown > 0.5 ? Green : _shown > 0.25 ? Amber : Red;
        if (led != _led)
        {
            _led = led;
            foreach (var c in _cells)
            {
                c.Fill.Background = led.Brush;
                c.Fill.BoxShadow = SkeumoMotion.Glow(led.Mid, 7, 0x70);
            }
            var readout = this.Get<TextBlock>("Readout");
            readout.Foreground = new SolidColorBrush(led.Top);
            readout.Effect = new DropShadowEffect { Color = led.Mid, BlurRadius = 5, OffsetX = 0, OffsetY = 0, Opacity = 0.6 };
        }

        // Each cell holds a quarter; the one being charged breathes gently and crackles faintly at the edge of its electrode
        var front = -1;
        for (var i = 0; i < CellCount; i++)
        {
            var level = Math.Clamp(_shown * CellCount - i, 0, 1);
            var width = BodyWidth * level;
            var cell = _cells[i];
            cell.Fill.Width = width;
            cell.Fill.IsVisible = width > CellHeight * 0.4;
            cell.Fill.Opacity = 1;
            if (level > 0 && level < 1) front = i;
        }
        if (IsCharging && front >= 0)
        {
            var cell = _cells[front];
            cell.Fill.Opacity = 0.8 + 0.2 * Math.Sin(_tick * 0.06);
        }
        else if (!IsCharging && _shown < LowCharge)
            _cells[0].Fill.Opacity = 0.45 + 0.55 * (0.5 + 0.5 * Math.Sin(_tick * 0.08));

        this.Get<TextBlock>("Readout").Text = $"{Math.Round(_shown * 100):0}%";
        UpdateBolt();
    }

    private void UpdateBolt()
    {
        var state = _shown >= 0.995 && Charge >= 0.999 ? BoltState.Full : IsCharging ? BoltState.Charging : BoltState.Off;
        var bolt = this.Get<Path>("Bolt");
        if (state != _bolt)
        {
            _bolt = state;
            if (state == BoltState.Off)
            {
                bolt.BindBrush(Shape.FillProperty, this, "SkLedOff");
                bolt.Effect = null;
            }
            else
            {
                var c = state == BoltState.Full ? Green.Mid : BoltCharging;
                bolt.Fill = new SolidColorBrush(c);
                bolt.Effect = new DropShadowEffect { Color = c, BlurRadius = 8, OffsetX = 0, OffsetY = 0, Opacity = 0.9 };
            }
        }
        bolt.Opacity = state == BoltState.Charging ? 0.7 + 0.3 * (0.5 + 0.5 * Math.Sin(_tick * 0.05)) : 1;
    }

    private sealed class Cell
    {
        public Border Fill = null!;
        public double Top;
    }

    // Electrode colour: bright top, body, dark bottom (a lit cylinder)
    private sealed class Led
    {
        public Led(string top, string mid, string bottom)
        {
            Top = Color.Parse(top);
            Mid = Color.Parse(mid);
            Brush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops = { new GradientStop(Top, 0), new GradientStop(Mid, 0.45), new GradientStop(Color.Parse(bottom), 1) }
            };
        }

        public Color Top { get; }
        public Color Mid { get; }
        public IBrush Brush { get; }
    }
}
