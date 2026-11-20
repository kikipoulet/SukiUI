using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class RockerSwitch
{
    private static readonly HyperSource SharedSource = HyperSource.Shared("""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 80 128">
          <defs>
            <linearGradient id="sr-paddle-off" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0" stop-color="#8D99B1" />
              <stop offset="0.48" stop-color="#586278" />
              <stop offset="0.52" stop-color="#3B4558" />
              <stop offset="1" stop-color="#262D3A" />
            </linearGradient>
            <linearGradient id="sr-paddle-on" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0" stop-color="#262D3A" />
              <stop offset="0.48" stop-color="#3B4558" />
              <stop offset="0.52" stop-color="#8D99B1" />
              <stop offset="1" stop-color="#586278" />
            </linearGradient>
          </defs>

          <!-- Terminals -->
          <path d="M 17,110 H 31 V 121 A 3,3 0 0 1 28,124 H 20 A 3,3 0 0 1 17,121 Z" fill="var(--SkeumoRodV)" stroke="var(--SkeumoEdge)" stroke-width="0.7" />
          <circle cx="24" cy="118.5" r="3.5" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
          <path d="M 49,110 H 63 V 121 A 3,3 0 0 1 60,124 H 52 A 3,3 0 0 1 49,121 Z" fill="var(--SkeumoRodV)" stroke="var(--SkeumoEdge)" stroke-width="0.7" />
          <circle cx="56" cy="118.5" r="3.5" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />

          <!-- Wall plate -->
          <rect x="2" y="2" width="76" height="112" rx="12" fill="var(--SkeumoBody)" stroke="var(--SkeumoEdge)" stroke-width="1.1" filter="url(#skeumo-shadow)" />
          <rect x="2" y="2" width="76" height="112" rx="12" fill="var(--SkeumoSheen)" />
          <rect x="8" y="4" width="64" height="2" rx="1" fill="#FFFFFF" fill-opacity="0.18" />

          <circle cx="40" cy="11" r="3" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
          <rect x="37.5" y="10.4" width="5" height="1.2" fill="var(--SkeumoSlot)" />
          <circle cx="40" cy="105" r="3" fill="var(--SkeumoAxle)" stroke="var(--SkeumoEdge)" stroke-width="0.5" />
          <rect x="37.5" y="104.4" width="5" height="1.2" fill="var(--SkeumoSlot)" />

          <!-- Rocker recess -->
          <rect x="17" y="20" width="46" height="76" rx="9" fill="var(--SkeumoDeepRecess)" />

          <!-- Paddle off & on (crossfaded) -->
          <rect data-part="paddleOff" x="20" y="23" width="40" height="70" rx="7"
                fill="url(#sr-paddle-off)" stroke="var(--SkeumoEdge)" stroke-width="0.8" filter="url(#skeumo-shadow)" />
          <rect data-part="paddleOn" x="20" y="23" width="40" height="70" rx="7"
                fill="url(#sr-paddle-on)" stroke="var(--SkeumoEdge)" stroke-width="0.8" />
        </svg>
        """);

    protected override HyperSource Source => SharedSource;
}
