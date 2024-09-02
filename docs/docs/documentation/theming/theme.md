# Light & Dark

SukiUI use the [Theme Variant](https://docs.avaloniaui.net/docs/guides/styles-and-resources/how-to-use-theme-variants) system provided by `AvaloniaUI`.

However, the SukiTheme class provide a wrapper to change Light/Dark theme.
#### Dark
![{CFF9284D-F8E2-48C5-A837-05BB4BEA0673}](https://github.com/user-attachments/assets/bdfeec4e-d0e7-4d7e-b075-b0616720acbd)

#### Light

![{4E906261-7E2A-472E-B21E-FC038B1CFDF5}](https://github.com/user-attachments/assets/84dd83b4-be4f-4a0f-8c86-4d0c0e01e3ea)


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
