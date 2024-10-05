# Color

`SukiTheme` allows app to switch color themes easily

![themechanging](https://github.com/user-attachments/assets/ae55a431-3b4e-4673-b14b-bec46fe22bf6)

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