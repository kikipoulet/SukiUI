using Avalonia.Media;
using SukiUI.Enums;

namespace SukiUI.Models;

public record SukiColorTheme
{
    public SukiColor Theme { get; }
    
    public Color Primary { get; }

    public IBrush PrimaryBrush => new SolidColorBrush(Primary);
    
    public Color IntBorder { get; }

    public IBrush IntBorderBrush => new SolidColorBrush(IntBorder);
    
    public SukiColorTheme(SukiColor theme, Color primary, Color intBorder)
    {
        Theme = theme;
        Primary = primary;
        IntBorder = intBorder;
    }
}