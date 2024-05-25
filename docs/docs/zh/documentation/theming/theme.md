# 明暗主题切换

SukiUI 借助由 `AvaloniaUI` 提供的 [主题变体](https://docs.avaloniaui.net/zh-Hans/docs/guides/styles-and-resources/how-to-use-theme-variants) 轻松实现主题切换

<img src="https://i.ibb.co/XzzfB9r/theming-theme.gif" alt="theming-theme">

## 切换至暗色

```csharp
SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Dark);
```

## 切换至亮色

```csharp
SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Light);
```

## 明暗切换

```csharp
SukiTheme.GetInstance().SwitchBaseTheme();
```

## ThemeChanged 事件

```csharp
SukiTheme.GetInstance().OnBaseThemeChanged += variant =>
{
    Console.WriteLine("Theme changed triggered !");
};
```