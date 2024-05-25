---
layout: home

hero:
  name: "SukiUI"
  text: "A Desktop UI Library for Avalonia"
  tagline: Modern, Simple, Animated
  actions:
    - theme: brand
      text: 🚀 Get Started
      link: /documentation/getting-started/introduction
    - theme: alt
      text: 📄 Documentation
      link: /documentation
  image:
    src: ./suki.webp
    alt: SukiUILogo

features:
  - title: 🔧 Flexible Theme Customization
    details: Support seamless switching themes and colors to align with your app's identity.
  - title: 🎞️ Rich Animation
    details: Captivate users with dynamic, fluid animations that make interactions delightful.
  - title: 🖥️ Desktop Cross-platform
    details: Based on AvaloniaUI, achieve broad compatibility with consistent performance on Windows, macOS, and Linux.
---

<style>
:root {
  --vp-home-hero-name-color: transparent;
  --vp-home-hero-name-background: -webkit-linear-gradient(120deg, #ede0b3 50%, #8f4136);
}

@media (min-width: 640px) {
  :root {
    --vp-home-hero-image-filter: blur(56px);
  }
}

@media (min-width: 960px) {
  :root {
    --vp-home-hero-image-filter: blur(68px);
  }
}
</style>