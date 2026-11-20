using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class Battery
{
    private static readonly HyperSource SharedSource = HyperSource.Shared("""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 120 180">
          <!-- Terminals -->
          <path d="M 24,10 A 4,4 0 0 1 28,6 H 40 A 4,4 0 0 1 44,10 V 19 A 1,1 0 0 1 43,20 H 25 A 1,1 0 0 1 24,19 Z"
                fill="var(--SkeumoRodV)" stroke="var(--SkeumoEdge)" stroke-width="0.8" />
          <ellipse cx="34" cy="10" rx="5" ry="2.5" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
          <path d="M 76,10 A 4,4 0 0 1 80,6 H 92 A 4,4 0 0 1 96,10 V 19 A 1,1 0 0 1 95,20 H 77 A 1,1 0 0 1 76,19 Z"
                fill="var(--SkeumoRodV)" stroke="var(--SkeumoEdge)" stroke-width="0.8" />
          <ellipse cx="86" cy="10" rx="5" ry="2.5" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />

          <!-- Case -->
          <rect x="8" y="18" width="104" height="152" rx="16" fill="var(--SkeumoBody)" stroke="var(--SkeumoEdge)" stroke-width="1.2" filter="url(#skeumo-shadow)" />
          <rect x="8" y="18" width="104" height="152" rx="16" fill="var(--SkeumoSheen)" />
          <rect x="16" y="20" width="88" height="2.2" rx="1.1" fill="#FFFFFF" fill-opacity="0.18" />

          <!-- Grip ridges -->
          <path d="M 13,60 V 130 M 16,60 V 130 M 104,60 V 130 M 107,60 V 130" fill="none" stroke="var(--SkeumoGroove)" stroke-width="1" />

          <!-- Dug window -->
          <rect x="22" y="48" width="76" height="108" rx="10" fill="var(--SkeumoDeepRecess)" />

          <!-- Dynamic cells & readout inside dug window -->
          <rect data-part="cells" data-window="true" x="22" y="48" width="76" height="108" rx="10" />

          <!-- Window glass -->
          <rect x="22" y="48" width="76" height="108" rx="10" fill="var(--SkeumoSheen)" stroke="var(--SkeumoGlassRim)" stroke-width="1" />

          <!-- Rubber feet -->
          <path d="M 20,169 H 42 V 172 A 3,3 0 0 1 39,175 H 23 A 3,3 0 0 1 20,172 Z" fill="var(--SkeumoRubber)" />
          <path d="M 78,169 H 100 V 172 A 3,3 0 0 1 97,175 H 81 A 3,3 0 0 1 78,172 Z" fill="var(--SkeumoRubber)" />
        </svg>
        """);

    protected override HyperSource Source => SharedSource;
}
