using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SukiUI.ColorTheme;
using SukiUI.Enums;

namespace SukiUI.Controls;

public class Badge : ContentControl
{
    public static readonly StyledProperty<NotificationType> AppearanceProperty =
        AvaloniaProperty.Register<Badge, NotificationType>(nameof(Appearance), NotificationType.Info);

    public NotificationType Appearance
    {
        get => GetValue(AppearanceProperty);
        set
        {
            BadgeBackground = value switch
            {
                NotificationType.Info => NotificationColor.InfoIconForeground,
                NotificationType.Success => NotificationColor.SuccessIconForeground,
                NotificationType.Warning => NotificationColor.WarningIconForeground,
                NotificationType.Error => NotificationColor.ErrorIconForeground,
                _ => NotificationColor.InfoIconForeground
            };
            
            SetValue(AppearanceProperty, value);
        }
    }
    
    public static readonly StyledProperty<IBrush?> BadgeBackgroundProperty =
        AvaloniaProperty.Register<Badge, IBrush?>(nameof(BadgeBackground), NotificationColor.InfoIconForeground);

    public IBrush? BadgeBackground
    {
        get => GetValue(BadgeBackgroundProperty);
        set => SetValue(BadgeBackgroundProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsOpaqueProperty =
        AvaloniaProperty.Register<Badge, bool>(nameof(IsOpaque), false);

    public bool IsOpaque
    {
        get => GetValue(IsOpaqueProperty);
        set => SetValue(IsOpaqueProperty, value);
    }
    
    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<Badge, string>(nameof(Header), string.Empty);

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}