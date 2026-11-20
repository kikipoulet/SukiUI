using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class Fan
{
    private static readonly HyperSource SharedSource = HyperSource.Shared("""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 160">
          <!-- Cable gland under housing -->
          <path d="M 121,154 H 135 V 162 A 4,4 0 0 1 131,166 H 125 A 4,4 0 0 1 121,162 Z"
                fill="var(--SkeumoRodV)" stroke="var(--SkeumoEdge)" stroke-width="0.7" />

          <!-- Housing -->
          <rect x="0" y="0" width="160" height="160" rx="28" fill="var(--SkeumoBody)" stroke="var(--SkeumoEdge)" stroke-width="1.2" filter="url(#skeumo-shadow)" />
          <rect x="0" y="0" width="160" height="160" rx="28" fill="var(--SkeumoSheen)" />
          <rect x="16" y="2" width="128" height="2.2" rx="1.1" fill="#FFFFFF" fill-opacity="0.18" />

          <!-- Corner screws -->
          <circle cx="13" cy="13" r="4" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
          <rect x="10" y="12.4" width="6" height="1.2" fill="var(--SkeumoSlot)" transform="rotate(25 13 13)" />
          <circle cx="147" cy="13" r="4" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
          <rect x="144" y="12.4" width="6" height="1.2" fill="var(--SkeumoSlot)" transform="rotate(-30 147 13)" />
          <circle cx="13" cy="147" r="4" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
          <rect x="10" y="146.4" width="6" height="1.2" fill="var(--SkeumoSlot)" transform="rotate(60 13 147)" />
          <circle cx="147" cy="147" r="4" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
          <rect x="144" y="146.4" width="6" height="1.2" fill="var(--SkeumoSlot)" transform="rotate(10 147 147)" />

          <!-- Venturi recess -->
          <circle cx="80" cy="80" r="68" fill="var(--SkeumoDeepRecess)" />

          <!-- Motor glow & blur ghosts behind rotor -->
          <rect data-part="venturiFx" data-window="true" x="12" y="12" width="136" height="136" rx="68" />

          <!-- Rotor blades (5 blades around 80,80) -->
          <g data-part="rotor" data-pivot="80 80">
            <g transform="translate(12 12)">
              <path d="M 70,58 C 78,36 100,22 120,30 C 122,48 104,62 78,68 Z" fill="var(--SkeumoMetal)" stroke="var(--SkeumoEdge)" stroke-width="0.7" />
              <path d="M 70,58 C 78,36 100,22 120,30 C 122,48 104,62 78,68 Z" fill="var(--SkeumoMetal)" stroke="var(--SkeumoEdge)" stroke-width="0.7" transform="rotate(72 68 68)" />
              <path d="M 70,58 C 78,36 100,22 120,30 C 122,48 104,62 78,68 Z" fill="var(--SkeumoMetal)" stroke="var(--SkeumoEdge)" stroke-width="0.7" transform="rotate(144 68 68)" />
              <path d="M 70,58 C 78,36 100,22 120,30 C 122,48 104,62 78,68 Z" fill="var(--SkeumoMetal)" stroke="var(--SkeumoEdge)" stroke-width="0.7" transform="rotate(216 68 68)" />
              <path d="M 70,58 C 78,36 100,22 120,30 C 122,48 104,62 78,68 Z" fill="var(--SkeumoMetal)" stroke="var(--SkeumoEdge)" stroke-width="0.7" transform="rotate(288 68 68)" />
            </g>
          </g>

          <!-- Hub -->
          <circle data-part="hubRing" cx="80" cy="80" r="20" fill="none" stroke="var(--SukiPrimaryColor)" stroke-width="2" />
          <circle cx="80" cy="80" r="16" fill="var(--SkeumoDarkKnob)" stroke="var(--SkeumoEdge)" stroke-width="0.8" />
          <circle cx="80" cy="80" r="9" fill="var(--SkeumoAxle)" />
          <ellipse cx="76.5" cy="73.7" rx="3.5" ry="1.75" fill="#FFFFFF" fill-opacity="0.5" />

          <!-- Air particles -->
          <rect data-part="air" data-window="true" x="0" y="0" width="160" height="160" />

          <!-- Front grille -->
          <circle cx="80" cy="80" r="66" fill="none" stroke="var(--SkeumoEdge)" stroke-opacity="0.45" stroke-width="1.4" />
          <circle cx="80" cy="80" r="46" fill="none" stroke="var(--SkeumoEdge)" stroke-opacity="0.45" stroke-width="1.2" />
          <circle cx="80" cy="80" r="28" fill="none" stroke="var(--SkeumoEdge)" stroke-opacity="0.45" stroke-width="1.2" />
          <path d="M 33,33 L 66,66 M 127,33 L 94,66 M 33,127 L 66,94 M 127,127 L 94,94"
                fill="none" stroke="var(--SkeumoEdge)" stroke-opacity="0.45" stroke-width="1.6" stroke-linecap="round" />
        </svg>
        """);

    protected override HyperSource Source => SharedSource;
}
