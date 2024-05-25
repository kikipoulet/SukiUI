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

You're now ready to use SukiUI !