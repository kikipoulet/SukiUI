# Hosts

SukiUI provides a property in `SukiWindow` that allows you to render any arbitrary control or content above all others, including the title bar.

```xml
<!-- XMLNS definitions omitted for brevity -->
<suki:SukiWindow>
	<suki:SukiWindow.Hosts>
		<!-- Your hosts here -->
	</suki:SukiWindow.Hosts>
</suki:SukiWindow>
```

By default SukiUI ships with two hosts and associated controls and APIs for interacting with them, these are [SukiDialogHost](./dialog) and [SukiToastHost](./toast).

::: warning
`suki:SukiWindow.Hosts` is only valid in `SukiWindow`, please be careful not to declare it in other pages (`Views`), it will have no effect.
:::