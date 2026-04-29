using Avalonia;
using Avalonia.Controls;

namespace SukiUI.Controls;

public class SettingsLayoutItem : Control
{
    public static readonly DirectProperty<SettingsLayoutItem, string?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<SettingsLayoutItem, string?>(
            nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);

    public string? Header
    {
        get { return _header; }
        set { SetAndRaise(HeaderProperty, ref _header, value); }
    }

    private string? _header;

    public static readonly DirectProperty<SettingsLayoutItem, Control?> ContentProperty =
    AvaloniaProperty.RegisterDirect<SettingsLayoutItem, Control?>(
        nameof(Content),
        o => o.Content,
        (o, v) => o.Content = v);

    public Control? Content
    {
        get { return _content; }
        set { SetAndRaise(ContentProperty, ref _content, value); }
    }

    private Control? _content;
}