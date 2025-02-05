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

::: warning
如果没有设置主题色 `ThemeColor`，创建的窗口和许多控件都会变成透明
:::

## 本地化

Avalonia 中的一些内置控件包含文本信息（例如 TextBox 的默认上下文菜单）。您可以通过设置 Suki UI 的 `Locale` 来选择这些文本的语言。Suki UI 默认使用英语，但如果您需要使用其他语言（如中文），可以按如下方式设置：

```xml
<Application
    ...
    xmlns:sukiUi="clr-namespace:SukiUI;assembly=SukiUI"
    >
    <Application.Styles>
        <sukiUi:SukiTheme Locale="zh-CN" ThemeColor="Blue"  /> // [!code highlight]
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

---

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

顺利完成以上步骤后，SukiUI 项目便已成功创建。创建项目后，建议参阅 [设置主题](/zh/documentation/theming/basic) 和 [SukiWindow 页面](/zh/documentation/controls/layout/sukiwindow)。

::: warning
如果你遇到了包括但不限于以下异常：
- `SukiWindow` not found [Issue#265](https://github.com/kikipoulet/SukiUI/issues/265)
- System.MissingMethodException: Method not found: System.Collections.Generic.IReadOnlyList`1<System.Object> Avalonia.Markup.Xaml.XamlIl.Runtime.IAvaloniaXamlIlEagerParentStackProvider.get_DirectParents() [Issue#276](https://github.com/kikipoulet/SukiUI/issues/276)
- Unable to resolve type `SukiTheme` from namespace clr-namespace:SukiUI;assembly=SukiUI [Discussion#276](https://github.com/kikipoulet/SukiUI/discussions/278)

有两种可能的解决方案：
- 调整 `Avalonia` 和 `SukiUI` 的版本直到异常消失
- 在保证 `Avalonia` 是最新版本（包括 beta）的情况下，引用来自 [Github Action](https://github.com/kikipoulet/SukiUI/actions/workflows/build.yml) 的 `.dll` 构建，然后执行[以下步骤](/zh/documentation/getting-started/installation#通过-github-action-安装)
:::