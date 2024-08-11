# CircleProgressBar

圆形进度条

## 展示

<img src="/controls/progress/circleprogressbar.gif" height="300px" width="200px"/>

## 示例

```xml
<suki:CircleProgressBar IsIndeterminate="{Binding IsIndeterminate}"
                        StrokeWidth="11"
                        Value="{Binding ProgressValue}">
    <TextBlock Margin="0,2,0,0"  // 百分比显示
               Classes="h3"
               IsVisible="{Binding IsTextVisible}"
               Text="{Binding ProgressValue, StringFormat={}{0:#0}%}" />
</suki:CircleProgressBar>
```

## 参阅

[Demo: SukiUI.Demo/Features/ControlsLibrary/ProgressView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/ProgressView.axaml)

[API: Controls/CircleProgressBar.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/CircleProgressBar.axaml.cs)