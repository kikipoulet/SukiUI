using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class SpringLever
{
    private static readonly HyperSource SharedSource = HyperSource.Shared("""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 110 104">
          <defs>
            <linearGradient id="lv-bevel" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0" stop-color="#FFFFFF" stop-opacity="0.4" />
              <stop offset="0.12" stop-color="#FFFFFF" stop-opacity="0" />
              <stop offset="0.85" stop-color="#000000" stop-opacity="0" />
              <stop offset="1" stop-color="#000000" stop-opacity="0.2" />
            </linearGradient>
          </defs>

          <!-- Arc track + center notch -->
          <path d="M 20.59,32.85 A 60,60 0 0 1 89.41,32.85" fill="none" stroke="var(--SkeumoTrack)" stroke-width="6" stroke-linecap="round" />
          <rect x="54" y="18" width="2" height="8" rx="1" fill="var(--SkeumoEdge)" />

          <!-- Dynamic arc fill & knob glow drawn behind lever -->
          <rect data-part="overlay" data-window="true" x="0" y="0" width="110" height="104" />

          <!-- Lever: rod + knob, rotates around pivot (55, 82) -->
          <g data-part="lever" data-pivot="55 82">
            <rect x="51" y="16" width="8" height="58" rx="4" fill="var(--SkeumoRodV)" stroke="var(--SkeumoEdge)" stroke-width="0.6" />
            <circle cx="55" cy="25" r="13" fill="var(--SkeumoDarkKnob)" stroke="var(--SkeumoEdge)" stroke-width="1" filter="url(#skeumo-shadow)" />
            <ellipse cx="50" cy="16.8" rx="4" ry="1.75" fill="#FFFFFF" fill-opacity="0.5" />
          </g>

          <!-- Pivot boss: half-cylinder -->
          <path d="M 55,66 H 55 A 15,15 0 0 1 70,81 V 85 A 3,3 0 0 1 67,88 H 43 A 3,3 0 0 1 40,85 V 81 A 15,15 0 0 1 55,66 Z"
                fill="var(--SkeumoBase)" stroke="var(--SkeumoEdge)" stroke-width="0.8" filter="url(#skeumo-shadow)" />
          <rect x="45" y="67.5" width="20" height="1.6" rx="0.8" fill="#FFFFFF" fill-opacity="0.35" />
          <circle cx="55" cy="78" r="4.5" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.6" />

          <!-- Base plate -->
          <rect x="17" y="86" width="76" height="16" rx="6" fill="var(--SkeumoBase)" stroke="var(--SkeumoEdge)" stroke-width="1" filter="url(#skeumo-shadow)" />
          <rect x="17" y="86" width="76" height="16" rx="6" fill="url(#lv-bevel)" />
          <rect x="22" y="87.5" width="66" height="1.6" rx="0.8" fill="#FFFFFF" fill-opacity="0.35" />
          <circle cx="25.2" cy="94.2" r="2.2" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
          <rect x="22.4" y="93.5" width="5.6" height="1.2" fill="var(--SkeumoSlot)" transform="rotate(25 25.2 94.1)" />
          <circle cx="84.8" cy="94.2" r="2.2" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
          <rect x="82" y="93.5" width="5.6" height="1.2" fill="var(--SkeumoSlot)" transform="rotate(-20 84.8 94.1)" />
        </svg>
        """);

    protected override HyperSource Source => SharedSource;
}
