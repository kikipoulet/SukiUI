# Dialogs

SukiUI provides a [host](./hosts) which can display dialogs easily at any level of your application. As standard we recommend simply using it in `SukiWindow.Hosts` as this provides the best experience, however dialogs can be localised within whatever context you require.

The host is designed in such a way as to be MVVM friendly and as long as you have access to the `ISukiDialogManager` instance used for a given `SukiDialogHost` you can display dialogs in it.

Here is a simple example setup using MVVM:

### View

```xml
<!-- XMLNS definitions omitted for brevity -->
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

If you do not wish to use MVVM, or would rather a simpler solution that "just works", then you can choose to implement it like this:

### AXAML

```xml
<!-- XMLNS definitions omitted for brevity -->
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

### Usage

```cs
MainWindow.DialogManager.CreateDialog()
	.TryShow();
```

## Displaying Dialogs

In order to construct and therefore display dialogs, a fluent style builder is provided that makes constructing dialogs simple. To begin constructing a dialog, it's recommended to call the extension method `.CreateDialog()` on the `ISukiDialogManager` instance you want it to be displayed in.

From here method calls can be chained to construct the dialog as desired, most of these are self explanatory and have associated XMLDocs for convenience.

Finally to display a dialog the `.TryShow()` method can be called to attempt to show the dialog if none is currently shown.

Here is a simple example, expanding on the `ViewModel` above:

```cs
public void DisplayDialog()
{
	DialogManager.CreateDialog()
		.WithTitle("Example Dialog")
		.WithContent("The content of an example dialog can be seen here.")
		.TryShow();
}
```

![dialog](https://github.com/user-attachments/assets/efd34873-b4c1-45bf-a14b-d7a7b11a77c1)


## Dismissing Dialogs

By default, dialogs have no mechanism to be dismissed. In order to add dismissal mechanisms to a dialog it's necessary to use the `.Dismiss()` method, at which point you can provide a method by which the dialogs can be dismissed. Currently the only standalone dismissal is `.ByClickingBackground()` which will dismiss the dialog when the user clicks outside of it.

Here is an example of an empty dialog which will be dismissed when the background is clicked.

```cs
public void DisplayDialog()
{
	DialogManager.CreateDialog()
            .Dismiss().ByClickingBackground()
            .TryShow();
}
```

The other method of dismissing dialogs involves the use of action buttons, discussed in the next section.

## Interactions

Dialogs can be interacted with simply by including a call to `.WithActionButton()`, this can be supplied with some content for the button, an on-clicked callback and optionally the `dismissOnClick` parameter can be set to `true` if you want the dialog to close immediately on clicking that specific button. Any number of `.WithActionButton()` calls can be included in the chain to add any number of buttons.

Here is an example of a dialog with two buttons, one of which will dismiss the dialog.

```cs
public void DisplayDialog()
{
    dialogManager.CreateDialog()
        .WithActionButton("Don't Close", _ => { })
        .WithActionButton("Close ", _ => { }, true)  // last parameter optional
        .TryShow();
}
```

![dialogclose](https://github.com/user-attachments/assets/3d07344f-c302-400a-b2cf-88865e7713ba)


## MessageBox Style

It is possible to use the `.OfType()` method to cause the dialog to use an included MessageBox style, the styles included are: `Information`, `Success`, `Warning` and `Error`. 

![dialogtypes](https://github.com/user-attachments/assets/1c596315-5e9a-4f4c-b577-e27d0d6b0a1d)

