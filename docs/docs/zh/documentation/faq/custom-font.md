# 如何使用自定义字体

以下是创建一个新的 SukiUI 项目后的 `App.axaml` 文件：

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="SukiTest.App"
             xmlns:local="using:SukiTest"
             xmlns:sukiUi="clr-namespace:SukiUI;assembly=SukiUI"
             RequestedThemeVariant="Default">
             <!-- "Default" ThemeVariant follows system theme variant. "Dark" or "Light" are other available options. -->

    <Application.DataTemplates>
        <local:ViewLocator/>
    </Application.DataTemplates>
  
    <Application.Styles>
        <sukiUi:SukiTheme ThemeColor="Blue" />
    </Application.Styles>
</Application>
```

假设存在一个字体文件 `Assets/MiSans-Bold.ttf`，并将其构建行为设置为 `AvaloniaResource`

然后替换 `DefaultFontFamily` 为你的字体：

```xml
<Application
    RequestedThemeVariant="Default"
    x:Class="SukiTest.App"
    xmlns="https://github.com/avaloniaui"
    xmlns:local="using:SukiTest"
    xmlns:sukiUi="clr-namespace:SukiUI;assembly=SukiUI"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!--  "Default" ThemeVariant follows system theme variant. "Dark" or "Light" are other available options.  -->

    <Application.Resources> // [!code highlight]
        <ResourceDictionary> // [!code highlight]
            <FontFamily x:Key="DefaultFontFamily">avares://SukiTest/Assets/MiSans-Bold.ttf#MiSans</FontFamily> // [!code highlight]
        </ResourceDictionary> // [!code highlight]
    </Application.Resources> // [!code highlight]

    <Application.DataTemplates>
        <local:ViewLocator />
    </Application.DataTemplates>

    <Application.Styles>
        <sukiUi:SukiTheme ThemeColor="Blue" />
    </Application.Styles>
</Application>
```

::: tip
`#MiSans` 在其他字体中的名称都不一样，你可以使用类似 `Windows 字体查看器` 等软件查看字体名称
:::