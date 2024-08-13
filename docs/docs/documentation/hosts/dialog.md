# Dialogs

SukiUI provides a [host](./hosts) which can display dialogs easily at any level of your application. As standard we recommend simply using it in `SukiWindow.Hosts` as this provides the best experience, however dialogs can be localised within whatever context you require.

The host is designed in such a way as to be MVVM friendly and as long as you have access to the `ISukiDialogManager` instance used for a given `SukiDialogHost` you can display dialogs in it.