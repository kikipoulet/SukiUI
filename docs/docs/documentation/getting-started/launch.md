# Launch

## Modify your App.axaml

Include SukiUI styles in your `App.axaml`

```xml
<Application
    ...
    xmlns:sukiUi="clr-namespace:SukiUI;assembly=SukiUI" // [!code highlight]
    >
    <Application.Styles>
        <sukiUi:SukiTheme ThemeColor="Blue"  /> // [!code highlight]
    </Application.Styles>
</Application>
```

::: warning
If a default `ThemeColor` is not set and you do not set the theme by any other means, your window and many controls will be completely transparent.
:::

## Use SukiWindow as MainWindow

Change MainWindow from Window class to SukiWindow class.

Original `MainWindow.axaml`:

```xml
<Window
    x:Class="SukiTest.MainWindow"
    xmlns="https://github.com/avaloniaui"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
</Window>
```

Modified `MainWindow.axaml`: 

```xml
<sukiUi:SukiWindow // [!code highlight]
    x:Class="SukiTest.MainWindow"
    xmlns="https://github.com/avaloniaui"
    xmlns:sukiUi="clr-namespace:SukiUI.Controls;assembly=SukiUI" // [!code highlight]
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
</sukiUi:SukiWindow> // [!code highlight]
```

Original `MainWindow.axaml.cs`: 

```csharp
using Avalonia.Controls;

namespace SukiTest;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```

Modified `MainWindow.axaml.cs`: 

```csharp
using SukiUI.Controls; // [!code highlight]

namespace SukiTest;

public partial class MainWindow : SukiWindow // [!code highlight]
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```

## Done

You're now ready to use SukiUI ! We advise you to theme your app now. Please check the [Theming Page](https://kikipoulet.github.io/SukiUI/documentation/theming/basic.html) and the [SukiWindow Page](https://kikipoulet.github.io/SukiUI/documentation/controls/layout/sukiwindow.html)

::: warning
If you encounter the following exception:
- `SukiWindow` not found [Issue#265](https://github.com/kikipoulet/SukiUI/issues/265)
- System.MissingMethodException: Method not found: System.Collections.Generic.IReadOnlyList`1<System.Object> Avalonia.Markup.Xaml.XamlIl.Runtime.IAvaloniaXamlIlEagerParentStackProvider.get_DirectParents() [Issue#276](https://github.com/kikipoulet/SukiUI/issues/276)
- Unable to resolve type `SukiTheme` from namespace clr-namespace:SukiUI;assembly=SukiUI [Discussion#276](https://github.com/kikipoulet/SukiUI/discussions/278)
- and other similar exceptions

There are two possible solutions to try:
- Upgrade or downgrade the version of `Avalonia` and `SukiUI` until the exception is resolved
- While ensuring that `Avalonia` is up to date (beta), reference the build `.dll` from [Github Action](https://github.com/kikipoulet/SukiUI/actions/workflows/build.yml) and proceed with the [following steps](/documentation/getting-started/installation#via-github-action)
:::