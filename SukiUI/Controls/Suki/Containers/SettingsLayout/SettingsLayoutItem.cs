using Avalonia;
using Avalonia.Controls;

namespace SukiUI.Controls
{
    public class SettingsLayoutItem : Control
    {
        public static readonly StyledProperty<string?> HeaderProperty =
            AvaloniaProperty.Register<SettingsLayoutItem, string?>(nameof(Header));

        public static readonly DirectProperty<SettingsLayoutItem, Control?> ContentProperty =
            AvaloniaProperty.RegisterDirect<SettingsLayoutItem, Control?>(
                nameof(Content),
                o => o.Content,
                (o, v) => o.Content = v);

        private Control? _content;

        public string? Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public Control? Content
        {
            get => _content;
            set => SetAndRaise(ContentProperty, ref _content, value);
        }
    }
}