using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class Lamp
{
    private static readonly HyperSource SharedSource = HyperSource.Shared("""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 190 220">
          <defs>
            <radialGradient id="lp-bulb-glow" cx="0.5" cy="0.45" r="0.5">
              <stop offset="0" stop-color="#FFFBEF" />
              <stop offset="0.3" stop-color="#FFE2A6" />
              <stop offset="1" stop-color="#FFB35C" stop-opacity="0.85" />
            </radialGradient>
          </defs>

          <g transform="translate(20 21)">
            <!-- Holder stem & foot -->
            <rect x="70" y="150" width="10" height="34" fill="var(--SkeumoRodV)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
            <rect x="32" y="182" width="86" height="14" rx="7" fill="var(--SkeumoBody)" stroke="var(--SkeumoEdge)" stroke-width="1" filter="url(#skeumo-shadow)" />
            <rect x="38" y="183.4" width="74" height="1.6" rx="0.8" fill="#FFFFFF" fill-opacity="0.35" />

            <!-- Bulb glass back -->
            <path d="M 57,114 C 57,100 35,92 35,64 A 40,40 0 1 1 115,64 C 115,92 93,100 93,114 Z" fill="var(--SkeumoGlassBack)" />
          </g>

          <!-- Dynamic bulb glow -->
          <path data-part="bulbGlow" transform="translate(20 21)"
                d="M 57,114 C 57,100 35,92 35,64 A 40,40 0 1 1 115,64 C 115,92 93,100 93,114 Z"
                fill="url(#lp-bulb-glow)" />

          <g transform="translate(20 21)">
            <!-- Support wires & cold filament -->
            <path d="M 64,114 L 64,90 L 62,70 M 86,114 L 86,90 L 88,70 M 75,114 L 75,86"
                  fill="none" stroke="var(--SkeumoEdge)" stroke-width="1.1" stroke-linecap="round" />
          </g>

          <path data-part="filamentCold" transform="translate(20 21)"
                d="M 62,70 L 65,63 L 68,70 L 71,63 L 74,70 L 77,63 L 80,70 L 83,63 L 86,70 L 88,70"
                fill="none" stroke="var(--SkeumoTick)" stroke-width="1.3" stroke-linejoin="round" stroke-linecap="round" />

          <g transform="translate(20 21)">
            <!-- Glass walls & reflections -->
            <path d="M 57,114 C 57,100 35,92 35,64 A 40,40 0 1 1 115,64 C 115,92 93,100 93,114 Z"
                  fill="none" stroke="var(--SkeumoGlassRim)" stroke-width="1.6" />
            <path d="M 46,58 A 30,30 0 0 1 70,33" fill="none" stroke="#FFFFFF" stroke-opacity="0.4" stroke-width="3" stroke-linecap="round" />
            <path d="M 104,72 A 30,30 0 0 1 98,86" fill="none" stroke="#FFFFFF" stroke-opacity="0.16" stroke-width="2" stroke-linecap="round" />

            <!-- Screw socket -->
            <path d="M 58,112 H 92 A 3,3 0 0 1 95,115 V 136 A 6,6 0 0 1 89,142 H 61 A 6,6 0 0 1 55,136 V 115 A 3,3 0 0 1 58,112 Z"
                  fill="var(--SkeumoRodV)" stroke="var(--SkeumoEdge)" stroke-width="0.8" filter="url(#skeumo-shadow)" />
            <path d="M 56,118 L 94,115 M 56,124 L 94,121 M 56,130 L 94,127 M 56,136 L 94,133"
                  fill="none" stroke="var(--SkeumoGroove)" stroke-width="1.2" />
            <path d="M 62,141 H 88 V 143 A 4,4 0 0 1 84,147 H 66 A 4,4 0 0 1 62,143 Z" fill="var(--SkeumoDeepRecess)" />
            <path d="M 70,146 H 80 V 148 A 3,3 0 0 1 77,151 H 73 A 3,3 0 0 1 70,148 Z"
                  fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
          </g>
        </svg>
        """);

    protected override HyperSource Source => SharedSource;
}
