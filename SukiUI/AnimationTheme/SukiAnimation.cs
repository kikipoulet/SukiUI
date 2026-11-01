using Avalonia;
using SukiUI.ControlsAnimation;

namespace SukiUI.Theme
{
    /// <summary>
    /// Sets the <see cref="SukiAnimationProfile"/> of one element and of everything inside it (its template, its
    /// content, its popups) over the app-wide <see cref="SukiAnimationTheme.Current"/>; unset follows the level above.
    /// Read at each animation start, like the global profile: a change applies to the next animation.
    /// Works from a style too: <c>&lt;Setter Property="suki:SukiAnimation.Profile" Value="Minimal" /&gt;</c>.
    /// </summary>
    public static class SukiAnimation
    {
        public static readonly AttachedProperty<SukiAnimationProfile?> ProfileProperty =
            AvaloniaProperty.RegisterAttached<StyledElement, SukiAnimationProfile?>("Profile", typeof(SukiAnimation), inherits: true);

        public static SukiAnimationProfile? GetProfile(StyledElement element) => element.GetValue(ProfileProperty);

        public static void SetProfile(StyledElement element, SukiAnimationProfile? value) => element.SetValue(ProfileProperty, value);
    }
}
