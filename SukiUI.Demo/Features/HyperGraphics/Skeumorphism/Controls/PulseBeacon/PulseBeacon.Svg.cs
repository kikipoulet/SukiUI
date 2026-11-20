using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class PulseBeacon
{
    private static readonly HyperSource SharedSource = HyperSource.Shared("""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 120 120">
          <circle cx="60" cy="60" r="50" fill="var(--SkeumoBody)"
                  stroke="var(--SkeumoEdge)" stroke-width="1.2" filter="url(#skeumo-shadow)" />
          <circle cx="60" cy="60" r="36" fill="var(--SkeumoDeepRecess)"
                  stroke="var(--SkeumoEdge)" stroke-width="0.8" />
          <circle data-part="halo" cx="60" cy="60" r="28" fill="none"
                  stroke="var(--SukiPrimaryColor)" stroke-width="4" filter="url(#skeumo-glow)" />
          <g data-part="rotor" data-pivot="60 60">
            <rect x="57" y="28" width="6" height="22" rx="3" fill="var(--SukiPrimaryColor)" />
            <circle cx="60" cy="60" r="10" fill="var(--SkeumoDarkKnob)" stroke="var(--SkeumoEdge)" stroke-width="0.8" />
            <circle cx="60" cy="60" r="4" fill="var(--SukiPrimaryColor)" />
          </g>
        </svg>
        """);

    protected override HyperSource Source => SharedSource;
}
