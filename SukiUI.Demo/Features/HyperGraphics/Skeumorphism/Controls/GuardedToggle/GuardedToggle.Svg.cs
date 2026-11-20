using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class GuardedToggle
{
    private static readonly HyperSource SharedSource = HyperSource.Shared("""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 110 160">
          <!-- Wire terminals -->
          <path d="M 23,3 A 3,3 0 0 1 26,0 H 34 A 3,3 0 0 1 37,3 V 11 A 2,2 0 0 1 35,13 H 25 A 2,2 0 0 1 23,11 Z"
                fill="var(--SkeumoRodV)" stroke="var(--SkeumoEdge)" stroke-width="0.7" />
          <circle cx="30" cy="7.5" r="3.5" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
          <path d="M 73,3 A 3,3 0 0 1 76,0 H 84 A 3,3 0 0 1 87,3 V 11 A 2,2 0 0 1 85,13 H 75 A 2,2 0 0 1 73,11 Z"
                fill="var(--SkeumoRodV)" stroke="var(--SkeumoEdge)" stroke-width="0.7" />
          <circle cx="80" cy="7.5" r="3.5" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />

          <!-- Base plate -->
          <rect x="15" y="10" width="80" height="140" rx="14" fill="var(--SkeumoBody)" stroke="var(--SkeumoEdge)" stroke-width="1.2" filter="url(#skeumo-shadow)" />
          <rect x="15" y="10" width="80" height="140" rx="14" fill="var(--SkeumoSheen)" />
          <rect x="20" y="12" width="70" height="2.2" rx="1.1" fill="#FFFFFF" fill-opacity="0.18" />
          <circle cx="23.5" cy="141.5" r="2.5" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
          <circle cx="86.5" cy="141.5" r="2.5" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />

          <!-- Bushing -->
          <circle cx="55" cy="92" r="16" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.8" />
          <circle cx="55" cy="92" r="8" fill="var(--SkeumoDeepRecess)" />

          <!-- Bat lever rod (scales vertically around 55,92) -->
          <rect data-part="rod" data-pivot="55 92" x="51" y="92" width="8" height="36" rx="4"
                fill="var(--SkeumoRodV)" stroke="var(--SkeumoEdge)" stroke-width="0.6" />

          <!-- Bat tip (moves vertically) -->
          <g data-part="tip" data-pivot="55 129">
            <circle cx="55" cy="129" r="8" fill="var(--SkeumoDarkKnob)" stroke="var(--SkeumoEdge)" stroke-width="0.8" />
            <ellipse cx="53" cy="124.5" rx="3" ry="1.5" fill="#FFFFFF" fill-opacity="0.5" />
          </g>

          <!-- Safety cover hinge & flap -->
          <rect data-part="coverHinge" x="29" y="54" width="52" height="7" rx="3.5"
                fill="var(--SkeumoRodH)" stroke="var(--SkeumoEdge)" stroke-width="0.7" filter="url(#skeumo-shadow)" />
          <g data-part="cover" data-pivot="55 58">
            <path d="M 31,62 A 4,4 0 0 1 35,58 H 75 A 4,4 0 0 1 79,62 V 132 A 12,12 0 0 1 67,144 H 43 A 12,12 0 0 1 31,132 Z"
                  fill="var(--SkeumoGlassBack)" fill-opacity="0.85" />
            <path d="M 31,62 A 4,4 0 0 1 35,58 H 75 A 4,4 0 0 1 79,62 V 132 A 12,12 0 0 1 67,144 H 43 A 12,12 0 0 1 31,132 Z"
                  fill="var(--SkeumoSheen)" stroke="var(--SkeumoGlassRim)" stroke-width="1.4" />
            <rect x="37" y="66" width="4" height="64" rx="2" fill="#FFFFFF" fill-opacity="0.2" />
            <rect x="41" y="133" width="28" height="5" rx="2.5" fill="var(--SkeumoRubberGrad)" />
          </g>
        </svg>
        """);

    protected override HyperSource Source => SharedSource;
}
