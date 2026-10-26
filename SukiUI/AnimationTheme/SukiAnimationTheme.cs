namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// The app-wide animation switchboard: which <see cref="SukiAnimationProfile"/> is active.
    /// Families read <see cref="Current"/> at animation start, so a live <see cref="Use"/> never
    /// interrupts a running animation — the next one picks up the change. UI thread.
    /// </summary>
    public static class SukiAnimationTheme
    {
        /// <summary>The active profile — <see cref="SukiAnimationProfile.Normal"/> by default.</summary>
        public static SukiAnimationProfile Current { get; private set; } = SukiAnimationProfile.Normal;

        /// <summary>Switches the active profile; it applies to animations started afterwards.</summary>
        public static void Use(SukiAnimationProfile profile)
            => Current = profile ?? throw new ArgumentNullException(nameof(profile));
    }
}
