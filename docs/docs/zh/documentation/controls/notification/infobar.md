# InfoBar

`InfoBar` 是一个提醒控件，用于向用户传达特定严重程度的信息

## 展示

<img src="/controls/notification/infobar.gif" width="300px" />

## 示例

```xml
<suki:InfoBar 
        Title="Info"
        IsOpaque="{Binding IsOpaque}"
        IsClosable="{Binding IsClosable}"
        IsOpen="{Binding IsOpen, Mode=TwoWay}"
        Severity="Warning"
        Message="Hello World!" />
```

`Severity` 的值:
- Information
- Success
- Warning
- Error

## 参阅

[Demo: SukiUI.Demo/Features/ControlsLibrary/InfoBarView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/InfoBarView.axaml)

[API: Controls/InfoBar.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/InfoBar.axaml.cs)