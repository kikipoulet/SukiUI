# Toasts

SukiUI 提供了一个用于弹出消息提醒的[可选窗口控件](./hosts)，该控件可以很轻易地在 `SukiWindow.Hosts` 添加（这也是最推荐且能达到最佳效果的使用方法）

该消息提醒控件对 MVVM 设计模式友好，同时你也可以通过 `ISukiToastManager` 来获得给定的 `SukiToastHost` 实例，从而显示提示消息。

以下是一些 MVVM 设计模式下使用的例子：

### View
```xml
<!-- XMLNS 定义已略去 -->
<suki:SukiWindow>
	<suki:SukiWindow.Hosts>
		<suki:SukiToastHost Manager="{Binding ToastManager}"/>
	</suki:SukiWindow.Hosts>
<suki:SukiWindow>
```

### ViewModel
```cs
public class ExampleViewModel
{
	public ISukiToastManager ToastManager { get; } = new();
}
```

可以通过修改 `MaxToasts` 的值以达到限制消息弹出数量的效果

---

如果你并未使用 MVVM 设计模式，只是想做一些简单实现，可以参考以下方法：

### AXAML
```xml
<!-- XMLNS 定义已略去 -->
<suki:SukiWindow>
	<suki:SukiWindow.Hosts>
		<suki:SukiToastHost Name="ToastManager"/>
	</suki:SukiWindow.Hosts>
<suki:SukiWindow>
```

### Code-Behind
```cs
public class MainWindow : SukiWindow
{
	public static ISukiToastManager ToastManager = new SukiToastManager();

	public MainWindow()
	{
		InitializeComponent();
		ToastHost.Manager = ToastManager;
	}
}
```

### 用法

```cs
MainWindow.ToastManager.CreateToast()
	.Queue();
```

## 显示消息提醒

SukiUI 实现了一个现代的消息构造器。构造时推荐在 `ISukiToastManager` 的实例上调用 `.CreateToast()` 扩展方法

构造操作的体验是链式的，且均提供了 XML 文档

最后通过调用 `.Queue()` 方法来让该消息进入队列中，并立即显示消息

以下是在 `ViewModel` 中的用例：

```cs
public void DisplayToast()
{
	ToastManager.CreateToast()
		.WithTitle("Example Toast")
		.WithContent("The content of an example toast can be seen here.")
		.Queue();
}
```

## 自动消失

通常地，当消息提醒数量超过预设的最大值后，最老的消息将会立即消失以腾出空间。

但是，你也可以通过调用 `.Dismiss()` 方法来设置消失的条件

以下是让一个消息在3秒钟后/被点击后消失的用例：

```cs
public void DisplayToast()
{
	ToastManager.CreateToast()
		.Dismiss().After(TimeSpan.FromSeconds(3))
        .Dismiss().ByClicking()
        .Queue();
}
```

## 交互

SukiUI 提供了两个默认的消息回调，分别是 `.OnClicked()` 和 `.OnDismissed()`

同时，可以通过 `.WithActionButton()` 方法来实现更复杂的交互操作

以下是一个显示3秒钟后消失，点击后和消失后会调用命令行输出，按特定按钮会触发 Action 的消息用例：

```cs
public void DisplayToast()
{
	ToastManager.CreateToast()
		.Dismiss().After(TimeSpan.FromSeconds(3))
        .OnClicked(_ => Console.WriteLine("Toast Clicked!"))
        .OnDismissed(_ => Console.WriteLine("Toast Was Dismissed!")) 
        .WithActionButton("Dismiss", _ => { }, true)
        .Queue();
}
```