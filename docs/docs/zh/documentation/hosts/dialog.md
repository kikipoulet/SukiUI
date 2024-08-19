# Dialogs

SukiUI 提供了一个用于弹出对话框的[可选窗口控件](./hosts)，该控件可以很轻易地在 `SukiWindow.Hosts` 添加（这也是最推荐且能达到最佳效果的使用方法）

该对话框对 MVVM 设计模式友好，同时你也可以通过 `ISukiDialogManager` 来获得给定的 `SukiDialogHost` 实例，从而显示对话框。