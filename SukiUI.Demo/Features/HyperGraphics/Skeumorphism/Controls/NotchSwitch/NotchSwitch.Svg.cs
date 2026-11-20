using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class NotchSwitch
{
    private static readonly HyperSource SharedSource = HyperSource.Shared("""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 150 100">
          <!-- Dug rail -->
          <rect x="95" y="16" width="8" height="68" rx="4" fill="var(--SkeumoTrack)" />
          <rect x="97" y="84" width="4" height="0.8" fill="#FFFFFF" fill-opacity="0.14" />

          <!-- Dynamic accent fill inside rail -->
          <rect data-part="fillLayer" data-window="true" x="95" y="16" width="8" height="68" rx="4" />

          <!-- Knob (default at bottom notch y=70) -->
          <g data-part="knob" data-pivot="99 77">
            <rect x="88" y="70" width="22" height="14" rx="4" fill="var(--SkeumoMetal)" filter="url(#skeumo-shadow)" />
            <rect x="94" y="76" width="10" height="2" fill="var(--SukiPrimaryColor)" />
          </g>
        </svg>
        """);

    protected override HyperSource Source => SharedSource;
}
