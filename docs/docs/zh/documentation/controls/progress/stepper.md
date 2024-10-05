# Stepper

指引用户分步骤完成一项任务的控件

<img src="https://sleekshot.app/api/download/9UWKAIevk5i2"/>

## 用法

### Axaml

```xml .axaml
<suki:Stepper Index="{Binding StepIndex}" Steps="{Binding Steps}" />
```

### ViewModel

```csharp
[ObservableProperty] private int _stepIndex = 1;
public IEnumerable<string> Steps { get; } = 
                           ["First Step", "Second Step", "Third Step"];
```

## 使用另一种样式

<img src="https://sleekshot.app/api/download/siVzTBuU6zhn"/>

### Axaml

```xml .axaml
<suki:Stepper AlternativeStyle="True" />
```

## 参阅

[Demo: SukiUI.Demo/Features/ControlsLibrary/ProgressView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/ProgressView.axaml)

[API: Controls/Stepper.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/Stepper.axaml.cs)