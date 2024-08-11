# Stepper

指引用户分步骤完成一项任务的控件

## 展示

<img src="/controls/progress/stepper.gif" height="300px" width="300px"/>

## 示例

```xml .axaml
<suki:Stepper Index="{Binding StepIndex}" Steps="{Binding Steps}" />
```

```csharp
[ObservableProperty] private int _stepIndex = 1;
public IEnumerable<string> Steps { get; } = 
                           ["First Step", "Second Step", "Third Step"];
```

## 参阅

[Demo: SukiUI.Demo/Features/ControlsLibrary/ProgressView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/ProgressView.axaml)

[API: Controls/Stepper.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/Stepper.axaml.cs)