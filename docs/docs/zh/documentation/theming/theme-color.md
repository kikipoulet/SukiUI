# 主题色

`SukiTheme` 也可以十分简单地切换应用的主题色

![themechanging](https://github.com/user-attachments/assets/ae55a431-3b4e-4673-b14b-bec46fe22bf6)

## 在可用的主题色间切换

```csharp
SukiTheme.GetInstance().SwitchColorTheme();
```

## 切换到一个指定的主题色

```csharp
SukiTheme.GetInstance().ChangeColorTheme(SukiColor.Red);
```

## 创建/注册/切换到一个自定义主题色

```csharp
var PurpleTheme = new SukiColorTheme("Purple", Colors.Purple, Colors.DarkBlue);
SukiTheme.GetInstance().AddColorTheme(PurpleTheme);
SukiTheme.GetInstance().ChangeColorTheme(PurpleTheme);
```

## 订阅 ColorChanged 事件

```csharp
SukiTheme.GetInstance().OnColorThemeChanged += theme =>
{
     Console.WriteLine("Color theme change triggered !");
};
```