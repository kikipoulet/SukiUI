# Dialogs

SukiUI 提供了一个用于弹出对话框的[可选窗口控件](./hosts)，该控件可以很轻易地在 `SukiWindow.Hosts` 添加（这也是最推荐且能达到最佳效果的使用方法）

该对话框对 MVVM 设计模式友好，同时你也可以通过 `ISukiDialogManager` 来获得给定的 `SukiDialogHost` 实例，从而显示对话框。

以下是一些 MVVM 设计模式下使用的例子：

### View

```xml
<!-- XMLNS 定义已略去 -->
<suki:SukiWindow>
	<suki:SukiWindow.Hosts>
		<suki:SukiDialogHost Manager="{Binding DialogManager}"/>
	</suki:SukiWindow.Hosts>
<suki:SukiWindow>
```

### ViewModel

```cs
public class ExampleViewModel
{
	public ISukiDialogManager DialogManager { get; } = new SukiDialogManager();
}
```
---

如果你并未使用 MVVM 设计模式，只是想做一些简单实现，可以参考以下方法：

### AXAML

```xml
<!-- XMLNS 定义已略去 -->
<suki:SukiWindow>
	<suki:SukiWindow.Hosts>
		<suki:SukiDialogHost Name="DialogHost"/>
	</suki:SukiWindow.Hosts>
<suki:SukiWindow>
```

### Code-Behind

```cs
public class MainWindow : SukiWindow
{
	public static ISukiDialogManager DialogManager = new SukiDialogManager();

	public MainWindow()
	{
		InitializeComponent();
		DialogHost.Manager = DialogManager;
	}
}
```

### 用法

```cs
MainWindow.DialogManager.CreateDialog()
	.TryShow();
```

## 显示对话框

为了创建和显示对话框，SukiUI 提供了一种现代的构建方式。你可以在 `ISukiDialogManager` 实例上调用 `.CreateDialog()` 方法来开始构建对话框。

接下来，通过链式调用可以轻松设置对话框的标题、内容等。所有方法都有相应的 XML 注释说明。

构建完对话框后，调用 `.TryShow()` 方法即可显示对话框，前提是当前没有其他对话框正在显示。

例如，下面是一个简单的对话框示例：

```cs
public void DisplayDialog()
{
	DialogManager.CreateDialog()
		.WithTitle("示例对话框")
		.WithContent("这里是示例对话框的内容。")
		.TryShow();
}
```

## 关闭对话框

默认情况下，对话框没有自动关闭机制。要添加关闭方式，可以使用 `.Dismiss()` 方法。目前最常见的方式是 `.ByClickingBackground()`，即用户点击对话框外部时关闭对话框。

例如，下面的代码展示了一个点击背景即可关闭的空对话框：

```cs
public void DisplayDialog()
{
	DialogManager.CreateDialog()
		.Dismiss().ByClickingBackground()
		.TryShow();
}
```

另一种关闭对话框的方式是通过操作按钮，接下来将介绍这一点。

## 交互操作

你可以通过 `.WithActionButton()` 方法为对话框添加按钮。该方法可以设置按钮的文字、点击后的回调操作，并且可以选择性地通过 `dismissOnClick` 参数来控制点击后是否关闭对话框。你也可以添加多个按钮，根据需要设置不同的操作。

以下是一个包含两个按钮的对话框示例，其中一个按钮会关闭对话框：

```cs
public void DisplayDialog()
{
	DialogManager.CreateDialog()
		.WithActionButton("保持打开", _ => { })
		.WithActionButton("关闭", _ => { }, true)  // 点击后关闭对话框
		.TryShow();
}
```

## 消息框样式

你还可以通过 `.OfType()` 方法为对话框应用内置的消息框样式，目前支持的信息类型包括：`Information`, `Success`, `Warning` 和 `Error`。