using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Interactivity;
using SukiUI.ColorTheme;
using SukiUI.Content;

namespace SukiUI.Controls;

public class InfoBar : ContentControl
{
    private Button? _closeButton;

    static InfoBar()
    {
        SeverityProperty.Changed.AddClassHandler<InfoBar>((infoBar, change) =>
            infoBar.UpdateSeverity(change.GetNewValue<NotificationType>()));
    }

    public static readonly StyledProperty<NotificationType> SeverityProperty =
        AvaloniaProperty.Register<InfoBar, NotificationType>(nameof(Severity), NotificationType.Information);

    public NotificationType Severity
    {
        get => GetValue(SeverityProperty);
        set
        {
            SetValue(SeverityProperty, value);
        }
    }

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<InfoBar, object?>(nameof(Icon), Icons.InformationOutline);

    public object? Icon
    {
        get => GetValue(IconProperty);
        private set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<int> IconSizeProperty =
        AvaloniaProperty.Register<InfoBar, int>(nameof(IconSize), 12);

    public int IconSize
    {
        get => GetValue(IconSizeProperty);
        private set => SetValue(IconSizeProperty, value);
    }

    public static readonly StyledProperty<IBrush?> IconForegroundProperty =
        AvaloniaProperty.Register<InfoBar, IBrush?>(nameof(IconForeground), NotificationColor.InfoIconForeground);

    public IBrush? IconForeground
    {
        get => GetValue(IconForegroundProperty);
        private set => SetValue(IconForegroundProperty, value);
    }

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<InfoBar, bool>(nameof(IsOpen), true);

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<InfoBar, bool>(nameof(IsClosable), true);

    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }


    public static readonly StyledProperty<bool> IsOpaqueProperty =
        AvaloniaProperty.Register<InfoBar, bool>(nameof(IsOpaque), false);

    public bool IsOpaque
    {
        get => GetValue(IsOpaqueProperty);
        set => SetValue(IsOpaqueProperty, value);
    }

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<InfoBar, string>(nameof(Title), string.Empty);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="TitleOrientation"/> property
    /// </summary>
    public static readonly StyledProperty<Orientation> TitleOrientationProperty =
        WrapPanel.OrientationProperty.AddOwner<InfoBar>();

    public Orientation TitleOrientation
    {
        get => GetValue(TitleOrientationProperty);
        set => SetValue(TitleOrientationProperty, value);
    }

    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<InfoBar, string>(nameof(Message), string.Empty);

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="MessageTextAlignment"/> property
    /// </summary>
    public static readonly StyledProperty<TextAlignment> MessageTextAlignmentProperty =
        TextBlock.TextAlignmentProperty.AddOwner<InfoBar>();

    public TextAlignment MessageTextAlignment
    {
        get => GetValue(MessageTextAlignmentProperty);
        set => SetValue(MessageTextAlignmentProperty, value);
    }

    public static readonly StyledProperty<bool> IsTextSelectableProperty =
        AvaloniaProperty.Register<InfoBar, bool>(nameof(IsTextSelectable));

    public bool IsTextSelectable
    {
        get => GetValue(IsTextSelectableProperty);
        set => SetValue(IsTextSelectableProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_closeButton is not null)
            _closeButton.Click -= CloseButtonOnClick;
        _closeButton = e.NameScope.Get<Button>("PART_CloseButton");
        _closeButton.Click += CloseButtonOnClick;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (ContextMenu is null) return;
    }

    private void CloseButtonOnClick(object? sender, RoutedEventArgs e) => IsOpen = false;

    private void UpdateSeverity(NotificationType severity)
    {
        SetCurrentValue(IconProperty, severity switch
        {
            NotificationType.Information => Icons.InformationOutline,
            NotificationType.Success => Icons.Check,
            NotificationType.Warning or NotificationType.Error => Icons.AlertOutline,
            _ => Icons.InformationOutline
        });
        SetCurrentValue(IconForegroundProperty, severity switch
        {
            NotificationType.Information => NotificationColor.InfoIconForeground,
            NotificationType.Success => NotificationColor.SuccessIconForeground,
            NotificationType.Warning => NotificationColor.WarningIconForeground,
            NotificationType.Error => NotificationColor.ErrorIconForeground,
            _ => NotificationColor.InfoIconForeground
        });
    }
}
