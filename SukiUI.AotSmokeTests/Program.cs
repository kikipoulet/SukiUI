using Avalonia;
using Avalonia.Headless;
using Avalonia.Controls;
using SukiUI;
using SukiUI.Enums;
using SukiUI.Motion;

AppBuilder.Configure<Application>()
    .UseHeadless(new AvaloniaHeadlessPlatformOptions())
    .SetupWithoutStarting();

var theme = new SukiTheme();
Application.Current!.Styles.Add(theme);
foreach (var level in Enum.GetValues<SukiCompactness>())
{
    theme.Compactness = level;
    var expected = level switch
    {
        SukiCompactness.SuperCompact => new Thickness(12, 4),
        SukiCompactness.Compact => new Thickness(16, 6),
        SukiCompactness.Normal => new Thickness(20, 8),
        SukiCompactness.Big => new Thickness(24, 11),
        _ => throw new ArgumentOutOfRangeException()
    };
    if (!theme.TryGetResource("SukiCompactness.Button.Padding", null, out var padding) || !expected.Equals(padding))
        throw new InvalidOperationException($"Wrong Button.Padding at {level}: {padding}.");
}

theme.Compactness = SukiCompactness.Normal;
// One resource from every compiled dictionary, including the shared global tokens.
string[] keys =
[
    "SmallCornerRadius", "SukiCompactness.Button.FontSize", "SukiCompactness.TabItem.FontSize",
    "SukiCompactness.Menu.Height", "SukiCompactness.MenuItem.FontSize",
    "SukiCompactness.ToggleSwitch.Track.Width", "SukiCompactness.GlassCard.Padding",
    "SukiCompactness.SukiSideMenu.Pane.CompactWidth", "SukiCompactness.SukiSideMenuItem.Row.Margin"
];
foreach (var key in keys)
    if (!theme.TryGetResource(key, null, out _))
        throw new InvalidOperationException($"Missing compactness resource: {key}.");

var scoped = new Border();
SukiUI.Theme.Compactness.SetLevel(scoped, SukiCompactness.Big);
if (!scoped.TryFindResource("SukiCompactness.Button.Padding", out var scopedPadding) ||
    !new Thickness(24, 11).Equals(scopedPadding))
    throw new InvalidOperationException("Scoped compactness was not applied.");
SukiUI.Theme.Compactness.SetLevel(scoped, null);
Console.WriteLine("Compactness resources loaded.");

if (SukiEase.Presets.Count != 98 || SukiEase.Find("Classic.Cubic.Out") is null)
    throw new InvalidOperationException("The easing catalog is incomplete.");
foreach (var preset in SukiEase.Presets)
    if (!ReferenceEquals(preset.Ease, SukiEase.Find("SukiEase." + preset.Path)) ||
        preset.Ease.Ease(0) != 0 || preset.Ease.Ease(1) != 1 || !double.IsFinite(preset.Ease.Ease(0.5)))
        throw new InvalidOperationException($"Invalid easing preset: {preset.Path}.");
Console.WriteLine($"Native AOT smoke test passed: {SukiEase.Presets.Count} easing presets.");
