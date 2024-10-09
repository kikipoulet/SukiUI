# How to use Custom Font

Here is the `App.axaml` after creating your SukiUI project:

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

Assuming that there exists a font file `Assets/FontName-xxx.ttf` and set the build action of that to `AvaloniaResource`

Then replace the `DefaultFontFamily` with your font:

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
            <FontFamily x:Key="DefaultFontFamily">avares://YourProject/Assets#FontName</FontFamily> // [!code highlight]
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