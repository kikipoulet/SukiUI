# SukiWindow

In SukiUI, `SukiWindow` replaces `Window` as the basis for building apps.

![SukiWindow](https://github.com/user-attachments/assets/9be7f60b-d694-42dd-86ff-490ea80a3347)

## Background Style

SukiUI let you choose between 3 distinct background option, from the "Bubble", perfect to enhance the glassmorphism design of the library to the classic "Flat" background.

Note that the background are dynamically created to match your color theme - Blue in this documentation.

## Bubble

```xml
<suki:SukiWindow  BackgroundStyle="Bubble">
    <!-- Content -->
<suki:SukiWindow/>
```

#### Dark

![sukiwindow - dark](https://github.com/user-attachments/assets/bdfeec4e-d0e7-4d7e-b075-b0616720acbd)

#### Light

![sukiwindow - light](https://github.com/user-attachments/assets/84dd83b4-be4f-4a0f-8c86-4d0c0e01e3ea)

## Gradient

```xml
<suki:SukiWindow  BackgroundStyle="Gradient">
    <!-- Content -->
<suki:SukiWindow/>
```

#### Dark

![sukiwindow - dark](https://github.com/user-attachments/assets/491a5e69-7b2f-4db0-87d0-6925aa79dee4)

#### Light

![sukiwindow - light](https://github.com/user-attachments/assets/7ef7bfcb-3fcf-4993-9aa6-aa1616c8a2e9)

## Flat

```xml
<suki:SukiWindow  BackgroundStyle="Flat">
    <!-- Content -->
<suki:SukiWindow/>
```

#### Dark
![sukiwindow - dark](https://github.com/user-attachments/assets/2ff1b465-570b-4681-87b5-46fbc618e670)

#### Light

![sukiwindow - light](https://github.com/user-attachments/assets/bdeee364-3bb6-4509-8427-f150569618a9)

## Functionalities

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

### Right TitleBar Control

<img src="https://sleekshot.app/api/download/aLrqQYoOd9N2" />

```xml
    <suki:SukiWindow.RightWindowTitleBarControls>
        <!-- Controls show on the right of title bar -->
    </suki:SukiWindow.RightWindowTitleBarControls>
```

## See Also

[Demo: SukiUI.Demo/SukiUIDemoView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/SukiUIDemoView.axaml)

[API: Controls/SukiWindow.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/SukiWindow.axaml.cs)
