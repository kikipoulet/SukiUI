using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class Tank
{
    private static readonly HyperSource SharedSource = HyperSource.Shared("""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 288 200">
          <defs>
            <linearGradient id="tk-in-grad" x1="0" y1="0" x2="1" y2="0">
              <stop offset="0.02" stop-color="#000000" />
              <stop offset="0.12" stop-color="#595959" />
              <stop offset="0.24" stop-color="#FFFFFF" />
              <stop offset="1" stop-color="#FFFFFF" />
            </linearGradient>
            <linearGradient id="tk-out-grad" x1="0" y1="0" x2="1" y2="0">
              <stop offset="0" stop-color="#FFFFFF" />
              <stop offset="0.55" stop-color="#FFFFFF" />
              <stop offset="0.98" stop-color="#000000" />
            </linearGradient>
            <mask id="tk-in-mask" maskUnits="userSpaceOnUse" x="0" y="0" width="288" height="200">
              <rect x="0" y="0" width="288" height="200" fill="url(#tk-in-grad)" />
            </mask>
            <mask id="tk-out-mask" maskUnits="userSpaceOnUse" x="0" y="0" width="288" height="200">
              <rect x="0" y="0" width="288" height="200" fill="url(#tk-out-grad)" />
            </mask>
            <linearGradient id="tk-refl" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0" stop-color="#FFFFFF" stop-opacity="0.55" />
              <stop offset="0.6" stop-color="#FFFFFF" stop-opacity="0.15" />
              <stop offset="1" stop-color="#FFFFFF" stop-opacity="0" />
            </linearGradient>
          </defs>

          <!-- Inlet pipe (behind vessel) -->
          <g mask="url(#tk-in-mask)">
            <path d="M 0,16 L 72,16 Q 84,16 84,28 L 84,30" fill="none" stroke="var(--SkeumoTubeRim)" stroke-width="12" stroke-linecap="round" stroke-linejoin="round" />
            <path d="M 0,16 L 72,16 Q 84,16 84,28 L 84,30" fill="none" stroke="var(--SkeumoTubeBg)" stroke-width="9.6" stroke-linecap="round" stroke-linejoin="round" />
            <path d="M 0,16 L 72,16 Q 84,16 84,28 L 84,30" fill="none" stroke="var(--SkeumoTubeCore)" stroke-width="4.5" stroke-linecap="round" stroke-linejoin="round" />
          </g>
          <path data-part="inletFlow" mask="url(#tk-in-mask)" d="M 0,16 L 72,16 Q 84,16 84,28 L 84,30" fill="none" stroke="var(--SukiPrimaryColor)" stroke-width="6" stroke-linecap="round" stroke-linejoin="round" />
          <path mask="url(#tk-in-mask)" d="M 4,13.2 L 70,13.2" fill="none" stroke="#FFFFFF" stroke-opacity="0.25" stroke-width="1.4" stroke-linecap="round" />

          <!-- Outlet pipe (behind vessel and feet) -->
          <g mask="url(#tk-out-mask)">
            <path d="M 96,160 L 96,180 Q 96,192 108,192 L 288,192" fill="none" stroke="var(--SkeumoTubeRim)" stroke-width="12" stroke-linecap="round" stroke-linejoin="round" />
            <path d="M 96,160 L 96,180 Q 96,192 108,192 L 288,192" fill="none" stroke="var(--SkeumoTubeBg)" stroke-width="9.6" stroke-linecap="round" stroke-linejoin="round" />
            <path d="M 96,160 L 96,180 Q 96,192 108,192 L 288,192" fill="none" stroke="var(--SkeumoTubeCore)" stroke-width="4.5" stroke-linecap="round" stroke-linejoin="round" />
          </g>
          <path data-part="outletFlow" mask="url(#tk-out-mask)" d="M 96,160 L 96,180 Q 96,192 108,192 L 288,192" fill="none" stroke="var(--SukiPrimaryColor)" stroke-width="6" stroke-linecap="round" stroke-linejoin="round" />
          <path mask="url(#tk-out-mask)" d="M 110,189.2 L 284,189.2" fill="none" stroke="#FFFFFF" stroke-opacity="0.25" stroke-width="1.4" stroke-linecap="round" />

          <!-- Pipe particle layer (masked by code) -->
          <rect data-part="pipeLayer" data-window="true" x="0" y="0" width="288" height="200" />

          <!-- Slanted legs and pads -->
          <path d="M 47,146 L 57,146 L 50,177 L 40,177 Z" fill="var(--SkeumoLeg)" stroke="var(--SkeumoEdge)" stroke-width="0.6" stroke-linejoin="round" />
          <path d="M 111,146 L 121,146 L 128,177 L 118,177 Z" fill="var(--SkeumoLeg)" stroke="var(--SkeumoEdge)" stroke-width="0.6" stroke-linejoin="round" />
          <path d="M 49,150 L 46.5,161" fill="none" stroke="#FFFFFF" stroke-opacity="0.3" stroke-width="1.2" stroke-linecap="round" />
          <path d="M 113,150 L 115.5,161" fill="none" stroke="#FFFFFF" stroke-opacity="0.3" stroke-width="1.2" stroke-linecap="round" />
          <rect x="33" y="175" width="24" height="6" rx="3" fill="var(--SkeumoBase)" stroke="var(--SkeumoEdge)" stroke-width="0.8" filter="url(#skeumo-shadow)" />
          <rect x="111" y="175" width="24" height="6" rx="3" fill="var(--SkeumoBase)" stroke="var(--SkeumoEdge)" stroke-width="0.8" filter="url(#skeumo-shadow)" />
          <rect x="36" y="175.8" width="18" height="1.2" rx="0.6" fill="#FFFFFF" fill-opacity="0.35" />
          <rect x="114" y="175.8" width="18" height="1.2" rx="0.6" fill="#FFFFFF" fill-opacity="0.35" />

          <!-- Glass back -->
          <path d="M 68,34 H 100 A 34,34 0 0 1 134,68 V 148 A 16,16 0 0 1 118,164 H 50 A 16,16 0 0 1 34,148 V 68 A 34,34 0 0 1 68,34 Z" fill="var(--SkeumoGlassBack)" />

          <!-- Dynamic liquid and particles clipped to inner vessel -->
          <rect data-part="interior" data-window="true" x="36" y="36" width="96" height="126" rx="14" />

          <!-- Glass walls, reflections, graduation marks -->
          <path d="M 68,34 H 100 A 34,34 0 0 1 134,68 V 148 A 16,16 0 0 1 118,164 H 50 A 16,16 0 0 1 34,148 V 68 A 34,34 0 0 1 68,34 Z" fill="none" stroke="var(--SkeumoGlassRim)" stroke-width="1.6" />
          <path d="M 68,36 H 100 A 32,32 0 0 1 132,68 V 148 A 14,14 0 0 1 118,162 H 50 A 14,14 0 0 1 36,148 V 68 A 32,32 0 0 1 68,36 Z" fill="none" stroke="var(--SkeumoGlassInner)" stroke-width="1" />
          <rect x="44" y="52" width="6" height="96" rx="3" fill="url(#tk-refl)" opacity="0.7" />
          <rect x="122" y="56" width="2" height="86" rx="1" fill="#FFFFFF" fill-opacity="0.16" />
          <path d="M 48,52 Q 54,40 72,38" fill="none" stroke="#FFFFFF" stroke-opacity="0.35" stroke-width="2.5" stroke-linecap="round" />
          <rect x="112" y="133" width="8" height="1" fill="#FFFFFF" fill-opacity="0.35" />
          <rect x="114" y="105" width="6" height="1" fill="#FFFFFF" fill-opacity="0.25" />
          <rect x="112" y="77" width="8" height="1" fill="#FFFFFF" fill-opacity="0.35" />

          <!-- Top flange, outlet nipple, leg brackets -->
          <rect x="64" y="28" width="40" height="10" rx="5" fill="var(--SkeumoRodH)" stroke="var(--SkeumoEdge)" stroke-width="0.8" filter="url(#skeumo-shadow)" />
          <rect x="67" y="29.4" width="34" height="1.5" rx="0.75" fill="#FFFFFF" fill-opacity="0.45" />
          <circle cx="69" cy="33" r="2" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
          <circle cx="99" cy="33" r="2" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
          <rect x="90" y="160" width="12" height="8" rx="2" fill="var(--SkeumoRodV)" stroke="var(--SkeumoEdge)" stroke-width="0.6" />

          <rect x="42" y="141" width="20" height="9" rx="3" fill="var(--SkeumoBase)" stroke="var(--SkeumoEdge)" stroke-width="0.8" filter="url(#skeumo-shadow)" />
          <rect x="106" y="141" width="20" height="9" rx="3" fill="var(--SkeumoBase)" stroke="var(--SkeumoEdge)" stroke-width="0.8" filter="url(#skeumo-shadow)" />
          <rect x="44" y="142" width="16" height="1.2" rx="0.6" fill="#FFFFFF" fill-opacity="0.35" />
          <rect x="108" y="142" width="16" height="1.2" rx="0.6" fill="#FFFFFF" fill-opacity="0.35" />
        </svg>
        """);

    protected override HyperSource Source => SharedSource;
}
