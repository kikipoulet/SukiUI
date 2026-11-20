using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class Stirrer
{
    private static readonly HyperSource SharedSource = HyperSource.Shared("""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 180 200">
          <defs>
            <linearGradient id="st-refl" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0" stop-color="#FFFFFF" stop-opacity="0.55" />
              <stop offset="0.6" stop-color="#FFFFFF" stop-opacity="0.15" />
              <stop offset="1" stop-color="#FFFFFF" stop-opacity="0" />
            </linearGradient>
          </defs>

          <!-- Cable gland -->
          <path d="M 160,159 H 168 A 4,4 0 0 1 172,163 V 169 A 4,4 0 0 1 168,173 H 160 Z"
                fill="var(--SkeumoRodH)" stroke="var(--SkeumoEdge)" stroke-width="0.7" />

          <!-- Stirrer unit -->
          <rect x="15" y="140" width="150" height="52" rx="12" fill="var(--SkeumoBody)" stroke="var(--SkeumoEdge)" stroke-width="1.2" filter="url(#skeumo-shadow)" />
          <rect x="15" y="140" width="150" height="52" rx="12" fill="var(--SkeumoSheen)" />
          <rect x="28" y="132" width="124" height="10" rx="4" fill="var(--SkeumoRodH)" stroke="var(--SkeumoEdge)" stroke-width="0.7" />
          <rect x="32" y="133.4" width="116" height="1.6" rx="0.8" fill="#FFFFFF" fill-opacity="0.5" />

          <!-- Display window -->
          <rect x="26" y="154" width="58" height="24" rx="7" fill="var(--SkeumoFace)" />

          <!-- Vents -->
          <path d="M 118,156 h 3 v 22 h -3 z M 125,156 h 3 v 22 h -3 z M 132,156 h 3 v 22 h -3 z M 139,156 h 3 v 22 h -3 z M 146,156 h 3 v 22 h -3 z"
                fill="var(--SkeumoDeepRecess)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />

          <!-- Beaker glass back -->
          <path d="M 46,26 H 134 A 4,4 0 0 1 138,30 V 122 A 14,14 0 0 1 124,136 H 56 A 14,14 0 0 1 42,122 V 30 A 4,4 0 0 1 46,26 Z"
                fill="var(--SkeumoGlassBack)" />

          <!-- Beaker liquid, vortex, particles & stir bar -->
          <rect data-part="beaker" data-window="true" x="44" y="28" width="92" height="106" rx="12" />

          <!-- Beaker glass walls, spout, graduations & rim -->
          <path d="M 46,26 H 134 A 4,4 0 0 1 138,30 V 122 A 14,14 0 0 1 124,136 H 56 A 14,14 0 0 1 42,122 V 30 A 4,4 0 0 1 46,26 Z"
                fill="none" stroke="var(--SkeumoGlassRim)" stroke-width="1.6" />
          <path d="M 47,28 H 133 A 3,3 0 0 1 136,31 V 122 A 12,12 0 0 1 124,134 H 56 A 12,12 0 0 1 44,122 V 31 A 3,3 0 0 1 47,28 Z"
                fill="none" stroke="var(--SkeumoGlassInner)" stroke-width="1" />
          <path d="M 42,30 Q 36,26 34,22" fill="none" stroke="var(--SkeumoGlassRim)" stroke-width="2.2" stroke-linecap="round" />
          <rect x="50" y="36" width="5" height="88" rx="2.5" fill="url(#st-refl)" opacity="0.7" />
          <rect x="122" y="54" width="9" height="1" fill="#FFFFFF" fill-opacity="0.35" />
          <rect x="125" y="74" width="6" height="1" fill="#FFFFFF" fill-opacity="0.25" />
          <rect x="122" y="94" width="9" height="1" fill="#FFFFFF" fill-opacity="0.35" />
          <rect x="125" y="114" width="6" height="1" fill="#FFFFFF" fill-opacity="0.25" />
          <rect x="40" y="24" width="100" height="4" rx="2" fill="var(--SkeumoGlassRim)" />
        </svg>
        """);

    protected override HyperSource Source => SharedSource;
}
