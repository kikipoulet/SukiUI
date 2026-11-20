using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class Syringe
{
    private static readonly HyperSource SharedSource = HyperSource.Shared("""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 300 290">
          <defs>
            <linearGradient id="sy-manifold-grad" x1="0" y1="0" x2="1" y2="0">
              <stop offset="0.187" stop-color="#000000" />
              <stop offset="0.373" stop-color="#FFFFFF" />
              <stop offset="0.627" stop-color="#FFFFFF" />
              <stop offset="0.813" stop-color="#000000" />
            </linearGradient>
            <mask id="sy-manifold-mask" maskUnits="userSpaceOnUse" x="0" y="0" width="300" height="60">
              <rect x="0" y="0" width="300" height="60" fill="url(#sy-manifold-mask-grad)" />
            </mask>
            <linearGradient id="sy-manifold-mask-grad" x1="0" y1="0" x2="1" y2="0">
              <stop offset="0.187" stop-color="#000000" />
              <stop offset="0.373" stop-color="#FFFFFF" />
              <stop offset="0.627" stop-color="#FFFFFF" />
              <stop offset="0.813" stop-color="#000000" />
            </linearGradient>
            <linearGradient id="sy-refl" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0" stop-color="#FFFFFF" stop-opacity="0.55" />
              <stop offset="0.6" stop-color="#FFFFFF" stop-opacity="0.15" />
              <stop offset="1" stop-color="#FFFFFF" stop-opacity="0" />
            </linearGradient>
          </defs>

          <!-- In/Out manifold -->
          <rect mask="url(#sy-manifold-mask)" x="30" y="15" width="240" height="14" rx="7" fill="var(--SkeumoTubeBg)" stroke="var(--SkeumoTubeRim)" stroke-width="1.2" />
          <rect data-part="chanIn" mask="url(#sy-manifold-mask)" x="34" y="18.5" width="116" height="7" rx="3.5" fill="var(--SukiPrimaryColor)" />
          <rect data-part="chanOut" mask="url(#sy-manifold-mask)" x="150" y="18.5" width="116" height="7" rx="3.5" fill="var(--SukiPrimaryColor)" />
          <rect mask="url(#sy-manifold-mask)" x="40" y="16.6" width="220" height="2" rx="1" fill="#FFFFFF" fill-opacity="0.35" />

          <!-- Vertical neck -->
          <rect x="143" y="30" width="14" height="22" fill="var(--SkeumoRodV)" />
          <rect x="145.5" y="34" width="9" height="18" rx="2" fill="var(--SkeumoBore)" />

          <!-- Dynamic manifold & neck stream -->
          <rect data-part="streamLayer" data-window="true" x="30" y="15" width="240" height="40" />

          <!-- T-junction & rotor -->
          <rect x="136" y="9" width="28" height="26" rx="9" fill="var(--SkeumoBase)" stroke="var(--SkeumoEdge)" stroke-width="1.1" filter="url(#skeumo-shadow)" />
          <rect x="140" y="10.6" width="20" height="2" rx="1" fill="#FFFFFF" fill-opacity="0.25" />
          <g data-part="valveRotor" data-pivot="150 22">
            <circle cx="150" cy="22" r="11.5" fill="var(--SkeumoDarkKnob)" stroke="var(--SkeumoEdge)" stroke-width="0.9" />
            <circle cx="150" cy="22" r="9" fill="var(--SkeumoDeepRecess)" />
            <path d="M150,22 V32 M150,22 H140" fill="none" stroke="var(--SukiPrimaryColor)" stroke-width="7" stroke-linecap="round" stroke-opacity="0.28" />
            <path d="M150,22 V32 M150,22 H140" fill="none" stroke="var(--SukiPrimaryColor)" stroke-width="3.4" stroke-linecap="round" />
            <path d="M150,22 V32 M150,22 H140" fill="none" stroke="#FFFFFF" stroke-opacity="0.7" stroke-width="1" stroke-linecap="round" />
            <circle cx="150" cy="22" r="3.2" fill="var(--SkeumoRodV)" stroke="var(--SkeumoEdge)" stroke-width="0.6" />
          </g>
          <ellipse cx="146" cy="14" rx="4.5" ry="1.8" fill="#FFFFFF" fill-opacity="0.35" />
          <rect x="140" y="44" width="20" height="6" rx="3" fill="var(--SkeumoRodH)" stroke="var(--SkeumoEdge)" stroke-width="0.5" filter="url(#skeumo-shadow)" />

          <!-- Plunger rod behind glass -->
          <rect data-part="rodLayer" data-window="true" x="142" y="50" width="16" height="214" />

          <!-- Glass barrel back -->
          <path d="M 134,50 H 166 A 14,14 0 0 1 180,64 V 246 A 4,4 0 0 1 176,250 H 124 A 4,4 0 0 1 120,246 V 64 A 14,14 0 0 1 134,50 Z"
                fill="var(--SkeumoGlassBack)" />

          <!-- Liquid & particles inside barrel -->
          <rect data-part="barrel" data-window="true" x="122" y="50" width="56" height="200" rx="4" />

          <!-- Barrel glass walls & reflections -->
          <path d="M 134,50 H 166 A 14,14 0 0 1 180,64 V 246 A 4,4 0 0 1 176,250 H 124 A 4,4 0 0 1 120,246 V 64 A 14,14 0 0 1 134,50 Z"
                fill="none" stroke="var(--SkeumoGlassRim)" stroke-width="1.6" />
          <path d="M 134,52 H 166 A 12,12 0 0 1 178,64 V 245 A 3,3 0 0 1 175,248 H 125 A 3,3 0 0 1 122,245 V 64 A 12,12 0 0 1 134,52 Z"
                fill="none" stroke="var(--SkeumoGlassInner)" stroke-width="1" />
          <rect x="126" y="58" width="6" height="168" rx="3" fill="url(#sy-refl)" opacity="0.7" />
          <rect x="168" y="62" width="2" height="150" rx="1" fill="#FFFFFF" fill-opacity="0.16" />
          <ellipse cx="150" cy="58" rx="20" ry="3.5" fill="#FFFFFF" fill-opacity="0.14" />

          <!-- Barrel head & finger collar -->
          <rect x="116" y="48" width="68" height="11" rx="5.5" fill="var(--SkeumoRodH)" stroke="var(--SkeumoEdge)" stroke-width="0.8" filter="url(#skeumo-shadow)" />
          <rect x="119" y="49.5" width="62" height="1.6" rx="0.8" fill="#FFFFFF" fill-opacity="0.45" />
          <rect x="109" y="246" width="82" height="9" rx="4.5" fill="var(--SkeumoRodH)" stroke="var(--SkeumoEdge)" stroke-width="0.8" filter="url(#skeumo-shadow)" />
          <rect x="113" y="247.4" width="74" height="1.4" rx="0.7" fill="#FFFFFF" fill-opacity="0.45" />

          <!-- Handle -->
          <rect x="124" y="262" width="52" height="12" rx="6" fill="var(--SkeumoRodH)" stroke="var(--SkeumoEdge)" stroke-width="0.8" filter="url(#skeumo-shadow)" />
          <rect x="130" y="263.3" width="40" height="1.5" rx="0.75" fill="#FFFFFF" fill-opacity="0.5" />
          <path d="M140,266 V271 M145,266 V271 M150,266 V271 M155,266 V271 M160,266 V271" fill="none" stroke="#0B0F15" stroke-opacity="0.45" stroke-width="1" />

          <!-- Plunger seal (moves vertically) -->
          <g data-part="plunger">
            <rect x="122" y="50" width="56" height="12" rx="4.5" fill="var(--SkeumoRubberGrad)" stroke="var(--SkeumoEdge)" stroke-width="0.8" filter="url(#skeumo-shadow)" />
            <rect x="124" y="50.5" width="52" height="2" rx="1" fill="#FFFFFF" fill-opacity="0.3" />
            <rect x="123" y="53.5" width="54" height="1" fill="#667289" fill-opacity="0.55" />
            <rect x="123" y="56.5" width="54" height="1" fill="#667289" fill-opacity="0.55" />
            <rect x="122" y="58" width="56" height="4" rx="2" fill="var(--SukiPrimaryColor)" fill-opacity="0.25" />
          </g>
        </svg>
        """);

    protected override HyperSource Source => SharedSource;
}
