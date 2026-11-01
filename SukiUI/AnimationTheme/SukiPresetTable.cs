namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// One animation family's calibrations for a single profile: an immutable
    /// preset -&gt; calibration lookup declared as data. Trim/AOT-safe (enum-keyed
    /// <see cref="Dictionary{TKey,TValue}"/>, no reflection).
    /// </summary>
    public sealed class SukiPresetTable<TPreset, TCalib> where TPreset : struct, Enum
    {
        private readonly Dictionary<TPreset, TCalib> _calib;

        public SukiPresetTable(params (TPreset Preset, TCalib Calib)[] calib)
            => _calib = calib.ToDictionary(c => c.Preset, c => c.Calib);

        private SukiPresetTable(Dictionary<TPreset, TCalib> calib) => _calib = calib;

        public TCalib this[TPreset preset] => _calib[preset];

        /// <summary>A copy of this table with one calibration replaced; the original is untouched.</summary>
        public SukiPresetTable<TPreset, TCalib> With(TPreset preset, TCalib calib)
            => new(new Dictionary<TPreset, TCalib>(_calib) { [preset] = calib });
    }
}
