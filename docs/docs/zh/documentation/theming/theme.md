# 明暗主题切换

SukiUI 借助由 `AvaloniaUI` 提供的 [主题变体](https://docs.avaloniaui.net/zh-Hans/docs/guides/styles-and-resources/how-to-use-theme-variants) 轻松实现主题切换

## 主题

### 暗色

![dark theme](https://github.com/user-attachments/assets/bdfeec4e-d0e7-4d7e-b075-b0616720acbd)

### 亮色

![light theme](https://github.com/user-attachments/assets/84dd83b4-be4f-4a0f-8c86-4d0c0e01e3ea)

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