# Light & Dark

SukiUI use the [Theme Variant](https://docs.avaloniaui.net/docs/guides/styles-and-resources/how-to-use-theme-variants) system provided by `AvaloniaUI`.

However, the SukiTheme class provide an easiest wrapper to change Light/Dark theme.

<img src="https://i.ibb.co/XzzfB9r/theming-theme.gif" alt="theming-theme">

## Switch to Dark Theme

```csharp
SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Dark);
```

## Switch to Light Theme

```csharp
SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Light);
```

## Switch between Light/Dark Theme

```csharp
SukiTheme.GetInstance().SwitchBaseTheme();
```

## ThemeChanged Event

```csharp
SukiTheme.GetInstance().OnBaseThemeChanged += variant =>
{
    Console.WriteLine("Theme changed triggered !");
};
```