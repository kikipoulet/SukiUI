using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class ProgressViewModel() : DemoPageBase("Progress", MaterialIconKind.ProgressUpload)
{
    public IEnumerable<string> Steps { get; } =
    [
        "First Step", "Second Step", "Third Step"
    ];

    [ObservableProperty] private int _stepIndex = 1;
    [ObservableProperty] [Range(0d, 100d)] private double _progressValue = 50;
    [ObservableProperty] private bool _isTextVisible = true;
    [ObservableProperty] private bool _isIndeterminate;

    [RelayCommand]
    private void ChangeStep(bool isIncrement)
    {
        switch (isIncrement)
        {
            case true when StepIndex >= Steps.Count() - 1:
            case false when StepIndex <= 0:
                return;
            default:
                StepIndex += isIncrement ? 1 : -1;
                break;
        }
    }

    partial void OnIsIndeterminateChanged(bool value)
    {
        IsTextVisible = value switch
        {
            true when IsTextVisible => false,
            false when IsTextVisible == false => true,
            _ => IsTextVisible
        };
    }

    partial void OnIsTextVisibleChanged(bool value)
    {
        if (value && IsIndeterminate)
            IsIndeterminate = false;
    }
}