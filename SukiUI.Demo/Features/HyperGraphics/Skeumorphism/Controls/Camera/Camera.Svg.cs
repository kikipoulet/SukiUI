using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class Camera
{
    private static readonly HyperSource SharedSource = HyperSource.Shared("""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 332 112">
          <defs>
            <linearGradient id="cam-slide" x1="1" y1="0" x2="0" y2="0">
              <stop offset="0" stop-color="#B9CDF5" />
              <stop offset="0.5" stop-color="#5F7CC0" stop-opacity="0.6" />
              <stop offset="1" stop-color="#2A3A66" stop-opacity="0.25" />
            </linearGradient>
            <linearGradient id="cam-liquid" x1="1" y1="0" x2="0" y2="0">
              <stop offset="0" stop-color="#3A5AA8" />
              <stop offset="0.5" stop-color="#27407F" />
              <stop offset="1" stop-color="#1B2A55" />
            </linearGradient>
          </defs>

          <!-- Aluminium cell fittings -->
          <rect x="45" y="2" width="10" height="9" rx="1.5" fill="var(--SkeumoAluminiumH)" stroke="var(--SkeumoEdge)" stroke-width="0.6" />
          <rect x="47.5" y="9" width="5" height="9" fill="#9FB8FF" fill-opacity="0.35" stroke="#A9BDE6" stroke-opacity="0.5" stroke-width="0.5" />
          <rect x="45" y="101" width="10" height="9" rx="1.5" fill="var(--SkeumoAluminiumH)" stroke="var(--SkeumoEdge)" stroke-width="0.6" />
          <rect x="47.5" y="94" width="5" height="9" fill="#9FB8FF" fill-opacity="0.35" stroke="#A9BDE6" stroke-opacity="0.5" stroke-width="0.5" />

          <!-- Brushed aluminium holder -->
          <rect x="6" y="10" width="86" height="94" rx="9" fill="var(--SkeumoAluminium)" stroke="#6B7588" stroke-width="1.2" />
          <rect x="6" y="10" width="86" height="94" rx="9" fill="var(--SkeumoBevel)" />
          <rect x="9" y="12.5" width="80" height="1.8" rx="0.9" fill="#FFFFFF" fill-opacity="0.6" />
          <rect x="13" y="17" width="74" height="80" rx="6" fill="#3D4656" />
          <rect x="13.5" y="17.5" width="73" height="79" rx="5.8" fill="#06080C" />

          <!-- Chamber back + dynamic light + particles -->
          <rect x="14" y="18" width="72" height="78" rx="5" fill="#0D1220" />
          <rect data-part="chamber" data-window="true" x="14" y="18" width="72" height="78" rx="5" />
          <rect x="14" y="18" width="72" height="78" rx="5" fill="none" stroke="#000000" stroke-opacity="0.6" stroke-width="2" />
          <rect x="14" y="18" width="72" height="78" rx="5" fill="var(--SkeumoGloss)" stroke="#A9BDE6" stroke-opacity="0.3" stroke-width="0.8" />

          <!-- Cell screws -->
          <circle cx="10" cy="14" r="1.8" fill="var(--SkeumoAluminiumH)" stroke="#5A6478" stroke-width="0.5" />
          <circle cx="88" cy="14" r="1.8" fill="var(--SkeumoAluminiumH)" stroke="#5A6478" stroke-width="0.5" />
          <circle cx="10" cy="100" r="1.8" fill="var(--SkeumoAluminiumH)" stroke="#5A6478" stroke-width="0.5" />
          <circle cx="88" cy="100" r="1.8" fill="var(--SkeumoAluminiumH)" stroke="#5A6478" stroke-width="0.5" />

          <!-- Beam layer behind camera -->
          <rect data-part="beamLayer" data-window="true" x="86" y="0" width="90" height="112" />

          <!-- Camera (offset 156, -38) -->
          <g transform="translate(156 -38)">
            <!-- GigE cable -->
            <path d="M156,112 C 170,112 170,138 142,144 S 100,146 68,146" fill="none" stroke="var(--SkeumoRubber)" stroke-width="8" stroke-linecap="round" />
            <path d="M156,112 C 170,112 170,138 142,144 S 100,146 68,146" fill="none" stroke="var(--SkeumoRubberHi)" stroke-width="1.5" stroke-linecap="round" transform="translate(0 -2)" />

            <!-- Fins & collar -->
            <rect x="76" y="46" width="80" height="6" rx="2" fill="var(--SkeumoBody)" />
            <rect x="76" y="138" width="80" height="6" rx="2" fill="var(--SkeumoBody)" />
            <rect x="66" y="68" width="10" height="54" rx="2" fill="var(--SkeumoMetal)" />

            <!-- Cube, short lens & front ring -->
            <rect x="76" y="52" width="80" height="86" rx="12" fill="var(--SkeumoBody)" stroke="var(--SkeumoEdge)" stroke-width="1.2" filter="url(#skeumo-shadow)" />
            <rect x="38" y="64" width="28" height="62" rx="10" fill="var(--SkeumoBody)" stroke="var(--SkeumoEdge)" stroke-width="1" filter="url(#skeumo-shadow)" />
            <rect x="9" y="70" width="32" height="50" rx="8" fill="var(--SkeumoBody)" stroke="var(--SkeumoEdge)" stroke-width="1" filter="url(#skeumo-shadow)" />

            <!-- Cube details -->
            <path d="M84,40 h5 v9 h-5 z M94,40 h5 v9 h-5 z M104,40 h5 v9 h-5 z M114,40 h5 v9 h-5 z M124,40 h5 v9 h-5 z M134,40 h5 v9 h-5 z M144,40 h5 v9 h-5 z"
                  fill="var(--SkeumoSlot)" />
            <path d="M84,58 h3 v20 h-3 z M90,58 h3 v20 h-3 z M96,58 h3 v20 h-3 z M102,58 h3 v20 h-3 z"
                  fill="var(--SkeumoRecess)" stroke="var(--SkeumoEdge)" stroke-width="0.6" />
            <rect x="76" y="52" width="80" height="86" rx="12" fill="var(--SkeumoSheen)" />
            <rect x="80" y="54" width="72" height="2.5" rx="1.2" fill="#FFFFFF" fill-opacity="0.18" />

            <!-- Nameplate -->
            <rect x="88" y="84" width="52" height="24" rx="7" fill="var(--SkeumoDisplay)" stroke="var(--SukiPrimaryColor)" stroke-width="0.8" />

            <!-- LEDs -->
            <circle cx="109" cy="124" r="5" fill="#46D38B" fill-opacity="0.2" />
            <circle cx="117" cy="124" r="5" fill="#7FA3FF" fill-opacity="0.25" />
            <circle cx="109" cy="124" r="2.2" fill="#46D38B" />
            <circle cx="117" cy="124" r="2.2" fill="#7FA3FF" />
            <circle cx="125" cy="124" r="2.2" fill="#3A4558" />

            <!-- Screws -->
            <circle cx="79" cy="132" r="2" fill="#1A1F29" stroke="#667289" stroke-width="0.6" />
            <circle cx="147" cy="132" r="2" fill="#1A1F29" stroke="#667289" stroke-width="0.6" />
            <circle cx="147" cy="58" r="2" fill="#1A1F29" stroke="#667289" stroke-width="0.6" />

            <!-- Connector -->
            <rect x="156" y="100" width="10" height="24" rx="3" fill="var(--SkeumoMetal)" stroke="var(--SkeumoEdge)" stroke-width="1" />
            <rect x="158" y="105" width="6" height="14" rx="1.5" fill="#06080C" />

            <!-- Front ring, knurling, glowing accent ring, lens -->
            <rect x="12" y="72" width="22" height="2" rx="1" fill="#FFFFFF" fill-opacity="0.28" />
            <path d="M20,76 V114 M24,76 V114 M28,76 V114" fill="none" stroke="var(--SkeumoGroove)" stroke-width="1" />
            <rect x="33" y="70" width="3" height="50" rx="1.5" fill="var(--SukiPrimaryColor)" filter="url(#skeumo-glow)" />
            <ellipse cx="13" cy="95" rx="6.5" ry="24" fill="#10151D" stroke="var(--SkeumoRim)" stroke-width="1.6" />
            <ellipse cx="13" cy="95" rx="4.5" ry="20" fill="var(--SkeumoGlassDome)" />
            <ellipse cx="13" cy="95" rx="2.8" ry="12" fill="none" stroke="#DBE8FF" stroke-opacity="0.3" stroke-width="0.8" />
            <ellipse cx="11.5" cy="84" rx="1.3" ry="7" fill="#FFFFFF" fill-opacity="0.55" />

            <!-- C-mount grooves -->
            <path d="M42,66 V124 M44,66 V124" fill="none" stroke="var(--SkeumoGroove)" stroke-width="0.8" />
          </g>

          <ellipse data-part="lensGlow" cx="169" cy="57" rx="4" ry="17" fill="#DBE8FF" />
        </svg>
        """);

    protected override HyperSource Source => SharedSource;
}
