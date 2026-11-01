using System;
using Avalonia;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Turning a part around an axis with the hand, the robust way.
/// The rotation is accumulated move after move (each step is a small delta, so it can never flip sign when the hand
/// passes 180 degrees away from where it started), clamped to the travel at every step (no wind-up: coming back
/// from beyond a stop moves the part at once), and moves too close to the axis are ignored (the angle is meaningless there).
/// </summary>
internal sealed class RotaryDrag
{
    private const double DeadRadius = 8;

    private readonly Func<Point> _axis;
    private double _lastPointerDeg;
    private bool _hasPointer;

    /// <param name="axis">Axis of rotation in the coordinates used for the pointer.</param>
    /// <param name="pointerAngle">0 degrees direction and orientation, so that "angle" means the same as the part's angle.</param>
    public RotaryDrag(Func<Point> axis, Func<Point, Point, double> pointerAngle)
    {
        _axis = axis;
        PointerAngle = pointerAngle;
    }

    public Func<Point, Point, double> PointerAngle { get; }

    /// <summary>Current angle of the part (clamped).</summary>
    public double Angle { get; private set; }

    public bool IsActive { get; private set; }

    public void Begin(Point pointer, double partAngle)
    {
        IsActive = true;
        Angle = partAngle;
        _hasPointer = TryAngle(pointer, out _lastPointerDeg);
    }

    /// <summary>Moves the part by the hand's rotation since the last move. Returns true if the angle changed.</summary>
    public bool Move(Point pointer, double min, double max)
    {
        if (!IsActive || !TryAngle(pointer, out var deg)) return false;
        if (!_hasPointer)
        {
            _lastPointerDeg = deg;
            _hasPointer = true;
            return false;
        }
        var delta = SkeumoMotion.AngleDelta(_lastPointerDeg, deg);
        _lastPointerDeg = deg;
        var next = Math.Clamp(Angle + delta, min, max);
        if (Math.Abs(next - Angle) < 1e-9) return false;
        Angle = next;
        return true;
    }

    public void End() => IsActive = false;

    private bool TryAngle(Point pointer, out double deg)
    {
        var axis = _axis();
        var dx = pointer.X - axis.X;
        var dy = pointer.Y - axis.Y;
        if (dx * dx + dy * dy < DeadRadius * DeadRadius)
        {
            deg = 0;
            return false;
        }
        deg = PointerAngle(pointer, axis);
        return true;
    }

    /// <summary>0 = up (12 o'clock), positive = clockwise.</summary>
    public static double FromTop(Point p, Point axis) => Math.Atan2(p.X - axis.X, axis.Y - p.Y) * 180 / Math.PI;

    /// <summary>0 = right (3 o'clock), positive = clockwise.</summary>
    public static double FromRight(Point p, Point axis) => Math.Atan2(p.Y - axis.Y, p.X - axis.X) * 180 / Math.PI;
}
