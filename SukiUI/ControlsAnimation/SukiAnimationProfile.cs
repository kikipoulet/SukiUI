namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// The app-wide animation manifest: one <see cref="SukiPresetTable{TPreset,TCalib}"/> per
    /// family, composed with <c>with</c> and switched live through <see cref="SukiAnimationTheme"/>.
    /// The <c>required</c> members make a family missing from a profile a compile error.
    /// </summary>
    public sealed record SukiAnimationProfile
    {
        public required SukiPresetTable<SukiPressPreset, SukiPressProfile> Press { get; init; }
        public required SukiPresetTable<SukiPopupPreset, SukiPopupProfile> Popup { get; init; }
        public required SukiPresetTable<SukiDialogPreset, SukiDialogProfile> Dialog { get; init; }
        public required SukiPresetTable<SukiTogglePreset, SukiToggleProfile> Toggle { get; init; }
        public required SukiPresetTable<SukiTypingPreset, SukiTypingProfile> Typing { get; init; }
        public required SukiPresetTable<SukiToastPreset, SukiToastProfile> Toast { get; init; }

        /// <summary>The historical calibrations — the default profile.</summary>
        public static readonly SukiAnimationProfile Normal = new()
        {
            Press = SukiPressProfile.Normal,
            Popup = SukiPopupProfile.Normal,
            Dialog = SukiDialogProfile.Normal,
            Toggle = SukiToggleProfile.Normal,
            Typing = SukiTypingProfile.Normal,
            Toast = SukiToastProfile.Normal,
        };

        /// <summary>The sober calibrations, family by family.</summary>
        public static readonly SukiAnimationProfile Lite = new()
        {
            Press = SukiPressProfile.Lite,
            Popup = SukiPopupProfile.Lite,
            Dialog = SukiDialogProfile.Lite,
            Toggle = SukiToggleProfile.Lite,
            Typing = SukiTypingProfile.Lite,
            Toast = SukiToastProfile.Lite,
        };
    }
}
