# Stepper

A bar that guides users through the steps of a task

<img src="https://sleekshot.app/api/download/9UWKAIevk5i2"/>

## Usage

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

## Alternative Style 

<img src="https://sleekshot.app/api/download/siVzTBuU6zhn"/>

### Axaml

```xml .axaml
<suki:Stepper AlternativeStyle="True" />
```

## See Also

[Demo: SukiUI.Demo/Features/ControlsLibrary/ProgressView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/ProgressView.axaml)

[API: Controls/Stepper.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/Stepper.axaml.cs)
