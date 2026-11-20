using System.Globalization;
using System.Text;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class RotaryKnob
{
    private static readonly HyperSource SharedSource = HyperSource.Shared(BuildSvg());

    private static string BuildSvg()
    {
        var knurling = new StringBuilder();
        for (var i = 0; i < 36; i++)
        {
            var d = i * 10 * System.Math.PI / 180.0;
            var x1 = (60.0 + 28.5 * System.Math.Sin(d)).ToString("0.##", CultureInfo.InvariantCulture);
            var y1 = (60.0 - 28.5 * System.Math.Cos(d)).ToString("0.##", CultureInfo.InvariantCulture);
            var x2 = (60.0 + 32.0 * System.Math.Sin(d)).ToString("0.##", CultureInfo.InvariantCulture);
            var y2 = (60.0 - 32.0 * System.Math.Cos(d)).ToString("0.##", CultureInfo.InvariantCulture);
            knurling.Append($"<line x1=\"{x1}\" y1=\"{y1}\" x2=\"{x2}\" y2=\"{y2}\" stroke=\"var(--SkeumoGroove)\" stroke-width=\"1\" />");
        }

        return $$"""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 120 120">
          <!-- Wire terminal -->
          <path d="M 53,108 H 67 V 119 A 3,3 0 0 1 64,122 H 56 A 3,3 0 0 1 53,119 Z"
                fill="var(--SkeumoRodV)" stroke="var(--SkeumoEdge)" stroke-width="0.7" />

          <!-- Base plate -->
          <circle cx="60" cy="60" r="56" fill="var(--SkeumoBase)" stroke="var(--SkeumoEdge)" stroke-width="1" filter="url(#skeumo-shadow)" />
          <circle cx="60" cy="60" r="56" fill="var(--SkeumoBevel)" />

          <!-- Ticks & accent arc drawn behind knob -->
          <rect data-part="arcOverlay" data-window="true" x="0" y="0" width="120" height="120" />

          <!-- Knob body (fixed lighting) -->
          <circle cx="60" cy="60" r="33" fill="var(--SkeumoDarkKnob)" stroke="var(--SkeumoEdge)" stroke-width="1" filter="url(#skeumo-shadow)" />

          <!-- Rotating knurling & indicator -->
          <g data-part="knob" data-pivot="60 60">
            {{knurling}}
            <rect x="58" y="33" width="4" height="13" rx="2" fill="var(--SukiPrimaryColor)" filter="url(#skeumo-glow)" />
          </g>

          <!-- Top cap & specular highlight -->
          <circle cx="60" cy="60" r="19" fill="var(--SkeumoBase)" fill-opacity="0.85" stroke="var(--SkeumoEdge)" stroke-width="0.6" />
          <ellipse cx="48" cy="35.5" rx="10" ry="3.5" fill="#FFFFFF" fill-opacity="0.4" />
        </svg>
        """;
    }

    protected override HyperSource Source => SharedSource;
}
