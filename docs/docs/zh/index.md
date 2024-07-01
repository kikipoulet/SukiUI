---
layout: home

hero:
  name: "SukiUI"
  text: "一个 Avalonia 控件库"
  tagline: 扁平，简单，灵动
  actions:
    - theme: brand
      text: 🚀 开始
      link: /zh/documentation/getting-started/introduction
    - theme: alt
      text: 📄 文档
      link: /zh/documentation
  image:
    src: ../suki.webp
    alt: SukiUILogo

features:
  - title: 🔧 灵活地自定义主题
    details: 支持切换明/暗主题和各种主题色
  - title: 🎞️ 丰富的动画
    details: 为大量控件设计了动画，体验更丝滑
  - title: 🖥️ 桌面端跨平台
    details: 在 AvaloniaUI 框架的基础上，SukiUI 在 Windows，macOS，和 Linux 上都能使用
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