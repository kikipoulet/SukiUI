# Color

`SukiTheme` allows app to switch color themes easily

<img src="/theming/theming-color.gif" alt="theming-color">

## Switch between available Color Theme

```csharp
SukiTheme.GetInstance().SwitchColorTheme();
```

## Switch to a specific Color Theme

```csharp
SukiTheme.GetInstance().ChangeColorTheme(SukiColor.Red);
```

## Create a Custom Color Theme, register it and switch to it

```csharp
var PurpleTheme = new SukiColorTheme("Purple", Colors.Purple, Colors.DarkBlue);
SukiTheme.GetInstance().AddColorTheme(PurpleTheme);
SukiTheme.GetInstance().ChangeColorTheme(PurpleTheme);
```

## ColorChanged Event

```csharp
SukiTheme.GetInstance().OnColorThemeChanged += theme =>
{
     Console.WriteLine("Color theme change triggered !");
};
```