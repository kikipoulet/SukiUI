# Glass Card

一个简单的卡片式控件，可以往里面塞各种内容

## 主题色

### 暗色

<img src="https://sleekshot.app/api/download/GfUS8bUZT6TQ"/>

### 亮色

<img src="https://sleekshot.app/api/download/M1xlZe6nmF72"/>

```xml
<GlassCard>
    <!-- 内容 -->
</GlassCard>
```

## 其他样式

### Primary

<img src="https://sleekshot.app/api/download/DC48QzG5R1XT"/>

```xml
<GlassCard Classes="Primary">
    <!-- 内容 -->
</GlassCard>
```

### Accent

<img src="https://sleekshot.app/api/download/cclRRlzglCB8"/>

```xml
<GlassCard Classes="Accent">
    <!-- 内容 -->
</GlassCard>
```

### Opaque

<img src="https://sleekshot.app/api/download/MW4zpqNj9FLK"/>

```xml
<GlassCard IsOpaque="True">
    <!-- 内容 -->
</GlassCard>
```

### 可交互

![interactive](https://github.com/user-attachments/assets/0a1ba6ac-b3e0-4eb6-ad7e-f782820a0506)

```xml
<GlassCard IsInteractive="True">
    <!-- 内容 -->
</GlassCard>
```

## 动画

通过将 `GlassCard` 在 `CompositionAnimations` 的 `IsAnimated` 属性设为 `True` 以启用动画 (默认启用)，这样 `GlassCard` 就能让其 `Opacity` 和 `Size` 的更改带有过渡效果。

![animated](https://github.com/user-attachments/assets/b38d4ec2-067a-443c-ac20-65a2f6302920)

## 参阅

[Demo: SukiUI.Demo/Features/ControlsLibrary/CardsView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/CardsView.axaml)

[API: Controls/GlassMorphism/GlassCard.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/GlassMorphism/GlassCard.axaml.cs)