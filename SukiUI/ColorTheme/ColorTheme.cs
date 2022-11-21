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
    
    public static void LoadLightTheme(Application app)
    {
        app.Styles.Add(new Light());
    }  
}

public class Dark : Styles
{
    public Dark() => AvaloniaXamlLoader.Load(this);
}

public class Light : Styles
{
    public Light() => AvaloniaXamlLoader.Load(this);
}