using System;

namespace SukiUI.Motion
{
    /// <summary>The motion engine's process-wide instrumentation — the benchmark readout
    /// surface (successor of the old public SukiTicker's externally-used members).</summary>
    public static class SukiMotionStats
    {
        /// <summary>Total milliseconds spent invoking subscribers (all TopLevels) since
        /// process start. Read deltas around a window to get that window's engine cost.</summary>
        public static double TotalDispatchMs => SukiTicker.TotalDispatchMs;

        /// <summary>Number of frame dispatches since process start.</summary>
        public static long DispatchCount => SukiTicker.DispatchCount;

        /// <summary>Average ms per dispatch — subscriber callbacks only.</summary>
        public static double AverageDispatchMs => SukiTicker.AverageDispatchMs;

        /// <summary>Monotonic elapsed time since the ticker's epoch — the single time base
        /// of the layer.</summary>
        public static TimeSpan Now => SukiTicker.Now;
    }
}
