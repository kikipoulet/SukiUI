# SukiWindow

在 SukiUI 中, `SukiWindow` 代替 `Window` 成为构建 App 的基础

![SukiWindow](https://github.com/user-attachments/assets/9be7f60b-d694-42dd-86ff-490ea80a3347)

## 背景样式

SukiUI 允许你在三种不同的背景选项之间进行选择，分别是“气泡”（Bubble），即玻璃拟态设计；以及经典的“平面”（Flat）背景。

需要注意的是，背景会根据你的主题颜色（此处为蓝色）动态生成。

## Bubble

```xml
<suki:SukiWindow BackgroundStyle="Bubble">
    <!-- 内容 -->
<suki:SukiWindow/>
```

#### 暗色

![sukiwindow - dark](https://github.com/user-attachments/assets/bdfeec4e-d0e7-4d7e-b075-b0616720acbd)

#### 亮色

![sukiwindow - light](https://github.com/user-attachments/assets/84dd83b4-be4f-4a0f-8c86-4d0c0e01e3ea)

## Gradient

```xml
<suki:SukiWindow BackgroundStyle="Gradient">
    <!-- 内容 -->
<suki:SukiWindow/>
```

#### 暗色

![sukiwindow - dark](https://github.com/user-attachments/assets/491a5e69-7b2f-4db0-87d0-6925aa79dee4)

#### 亮色

![sukiwindow - light](https://github.com/user-attachments/assets/7ef7bfcb-3fcf-4993-9aa6-aa1616c8a2e9)

## Flat

```xml
<suki:SukiWindow BackgroundStyle="Flat">
    <!-- 内容 -->
<suki:SukiWindow/>
```

#### 暗色

![sukiwindow - dark](https://github.com/user-attachments/assets/2ff1b465-570b-4681-87b5-46fbc618e670)

#### 亮色

![sukiwindow - light](https://github.com/user-attachments/assets/bdeee364-3bb6-4509-8427-f150569618a9)

## 功能

### Logo

<img src="https://sleekshot.app/api/download/AQ6CiLMLhBaA" />

```xml
    <suki:SukiWindow.LogoContent>
        <!-- Logo -->
    </suki:SukiWindow.LogoContent>
```

### Menu

<img src="https://sleekshot.app/api/download/iGuqowytQiOn" />

```xml
<suki:SukiWindow IsMenuVisible="True">
    <suki:SukiWindow.MenuItems>
        <!-- Menu -->
    </suki:SukiWindow.MenuItems>
<suki:SukiWindow/>
```

### 添加标题栏右侧控件

<img src="https://sleekshot.app/api/download/aLrqQYoOd9N2" />

```xml
    <suki:SukiWindow.RightWindowTitleBarControls>
        <!-- 显示在标题栏右侧的控件 -->
    </suki:SukiWindow.RightWindowTitleBarControls>
```

## 参阅

[Demo: SukiUI.Demo/SukiUIDemoView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/SukiUIDemoView.axaml)

[API: Controls/SukiWindow.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/SukiWindow.axaml.cs)