using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism;

public partial class SkeumorphismViewModel
{
    public IReadOnlyList<AiMascotMood> MascotMoods { get; } = Enum.GetValues<AiMascotMood>();

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(MascotMoodText))] private AiMascotMood _mascotMood = AiMascotMood.Idle;

    [ObservableProperty] private bool _mascotFollowPointer = true;

    public string MascotMoodText => MascotMood.ToString();
}
