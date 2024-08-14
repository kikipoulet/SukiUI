# Toasts

SukiUI provides a [host](./hosts) which can display toasts easily at any level of your application. As standard we recommend simply using it in `SukiWindow.Hosts` as this provides the best experience, however toasts can be localised within whatever context you require.

The host is designed in such a way as to be MVVM friendly and as long as you have access to the `ISukiToastManager` instance used for a given `SukiToastHost` you can display toasts in it.

Here is a simple example setup using MVVM:

### View
```xml
<!-- XMLNS definitions omitted for brevity -->
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

Is is possible to set `MaxToasts` to limit the number of toasts displayed in any given host.

If you do not wish to use MVVM, or would rather a simpler solution that "just works", then you can choose to implement it like this:

### AXAML
```xml
<!-- XMLNS definitions omitted for brevity -->
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

// and then...

MainWindow.ToastManager.CreateToast()
	.Queue();
```
### Usage
```cs
MainWindow.ToastManager.CreateToast()
	.Queue();
```


## Displaying Toasts

In order to construct and therefore display toasts, a fluent style builder is provided that makes constructing toasts simple. To begin constructing a toast, it's recommended to call the extension method `.CreateToast()` on the `ISukiToastManager` instance you want it to be displayed in.

From here method calls can be chained to construct the toast as desired, most of these are self explanatory and have associated XMLDocs for convenience.

Finally to display a toast the `.Queue()` method can be called to immediately queue the toast for display.

Here is a simple example, expanding on the ViewModel above:

```cs
public void DisplayToast()
{
	ToastManager.CreateToast()
		.WithTitle("Example Toast")
		.WithContent("The content of an example toast can be seen here.")
		.Queue();
}
```

## Dismissing Toasts

By default, toasts have no mechanism to be dismissed other than the capacity of the `SukiToastHost` being exceeded, at which point older toasts are dismissed to make room. In order to add dismissal mechanisms to a toast it's necessary to use the `.Dismiss()` method, at which point you can provide a method by which the toast can be dismissed. It is possible to have more than 1 dismissal method for a toast.

Here is an example of an empty toast which will be dismissed after 3 seconds or if clicked:

```cs
public void DisplayToast()
{
	ToastManager.CreateToast()
		.Dismiss().After(TimeSpan.FromSeconds(3))
        .Dismiss().ByClicking()
        .Queue();
}
```

## Interactions

A pair of basic callbacks are provided for user interaction with toasts, these are `.OnClicked()` and `.OnDismissed()`. Beyond this it's also possible to create more complex interactions with toasts via the `.WithActionButton()` method.

Here is an example of a toast with a button which will be dismissed after 3 seconds, the toast itself can be clicked any number of times but the button will cause the toast to be dismissed immediately:

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