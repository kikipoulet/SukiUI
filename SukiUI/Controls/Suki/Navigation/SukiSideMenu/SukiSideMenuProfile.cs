using System;
using System.ComponentModel;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    public enum SukiSideMenuPreset
    {
        SideMenu
    }

    public sealed record SukiSideMenuProfile(
        // ---- hover & press ----
        [property: Description("duration of the hover plate fading in and out")]
        TimeSpan HoverDuration,
        [property: Description("scale the icon swells to under the pointer")]
        double HoverIconScale,
        [property: Description("scale the row sinks to while pressed")]
        double PressScale,
        [property: Description("duration of the press-down")]
        TimeSpan PressDuration,
        [property: Description("angular frequency of the spring that brings the row back after a press")]
        double ReleaseOmega,
        [property: Description("damping of the release spring (higher = less rebound)")]
        double ReleaseDecay,

        // ---- selection plate (the liquid hand-off) ----
        [property: Description("angular frequency of the spring that grows the selection plate in")]
        double PlateGrowOmega,
        [property: Description("damping of the plate's grow spring")]
        double PlateGrowDecay,
        [property: Description("angular frequency of the spring that drains the plate of the row that is left")]
        double PlateDrainOmega,
        [property: Description("damping of the plate's drain spring")]
        double PlateDrainDecay,
        [property: Description("how long the new plate waits for the old one to start draining")]
        TimeSpan PlateHandoffDelay,
        [property: Description("duration of the plate's opacity ramp")]
        TimeSpan PlateFade,

        // ---- the selected row's content ----
        [property: Description("angular frequency of the spring behind the icon and title poses")]
        double PoseOmega,
        [property: Description("damping of the pose spring")]
        double PoseDecay,
        [property: Description("scale of the selected icon when the menu is collapsed")]
        double SelectedIconScale,
        [property: Description("scale the icon shrinks to while it slides out under the title")]
        double HiddenIconScale,

        // ---- sub-menus ----
        [property: Description("duration of a sub-menu unrolling or rolling back (a height is a timed spring: it lands on time)")]
        TimeSpan UnrollDuration,
        [property: Description("the curve of a sub-menu unrolling (a height: it must not go beyond its target)")]
        SukiEase UnrollEase,
        [property: Description("horizontal distance each sub-item slides in from")]
        double ChildOffsetX,
        [property: Description("delay between two sub-items")]
        TimeSpan ChildStagger,
        [property: Description("delay before the first sub-item starts")]
        TimeSpan ChildInitialDelay,
        [property: Description("angular frequency of the sub-items' spring")]
        double ChildOmega,
        [property: Description("damping of the sub-items' spring")]
        double ChildDecay,

        // ---- menu open / closed ----
        [property: Description("how long titles and header wait for the pane to open before they appear")]
        TimeSpan RevealDelay,
        [property: Description("duration of the titles and header fading in")]
        TimeSpan RevealDuration,
        [property: Description("duration of the titles and header fading out")]
        TimeSpan HideDuration,
        [property: Description("horizontal distance titles and header slide in from")]
        double RevealOffsetX,
        [property: Description("blur radius the titles and header come out of when they reappear (very light; 0 for none)")]
        double RevealBlur,
        [property: Description("duration of the pane opening or closing; the rows, header, footer and search ride on it")]
        TimeSpan PaneDuration,
        [property: Description("the curve of the pane (it moves the page next to it: it must not overshoot)")]
        SukiEase PaneEase,
        [property: Description("duration of a half turn: the pane toggle and the sub-menu chevrons")]
        TimeSpan RotateDuration,
        [property: Description("the curve of a half turn (no bounce)")]
        SukiEase RotateEase,

        // ---- search filter ----
        [property: Description("duration of a row folding away or back")]
        TimeSpan FilterDuration,
        [property: Description("the curve of a row folding (a height: it must not go beyond its target)")]
        SukiEase FilterEase,

        // ---- search field ----
        [property: Description("duration of the search stretching and closing back")]
        TimeSpan SearchDuration,
        [property: Description("the curve of the search stretching")]
        SukiEase SearchEase)
    {
        public Spring Release => new(ReleaseOmega, ReleaseDecay);
        public Spring PlateGrow => new(PlateGrowOmega, PlateGrowDecay);
        public Spring PlateDrain => new(PlateDrainOmega, PlateDrainDecay);
        public Spring Pose => new(PoseOmega, PoseDecay);
        public Spring Child => new(ChildOmega, ChildDecay);

        #region Normal

        // A row reacts under the pointer, sinks on press and springs back; the selection is one
        // liquid plate that drains out of the old row toward the new one and fills it from the
        // side the choice came from; a selected title slides over the icon it replaces; sub-menus
        // unroll with their items arriving one after the other. The pane opens and closes without
        // overshoot (it moves the page next to it); rows, header, footer and search ride on the
        // very same timed curve, so they stay in step and everything lands on time.
        public static readonly SukiSideMenuProfile SideMenu = new(
            HoverDuration: TimeSpan.FromMilliseconds(140),
            HoverIconScale: 1.1,
            PressScale: 0.975,
            PressDuration: TimeSpan.FromMilliseconds(90),
            ReleaseOmega: 16.0,
            ReleaseDecay: 11.0,
            PlateGrowOmega: 20.0,
            PlateGrowDecay: 26.0,
            PlateDrainOmega: 26.0,
            PlateDrainDecay: 58.0,
            PlateHandoffDelay: TimeSpan.FromMilliseconds(55),
            PlateFade: TimeSpan.FromMilliseconds(120),
            PoseOmega: 16.0,
            PoseDecay: 30.0,
            SelectedIconScale: 1.12,
            HiddenIconScale: 0.7,
            UnrollDuration: TimeSpan.FromMilliseconds(420),
            UnrollEase: SukiEase.DesignLibraries.Material3.Emphasized,
            ChildOffsetX: -14.0,
            ChildStagger: TimeSpan.FromMilliseconds(32),
            ChildInitialDelay: TimeSpan.FromMilliseconds(50),
            ChildOmega: 20.0,
            ChildDecay: 22.0,
            RevealDelay: TimeSpan.FromMilliseconds(170),
            RevealDuration: TimeSpan.FromMilliseconds(540),
            HideDuration: TimeSpan.FromMilliseconds(150),
            RevealOffsetX: -10.0,
            RevealBlur: 25.0,
            PaneDuration: TimeSpan.FromMilliseconds(650),
            PaneEase: SukiEase.Signature.SoftClose,
            RotateDuration: TimeSpan.FromMilliseconds(650),
            RotateEase: SukiEase.Signature.SoftClose,
            FilterDuration: TimeSpan.FromMilliseconds(360),
            FilterEase: SukiEase.DesignLibraries.Material3.Emphasized,
            SearchDuration: TimeSpan.FromMilliseconds(320),
            SearchEase: SukiEase.DesignLibraries.Material3.Emphasized);

        public static readonly SukiPresetTable<SukiSideMenuPreset, SukiSideMenuProfile> Normal =
            new((SukiSideMenuPreset.SideMenu, SideMenu));

        #endregion

        #region Lite

        // Sober menu: no swell, no cascade, critically damped springs and short fades.
        // (What moves a height, a width or a plate's scale never overshoots in any profile: it
        // would go below zero, or push the page next to the pane.)
        public static readonly SukiSideMenuProfile SideMenuLite = SideMenu with
        {
            HoverIconScale = 1.0,
            PressScale = 1.0,
            ReleaseDecay = 32.0,
            PlateGrowOmega = 24.0,
            PlateGrowDecay = 48.0,
            PlateDrainOmega = 28.0,
            PlateDrainDecay = 62.0,
            PlateHandoffDelay = TimeSpan.Zero,
            PoseOmega = 24.0,
            PoseDecay = 48.0,
            SelectedIconScale = 1.0,
            HiddenIconScale = 1.0,
            UnrollDuration = TimeSpan.FromMilliseconds(220),
            UnrollEase = SukiEase.Classic.Cubic.Out,
            ChildOffsetX = 0.0,
            ChildStagger = TimeSpan.Zero,
            ChildInitialDelay = TimeSpan.Zero,
            ChildOmega = 24.0,
            ChildDecay = 48.0,
            RevealDelay = TimeSpan.FromMilliseconds(60),
            RevealDuration = TimeSpan.FromMilliseconds(140),
            RevealOffsetX = 0.0,
            RevealBlur = 0.0,
            PaneDuration = TimeSpan.FromMilliseconds(420),
            PaneEase = SukiEase.Classic.Cubic.Out,
            RotateDuration = TimeSpan.FromMilliseconds(300),
            RotateEase = SukiEase.Classic.Cubic.Out,
            FilterDuration = TimeSpan.FromMilliseconds(250),
            FilterEase = SukiEase.Classic.Cubic.Out,
            SearchDuration = TimeSpan.FromMilliseconds(200),
            SearchEase = SukiEase.Classic.Cubic.Out,
        };

        public static readonly SukiPresetTable<SukiSideMenuPreset, SukiSideMenuProfile> Lite =
            new((SukiSideMenuPreset.SideMenu, SideMenuLite));

        #endregion
    }
}
