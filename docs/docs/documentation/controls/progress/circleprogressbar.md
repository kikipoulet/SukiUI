# CircleProgressBar

Circle Progress Bar

## Show

<img src="/controls/progress/circleprogressbar.gif" height="300px" width="200px"/>

## Example

```xml
<suki:CircleProgressBar IsIndeterminate="{Binding IsIndeterminate}"
                        StrokeWidth="11"
                        Value="{Binding ProgressValue}">
    <TextBlock Margin="0,2,0,0"  // Show percentage
               Classes="h3"
               IsVisible="{Binding IsTextVisible}"
               Text="{Binding ProgressValue, StringFormat={}{0:#0}%}" />
</suki:CircleProgressBar>
```

## See Also

[Demo: SukiUI.Demo/Features/ControlsLibrary/ProgressView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/ProgressView.axaml)

[API: Controls/CircleProgressBar.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/CircleProgressBar.axaml.cs)