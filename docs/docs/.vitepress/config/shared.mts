import { defineConfig } from 'vitepress'

export const shared = defineConfig({
  base: '/SukiUI/',
  title: "SukiUI",

  themeConfig: {
    logo: { src: '/suki.webp', width: 24, height: 24 },
    socialLinks: [
      { icon: 'github', link: 'https://github.com/kikipoulet/SukiUI' }
    ]
  }
})
