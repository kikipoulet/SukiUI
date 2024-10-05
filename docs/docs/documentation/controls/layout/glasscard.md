# Glass Card

A simple Card control that can present any control inside.

## Theme

### Dark Theme

<img src="https://sleekshot.app/api/download/GfUS8bUZT6TQ"/>

### Light Theme

<img src="https://sleekshot.app/api/download/M1xlZe6nmF72"/>

```xml
<GlassCard>
    <!-- Content -->
</GlassCard>
```

## Alternative Style

### Primary

<img src="https://sleekshot.app/api/download/DC48QzG5R1XT"/>

```xml
<GlassCard Classes="Primary">
    <!-- Content -->
</GlassCard>
```

### Accent

<img src="https://sleekshot.app/api/download/cclRRlzglCB8"/>

```xml
<GlassCard Classes="Accent">
    <!-- Content -->
</GlassCard>
```

### Opaque

<img src="https://sleekshot.app/api/download/MW4zpqNj9FLK"/>

```xml
<GlassCard IsOpaque="True">
    <!-- Content -->
</GlassCard>
```

### Interactive

![interactive](https://github.com/user-attachments/assets/0a1ba6ac-b3e0-4eb6-ad7e-f782820a0506)

```xml
<GlassCard IsInteractive="True">
    <!-- Content -->
</GlassCard>
```

## Animations

`GlassCard` are animated with `CompositionAnimations` by the property `IsAnimated` set to `True` by default. `Opacity` changes and `Size` changes of the `GlassCard` are automatically animated.

![animated](https://github.com/user-attachments/assets/b38d4ec2-067a-443c-ac20-65a2f6302920)

## See Also

[Demo: SukiUI.Demo/Features/ControlsLibrary/CardsView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/CardsView.axaml)

[API: Controls/GlassMorphism/GlassCard.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/GlassMorphism/GlassCard.axaml.cs)
