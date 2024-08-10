# SukiWindow

在 SukiUI 中, `SukiWindow` 代替 `Window` 成为构建 App 的基础

## 展示

<img src="/controls/layout/sukiwindow.webp" />

## 示例

```xml
<suki:SukiWindow>
    <suki:SukiWindow.LogoContent>
        <!-- Logo -->
    </suki:SukiWindow.LogoContent>

    <suki:SukiWindow.MenuItems>
        <!-- Menu -->
    </suki:SukiWindow.MenuItems>

    <suki:SukiWindow.RightWindowTitleBarControls>
        <!-- Controls show on the right of title bar -->
    </suki:SukiWindow.RightWindowTitleBarControls>
</suki:SukiWindow>
```

## 参阅

[Demo: SukiUI.Demo/SukiUIDemoView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/SukiUIDemoView.axaml)

[API: Controls/SukiWindow.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/SukiWindow.axaml.cs)