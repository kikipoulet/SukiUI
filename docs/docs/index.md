---
# https://vitepress.dev/reference/default-theme-home-page
layout: home

hero:
  name: "SukiUI"
  text: "A Desktop UI Library for Avalonia"
  tagline: Modern, Simple, Animated
  actions:
    - theme: brand
      text: Get Started
      link: /markdown-examples
    - theme: alt
      text: Documentation
      link: /api-examples
  image:
    src: https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/OIG.N5o-removebg-preview.png
    alt: SukiUILogo

features:
  - title: Feature A
    details: Lorem ipsum dolor sit amet, consectetur adipiscing elit
  - title: Feature B
    details: Lorem ipsum dolor sit amet, consectetur adipiscing elit
  - title: Feature C
    details: Lorem ipsum dolor sit amet, consectetur adipiscing elit
---

<style>
:root {
  --vp-home-hero-name-color: transparent;
  --vp-home-hero-name-background: -webkit-linear-gradient(120deg, #ede0b3 30%, #8f4136);
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