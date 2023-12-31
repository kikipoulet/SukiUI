using Avalonia;
using Avalonia.Controls.Primitives;

namespace SukiUI.Controls;

public class SukiSideMenuItem : TemplatedControl
{
    public static readonly StyledProperty<object> IconProperty = 
        AvaloniaProperty.Register<SukiSideMenuItem, object>(nameof(Icon));

    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string> HeaderProperty = 
        AvaloniaProperty.Register<SukiSideMenuItem, string>(nameof(Header));

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly StyledProperty<object> PageContentProperty = 
        AvaloniaProperty.Register<SukiSideMenuItem, object>(nameof(PageContent));

    public object PageContent
    {
        get => GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }
}