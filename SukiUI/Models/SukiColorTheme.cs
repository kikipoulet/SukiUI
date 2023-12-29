using Avalonia.Media;
using SukiUI.Enums;

namespace SukiUI.Models;

public record SukiColorTheme
{
    public SukiColor Theme { get; }
    
    public Color Primary { get; }

    public IBrush PrimaryBrush => new SolidColorBrush(Primary);
    
    public Color Accent { get; }

    public IBrush AccentBrush => new SolidColorBrush(Accent);
    
    public SukiColorTheme(SukiColor theme, Color primary, Color accent)
    {
        Theme = theme;
        Primary = primary;
        Accent = accent;
    }
}