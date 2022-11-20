using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace SukiUI;

public static class ColorTheme
{
    public static void LoadDarkTheme(Application app)
    {
        app.Styles.Add(new Dark());
    }   
}

public class Dark : Styles
{
    public Dark() => AvaloniaXamlLoader.Load(this);
}