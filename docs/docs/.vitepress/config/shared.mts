import { defineConfig } from 'vitepress'

export const shared = defineConfig({
  base: '/SukiUI/',
  title: "SukiUI",

  head: [
    ['link', { rel: 'icon', type: 'image/webp', href: '/suki.webp' }],
  ],

  themeConfig: {
    logo: { src: '/suki.webp', width: 24, height: 24 },
    socialLinks: [
      { icon: 'github', link: 'https://github.com/kikipoulet/SukiUI' }
    ]
  }
})
