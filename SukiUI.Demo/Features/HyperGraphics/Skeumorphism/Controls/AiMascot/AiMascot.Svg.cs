using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public sealed partial class AiMascot
{
    private const string MascotSvg = """
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 60 60">
  <defs>
    <linearGradient id="mascot-glass" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0" stop-color="#FCE7F3" stop-opacity=".95"/>
      <stop offset=".5" stop-color="#EDE4FF" stop-opacity=".92"/>
      <stop offset="1" stop-color="#FFE4CC" stop-opacity=".9"/>
    </linearGradient>
    <linearGradient id="mascot-rim" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0" stop-color="#EB2F96"/>
      <stop offset=".5" stop-color="#722ED1"/>
      <stop offset="1" stop-color="#FA541C"/>
    </linearGradient>
    <radialGradient id="mascot-particle" cx=".4" cy=".3" r=".8">
      <stop offset="0" stop-color="#FFE1D2"/>
      <stop offset="1" stop-color="#FA541C"/>
    </radialGradient>
    <circle data-zone="orbit" cx="29" cy="32" r="22.63"/>
    <circle data-zone="rest" cx="36.5" cy="14.2" r="3.3"/>
    <path data-path="scan" d="M17 53.5H41"/>
    <path data-path="doubt" d="M23 8L35 9.5"/>
    <path data-path="sparkA" d="M11 20V13"/>
    <path data-path="sparkB" d="M50 30V23"/>
    <path data-path="sleep" d="M45.3 21.4L50.3 11.4"/>
    <path data-path="confA" d="M29.2 17L15 5"/>
    <path data-path="confB" d="M29.2 17L43 4"/>
    <path data-path="confC" d="M29.2 17L9 18"/>
    <path data-path="confD" d="M29.2 17L50 16"/>
    <path data-path="snowA" d="M14 6L16 24"/>
    <path data-path="snowB" d="M46 4L44 22"/>
    <path data-path="noteA" d="M50 22L54 6"/>
    <path data-path="noteB" d="M52 27L47 9"/>
  </defs>

  <rect data-part="confA" data-pivot="29.2 17" x="28" y="16.3" width="2.4" height="1.4" rx=".5" fill="#EB2F96"/>
  <rect data-part="confB" data-pivot="29.2 17" x="28" y="16.3" width="2.4" height="1.4" rx=".5" fill="#722ED1"/>
  <rect data-part="confC" data-pivot="29.2 17" x="28" y="16.3" width="2.4" height="1.4" rx=".5" fill="#FA541C"/>
  <rect data-part="confD" data-pivot="29.2 17" x="28" y="16.3" width="2.4" height="1.4" rx=".5" fill="#EB2F96"/>
  <circle data-part="ringA" data-pivot="29 32" cx="29" cy="32" r="15.5" fill="none" stroke="#722ED1" stroke-width="1"/>
  <circle data-part="ringB" data-pivot="29 32" cx="29" cy="32" r="15.5" fill="none" stroke="#EB2F96" stroke-width="1"/>

  <circle data-part="body" data-pivot="29 47.5" cx="29" cy="32" r="15.5" fill="url(#mascot-glass)" stroke="url(#mascot-rim)" stroke-width="1.8"/>
  <rect data-part="scan" data-window="true" x="14.4" y="17.4" width="29.2" height="29.2" rx="14.6"/>
  <circle data-part="chill" data-pivot="29 32" cx="29" cy="32" r="15.5" fill="#9FD6FF" fill-opacity=".18"/>
  <ellipse data-part="shine" cx="23" cy="22.8" rx="5.5" ry="2.4" transform="rotate(-28 23 22.8)" fill="#FFFFFF" fill-opacity=".85"/>
  <g data-part="cheeks" data-pivot="29 36.4" fill="#EB2F96" fill-opacity=".22">
    <ellipse cx="20.6" cy="36.4" rx="2.2" ry="1.3"/>
    <ellipse cx="37.4" cy="36.4" rx="2.2" ry="1.3"/>
  </g>

  <g data-part="eyes" data-pivot="29 32" fill="#3B1C6E">
    <rect x="23" y="28.5" width="3.4" height="7" rx="1.7"/>
    <rect x="31.6" y="28.5" width="3.4" height="7" rx="1.7"/>
  </g>
  <g data-part="eyesDoubt" data-pivot="29 32" fill="#3B1C6E">
    <rect x="23" y="28.5" width="3.4" height="7" rx="1.7"/>
    <rect x="31.6" y="30.3" width="3.4" height="4.6" rx="1.7"/>
  </g>
  <path data-part="eyesHappy" data-pivot="29 32" d="M22.8 33.4Q24.7 29.4 26.6 33.4M31.4 33.4Q33.3 29.4 35.2 33.4" fill="none" stroke="#3B1C6E" stroke-width="2" stroke-linecap="round"/>
  <path data-part="eyesSleep" data-pivot="29 32" d="M23 33.4Q24.7 34.8 26.4 33.4M31.6 33.4Q33.3 34.8 35 33.4" fill="none" stroke="#3B1C6E" stroke-width="1.6" stroke-linecap="round"/>
  <path data-part="eyesX" data-pivot="29 32" d="M23 29.6L26.4 33.8M26.4 29.6L23 33.8M31.6 29.6L35 33.8M35 29.6L31.6 33.8" fill="none" stroke="#3B1C6E" stroke-width="1.6" stroke-linecap="round"/>
  <path data-part="spiralL" data-pivot="24.7 32" d="M24.7 32A.7 .7 0 1 1 25.4 32.7A1.4 1.4 0 1 1 24 31.3A2.1 2.1 0 1 1 26.1 33.4" fill="none" stroke="#3B1C6E" stroke-width="1.2" stroke-linecap="round"/>
  <path data-part="spiralR" data-pivot="33.3 32" d="M33.3 32A.7 .7 0 1 1 34 32.7A1.4 1.4 0 1 1 32.6 31.3A2.1 2.1 0 1 1 34.7 33.4" fill="none" stroke="#3B1C6E" stroke-width="1.2" stroke-linecap="round"/>
  <g data-part="shades" data-pivot="29 31" fill="#1D1430">
    <rect x="21" y="28.4" width="7.4" height="5" rx="2"/>
    <rect x="29.6" y="28.4" width="7.4" height="5" rx="2"/>
    <rect x="28" y="29.2" width="2" height=".9"/>
  </g>
  <path data-part="glare" data-pivot="29 30.9" d="M28.6 28.4H30.2L29.2 33.4H27.6Z" fill="#FFFFFF" fill-opacity=".7"/>
  <g data-part="glints" data-pivot="29 32" fill="#FFFFFF" fill-opacity=".8">
    <circle cx="25.4" cy="30" r=".7"/>
    <circle cx="34" cy="30" r=".7"/>
  </g>

  <ellipse data-part="mouth" data-pivot="29 39" cx="29" cy="39" rx="1.9" ry="1.7" fill="#3B1C6E"/>
  <path data-part="frown" data-pivot="29 39.8" d="M26.6 40.2Q29 39 31.4 40.4" fill="none" stroke="#3B1C6E" stroke-width="1.3" stroke-linecap="round"/>

  <rect data-part="teeth" data-pivot="29 39.5" x="26.8" y="38.6" width="4.4" height="1.8" rx=".6" fill="#3B1C6E"/>
  <path data-part="smirk" data-pivot="29 39.6" d="M26.4 39.6Q29.6 41.4 32 38.6" fill="none" stroke="#3B1C6E" stroke-width="1.3" stroke-linecap="round"/>
  <path data-part="drop" data-pivot="42 25.2" d="M42 23.5Q43.6 25.9 42 26.9Q40.4 25.9 42 23.5Z" fill="#9FD6FF"/>
  <g data-part="question" data-pivot="51 27">
    <path d="M48.6 24.6Q48.6 22 51.1 22Q53.6 22 53.6 24.4Q53.6 25.9 52.1 26.7Q51.1 27.3 51.1 28.8" fill="none" stroke="#B37FEB" stroke-width="1.4" stroke-linecap="round"/>
    <circle cx="51.1" cy="31.2" r=".85" fill="#B37FEB"/>
  </g>

  <path data-part="particle" data-pivot="45 16" d="M44.76 12.64L47.08 13.6L48.28 15.84L47.48 18.16L45.24 19.28L42.68 18.48L41.8 16.16L42.68 13.68Z" fill="url(#mascot-particle)"/>

  <path data-part="heart" data-pivot="45 15.6" d="M45 19.2C42 17 40.5 15.6 40.5 14A2.2 2.2 0 0 1 45 13.2A2.2 2.2 0 0 1 49.5 14C49.5 15.6 48 17 45 19.2Z" fill="url(#mascot-particle)"/>
  <path data-part="snowA" data-pivot="14 6" d="M14 4.4V7.6M12.6 5.2L15.4 6.8M12.6 6.8L15.4 5.2" fill="none" stroke="#9FD6FF" stroke-width=".7" stroke-linecap="round"/>
  <path data-part="snowB" data-pivot="46 4" d="M46 2.4V5.6M44.6 3.2L47.4 4.8M44.6 4.8L47.4 3.2" fill="none" stroke="#9FD6FF" stroke-width=".7" stroke-linecap="round"/>
  <g data-part="noteA" data-pivot="45 18">
    <path d="M44.2 19V14L47.2 13V18" fill="none" stroke="#722ED1" stroke-width=".8"/>
    <ellipse cx="43.4" cy="19" rx="1.1" ry=".8" fill="#722ED1"/>
    <ellipse cx="46.4" cy="18" rx="1.1" ry=".8" fill="#722ED1"/>
  </g>
  <g data-part="noteB" data-pivot="47 20">
    <path d="M46.2 21V16L49.2 15V20" fill="none" stroke="#EB2F96" stroke-width=".8"/>
    <ellipse cx="45.4" cy="21" rx="1.1" ry=".8" fill="#EB2F96"/>
    <ellipse cx="48.4" cy="20" rx="1.1" ry=".8" fill="#EB2F96"/>
  </g>
  <circle data-part="trailA" cx="47.5" cy="13.5" r=".9" fill="#FA541C"/>
  <circle data-part="trailB" cx="49.5" cy="11" r=".7" fill="#FA541C"/>
  <circle data-part="trailC" cx="51.3" cy="8.6" r=".5" fill="#FA541C"/>
  <circle data-part="sparkA" data-pivot="11 20" cx="11" cy="20" r="1.3" fill="#FA541C"/>
  <circle data-part="sparkB" data-pivot="50 30" cx="50" cy="30" r="1" fill="#EB2F96"/>
  <path data-part="zA" data-pivot="45.3 21.4" d="M44 20h2.6L44 22.8h2.6" fill="none" stroke="#722ED1" stroke-width=".9" stroke-linecap="round" stroke-linejoin="round"/>
  <path data-part="zB" data-pivot="45 21.1" d="M44 20h2L44 22.2h2" fill="none" stroke="#722ED1" stroke-width=".8" stroke-linecap="round" stroke-linejoin="round"/>
</svg>
""";

    private static readonly HyperSource SharedSource = HyperSource.Shared(MascotSvg);

    protected override HyperSource Source => SharedSource;
}
