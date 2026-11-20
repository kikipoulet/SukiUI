using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class TriSwitch
{
    private static readonly HyperSource SharedSource = HyperSource.Shared("""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 96 48">
          <!-- Wire terminal -->
          <path d="M 79,34 H 89 V 43 A 3,3 0 0 1 86,46 H 82 A 3,3 0 0 1 79,43 Z"
                fill="var(--SkeumoRodV)" stroke="var(--SkeumoEdge)" stroke-width="0.7" />

          <!-- Plate -->
          <rect x="2" y="2" width="92" height="40" rx="9" fill="var(--SkeumoBody)" stroke="var(--SkeumoEdge)" stroke-width="1" filter="url(#skeumo-shadow)" />
          <rect x="2" y="2" width="92" height="40" rx="9" fill="var(--SkeumoSheen)" />
          <rect x="8" y="3.5" width="80" height="1.6" rx="0.8" fill="#FFFFFF" fill-opacity="0.18" />

          <!-- Dug slot -->
          <rect x="12" y="9" width="72" height="8" rx="4" fill="var(--SkeumoDeepRecess)" />

          <!-- Dynamic accent fill inside slot -->
          <rect data-part="fillLayer" data-window="true" x="12" y="9" width="72" height="8" rx="4" />

          <!-- Knob (default at detent 0: x=9..31, center x=20) -->
          <g data-part="knob" data-pivot="20 13">
            <rect x="9" y="4" width="22" height="18" rx="5" fill="var(--SkeumoRodH)" stroke="var(--SkeumoEdge)" stroke-width="0.8" filter="url(#skeumo-shadow)" />
            <rect x="13" y="8" width="1" height="10" fill="var(--SkeumoGroove)" />
            <rect x="26" y="8" width="1" height="10" fill="var(--SkeumoGroove)" />
            <rect x="19" y="6" width="2" height="14" rx="1" fill="var(--SukiPrimaryColor)" filter="url(#skeumo-glow)" />
          </g>
        </svg>
        """);

    protected override HyperSource Source => SharedSource;
}
