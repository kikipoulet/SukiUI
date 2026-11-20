namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class NetworkMap
{
    private const string Svg = """
        <svg viewBox="0 0 288 222" xmlns="http://www.w3.org/2000/svg">
          <defs>
            <linearGradient id="netScreenGlow" x1="0%" y1="0%" x2="100%" y2="100%">
              <stop offset="0%" stop-color="var(--SukiPrimaryColor)" />
              <stop offset="100%" stop-color="var(--SukiPrimaryColor75)" />
            </linearGradient>

            <linearGradient id="netScreenSheen" x1="0%" y1="0%" x2="50%" y2="100%">
              <stop offset="0%" stop-color="#38FFFFFF" />
              <stop offset="100%" stop-color="#00FFFFFF" />
            </linearGradient>
          </defs>

          <!-- Background window for cables, halos, broadcast ripples, and packets -->
          <rect x="0" y="0" width="288" height="222" fill="none" data-window="true" data-part="networkFx" />

          <!-- ROUTER (111, 26, 66x30) -->
          <g transform="translate(111, 26)">
            <rect x="6" y="0" width="4" height="18" rx="2" fill="var(--SkeumoAxle)" transform="rotate(-14 8 18)" />
            <rect x="56" y="0" width="4" height="18" rx="2" fill="var(--SkeumoAxle)" transform="rotate(14 58 18)" />
            <rect x="0" y="12" width="66" height="18" rx="6" fill="var(--SkeumoBase)"
                  stroke="var(--SkeumoRodH)" stroke-width="1" filter="url(#skeumo-shadow)" />
          </g>
          <circle cx="159" cy="47" r="1.7" fill="var(--SukiPrimaryColor)" opacity="0.55" data-part="routerPower" />
          <circle cx="166" cy="47" r="1.7" fill="var(--SukiPrimaryColor)" data-part="routerLed" />

          <!-- SWITCH (100, 99, 88x26) -->
          <g transform="translate(100, 99)">
            <rect x="0" y="0" width="88" height="26" rx="6" fill="var(--SkeumoBase)"
                  stroke="var(--SkeumoRodH)" stroke-width="1" filter="url(#skeumo-shadow)" />
            <rect x="15" y="16" width="10" height="7" rx="1.5" fill="var(--SkeumoDeepRecess)" stroke="var(--SkeumoAxle)" stroke-width="0.6" />
            <rect x="31" y="16" width="10" height="7" rx="1.5" fill="var(--SkeumoDeepRecess)" stroke="var(--SkeumoAxle)" stroke-width="0.6" />
            <rect x="47" y="16" width="10" height="7" rx="1.5" fill="var(--SkeumoDeepRecess)" stroke="var(--SkeumoAxle)" stroke-width="0.6" />
            <rect x="63" y="16" width="10" height="7" rx="1.5" fill="var(--SkeumoDeepRecess)" stroke="var(--SkeumoAxle)" stroke-width="0.6" />
          </g>
          <circle cx="120" cy="111.1" r="1.7" fill="var(--SukiPrimaryColor)" data-part="ledA" />
          <circle cx="136" cy="111.1" r="1.7" fill="var(--SukiPrimaryColor)" data-part="ledB" />
          <circle cx="152" cy="111.1" r="1.7" fill="var(--SukiPrimaryColor)" data-part="ledR" />
          <circle cx="168" cy="111.1" r="1.7" fill="var(--SukiPrimaryColor)" data-part="ledC" />

          <!-- PC A (12, 66, 56x46) -->
          <g transform="translate(12, 66)">
            <rect x="23" y="33" width="10" height="10" rx="2" fill="var(--SkeumoAxle)" />
            <rect x="14" y="41" width="28" height="5" rx="2.5" fill="var(--SkeumoDarkKnob)" filter="url(#skeumo-shadow)" />
            <rect x="2" y="0" width="52" height="36" rx="6" fill="var(--SkeumoBase)"
                  stroke="var(--SkeumoRodH)" stroke-width="1" filter="url(#skeumo-shadow)" />
            <rect x="6.5" y="4" width="43" height="25" rx="3.5" fill="var(--SkeumoFace)" />
          </g>
          <g data-part="screenA" transform="translate(12, 66)">
            <rect x="6.5" y="4" width="43" height="25" rx="3.5" fill="url(#netScreenGlow)" />
            <rect x="10.5" y="8" width="18" height="3" rx="1.5" fill="#55FFFFFF" />
            <rect x="10.5" y="13" width="28" height="2.5" rx="1.25" fill="#33FFFFFF" />
            <rect x="10.5" y="17.5" width="21" height="2.5" rx="1.25" fill="#33FFFFFF" />
            <rect x="10.5" y="22" width="13" height="2.5" rx="1.25" fill="#33FFFFFF" />
            <rect x="6.5" y="4" width="43" height="25" rx="3.5" fill="url(#netScreenSheen)" />
          </g>

          <!-- PC B (38, 156, 56x46) -->
          <g transform="translate(38, 156)">
            <rect x="23" y="33" width="10" height="10" rx="2" fill="var(--SkeumoAxle)" />
            <rect x="14" y="41" width="28" height="5" rx="2.5" fill="var(--SkeumoDarkKnob)" filter="url(#skeumo-shadow)" />
            <rect x="2" y="0" width="52" height="36" rx="6" fill="var(--SkeumoBase)"
                  stroke="var(--SkeumoRodH)" stroke-width="1" filter="url(#skeumo-shadow)" />
            <rect x="6.5" y="4" width="43" height="25" rx="3.5" fill="var(--SkeumoFace)" />
          </g>
          <g data-part="screenB" transform="translate(38, 156)">
            <rect x="6.5" y="4" width="43" height="25" rx="3.5" fill="url(#netScreenGlow)" />
            <rect x="10.5" y="8" width="18" height="3" rx="1.5" fill="#55FFFFFF" />
            <rect x="10.5" y="13" width="28" height="2.5" rx="1.25" fill="#33FFFFFF" />
            <rect x="10.5" y="17.5" width="21" height="2.5" rx="1.25" fill="#33FFFFFF" />
            <rect x="10.5" y="22" width="13" height="2.5" rx="1.25" fill="#33FFFFFF" />
            <rect x="6.5" y="4" width="43" height="25" rx="3.5" fill="url(#netScreenSheen)" />
          </g>

          <!-- PC C (194, 156, 56x46) -->
          <g transform="translate(194, 156)">
            <rect x="23" y="33" width="10" height="10" rx="2" fill="var(--SkeumoAxle)" />
            <rect x="14" y="41" width="28" height="5" rx="2.5" fill="var(--SkeumoDarkKnob)" filter="url(#skeumo-shadow)" />
            <rect x="2" y="0" width="52" height="36" rx="6" fill="var(--SkeumoBase)"
                  stroke="var(--SkeumoRodH)" stroke-width="1" filter="url(#skeumo-shadow)" />
            <rect x="6.5" y="4" width="43" height="25" rx="3.5" fill="var(--SkeumoFace)" />
          </g>
          <g data-part="screenC" transform="translate(194, 156)">
            <rect x="6.5" y="4" width="43" height="25" rx="3.5" fill="url(#netScreenGlow)" />
            <rect x="10.5" y="8" width="18" height="3" rx="1.5" fill="#55FFFFFF" />
            <rect x="10.5" y="13" width="28" height="2.5" rx="1.25" fill="#33FFFFFF" />
            <rect x="10.5" y="17.5" width="21" height="2.5" rx="1.25" fill="#33FFFFFF" />
            <rect x="10.5" y="22" width="13" height="2.5" rx="1.25" fill="#33FFFFFF" />
            <rect x="6.5" y="4" width="43" height="25" rx="3.5" fill="url(#netScreenSheen)" />
          </g>
        </svg>
        """;
}
