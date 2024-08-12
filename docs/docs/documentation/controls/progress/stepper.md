# Stepper

A bar that guides users through the steps of a task

## Show

<img src="/controls/progress/stepper.gif" height="300px" width="300px"/>

## Example

```xml .axaml
<suki:Stepper Index="{Binding StepIndex}" Steps="{Binding Steps}" />
```

```csharp
[ObservableProperty] private int _stepIndex = 1;
public IEnumerable<string> Steps { get; } = 
                           ["First Step", "Second Step", "Third Step"];
```

## See Also

[Demo: SukiUI.Demo/Features/ControlsLibrary/ProgressView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/ProgressView.axaml)

[API: Controls/Stepper.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/Stepper.axaml.cs)