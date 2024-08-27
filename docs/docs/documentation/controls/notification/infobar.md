# InfoBar

`InfoBar` is a control that displays a message and can be used to show specific severity message to the user.

## Show

<img src="/controls/notification/infobar.gif" width="300px" />

## Example

```xml
<suki:InfoBar 
        Title="Info"
        IsOpaque="{Binding IsOpaque}"
        IsClosable="{Binding IsClosable}"
        IsOpen="{Binding IsOpen, Mode=TwoWay}"
        Severity="Warning"
        Message="Hello World!" />
```

`Severity` property allows:
- Information
- Success
- Warning
- Error

## See Also

[Demo: SukiUI.Demo/Features/ControlsLibrary/InfoBarView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/InfoBarView.axaml)

[API: Controls/InfoBar.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/InfoBar.axaml.cs)