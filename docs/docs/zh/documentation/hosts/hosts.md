# Hosts

SukiUI 在 `SukiWindow` 内提供了 `Hosts` 属性，可以在该属性内添加任意控件，而这些控件将会显示在其他所有子控件的上层（包括标题栏）

```xml
<!-- XMLNS 定义已略去 -->
<suki:SukiWindow>
	<suki:SukiWindow.Hosts>
		<!-- 你的控件 -->
	</suki:SukiWindow.Hosts>
</suki:SukiWindow>
```

SukiUI 本身提供两个可选的窗口控件，即 [SukiDialogHost](./dialog) 和 [SukiToastHost](./toast)

::: warning
`suki:SukiWindow.Hosts` 仅在 `SukiWindow` 有效，请注意不要不小心在页面（`Views`）中声明，这将没有任何效果。
:::