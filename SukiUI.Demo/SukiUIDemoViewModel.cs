using System.Collections.Generic;
using Avalonia.Collections;
using SukiUI.Demo.Common;
using SukiUI.Demo.Features;

namespace SukiUI.Demo;

public partial class SukiUIDemoViewModel : ViewAwareObservableObject
{
    public AvaloniaList<FeatureBase> Features { get; } = [];
    
    public SukiUIDemoViewModel(IEnumerable<FeatureBase> features)
    {
        Features.AddRange(features);
    }
}