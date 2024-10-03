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

It is possible to set `MaxToasts` to limit the number of toasts displayed in any given host.

---

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
```

### Usage

```cs
MainWindow.ToastManager.CreateToast()
	.Queue();
```

<br/>

## Displaying Toasts

In order to construct and therefore display toasts, a fluent style builder is provided that makes constructing toasts simple. To begin constructing a toast, it's recommended to call the extension method `.CreateToast()` on the `ISukiToastManager` instance you want it to be displayed in.

From here method calls can be chained to construct the toast as desired, most of these are self explanatory and have associated XMLDocs for convenience.

Finally to display a toast the `.Queue()` method can be called to immediately queue the toast for display.

Here is a simple example, expanding on the `ViewModel` above:

```cs
public void DisplayToast()
{
	ToastManager.CreateToast()
		.WithTitle("Example Toast")
		.WithContent("The content of an example toast can be seen here.")
		.Queue();
}
```


![toastsimple](https://github.com/user-attachments/assets/841b13a3-7983-4f39-9c15-3ce97510ba0d)


<br/>

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
<br/>

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

<br/>

## Toast Types

### Information

![toastsimple](https://github.com/user-attachments/assets/6a9f14b6-64a9-4a7b-a6b6-e15d8ad80ebc)


```cs
public void DisplayToast()
{
	ToastManager.CreateToast()
            .OfType(NotificationType.Information)
            .Queue();
}
```

### Success


![success](https://github.com/user-attachments/assets/71ea5077-21b6-4f8b-bbe8-7ef2760041ef)


```cs
public void DisplayToast()
{
	ToastManager.CreateToast()
            .OfType(NotificationType.Success)
            .Queue();
}
```

### Warning

![warning](https://github.com/user-attachments/assets/303999ab-44ba-4819-82ad-a8869c7ca5f3)


```cs
public void DisplayToast()
{
	ToastManager.CreateToast()
            .OfType(NotificationType.Warning)
            .Queue();
}
```

### Error

![error](https://github.com/user-attachments/assets/686da808-e594-41cf-b44a-ae586eadedc7)


```cs
public void DisplayToast()
{
	ToastManager.CreateToast()
            .OfType(NotificationType.Error)
            .Queue();
}
```





<br/>

## Loading Toast

![loading](https://github.com/user-attachments/assets/7857721a-e7a0-4bf5-beff-31363c606ce4)


```cs
public void DisplayToast()
{
	ToastManager.CreateToast()
            .WithLoadingState(true)
            .Queue();
}
```




<br/>

## Complex Interaction

Here is an example of an "Update" toast.


![loading](https://github.com/user-attachments/assets/479d7e09-a37b-4595-85a5-02c669b8592a)


```cs
    private void ShowActionToast()
    {
        toastManager.CreateToast()
            .WithTitle("Update Available")
            .WithContent("Information, Update v1.0.0.0 is Now Available.")
            .WithActionButtonNormal("Later", _ => { }, true)
            .WithActionButton("Update", _ => ShowUpdatingToast(), true)
            .Queue();
    }

    private void ShowUpdatingToast()
    {
        var progress = new ProgressBar() { Value = 0, ShowProgressText = true };
        var toast = toastManager.CreateToast()
            .WithTitle("Updating...")
            .WithContent(progress)
            .Queue();
        var timer = new Timer(20);
        timer.Elapsed += (_, _) =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                progress.Value += 1;
                if (progress.Value < 100) return;
                timer.Dispose();
                toastManager.Dismiss(toast);
            });
        };
        timer.Start();
    }
```
