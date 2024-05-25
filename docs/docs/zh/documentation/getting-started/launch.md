# 启动应用

## 修改 App.axaml

在 `App.axaml` 的 `Styles` 中添加 `SukiTheme`

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

## 将 MainWindow 更改为 SukiWindow

原来的 `MainWindow.axaml`:

```xml
<Window
    x:Class="SukiTest.MainWindow"
    xmlns="https://github.com/avaloniaui"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
</Window>
```

修改后的 `MainWindow.axaml`: 

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

原来的 `MainWindow.axaml.cs`: 

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

修改后的 `MainWindow.axaml.cs`: 


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

## 完成

至此，SukiUI 安装完成