---
name: skeumo-suki
description: Design "Skeumo-Suki" controls for Avalonia + SukiUI, that is, physical objects (valves, levers, tanks, devices, emergency stop buttons) drawn in skeuomorphism but perfectly integrated into SukiUI's glassmorphism. Use it when a SukiUI interface must render a real device or gesture in an immersive and intuitive way without clashing with the rest of the application.
---

# Skeumo-Suki

> Physical objects drawn with the grammar of Suki glass: **neutral material tinted by the theme, a single soft light, the accent as the only energy, and a realism carried by behavior rather than by texture.**

SukiUI is built on glassmorphism: translucent cards, thin luminous edges, an accent color and plenty of air. It is elegant and clear, but abstract. Sometimes you want a screen to give the impression of **handling the thing itself**: pushing a piston, turning a valve, flipping a lever, watching the light of a device change when a parameter is adjusted.

Classic skeuomorphism (leather, wood, bitmap metal, heavy drop shadows) immediately clashes with SukiUI. Skeumo-Suki resolves that conflict. It borrows the **shape** and the **physics** of skeuomorphism, but keeps the **surface**, the **light** and the **palette** of SukiUI.

**Why we do Skeumo-Suki.** In practice it is almost always for the same reason: to have an AI build a very complex control (drawing, light, physics, themes) that a programmer would never have drawn alone, and then use it like any ordinary control. The programmer only sees a simple API. They bind properties, listen to an event, and their logic code stays clear. All the complexity stays locked inside the component. That is why the organization of the code (§6.5) matters as much as the rendering: a superb control whose API is not separated from the visual mechanics has missed its purpose.

**How to read this guide.** It conveys **reasons**, not designs. The numbers, colors and examples it contains describe cases that were encountered: they show how a principle translated at a given moment, not what the next control should become. Facing a new device, apply the reason (recognition, legibility, discretion, consistency with Suki), then tune the values on screen. When a value in the guide and the reason behind it diverge, the reason wins.

---

## Table of contents

1. [When to use it (and when not to)](#1-when-to-use-it-and-when-not-to)
2. [The ten laws](#2-the-ten-laws)
3. [Token system](#3-token-system)
4. [Light model](#4-light-model)
5. [Material recipes](#5-material-recipes)
6. [Anatomy and composition of a card](#6-anatomy-and-composition-of-a-card)
7. [Choosing the right control](#7-choosing-the-right-control)
8. [Behavior and physics](#8-behavior-and-physics)
9. [Light and dark themes, accent color](#9-light-and-dark-themes-accent-color)
10. [Avalonia technical pitfalls](#10-avalonia-technical-pitfalls)
11. [Working method](#11-working-method)
12. [Anti-patterns](#12-anti-patterns)
13. [Validation checklist](#13-validation-checklist)
14. [Complete example: tank and fill / drain lever](#14-complete-example-tank-and-fill--drain-lever)

---

## 1. When to use it (and when not to)

**Use Skeumo-Suki when:**

- the user drives a **real physical device** (pump, motor, camera, valve, actuator) and must recognize it;
- the setting is **continuous, analog or gestural**: a stroke, an angle, a speed, a level;
- the effect of a parameter is **hard to understand in numbers** and easy to understand by seeing it (more light, more noise, more flow);
- the state of the device must be readable **at a glance**: valve open or closed, liquid moving, motor running.

**Do not use it when:**

- the screen is used to enter data, configure or navigate: stay in pure SukiUI;
- the value must be **exact**: a Suki numeric field will always beat a rotary knob;
- the object has no credible physical equivalent. Do not invent a decorative metaphor.


---

## 2. The ten laws

### Law 1: Reuse GlassCard when you need a container
The container is always a `GlassCard` (title, padding, corners, glass badges). The skeuo object floats inside, **with no floor, no decorative scene, no frame**. Parts that truly belong to the device remain welcome: the feet of a tank, the screwed base plate of a lever. What is forbidden is decor: a floor, a drop shadow on the ground, a tray on which the object would sit. If the object has inputs or outputs (pipe, cable), they do not stop dead at the edge: they **melt into the glass** through a long fade (§5.8).

### Law 2: Metal is tinted by the theme
No neutral gray, no warm color. All hard materials (aluminum, steel, technical plastic) use the **cold blue-slate ramp** that matches SukiUI's backgrounds (see §3). The object looks "anodized in the theme".

### Law 3: Matter is neutral, energy is the accent
This is the central law. Everything that is **alive, active, carrying a state** is drawn in `SukiPrimaryColor` and `SukiPrimaryColor75`: a liquid, a light, an open bore, an indicator, the fill of a rail, a glow. Everything else is inert. Consequence: changing the Suki theme color recolors the liquid. **The accent becomes a physical material.**

**When reality imposes its color.** The law serves recognition, it does not override it. Some things have a color everyone knows: a light has a tint, electricity has its own, a charge level is read with warning colors, a gas is not colored like a liquid. There, imposing the accent betrays the object and reads as laziness: we saw indicators, a lamp and a puff of air rejected as long as they were in accent. The test is a question: *what color is the real thing?* If it has one, follow it (§3.5). The accent remains for what has no color of its own: a generic fill, the travel of a rail, the arc of a knob, any liquid.

### Law 4: One light, the same as the glass
The source comes from the top left, everywhere and always. Each part carries a **thin white specular line (1.5 to 2.5 px) at the top**, like the luminous edge of a GlassCard. Shadows are short and soft, like Suki elevations.

### Law 5: The interface's glass becomes the object's glass
Where reality contains glass (tank, tube, porthole, lens, chamber), use transparency, a double rim and a reflection. Glassmorphism no longer decorates: it justifies the object.

### Law 6: Vector, geometric, little detail
No bitmap, no noise, no texture. "Brushed metal" is only a gradient with 4 or 5 stops. Shapes are primitives with **large corner radii**, like Suki cards. Details are **rare and right** (screws, knurling, vents, engraving, indicator light), each drawn at 1 px with alpha. We suggest, we do not render.

### Law 7: Look like the real hardware
Draw **the device the user actually has in front of them**, with its characteristic parts (mount type, connector type, housing shape, technical markings). Recognition makes the immersion.

The **silhouette** comes before everything. A narrow, tall tube reads as a syringe or a test tube, not as a tank. A tank is a wide body with very rounded shoulders, a flange on top where the supply enters, an outlet at the bottom, and feet. If the object cannot be recognized as a flat silhouette, no gradient will save it (§11).

**Each object speaks its own language.** You do not lend a device the metaphor of another: an accumulator that fills like a liquid tank reads as a tank, not as an accumulator. Likewise, a gas is not read with the codes of a liquid (surface, drops, discrete particles): its matter is diffuse, with no sharp edge, and those qualities are what distinguish it. Before drawing, ask what the *own* visual idiom of the thing being represented is.

**The body is skeuomorphic, the display is design.** An indicator built into a device (level, charge, state) is information before it is an object: if it is given the same materiality as the housing (caps, cylinder shading, metal edges per segment), it reads as decor and loses legibility. Keep realism for what is handled and what contains, and treat what is *read* with the restraint of interface graphics.

### Law 8: Two themes, always
Each material exists in a **dark** and a **light** version. In light, the metal becomes pale silver and shadows are tinted midnight blue instead of black. Classic skeuo lives in a single lighting. Here, the object is re-anodized with the theme.

### Law 9: Realism is in the behavior
Springs, detents, inertia, press-in, flow, bubbles, reflections that move. The object is not an illustration, it is a **display of consequences**: when a parameter is adjusted, the matter shows what it does.

### Law 10: Precision stays standard Suki
Skeuo serves gestures. As soon as an **exact value** is needed, go back to Suki components: input field with fixed unit, `NumericUpDown`, `Flat` button, `ToggleSwitch`. The two worlds coexist in the same card.

But add an exact field **only if the user really needs it**. A jog lever has no value to enter: adding a "target level" field by principle weighs the card down for nothing. The gesture is enough when the intent is "more" or "less"; the exact field is required when the intent is "exactly this much".

---

## 3. Token system

Declare materials as **resources named by role**, in `ThemeDictionaries`. Prefix them with a short code specific to the component (`Vx`, `Pm`…) to avoid collisions between cards.

### 3.1 Metal ramp (role by role)

| Role | Dark | Light | Usage |
|---|---|---|---|
| Specular | `#E3E9F4` | `#FFFFFF` | Top of metal gradients, reflections |
| Light | `#8D99B1` | `#E3E9F4` | Lit face |
| Mid | `#586278` | `#D9E0EC` | Top of a housing |
| Body | `#3B4558` | `#CBD3E0` | Main tone of a part |
| Deep | `#2A3242` | `#B0BBCE` | Bottom of a part |
| Shadow | `#171C25` | `#98A4B9` | Bottom of the gradient, inner shadows |
| Edge (`Edge`) | `#667289` | `#A3AEC1` | 0.6 to 1.2 px outline of every part |
| Recess (`Recess`) | `#C705080D` | `#FF5E6A84` | Bores, holes, cavity bottoms |
| Slot / screw | `#0B0F15` | `#3A465C` | Screw slot, vents |

### 3.2 Glass

| Role | Dark | Light |
|---|---|---|
| Glass back (`GlassBack`) | `#B305080D → #591A2233 → #4D1A2233 → #BF05080D` (horizontal) | `#4D6F7F9E → #14FFFFFF → #10FFFFFF → #4D6F7F9E` |
| Outer rim (`GlassRim`) | `#47FFFFFF` | `#73607090` |
| Inner rim (`GlassInner`) | `#59000000` | `#40405070` |
| Tube rim (`TubeRim`) | `#57FFFFFF` | `#FF98A4B9` |
| Tube body (`TubeBg`) | `#B805080D` | `#FFD3DBE8` |
| Tube core (`TubeCore`) | `#00FFFFFF` (useless in dark) | `#FFF4F7FB` |

Glass is darker at the edges than in the center (thickness effect seen from the front): that is what the horizontal gradient of `GlassBack` does, and what the three layers of a tube do (rim → body → core, from widest to narrowest).

**Why the tube body is opaque in light.** A bent tube is drawn as stacked strokes (§5.8): the body is laid **on top of** the rim. If the body is almost transparent, like the back of a tank, the rim shows through over the whole width and the tube becomes a dark, flat blue-gray sausage. In dark, the body is already almost opaque, so the problem does not appear. In light, the body must be opaque and light, and the core even lighter.

### 3.3 Shadows

| Role | Dark | Light |
|---|---|---|
| Part elevation (`Shadow`) | `0 3 6 0 #80000000` | `0 3 6 0 #3D283250` |
| Raised button (`RaisedShadow`) | `0 4 10 0 #66000000, inset 0 1 0 0 #66FFFFFF` | `0 3 8 0 #33283250, inset 0 1 0 0 #99FFFFFF` |
| Small dome (`DomeShadow`) | `0 2 5 0 #99000000` | `0 2 4 0 #4D283250` |
| Dug window (`Inset`) | `inset 0 2 8 0 #CC000000` | `inset 0 1 4 0 #33283250` |
| Dug rail | `inset 0 1 3 0 #AA000000` | `inset 0 1 2 0 #33000000` |

In light, **never pure black in shadows**: use `#283250` with alpha.

### 3.4 Energy (accent)

| Role | Resource |
|---|---|
| Liquid, fill, active light | `SukiPrimaryColor` |
| Liquid edges, end of gradient | `SukiPrimaryColor75` |
| Stop, danger, alarm | `SukiDangerColor` |
| Active text on the object | `SukiText` |
| Inactive text, graduations, legends | `SukiLowText` |
| Separators, neutral edges | `SukiBorderBrush` |

Note: `SukiPrimaryColor` and `SukiText` are **`Color`s**, not brushes. In a `GradientStop`, use `Color="{DynamicResource SukiPrimaryColor}"`. In a `Background` or a `Foreground`, `{DynamicResource SukiPrimaryColor}` also works. In code, see §9.3.

### 3.5 "Real" colors

**The rule.** When the represented object has a strong conventional color, take it instead of the accent (Law 3). **Why:** recognition makes the immersion (Law 7), and a color that contradicts the convention forces the user to translate.

**What bounds it:**
- the real color is reserved for what carries the convention (the light, the level indicator, the matter being talked about). The body of the object stays in the metal ramp;
- it exists in a dark and a light version like any material (Law 8) and stays within the saturation of the rest of the object: an indicator must not be the loudest point of the card;
- a given color has only one meaning across the card (the warning red is not also the color of a wire);
- the red of stop and danger always comes from `SukiDangerColor`.

**Cases encountered** (illustrations, not models): a status light, a level indicator graded by thresholds, the light of a bulb, the dome of an emergency stop.

---

## 4. Light model

```
        ☀  source: top-left (≈ 35 %, 30 %)
         ╲
          ╲   ┌──────────────────────┐ ← thin white specular line 1.5–2.5 px
           ╲  │░░░░  sharp glint ░░  │ ← diagonal reflection with a hard edge
              │                      │
              │         body         │ ← vertical gradient light → dark
              │                      │
              └──────────────────────┘
                 ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓      ← short, soft shadow (0 3 6)
```

1. **Directional gradients.**
   - Housings: vertical, from light (top) to dark (bottom).
   - Horizontal cylindrical parts (rod, handle, metal tube): vertical, with the brightest point around 28-35 %.
   - Vertical cylindrical parts: the same, horizontally.
2. **Round parts** (knob, button, axle, lens): `RadialGradientBrush GradientOrigin="35%,30%"`.
3. **Specular line**: a `Border` 1.5 to 2.5 px high, rounded corners, `#2E–#99FFFFFF`, placed 1 to 2 px below the top edge and slightly narrower than the part.
4. **Glass reflection**: a diagonal gradient with a **hard cut**, the signature of glassmorphism:
   ```xml
   <LinearGradientBrush StartPoint="0%,0%" EndPoint="100%,100%">
       <GradientStop Offset="0"    Color="#38FFFFFF" />
       <GradientStop Offset="0.35" Color="#0AFFFFFF" />
       <GradientStop Offset="0.36" Color="#00FFFFFF" />  <!-- hard edge -->
       <GradientStop Offset="1"    Color="#00FFFFFF" />
   </LinearGradientBrush>
   ```
5. **Bevel** (edge of a plate): a layer on top with a vertical gradient `#B3FFFFFF` (0) → transparent (0.1) → transparent (0.9) → `#4D000000` (1).
6. **Emitted light**: always in accent, as a blurred colored `BoxShadow` (see §5.11). Emitted light is the only "neon" thing about the object.

---

## 5. Material recipes

All recipes are in Avalonia XAML. The `Xx…` keys are to be renamed with your component's prefix.

### 5.1 Metal housing (body of a device)

```xml
<!-- Dark -->
<LinearGradientBrush x:Key="XxBody" StartPoint="0%,0%" EndPoint="0%,100%">
    <GradientStop Offset="0"    Color="#FF586278" />
    <GradientStop Offset="0.12" Color="#FF3B4558" />
    <GradientStop Offset="0.55" Color="#FF2A3242" />
    <GradientStop Offset="1"    Color="#FF171C25" />
</LinearGradientBrush>
<!-- Light -->
<LinearGradientBrush x:Key="XxBody" StartPoint="0%,0%" EndPoint="0%,100%">
    <GradientStop Offset="0"    Color="#FFFFFFFF" />
    <GradientStop Offset="0.12" Color="#FFF1F4F9" />
    <GradientStop Offset="0.55" Color="#FFD9E0EC" />
    <GradientStop Offset="1"    Color="#FFB0BBCE" />
</LinearGradientBrush>
```

```xml
<Border Width="80" Height="86" CornerRadius="12"
        Background="{DynamicResource XxBody}"
        BorderBrush="{DynamicResource XxEdge}" BorderThickness="1.2"
        BoxShadow="{DynamicResource XxShadow}" />
<!-- diagonal reflection on top -->
<Border Width="80" Height="86" CornerRadius="12" Background="{StaticResource XxSheen}" IsHitTestVisible="False" />
<!-- specular line -->
<Border Width="72" Height="2.5" CornerRadius="1.2" Background="#2EFFFFFF" Margin="0,2,0,0" VerticalAlignment="Top" />
```

### 5.2 Machined / chrome metal (rod, handle, collar)

For a **horizontal** part, a vertical gradient (and the reverse for a vertical part):

```xml
<!-- Dark -->
<LinearGradientBrush x:Key="XxRod" StartPoint="0%,0%" EndPoint="0%,100%">
    <GradientStop Offset="0"    Color="#FF3B4558" />
    <GradientStop Offset="0.28" Color="#FFE3E9F4" />  <!-- line of light -->
    <GradientStop Offset="0.62" Color="#FF8590A6" />
    <GradientStop Offset="1"    Color="#FF262D3A" />
</LinearGradientBrush>
<!-- Light -->
<LinearGradientBrush x:Key="XxRod" StartPoint="0%,0%" EndPoint="0%,100%">
    <GradientStop Offset="0"    Color="#FF98A4B9" />
    <GradientStop Offset="0.28" Color="#FFFFFFFF" />
    <GradientStop Offset="0.62" Color="#FFCBD3E0" />
    <GradientStop Offset="1"    Color="#FF8D99B1" />
</LinearGradientBrush>
```

It is the same material as the ramp of §3.1: chrome is simply the full ramp squeezed onto a small height.

### 5.3 Brushed aluminum (plate, support)

A diagonal gradient with 5 **non-monotonic** stops. The light/dark alternations are enough to suggest brushing:

```xml
<LinearGradientBrush x:Key="XxAlu" StartPoint="0%,0%" EndPoint="100%,100%">
    <GradientStop Offset="0"    Color="#FFE8ECF2" />
    <GradientStop Offset="0.25" Color="#FFB9C1CE" />
    <GradientStop Offset="0.5"  Color="#FFD9DEE7" />
    <GradientStop Offset="0.78" Color="#FF9AA4B4" />
    <GradientStop Offset="1"    Color="#FF7C8799" />
</LinearGradientBrush>
```

Add the bevel layer of §4.5 on top.

### 5.4 Knob, button, axle

```xml
<!-- Dark: dark knob -->
<RadialGradientBrush x:Key="XxKnob" GradientOrigin="35%,30%">
    <GradientStop Offset="0"   Color="#FF7C88A3" />
    <GradientStop Offset="0.4" Color="#FF3B4558" />
    <GradientStop Offset="0.8" Color="#FF222A38" />
    <GradientStop Offset="1"   Color="#FF141922" />
</RadialGradientBrush>
<!-- Dark: light metal axle -->
<RadialGradientBrush x:Key="XxAxle" GradientOrigin="35%,30%">
    <GradientStop Offset="0"    Color="#FFE3E9F4" />
    <GradientStop Offset="0.4"  Color="#FF8D99B1" />
    <GradientStop Offset="0.75" Color="#FF586278" />
    <GradientStop Offset="1"    Color="#FF2A3242" />
</RadialGradientBrush>
```

```xml
<Grid Width="30" Height="30">
    <Border CornerRadius="15" Background="{DynamicResource XxKnob}"
            BorderBrush="{DynamicResource XxEdge}" BorderThickness="1"
            BoxShadow="0 3 6 0 #66000000" />
    <!-- highlight dot -->
    <Ellipse Width="9" Height="4" Margin="5,3,0,0" HorizontalAlignment="Left" VerticalAlignment="Top" Fill="#80FFFFFF" />
</Grid>
```

**"Pearl" slider thumb** (hinge between Suki and skeuo): a light sphere with a luminous accent dot at the center.

```xml
<RadialGradientBrush x:Key="XxThumb" GradientOrigin="35%,28%">
    <GradientStop Offset="0"   Color="#FFF4F7FC" />
    <GradientStop Offset="0.6" Color="#FFB9C4D8" />
    <GradientStop Offset="1"   Color="#FF6F7C93" />
</RadialGradientBrush>

<Border Width="22" Height="22" CornerRadius="11" Background="{DynamicResource XxThumb}" BoxShadow="0 3 7 0 #AA000000">
    <Border Name="ThumbDot" Width="7" Height="7" CornerRadius="4" Background="{DynamicResource SukiPrimaryColor}" />
</Border>
```

### 5.5 Rubber (gasket, stop)

```xml
<LinearGradientBrush x:Key="XxRubber" StartPoint="0%,0%" EndPoint="0%,100%">
    <GradientStop Offset="0"   Color="#FF4A5468" />
    <GradientStop Offset="0.3" Color="#FF262D3A" />
    <GradientStop Offset="1"   Color="#FF0B0F15" />
</LinearGradientBrush>
```

Add one or two horizontal 1 px ribs (`#8C667289`) and, if the part is in contact with a liquid, an accent band at 25 % opacity on the liquid side.

### 5.6 Glass (tank, barrel, porthole)

Glass is built **in layers**, from the back to the front:

```xml
<!-- 1. glass back -->
<Border Width="60" Height="200" CornerRadius="14,14,4,4" Background="{DynamicResource XxGlassBack}" IsHitTestVisible="False" />
<!-- 2. content (liquid, see 5.7) -->
<!-- 3. outer rim -->
<Border Width="60" Height="200" CornerRadius="14,14,4,4"
        BorderBrush="{DynamicResource XxGlassRim}" BorderThickness="1.6" IsHitTestVisible="False" />
<!-- 4. inner rim, inset by 2 px -->
<Border Margin="2" Width="56" Height="196" CornerRadius="12,12,3,3"
        BorderBrush="{DynamicResource XxGlassInner}" BorderThickness="1" IsHitTestVisible="False" />
<!-- 5. main vertical reflection (light side) -->
<Border Width="6" Height="168" CornerRadius="3" Opacity="0.7" IsHitTestVisible="False">
    <Border.Background>
        <LinearGradientBrush StartPoint="0%,0%" EndPoint="0%,100%">
            <GradientStop Offset="0"   Color="#8CFFFFFF" />
            <GradientStop Offset="0.6" Color="#26FFFFFF" />
            <GradientStop Offset="1"   Color="#00FFFFFF" />
        </LinearGradientBrush>
    </Border.Background>
</Border>
<!-- 6. thin secondary reflection (opposite side) -->
<Border Width="2" Height="150" CornerRadius="1" Background="#29FFFFFF" IsHitTestVisible="False" />
<!-- 7. top-edge ellipse (opening seen in perspective) -->
<Ellipse Width="40" Height="7" Fill="#24FFFFFF" IsHitTestVisible="False" />
```

For a **round porthole**, the same logic: a dark `RadialGradientBrush` disc (`#1B2538 → #0C121C → #05080D`, origin 38 %,30 %), two rim ellipses, and a thick white arc (`StrokeThickness="3"`, `#80FFFFFF`) at the top left.

### 5.7 Liquid (energy)

The liquid **is** the accent. A horizontal gradient `75 → 100 → 75` to suggest cylindrical volume, then three layers:

```xml
<Border Name="Liquid" CornerRadius="12,12,0,0" Opacity="0.95" BoxShadow="inset 0 0 12 0 #40FFFFFF">
    <Border.Background>
        <LinearGradientBrush StartPoint="0%,0%" EndPoint="100%,0%">
            <GradientStop Offset="0"   Color="{DynamicResource SukiPrimaryColor75}" />
            <GradientStop Offset="0.5" Color="{DynamicResource SukiPrimaryColor}" />
            <GradientStop Offset="1"   Color="{DynamicResource SukiPrimaryColor75}" />
        </LinearGradientBrush>
    </Border.Background>
    <Grid>
        <!-- luminous core -->
        <Border Width="26" HorizontalAlignment="Center" IsHitTestVisible="False">
            <Border.Background>
                <LinearGradientBrush StartPoint="0%,0%" EndPoint="100%,0%">
                    <GradientStop Offset="0"   Color="#00FFFFFF" />
                    <GradientStop Offset="0.5" Color="#47FFFFFF" />
                    <GradientStop Offset="1"   Color="#00FFFFFF" />
                </LinearGradientBrush>
            </Border.Background>
        </Border>
        <!-- vertical lighting: light surface, dark bottom (resource per theme) -->
        <Border Background="{DynamicResource XxLiquidShade}" IsHitTestVisible="False" />
        <!-- caustic where it touches a wall or a gasket -->
        <Border Height="10" VerticalAlignment="Bottom" IsHitTestVisible="False">
            <Border.Background>
                <LinearGradientBrush StartPoint="0%,0%" EndPoint="0%,100%">
                    <GradientStop Offset="0" Color="#00FFFFFF" />
                    <GradientStop Offset="1" Color="#47FFFFFF" />
                </LinearGradientBrush>
            </Border.Background>
        </Border>
        <!-- suspended particles or bubbles, driven by code -->
        <Canvas Name="LiquidParticles" IsHitTestVisible="False" />
    </Grid>
</Border>
```

`XxLiquidShade` (dark): `#38FFFFFF` (0) → `#00FFFFFF` (0.12) → `#00000000` (0.8) → `#47000000` (1). In light, replace the black at the bottom with `#26405070`.

**Cut the content to the shape of the container.** As soon as the container has rounded shoulders or bottom, do not give a `CornerRadius` to the liquid: place the liquid, the falling stream and the particles in an inner `Border` with `ClipToBounds="True"` and the radius of the inner glass. The liquid is then a plain rectangle anchored at the bottom, and it is the clip that follows the shape of the tank, whatever the level:

```xml
<Border Canvas.Left="36" Canvas.Top="36" Width="96" Height="126" CornerRadius="32,32,14,14" ClipToBounds="True">
    <Canvas Width="96" Height="126">
        <Line Name="FallStream" ... />            <!-- stream falling from the inlet to the fill -->
        <Border Name="Liquid" Width="96" Height="0" Canvas.Top="126" ... />  <!-- height driven in code -->
        <Canvas Name="TankParticles" />           <!-- suspended matter -->
    </Canvas>
</Border>
```

Keep an **air reserve** under the dome: the 100 % level does not fill the whole interior (for instance 112 useful px out of 126).

### 5.8 Tube or pipe that melts into the card

A real pipe is **bent**: it leaves a flange, goes down, turns and heads toward the edge of the card. So it is drawn with a single `Path` (lines + a `Q` curve for the bend) stroked several times, from the thickest to the thinnest:

```xml
<Canvas Width="288" Height="200" IsHitTestVisible="False">
    <Canvas.OpacityMask>
        <!-- long, soft fade to the right (see the rules below) -->
        <LinearGradientBrush StartPoint="0%,0%" EndPoint="100%,0%">
            <GradientStop Offset="0.55" Color="#FFFFFFFF" />
            <GradientStop Offset="0.98" Color="#00FFFFFF" />
        </LinearGradientBrush>
    </Canvas.OpacityMask>
    <!-- 1. rim  2. body  3. core: same geometry, decreasing thickness -->
    <Path Data="M 96,160 L 96,180 Q 96,192 108,192 L 288,192" Stroke="{DynamicResource XxTubeRim}"
          StrokeThickness="12" StrokeLineCap="Round" StrokeJoin="Round" />
    <Path Data="M 96,160 L 96,180 Q 96,192 108,192 L 288,192" Stroke="{DynamicResource XxTubeBg}"
          StrokeThickness="9.6" StrokeLineCap="Round" StrokeJoin="Round" />
    <Path Data="M 96,160 L 96,180 Q 96,192 108,192 L 288,192" Stroke="{DynamicResource XxTubeCore}"
          StrokeThickness="4.5" StrokeLineCap="Round" StrokeJoin="Round" />
    <!-- 4. energy: opacity driven by the flow -->
    <Path Name="OutletFlow" Data="M 96,160 L 96,180 Q 96,192 108,192 L 288,192"
          Stroke="{DynamicResource SukiPrimaryColor}" StrokeThickness="6"
          StrokeLineCap="Round" StrokeJoin="Round" Opacity="0" />
    <!-- 5. specular line on the right part, 2.8 px above the axis -->
    <Path Data="M 110,189.2 L 284,189.2" Stroke="#40FFFFFF" StrokeThickness="1.4" StrokeLineCap="Round" />
    <!-- 6. suspended particles (§8.3), created in code -->
    <Canvas Name="OutletParticles" />
</Canvas>
```

Some rules:

- **The fade is long.** It spreads over the whole straight length of the pipe, with an intermediate stop for a soft curve, and not over a few pixels. Below about 50 to 60 px, the eye reads a sharp cut rather than a fade. For an inlet on the left: `0.02 → #00`, `0.12 → #59`, `0.24 → #FF`.
- **The fade reaches zero where the object meets the edge of the card.** A fade that ends before the edge leaves a pipe that seems to stop in the void; a fade confined inside the padding reads as a cut. The scene must therefore extend to the edge of the glass (negative margins on the `Panel` if needed), and the length of the fade is judged in pixels on screen, not in the code.
- **Everything the pipe contains follows the same curve** (particles, reflections), otherwise the content sticks out of the fade and reveals the cut.
- **Check the fade on screen.** A technique that should work (an opacity mask on the layer) did not produce the expected effect in one case: a gradient carried directly by the pipe's brushes is an alternative. It is the visible result that decides, not the technique.
- **The ends are hidden.** A `StrokeLineCap="Round"` sticks out past the end point by **half the thickness**. A tube that enters a flange must stop early enough for its round end to stay hidden under the flange (for a thickness of 12, stop it at least 6 px before the bottom of the flange).
- **The pipe passes behind or under the parts it crosses.** An outlet that passes near a foot goes lower than the foot and passes under it. A pipe that passes under another control is simply drawn before it.

For a very simple straight pipe, a `Border` variant is enough:

```xml
<Canvas Width="300" Height="60" IsHitTestVisible="False">
    <Canvas.OpacityMask>
        <LinearGradientBrush StartPoint="0%,0%" EndPoint="100%,0%">
            <GradientStop Offset="0.05" Color="#00FFFFFF" />
            <GradientStop Offset="0.23" Color="#FFFFFFFF" />
            <GradientStop Offset="0.77" Color="#FFFFFFFF" />
            <GradientStop Offset="0.95" Color="#00FFFFFF" />
        </LinearGradientBrush>
    </Canvas.OpacityMask>
    <!-- glass sheath -->
    <Border Canvas.Top="15" Width="300" Height="14" CornerRadius="7"
            Background="{DynamicResource XxTubeBg}" BorderBrush="{DynamicResource XxTubeRim}" BorderThickness="1.2" />
    <!-- accent channel: opacity 0.7 if the path is active, 0.1 otherwise -->
    <Border Name="ChannelA" Canvas.Left="4" Canvas.Top="18.5" Width="146" Height="7" CornerRadius="3.5" Opacity="0.7">
        <Border.Background>
            <LinearGradientBrush StartPoint="0%,0%" EndPoint="0%,100%">
                <GradientStop Offset="0"   Color="{DynamicResource SukiPrimaryColor75}" />
                <GradientStop Offset="0.5" Color="{DynamicResource SukiPrimaryColor}" />
                <GradientStop Offset="1"   Color="{DynamicResource SukiPrimaryColor75}" />
            </LinearGradientBrush>
        </Border.Background>
    </Border>
    <!-- specular line of the tube -->
    <Border Canvas.Left="10" Canvas.Top="16.6" Width="280" Height="2" CornerRadius="1" Background="#59FFFFFF" />
</Canvas>
```

The mask is what gives the impression that the object **comes out of the interface**. Without it, the tube looks cut with a knife. The mask is relative to the bounds of the `Canvas` that carries it: give that `Canvas` the size of the scene so that the fade ends exactly at the edge of the control.

Do not use `StrokeDashArray` to make the liquid "move": regular dashes form a predictable, mechanical queue. Movement goes through particles (§8.3).

### 5.9 Small details (to be dosed)

| Detail | Recipe |
|---|---|
| Screw | `Ellipse` 3.6 to 4 px, fill `XxScrew` or a chrome gradient, outline `XxEdge` 0.5 px. With a slot: `Border` 6×1.5 px in `XxSlot`, rotated by 20 to 30° (never aligned at 0°). |
| Knurling | 3 to 5 parallel 1 px lines in `XxGroove` (`#B30B0F15` dark, `#554A5670` light). |
| Vents, slots | `Path` of small rectangles with a `#05070A → #232B38` gradient (light: `#8F9BB0 → #C3CCDB`), outline `XxEdge` 0.6 px. |
| LED indicator | Two stacked ellipses: a 10 px halo at 20-25 % alpha, then a solid 4.4 px dot. Off: `#3A4558` dot. |
| Engraving | `TextBlock` 4.5 to 6 px, `LetterSpacing` 0.6 to 1.2, color `XxEngrave` (`#8FA4CC` / `#5A6580`), `Tag="NoTranslate"` if the app auto-translates. |
| Nameplate | Dark `Border` `CornerRadius="7"`, `SukiPrimaryColor` rim 0.8 px, name in 8.5 px SemiBold `LetterSpacing="1.2"`, subtitle in accent 5.5 px. |
| Connector | Small `XxMetal` metal block + a `#06080C` black recess inside. |
| Cable | Thick `Path` (8 px, `StrokeLineCap="Round"`) in `XxCable`, doubled by a 1.5 px light line offset by -2 px in Y. |

Rule: **3 to 6 details per object**. Each detail must say something true about the device (it is screwed, it ventilates, it is connected, it is powered). Do not screw everything: screws on the flange and on the base plate of a lever are justified, screws on every foot bracket add noise. Likewise, thick hoops across a glass tank hide what matters (the liquid): prefer thin graduations.

### 5.10 Dug window (light variant)

When the object is a **screen** (curve, image, stream), you do not draw a device: you dig into the glass.

```xml
<Border Width="268" Height="112" CornerRadius="14" ClipToBounds="True"
        Background="{DynamicResource XxWindowBg}"
        BorderBrush="{DynamicResource SukiBorderBrush}" BorderThickness="1">
    <Grid>
        <!-- content -->
        <!-- very light window-glass reflection -->
        <Border IsHitTestVisible="False">
            <Border.Background>
                <LinearGradientBrush StartPoint="0%,0%" EndPoint="100%,100%">
                    <GradientStop Offset="0"   Color="#0FFFFFFF" />
                    <GradientStop Offset="0.4" Color="#00FFFFFF" />
                </LinearGradientBrush>
            </Border.Background>
        </Border>
        <!-- depth: inner shadow, LAST so that it goes over the content -->
        <Border CornerRadius="14" BoxShadow="{DynamicResource XxInset}" IsHitTestVisible="False" />
    </Grid>
</Border>
```

`XxWindowBg`: `#0A0E14` in dark, `#EEF2F8` in light. Aiming marks in accent at 35 % opacity in the corners complete the effect well.

### 5.11 Accent light (glow)

An emitted light is a `BoxShadow` colored with the accent, **set in code**, because a `BoxShadow` cannot reference a `DynamicResource` (see §10):

```csharp
private void ApplyAccentGlows()
{
    if (!this.TryFindResource("SukiPrimaryColor", ActualThemeVariant, out var res) || res is not Color accent) return;
    BoxShadows Glow(double blur, byte alpha, double spread = 0) =>
        new(new BoxShadow { Blur = blur, Spread = spread, Color = Color.FromArgb(alpha, accent.R, accent.G, accent.B) });

    this.Get<Border>("ThumbDot").BoxShadow = Glow(6, 0xFF);   // luminous dot
    this.Get<Border>("RailFill").BoxShadow = Glow(8, 0x66);   // rail fill
    // two-layer halo around a liquid
    this.Get<Border>("LiquidAura").BoxShadow = new BoxShadows(
        new BoxShadow { Blur = 18, Spread = 4,  Color = Color.FromArgb(0xB0, accent.R, accent.G, accent.B) },
        new[] { new BoxShadow { Blur = 42, Spread = 10, Color = Color.FromArgb(0x55, accent.R, accent.G, accent.B) } });
}
```

Place the halo **under** the object (z-order) and in a parent with `ClipToBounds="False"`. Otherwise the glow is cut dead.

### 5.12 Emergency stop button

A fixed metal collar and a backlit red glass dome. The glow is **outside** the collar so as not to be clipped.

```xml
<Button Classes="Void" Padding="0" ClipToBounds="False" ToolTip.Tip="Stop">
    <Grid ClipToBounds="False">
        <Border Width="46" Height="46" CornerRadius="23" BoxShadow="0 0 16 3 #59E5484D" IsHitTestVisible="False" />
        <Border Width="56" Height="56" CornerRadius="28"
                Background="{DynamicResource XxBase}" BorderBrush="{DynamicResource XxEdge}" BorderThickness="1"
                BoxShadow="{DynamicResource XxRaisedShadow}">
            <Grid Name="StopDome"> <!-- only this part moves when pressed -->
                <Border Width="44" Height="44" CornerRadius="22" BoxShadow="{DynamicResource XxDomeShadow}">
                    <Border.Background>
                        <RadialGradientBrush GradientOrigin="38%,30%">
                            <GradientStop Offset="0"   Color="#FFFF8A8A" />
                            <GradientStop Offset="0.5" Color="{DynamicResource SukiDangerColor}" />
                            <GradientStop Offset="1"   Color="#FF7A1218" />
                        </RadialGradientBrush>
                    </Border.Background>
                </Border>
                <Ellipse Width="26" Height="12" Margin="0,9,0,0" VerticalAlignment="Top" IsHitTestVisible="False">
                    <Ellipse.Fill>
                        <LinearGradientBrush StartPoint="0%,0%" EndPoint="0%,100%">
                            <GradientStop Offset="0" Color="#8CFFFFFF" />
                            <GradientStop Offset="1" Color="#00FFFFFF" />
                        </LinearGradientBrush>
                    </Ellipse.Fill>
                </Ellipse>
                <TextBlock Text="STOP" FontSize="10" FontWeight="Bold" Foreground="White"
                           HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0,4,0,0" />
            </Grid>
        </Border>
    </Grid>
</Button>
```

### 5.13 Lever base

A lever never comes out of a plain circle placed in the void. It rests on a **solid, horizontal base**: a screwed metal base plate, on which sits a half-cylindrical **boss** from which the rod emerges. The chrome axle remains visible at the center of the boss.

```xml
<!-- the lever rod is drawn BEFORE: it emerges from the boss -->
<!-- boss: half-cylinder resting on the plate -->
<Border Canvas.Left="40" Canvas.Top="66" Width="30" Height="22" CornerRadius="15,15,3,3"
        Background="{DynamicResource XxBase}" BorderBrush="{DynamicResource XxEdge}" BorderThickness="0.8"
        BoxShadow="{DynamicResource XxShadow}" />
<Border Canvas.Left="45" Canvas.Top="67.5" Width="20" Height="1.6" CornerRadius="0.8" Background="#59FFFFFF" />
<Ellipse Canvas.Left="50.5" Canvas.Top="73.5" Width="9" Height="9" Fill="{DynamicResource XxAxle}"
         Stroke="{DynamicResource XxEdge}" StrokeThickness="0.6" />
<!-- plate: horizontal block, bevel, specular line, two screws -->
<Border Canvas.Left="17" Canvas.Top="86" Width="76" Height="16" CornerRadius="6"
        Background="{DynamicResource XxBase}" BorderBrush="{DynamicResource XxEdge}" BorderThickness="1"
        BoxShadow="{DynamicResource XxShadow}" />
<Border Canvas.Left="17" Canvas.Top="86" Width="76" Height="16" CornerRadius="6">   <!-- bevel §4.5 -->
    <Border.Background>
        <LinearGradientBrush StartPoint="0%,0%" EndPoint="0%,100%">
            <GradientStop Offset="0" Color="#66FFFFFF" />
            <GradientStop Offset="0.12" Color="#00FFFFFF" />
            <GradientStop Offset="0.85" Color="#00000000" />
            <GradientStop Offset="1" Color="#33000000" />
        </LinearGradientBrush>
    </Border.Background>
</Border>
<Border Canvas.Left="22" Canvas.Top="87.5" Width="66" Height="1.6" CornerRadius="0.8" Background="#59FFFFFF" />
<Ellipse Canvas.Left="23" Canvas.Top="92" Width="4.4" Height="4.4" Fill="{DynamicResource XxAxle}"
         Stroke="{DynamicResource XxEdge}" StrokeThickness="0.5" />
<Border Canvas.Left="22.4" Canvas.Top="93.5" Width="5.6" Height="1.2" Background="{DynamicResource XxSlot}"
        RenderTransform="rotate(25deg)" />   <!-- slots never aligned, different angles from one screw to the next -->
```

### 5.14 Tank feet

**Slanted legs** (trapezoid `Path`s), wider at the top than at the bottom. They are fixed to the wall by a metal **bracket** and rest on a flat **pad**. The legs are drawn behind the glass, and the brackets in front.

```xml
<!-- leg: trapezoid with a horizontal metal gradient + a line of light on the left edge -->
<Path Data="M 47,146 L 57,146 L 50,177 L 40,177 Z" Fill="{DynamicResource XxLeg}"
      Stroke="{DynamicResource XxEdge}" StrokeThickness="0.6" StrokeJoin="Round" />
<Path Data="M 49,150 L 46.5,161" Stroke="#4DFFFFFF" StrokeThickness="1.2" StrokeLineCap="Round" />
<!-- pad -->
<Border Canvas.Left="33" Canvas.Top="175" Width="24" Height="6" CornerRadius="3"
        Background="{DynamicResource XxBase}" BorderBrush="{DynamicResource XxEdge}" BorderThickness="0.8"
        BoxShadow="{DynamicResource XxShadow}" />
<Border Canvas.Left="36" Canvas.Top="175.8" Width="18" Height="1.2" CornerRadius="0.6" Background="#59FFFFFF" />
<!-- bracket, in front of the glass, without a screw -->
<Border Canvas.Left="42" Canvas.Top="141" Width="20" Height="9" CornerRadius="3"
        Background="{DynamicResource XxBase}" BorderBrush="{DynamicResource XxEdge}" BorderThickness="0.8"
        BoxShadow="{DynamicResource XxShadow}" />
```

`XxLeg`: a horizontal gradient `#2A3242 → #8D99B1 (0.3) → #3B4558 (0.6) → #171C25` in dark, `#B0BBCE → #FFFFFF (0.3) → #CBD3E0 (0.6) → #98A4B9` in light.

---

## 6. Anatomy and composition of a card

### 6.1 Typical structure

```
GlassCard (Classes="Card", Padding=16)
├── Header          : title 17 DemiBold  +  state badge in a GlassCard (accent dot + 11 text)
├── Skeuo scene     : one or more skeuo controls composed in a Panel (the "mockup")
├── Commands        : Suki controls (exact value, action), only if needed (Law 10)
└── Contextual help (optional): advice that only appears in a risky situation
```

- Hierarchy: **title → object → commands**. The object takes the visual center.
- Typical card width: 300 to 320 px. Scene: about 220 to 290 px wide.
- Generous spacing between the scene and the commands (20 to 35 px): the object needs air to float.
- **One card = one scene = one device.** A scene may assemble several controls when together they form a single real device: a tank and the lever that commands its valves, a camera and its stop button. What is forbidden is to juxtapose two unrelated objects in one card.
- In a composed scene, **leave air between the parts**: a lever stuck against the tank looks as if it were placed on it. Push the actuator toward the opposite edge and widen the card if needed (for example from 300 to 320 px).
- The controls of a composed scene are **stacked in a `Panel`** rather than juxtaposed in a grid. That way, a pipe of the tank can pass under the base of the lever and melt into the edge of the card:

```xml
<Panel Width="288" Height="200">
    <ss:Tank Level="{Binding Level}" Inflow="{Binding Inflow}" Outflow="{Binding Outflow}" />
    <ss:SpringLever Margin="0,46,0,0" HorizontalAlignment="Right" VerticalAlignment="Top"
                    LeftLabel="Fill" RightLabel="Drain"
                    Value="{Binding LeverValue}" Released="Lever_OnReleased" />
</Panel>
```

### 6.2 The scene: a fixed mockup frame

Draw in a fixed-size `Canvas` (for example 332×112) that takes **exactly the coordinates of your SVG mockup**, and scale it with a `Viewbox`:

```xml
<Viewbox Width="268" Height="93" Stretch="Uniform" HorizontalAlignment="Center">
    <Canvas Width="332" Height="112">
        <!-- coordinates identical to the mockup -->
    </Canvas>
</Viewbox>
```

Each control has **its own frame**, as small as possible around its part: the tank in 288×200, the lever in 110×104 with its pivot at (55,82). You do not draw a lever in the tank's frame. It is the page that places the controls relative to each other (§6.1).

Warning: under a `Viewbox`, pointer coordinates must be read **in the Canvas frame** (`e.GetPosition(canvas)`), otherwise the interaction is shifted.

### 6.3 Layer order (z-order)

From the back to the front:

1. **Glows and halos** (under everything, not clickable)
2. **Hidden parts** (rod behind the glass, internal mechanism)
3. **Glass back**
4. **Content**: liquid, particles, image
5. **Glass walls**: rims, reflections
6. **Fixed metal parts**: heads, collars, base, screws
7. **Manipulable part** (gasket, knob, slider): **always in the foreground**. It is what the hand grabs.
8. **Texts and labels** on the object

Everything that is purely decorative gets `IsHitTestVisible="False"`. Interaction is handled by a single parent `Canvas` with `Background="Transparent"` (essential to receive clicks) and **wide hit zones** tested in code: wider than the visible part is more comfortable.

### 6.4 Text on the object

- State labels ("In" / "Out", "Fill" / "Drain"): 11 to 14 px Medium/DemiBold. **Active = `SukiText`, inactive = `SukiLowText`.** It is the text that indicates the state, not an added color. Toggle a class (`Classes.Set("Active", …)`) and let a style do the rest. Do not assign a brush in code: it would no longer follow the theme.

  ```xml
  <Style Selector="TextBlock.EndLabel"><Setter Property="Foreground" Value="{DynamicResource SukiLowText}" /></Style>
  <Style Selector="TextBlock.EndLabel.Active"><Setter Property="Foreground" Value="{DynamicResource SukiText}" /></Style>
  ```
- The labels at the ends of a travel are placed **above the ends** (centered on the end of the rail), not on the sides where they needlessly widen the scene.
- Numeric values: monospaced font (`Consolas`), SemiBold.
- Engravings and plates: see §5.9.

### 6.5 Code organization: the black box

This is the reason for the style to exist (see the introduction): the deliverable is not a drawing, it is **a complex component behind a simple API**. A skeuo control produces a lot of drawing and animation code (gradients, reflections, particles, timers, glows recomputed on theme change). This code does not help to understand the functionality. So it is isolated, and only the interface with the logic must remain readable.

**One control = one responsibility.** You do not build a "TankCard" that contains both the tank and the lever. A device that displays a state (tank, motor, camera) and an actuator that captures a gesture (lever, valve, stop button) are **two distinct controls**. Each has a universal API that describes only its own responsibility. It is the page, with its ViewModel, that links them:

| Control | Role | API |
|---|---|---|
| `Tank` | **Display**: shows what the device reports | `Level` (0..1), `Inflow` (0..1), `Outflow` (0..1) |
| `SpringLever` | **Actuator**: reports the user's intent | `Value` (-1..1, TwoWay), `LeftLabel`, `RightLabel`, `Released` event |
| Page + ViewModel | **Logic**: translates intent into commands, simulates or drives the device | `LeverValue` → curve → `Inflow` / `Outflow`, `CloseValves()` on release |

The universality test: the lever does not know it drives a tank. It only speaks of position and release, and could just as well drive a motor in jog. The tank does not know a lever exists. It only speaks of level and flows, and could be filled by an automatic pump. If a name in the API betrays the other control (`FlowRequested` on a tank, `Target` on a lever), the split is wrong.

The flows displayed by the tank are those **reported by the device**, not those requested by the lever. When the device closes the valve because the tank is empty, the flow stops on screen, even if the user is still pushing the lever.

Each control has its own subfolder and three files:

- `X.axaml`: the fixed drawing and the Dark/Light themes.
- `X.axaml.cs`: only the public API, short and documented (bindable properties, intent events if needed). It is the contract.
- `X.Rendering.cs`: partial class of the same control, with all the visual mechanics (tweens, timers, particles, glows).

Typical tree:

```
Skeumorphism/
├── Controls/
│   ├── Tank/          Tank.axaml · Tank.axaml.cs · Tank.Rendering.cs
│   ├── SpringLever/   SpringLever.axaml · SpringLever.axaml.cs · SpringLever.Rendering.cs
│   ├── Wire/          the link between an actuator and its device (§6.6)
│   └── Shared/        what several controls reuse: materials (tokens), motion (tweens, curves), common input handling
├── SkeumorphismPage.axaml(.cs)   composes the scene, relays Released
└── SkeumorphismViewModel.cs      all the logic
```

What is common to several controls (material set, motion primitives, gesture input logic) lives in `Shared/` and not in a control: each control thus stays readable in three files, and a defect fixed once is fixed everywhere.

The control knows no business object: it receives values and stages them, and it is the control that smooths its animation from the variations of those values (for example, the flow fades in when `Outflow` goes from 0 to 0.6). The ViewModel carries all the logic and ignores that an animation exists.

The only limit to the black box: **it decides how it looks, never what to send to the hardware nor when.** Stop on release, response curve, anti-spam or refusal of an impossible command stay in the ViewModel. The control just emits the user's intent, for example a normalized position or `Released`. The dead zone is an exception: it belongs to the gesture, hence to the lever, which already emits 0 at the center.

Code review covers the ViewModel, the wiring of the page and the contracts. The rendering is validated by eye.

Simple test: **a programmer must be able to use each control by reading only its `X.axaml.cs`**, for example:

```xml
<ss:SpringLever Value="{Binding Speed}" Released="OnLeverReleased" />
<ss:Tank Level="{Binding Level}" Inflow="{Binding Inflow}" Outflow="{Binding Outflow}" />
```

### 6.6 Linking the parts

**Why.** Separating the actuator from the device (§6.5) is a code decision; on screen, they must read as **a single system**. Without a visible link, the user has to guess what commands what. For each actuator on the page, we therefore ask two questions: *what is its device, and how does it talk to it?* An actuator without a link is incomplete.

**The nature of the link says the nature of the information.** Power or a command goes through a cable; matter, through a pipe; a simple measurement through a link more discreet than either. We choose the representation the real object would use, because it is what makes the role of the part readable: a pipe to say "this measures that" suggests a passage of matter.

**The link is a separate control.** It is purely a display (same split as the rest): a geometry expressed in the frame of the host panel, a material, and two **distinct** information inputs (§8.3): the continuous state of what flows and the event that has just occurred. It knows nothing of the parts it links.

**Anchor points are documented.** The page draws the link, so each control states, in the comment of its frame, where the link attaches (as it states its dimensions). A link starts and ends **under a part** (terminal, cable gland, collar), not in the void.

**The link passes behind the parts it joins.** It comes out from under the plate of an actuator instead of crossing it; otherwise it looks stuck on top (same reason as for a pipe under a base, §6.1).

**A link that comes from outside the scene** obeys Law 1: it melts into the glass instead of stopping at the edge.

**Information travels from the source to the destination.** A pure measurement link has no direction of circulation and should not display one.

---

## 7. Choosing the right control

| User intent | Skeumo-Suki control | Suki control |
|---|---|---|
| Move something along a travel | Drag the part directly (piston, carriage) | Position field + unit |
| Drive a speed or a flow continuously (jog), in one or two directions | **Spring lever** with a dead zone (left/right = two opposite actions: fill/drain, reverse/advance) | - |
| Choose among 2 or 3 physical positions | **Detent switch** or rotary valve (click = toggle) | `ToggleSwitch` / segmented |
| Adjust a parameter with a visible effect | Hinge slider (Suki rail + pearl thumb) **and** a reacting object | Exact field + fixed unit |
| Launch an action | - | Full-width `Flat` button |
| Stop immediately | **Stop button** (red dome) | - |
| Exact number (repetitions…) | - | `NumericUpDown` |
| Enable an option | - | `ToggleSwitch` + title + description |

### 7.1 The exact field with a fixed unit

It is the natural partner of a **setting** slider (a parameter that keeps its value, like a duration or a gain). It does not accompany a jog actuator, which has no value to retain (Law 10). The value is on the right, the unit in a separate segment, **without +/- buttons**:

```xml
<controls:GlassCard CornerRadius="8" Padding="0">
    <Grid ColumnDefinitions="*,32">
        <TextBox Margin="16,7" Name="ValueBox" Classes="xxInput" />
        <Border Grid.Column="1" Background="#1A808080"
                BorderBrush="{DynamicResource SukiBorderBrush}" BorderThickness="1,0,0,0">
            <TextBlock FontSize="11" FontWeight="Medium" Foreground="{DynamicResource SukiLowText}"
                       HorizontalAlignment="Center" VerticalAlignment="Center" Text="ms" />
        </Border>
    </Grid>
</controls:GlassCard>
```

The `TextBox` is restyled: transparent background, no border, `Consolas` 14 SemiBold, right-aligned. Its template is reduced to a `TextPresenter` in a transparent `Border`.

### 7.2 The hinge slider

```xml
<Grid Name="Track" Height="28" Background="Transparent" Focusable="True" Cursor="Hand">
    <Canvas Name="Ticks" IsHitTestVisible="False" />                       <!-- detents generated in code -->
    <Border Name="Rail" Height="7" VerticalAlignment="Center" CornerRadius="4"
            Background="{DynamicResource XxRail}" IsHitTestVisible="False" />  <!-- dug rail: inset shadow in code -->
    <Border Name="Fill" Height="7" HorizontalAlignment="Left" VerticalAlignment="Center" CornerRadius="4"
            IsHitTestVisible="False">                                        <!-- accent fill + glow in code -->
        <Border.Background>
            <LinearGradientBrush StartPoint="0%,0%" EndPoint="100%,0%">
                <GradientStop Offset="0" Color="{DynamicResource SukiPrimaryColor75}" />
                <GradientStop Offset="1" Color="{DynamicResource SukiPrimaryColor}" />
            </LinearGradientBrush>
        </Border.Background>
    </Border>
    <!-- pearl thumb (§5.4), positioned by Margin.Left = x - 11 -->
</Grid>
<Grid ColumnDefinitions="*,Auto">
    <TextBlock FontSize="11" Foreground="{DynamicResource SukiLowText}" Text="Darker · sharp motion" />
    <TextBlock Grid.Column="1" FontSize="11" Foreground="{DynamicResource SukiLowText}" Text="Brighter" />
</Grid>
```

The **legends at both ends** describe the consequence ("brighter", "more noise"), not the unit. For a logarithmic parameter, position the thumb on a scale of detents (thirds or half-steps), not on a linear scale.

### 7.3 Keyboard and accessibility

- Rails and levers are `Focusable="True"`: arrows for one detent, Page Up/Down for a big step.
- Each `TextBox` gets an `AutomationProperties.Name` with the unit ("Duration (ms)").
- The mouse wheel on a detent switch changes the detent.
- Every skeuo action has an exact equivalent in the Suki commands or on the keyboard.

### 7.4 Choosing the form of an actuator

- **Discrete or continuous.** A finite number of named states calls for a **detent** organ: the state is read without interpretation. A travel or continuous rotary organ is for a quantity that varies; imposing it on a few states would force the user to guess where the positions are.
- **Proportion.** The actuator is subordinate to the device: if it crushes it, the card reads as "a big button with a small device". But it must remain graspable: the compromise is settled by the **hit zone**, which can be wider than the visible part, and by a really comfortable handle size, not by shrinking down to discomfort.
- **Placement.** Place it near where the effect occurs, at the level of the connection, because position tells the link (§6.6).
- **Marks.** A travel organ shows where it begins and where it ends (stops, graduation, label): one must know what the rest position means without trying it.

---

## 8. Behavior and physics

Skeumo-Suki is judged **in motion**. A screenshot shows only half of the style.

### 8.1 Primitive: timer tween

Simple, reliable, and it works on any property, including in a `RenderTransform`:

```csharp
private static void Tween(double from, double to, int ms, Action<double> set, Func<double, double>? ease = null)
{
    ease ??= t => 1 - Math.Pow(1 - t, 3);                       // cubic ease-out by default
    var start = DateTime.UtcNow;
    DispatcherTimer? timer = null;
    timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) =>
    {
        var t = Math.Min(1, (DateTime.UtcNow - start).TotalMilliseconds / ms);
        set(from + (to - from) * ease(t));
        if (t >= 1) timer!.Stop();
    });
    timer.Start();
}
```

### 8.2 Curve library

| Curve | Formula | Usage | Duration |
|---|---|---|---|
| Cubic ease-out | `1 - (1-t)³` | Detents, toggles, valve rotations | 200-300 ms |
| Damped spring | `2^(-10t) · sin((10t - 0.75) · 2π/3) + 1` | Return to center of a released lever | 600-800 ms |
| Press | keyframes 0 → 25 % pressed → 100 % released, `CubicEaseOut` | Physical button | 400-500 ms |
| Flash | opacity 0 → 1 (15 %) → 0 | Stop confirmation, alert | 600-800 ms |

### 8.3 Catalog of behaviors

**Spring lever (jog)**: the richest gesture of the style.
- *Two directions*: if the device has two opposite actions (fill/drain, advance/reverse), the lever goes both ways (±35°), with a label above each end. A lever that only goes one way when the object suggests both looks broken.
- *Relative anchoring and accumulation*: the lever turns by **the same delta** as the hand around the axis, and never jumps to the pointer. The pointer angle is **periodic**: measured relative to the click, it jumps as soon as the hand passes to the other side of the axis (beyond 180° from the start). So we accumulate the variations **in small steps** (each variation brought back between -180° and 180°), clamp at each step, and ignore the pointer when it is too close to the axis, where the angle is unstable. This rule holds for any rotary organ (lever, knob, valve): factor it into `Shared/` so that it is identical everywhere.
- *Dead zone* at the center (a few degrees): the lever emits 0.
- *Quadratic response* (in the ViewModel): `v = vMax · n · |n|`, with `n` the normalized angle outside the dead zone. You get fine adjustment at the center and full speed at the ends.
- *Anti-spam* (in the ViewModel): only send the command if the speed changes by at least a few %.
- *Release*: `Value` goes back to 0 **before** the animation, then `Released` is raised, then the lever returns to the center by a damped spring. The return tween is purely visual.
- *Grabbing during the return*: each tween carries an identifier. Grabbing the lever again increments the identifier, which neutralizes the running tween. A simple `if (!_engaged)` test is not enough: after a re-grab followed by a second release, two tweens would fight over the angle.
- *Safety*: treat `PointerCaptureLost` (alt-tab, popup) **exactly** like a release, as well as the detachment of the control from the visual tree.
- *Feedback*: accent glow under the knob, with an intensity proportional to the push, a rail arc filled from the center to the current angle (in the right sweep direction), and the label of the pushed side in `SukiText`.
- *Keyboard*: left/down and right/up arrows to push by one step, key release = lever release.

```csharp
private void Scene_OnPressed(object? s, PointerPressedEventArgs e)
{
    var c = (Canvas)s!;
    c.Focus();
    Engage();
    _lastPointerDeg = PointerAngle(e.GetPosition(c));        // relative anchoring: we accumulate steps
    e.Pointer.Capture(c);
    e.Handled = true;
}

private void Scene_OnMoved(object? s, PointerEventArgs e)
{
    if (!_engaged) return;
    var pointer = PointerAngle(e.GetPosition((Canvas)s!));
    var d = pointer - _lastPointerDeg;
    if (d > 180) d -= 360; else if (d < -180) d += 360;      // one step, never the total since the click
    _lastPointerDeg = pointer;
    SetAngle(Math.Clamp(_angle + d, -MaxDeg, MaxDeg));
}

private void Engage()
{
    _engaged = true;
    _tweenId++;                                              // neutralizes a return in progress
}

private void Release()
{
    if (!_engaged) return;
    _engaged = false;
    SetCurrentValue(ValueProperty, 0d);                      // stop first, without breaking the binding
    Released?.Invoke(this, EventArgs.Empty);
    var id = ++_tweenId;
    Tween(_angle, 0, 700, a => { if (id == _tweenId) SetAngle(a, drive: false); }, ElasticOut);
}

private void SetAngle(double deg, bool drive = true)
{
    _angle = deg;
    _rot.Angle = deg;
    var abs = Math.Abs(deg);
    var n = abs <= DeadDeg ? 0 : Math.Sign(deg) * (abs - DeadDeg) / (MaxDeg - DeadDeg);
    // ... glow, arc, labels ...
    if (drive) SetCurrentValue(ValueProperty, n);
}

private static double PointerAngle(Point p) => Math.Atan2(p.X - PivotX, PivotY - p.Y) * 180 / Math.PI;
private static double ElasticOut(double t) => Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * (2 * Math.PI / 3)) + 1;
```

**Detent switch**: 2 or 3 positions, 250 ms cubic ease-out slide, the rail fill follows the knob during the movement, the label of the active detent goes to `SukiText`, the mouse wheel changes the detent.

**Directly dragged part** (piston, carriage): the part follows the pointer **during** the drag (preview), the hardware command is sent **on release**. If the device is busy, the part returns to the last confirmed position.

**Rotary valve**: a click on the valve area toggles. The rotor turns (0° ↔ -90°) and its accent bore orients toward the active path. The active channel goes to 0.7 opacity, the other to 0.1, and the labels change color.

**Physical button**: only the dome is pressed in (top margin +6 px), the collar stays fixed. It can trigger a flash on the relevant part.

**Flow**: when the liquid circulates, the pipe fills with accent (opacity proportional to the flow), and **suspended particles** are carried by the stream. No scrolling dashes: a regular queue reads as an animation, not as a liquid. To keep the flow unpredictable, each particle has its own life:

- *random birth*: at each frame, we draw at random the number of particles to create (on average `0.9 × flow`), and not one particle at a fixed interval;
- *own speed*: a factor drawn between 0.55 and 1.45, multiplied by the current flow;
- *own position across the width of the pipe*: a random initial lateral offset (±2.4 px), plus a **Brownian drift** at each frame, bounded to the width of the tube;
- *own undulation*: `sin(s / λ + phase)`, with a wavelength `λ` drawn at random;
- *mix*: about 80 % light dots and 20 % dark dots, sizes from 1.2 to 2.8 px, varied alphas.

Particles follow the **center line of the pipe, bend included**: the `Path` (lines + sampled `Q` curve) is flattened into a polyline traversed by distance, which gives for each `s` a position and a normal. The lateral offset is applied along that normal.

```csharp
var (pos, normal) = route.At(p.S);
var lateral = p.Offset + 0.9 * Math.Sin(p.S / p.WobbleLength + p.Phase);
Canvas.SetLeft(p.E, pos.X + normal.X * lateral - p.E.Width / 2);
Canvas.SetTop(p.E, pos.Y + normal.Y * lateral - p.E.Height / 2);
```

The flow appears and disappears with a **fade**, never all at once: the displayed flow reaches the received flow by exponential smoothing (`shown += (target - shown) · 0.12` at each frame). When the flow falls to zero, the particles still in the pipe fade out in place instead of vanishing or continuing to rush. When filling, the particles that reach the end of the inlet pipe **fall** into the tank (gravity) along an accent stream, down to the surface.

**Living matter**: suspended particles that drift slowly (`x += vx + 0.05 · sin(tick·0.02 + phase)`), bounce off the walls and stay **always under the surface**. A particle that rises too high is pushed back down, and its opacity fades over the 8 px under the surface, so that it never "touches" the air. When the liquid circulates, the stirring accelerates (`× (1 + 3 · flow)`) and the matter drifts toward the outlet. A mix of light dots (bubbles) and a few dark dots (impurities) gives realism.

**Signal**: what a link (§6.6) uses to show that information is passing. There are two kinds, which are not signaled the same way:

- *An event* (the user has just changed a setting) is signaled **once, at the moment it occurs**, to acknowledge receipt: "your gesture has been taken into account". **Why:** a signal that repeats without cause is a lie (it suggests an activity that does not exist) and it distracts. So it is tied to the variation of the setpoint, never to a clock.
- *A continuous state* (something is really flowing) is shown continuously, from the state **reported by the device** and not from the gesture, and stays **in the background**.

**Intensity is proportioned to the role.** The acknowledgement must be noticed without surprising; the continuous signal must never compete with the object. In practice, the first setting is almost always too bright, too fast, too marked: start low and go up, rather than the opposite.

**The rendering evokes the nature of what flows** (an electric charge does not look like a fluid or a gas), so that the signal *says* something. An abstract form with no reference (a dot, a line) means nothing and reads as a defect.

### 8.4 The object as a display of consequences

For each parameter, ask yourself: *what changes physically when it is adjusted?* Then make it change **on the object**, from continuous formulas:

| Parameter | Visible consequence |
|---|---|
| Exposure time, light power | Width and opacity of a beam, illumination of a chamber, glow of a lens |
| Gain, amplification | Elements brighten **and** pixelate (low-resolution rendering enlarged without smoothing) |
| Speed, flow | Opacity of the flow in the pipe, density and speed of the carried particles, stirring in the tank |
| Level, volume | Height of the liquid, proportional halo |
| Rate | Rhythm of the pulses on the cable |
| Direction | Direction of the flow, lit path |
| Speed of a rotating organ | Beyond what the eye follows, it **blurs**: a rotation left sharp at high speed reads as a frozen or jerky object |

Add a **contextual tip** (amber box, light-bulb icon, one sentence) that only appears when the combination of settings is bad. Example: "Increase the exposure before the gain to keep a clean image."

### 8.5 State honesty

- If the device does not return its position, **estimate** the progress (it tends toward 97 % until the next step has begun) instead of displaying a false certainty.
- No new command during a movement: the object refuses it and returns to the confirmed state.
- An impossible command is refused by the ViewModel (emptying an empty tank, filling a full tank), and the device cuts off by itself at the limits. Since the display shows the **reported** flows, the flow stops on screen even if the user is still pushing the lever.
- During an automatic operation, the manual control is grayed out (opacity 0.4) and the action button becomes "Cancel".
- "Modified by the user" events are raised **only** by a user action, never when the code applies a value coming from elsewhere.

---

## 9. Light and dark themes, accent color

### 9.1 ThemeDictionaries

```xml
<UserControl.Resources>
    <ResourceDictionary>
        <ResourceDictionary.ThemeDictionaries>
            <ResourceDictionary x:Key="Dark">
                <!-- all the materials, dark version -->
            </ResourceDictionary>
            <ResourceDictionary x:Key="Light">
                <!-- same keys, light version -->
            </ResourceDictionary>
        </ResourceDictionary.ThemeDictionaries>
        <!-- materials identical in both themes (white reflections, fixed chrome…) -->
        <!-- brushes built on the accent -->
    </ResourceDictionary>
</UserControl.Resources>
```

These resources are used with `{DynamicResource …}`. Theme-independent brushes can stay as `{StaticResource …}`.

### 9.2 Transposing a material from dark to light

- Shift the whole ramp by **2 steps toward the light** (the body `#3B4558` becomes `#CBD3E0`).
- Shadows go from `#000000` to `#283250` and lose about half of their alpha.
- The glass back becomes almost transparent, tinted `#6F7F9E`.
- The white glass rims become blue-gray rims (`#607090`).
- Light engravings on a dark plate stay readable: in light, the plate becomes `#5E6A84` and the text stays white.
- A rendering drawn in code (`Render`) must also invert: light background, dark elements.

### 9.3 Reacting to theme and accent changes

```csharp
public MyCard()
{
    InitializeComponent();
    AttachedToVisualTree   += (_, _) => { ApplyAccentGlows(); _timer.Start(); };
    DetachedFromVisualTree += (_, _) => _timer.Stop();                     // no animation off screen
    ActualThemeVariantChanged += (_, _) => { ApplyAccentGlows(); RebuildTicks(); };
}

/// Suki resource of the current theme, whether it is a brush or a color.
private IBrush ThemeBrush(string key, IBrush fallback) =>
    this.TryFindResource(key, ActualThemeVariant, out var res)
        ? res switch { IBrush b => b, Color c => new SolidColorBrush(c), _ => fallback }
        : fallback;
```

Everything that is **created in code** (graduations, segments, bubbles, glows) must be recolored at each theme change.

---

## 10. Avalonia technical pitfalls

| Pitfall | Solution |
|---|---|
| `BoxShadow` cannot reference a dynamic color | Build the accent `BoxShadows` in code (§5.11). Neutral shadows can be `BoxShadows` resources in the ThemeDictionaries. |
| `SukiPrimaryColor` / `SukiText` are `Color`s | In a `GradientStop`, `Color="{DynamicResource …}"`. In code, convert with `ThemeBrush`. |
| Animating a `RenderTransform` with `Animation` does not always work depending on the version | Manual tween by `DispatcherTimer` (§8.1) that writes `RotateTransform.Angle` or `TranslateTransform.Y` directly. |
| A glow is cut dead | Parents with `ClipToBounds="False"`, glow placed outside the clipped container. |
| The `Canvas` does not receive clicks | `Background="Transparent"` (a `null` background is not hit-testable). |
| A decoration steals clicks | `IsHitTestVisible="False"` on everything that is not interactive. |
| `StrokeDashArray` / `StrokeDashOffset` seem wrong | They are expressed in **multiples of the stroke thickness**. |
| Pointer coordinates are shifted under a `Viewbox` | `e.GetPosition(sceneCanvas)`. |
| The drag gets stuck after an alt-tab | Handle `PointerCaptureLost` as a release. |
| Hardware commands block the interface | `Task.Run` for the command, `Dispatcher.UIThread.Post` to return to the interface. |
| Timers run on an invisible control | Start/Stop on `AttachedToVisualTree` / `DetachedFromVisualTree`. |
| A `RenderTargetBitmap` + `DrawImage` mix is blurry | For a deliberate pixelation, render at low resolution then enlarge without smoothing. |
| An automatic translator translates engravings | Mark them (for example `Tag="NoTranslate"`) according to your app's convention. |
| `CS0102: the type already contains a definition for 'Flow'` | Avalonia's name generator creates a field for each `Name`. An element named like a property of the control collides with it: name elements by their visual role (`OutletFlow`, `FlowLine`), never like a property of the API. |
| `CS0104: 'Path' is an ambiguous reference` | With implicit usings, `System.IO.Path` conflicts with `Avalonia.Controls.Shapes.Path`. Add `using Path = Avalonia.Controls.Shapes.Path;`. |
| The TwoWay binding of a value emitted by the control breaks | The control writes its own value with `SetCurrentValue(ValueProperty, v)`, never with `SetValue` or the setter: otherwise the local value replaces the binding. Declare the property with `defaultBindingMode: BindingMode.TwoWay`. |
| A rotary gesture "goes anywhere" beyond 180° | The pointer angle is periodic: accumulate bounded steps instead of comparing to the starting click (§8.3). |
| A glow or halo is truncated by the control's frame | The frame contains everything that radiates: size it for the halo (and not for the part alone), and do not enclose the glow in a parent that crops (`ClipToBounds`). |
| A label recolored in code no longer follows the theme | Toggle a class and let a style with `{DynamicResource …}` carry the color (§6.4). |
| A "square" liquid sticks out of the rounded shoulders of the container | Clip the content with an inner `Border` with `ClipToBounds="True"` and `CornerRadius` (§5.7), instead of rounding the liquid. |
| The end of a tube sticks out of a flange or a collar | `StrokeLineCap="Round"` extends the stroke by half its thickness: stop the path earlier (§5.8). |
| A bent tube is dark and flat in the light theme | The tube body, laid on the rim, is too transparent: make it opaque and light, and add a lighter core (§3.2). |

---

## 11. Working method

1. **Observe the real device.** Note 3 to 6 characteristic details and the moving parts.
2. **List the gestures and the consequences.** For each setting: which gesture, which exact value, which visible change?
3. **Mock up in interactive SVG** before writing XAML. Keep the same frame (the mockup becomes the `Canvas`). Iterating in SVG is ten times faster.
4. **Validate the silhouette** in flat fills, without gradients: if the object cannot be recognized, no gradient will save it.
5. **Apply the metal ramp, then the light, then the glass, then the accent**, in that order.
6. **Iterate one value at a time** (a color, an alpha, a radius) and compare. This style is tuned down to the detail.
7. **Split into controls**: a display, an actuator, each with its universal API and its own frame (§6.5). Write the `X.axaml.cs` contracts before the drawing.
8. **Port to XAML** with the role-based tokens and the ThemeDictionaries.
9. **Add the behavior** (§8), then test the physics by hand: spring, detents, abrupt release, alt-tab.
9bis. **Link the parts** (§6.6): for each actuator, its device and the link that unites them.
10. **Watch the real application run**, not just compile: the defects that matter cannot be seen in the code (cuts, the length of a fade, proportions, the brightness of a signal, truncated halos). A screen review in both themes before presenting avoids whole rounds of back and forth.
11. **Test the three variations**: dark theme, light theme, and **another accent color**. If changing the accent does not recolor the energy of the object, a color is hard-coded.

---

## 12. Anti-patterns

- ❌ Bitmap textures, noise, grain, a photo of the real device.
- ❌ Neutral grays (`#808080`), warm metals, wood, leather.
- ❌ Several light sources, or a light that comes from below.
- ❌ Long, hard drop shadows, pure black in the light theme.
- ❌ A floor, a tray or a decor under the object (the feet and base plates of the device itself are allowed).
- ❌ A moving part that comes out of a plain circle placed in the void: a lever rests on a base plate (§5.13).
- ❌ A tube or a cable cut dead at the edge of the card, a fade that is too short, an end that sticks out of a flange.
- ❌ Scrolling dashes to simulate a flow: a predictable queue is not a liquid.
- ❌ Generic energy (any liquid, fill, rail) in a fixed color instead of the accent.
- ❌ The accent imposed on what has a strong conventional color (light, charge level, gas): recognition is lost (§3.5).
- ❌ A metaphor borrowed from another object (an accumulator that fills like a tank, a gas drawn like a liquid).
- ❌ An indicator overloaded with materiality (caps, shading) instead of a sober display.
- ❌ An actuator with no visible link to its device, or a link that does not have the nature of what it carries.
- ❌ A signal that repeats without cause, or a continuous signal that competes with the object.
- ❌ A rotary gesture measured from the starting click (it jumps beyond 180°).
- ❌ A halo or glow truncated by the control's frame.
- ❌ A rotary knob to enter an exact value. +/- buttons next to a field.
- ❌ An exact field added by principle to an actuator that has no value to retain.
- ❌ An object that does not react to the settings: it is decoration, not Skeumo-Suki.
- ❌ A silhouette that evokes another object (a tank that looks like a syringe).
- ❌ A lever that jumps to the pointer instead of turning with the hand, or that only goes one way when the device has two opposite actions.
- ❌ A release that leaves the device running during the return animation.
- ❌ A single control that merges the display and the actuator, or an API that betrays the other control.
- ❌ Two unrelated devices in the same card, or skeuo on navigation and forms.
- ❌ Parts stuck to each other in a composed scene.
- ❌ A single theme version.
- ❌ Meaningless details (decorative rivets everywhere, screws on every foot, hoops that hide the liquid).

---

## 13. Validation checklist

**Suki integration**
- [ ] The card is a standard `GlassCard` (title, padding, glass badges).
- [ ] The object floats, with no floor or frame. Inputs and outputs melt into the glass, through a long fade.
- [ ] Exact values and actions use Suki components, and only when they are useful.

**Organization**
- [ ] One control per responsibility: display on one side, actuator on the other, linked by the page and its ViewModel.
- [ ] Each API is universal: no name betrays the other control.
- [ ] Each control fits in three files: `.axaml`, `.axaml.cs` (contract only), `.Rendering.cs`.
- [ ] The display shows the state reported by the device, not the user's request.

**Form**
- [ ] The silhouette is recognizable as a flat shape (a tank is wide, with shoulders, flange, outlet and feet).
- [ ] Moving parts rest on a solid base. The parts of a composed scene have air between them.
- [ ] Pipes pass behind or under what they cross, and their ends are hidden.

**Links**
- [ ] Each actuator is linked to its device, and the nature of the link matches what it carries.
- [ ] The link starts and ends under a part, and passes behind the parts it joins.
- [ ] Event and continuous state are distinguished: the event is signaled once, the continuous one stays discreet.
- [ ] Fades reach zero at the edge of the card and have been **seen** on screen.

**Material and light**
- [ ] All hard materials use the blue-slate ramp.
- [ ] A single light source at the top left, one specular line per part.
- [ ] Shadows are short and soft, tinted `#283250` in light.
- [ ] No bitmap. Large corner radii. 3 to 6 details, all meaningful.

**Energy**
- [ ] Energy with no conventional color (any liquid, fill, active bore) is in `SukiPrimaryColor` / `75`; what has a conventional color follows reality (§3.5).
- [ ] Changing the accent color recolors the energy of the object.
- [ ] Red is reserved for stop and comes from `SukiDangerColor`.

**Themes**
- [ ] Each material exists in `Dark` and `Light`, and the light version has really been looked at (tubes in particular).
- [ ] Elements created in code are recolored on theme change.
- [ ] Accent glows are rebuilt on theme change.

**Behavior**
- [ ] Each setting has a visible consequence on the object.
- [ ] Movements are damped (ease-out, spring, detents), never instantaneous. Flows appear and disappear with a fade.
- [ ] Particles (suspended, carried by a flow) have no predictable rhythm or trajectory.
- [ ] The lever goes both ways if the device has two opposite actions.
- [ ] A rotary gesture stays correct beyond 180° from the start.
- [ ] A speed that exceeds what the eye follows is rendered with blur.
- [ ] Nothing is truncated: halos, glows and reflections fit in the control's frame.
- [ ] Release and capture loss = immediate hardware stop.
- [ ] No command during a movement. The displayed state is honest.
- [ ] Timers stop when the control leaves the screen.

**Accessibility**
- [ ] Gestural controls are focusable and usable with the keyboard.
- [ ] Fields have an automation name with the unit.
- [ ] The state is also read through text (`SukiText` / `SukiLowText`), not only through color.

---

## 14. Complete example: tank and fill / drain lever

A card that shows a glass tank (the level is the accent) and a spring-loaded lever: pushed left, it fills through the top inlet; pushed right, it drains through the bottom outlet. The more you push, the stronger the flow. On release, the valves close immediately and the lever returns to the center.

The example applies the split of §6.5: **two independent controls** and a page that links them. The complete sources are in the demo: `SukiUI.Demo/Features/Helpers/Skeumorphism/` (Helpers → Skeumorphism menu). Below are the contracts, the page and the ViewModel in full, as well as the commented structure of the rendering.

```
Features/Helpers/Skeumorphism/
├── Controls/
│   ├── Tank/          Tank.axaml · Tank.axaml.cs · Tank.Rendering.cs
│   └── SpringLever/   SpringLever.axaml · SpringLever.axaml.cs · SpringLever.Rendering.cs
├── SkeumorphismPage.axaml(.cs)
└── SkeumorphismViewModel.cs
```

### 14.1 Tank contract: `Tank.axaml.cs`

A pure display. The tank does not know who fills it.

```csharp
/// <summary>
/// Skeumo-Suki glass tank with an inlet on top and an outlet at the bottom.
/// Display only: it shows the level and the flows it receives. Drawing and animation live in <c>Tank.Rendering.cs</c>.
/// </summary>
public partial class Tank : UserControl
{
    /// <summary>Liquid level (0 = empty .. 1 = full).</summary>
    public static readonly StyledProperty<double> LevelProperty =
        AvaloniaProperty.Register<Tank, double>(nameof(Level), coerce: (_, v) => Math.Clamp(v, 0, 1));

    /// <summary>Flow entering through the top inlet (0 = closed .. 1 = full flow).</summary>
    public static readonly StyledProperty<double> InflowProperty =
        AvaloniaProperty.Register<Tank, double>(nameof(Inflow), coerce: (_, v) => Math.Clamp(v, 0, 1));

    /// <summary>Flow leaving through the bottom outlet (0 = closed .. 1 = full flow).</summary>
    public static readonly StyledProperty<double> OutflowProperty =
        AvaloniaProperty.Register<Tank, double>(nameof(Outflow), coerce: (_, v) => Math.Clamp(v, 0, 1));

    public double Level   { get => GetValue(LevelProperty);   set => SetValue(LevelProperty, value); }
    public double Inflow  { get => GetValue(InflowProperty);  set => SetValue(InflowProperty, value); }
    public double Outflow { get => GetValue(OutflowProperty); set => SetValue(OutflowProperty, value); }

    public Tank()
    {
        InitializeComponent();
        InitializeRendering();
    }
}
```

### 14.2 Lever contract: `SpringLever.axaml.cs`

A pure actuator. The lever does not know what it drives.

```csharp
/// <summary>
/// Skeumo-Suki spring-loaded lever (jog). The user pushes it left or right; released, it springs back to the center.
/// It only reports the user's intent: what to do with it (response curve, anti-spam, safety) belongs to the host.
/// Drawing and animation live in <c>SpringLever.Rendering.cs</c>.
/// </summary>
public partial class SpringLever : UserControl
{
    /// <summary>
    /// Lever position set by the user: -1 (fully left) .. 1 (fully right), 0 inside the center dead zone.
    /// Set back to 0 immediately on release, before the spring-back animation.
    /// </summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<SpringLever, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Label shown above the left end of the travel.</summary>
    public static readonly StyledProperty<string?> LeftLabelProperty =
        AvaloniaProperty.Register<SpringLever, string?>(nameof(LeftLabel));

    /// <summary>Label shown above the right end of the travel.</summary>
    public static readonly StyledProperty<string?> RightLabelProperty =
        AvaloniaProperty.Register<SpringLever, string?>(nameof(RightLabel));

    public double Value       { get => GetValue(ValueProperty);      set => SetValue(ValueProperty, value); }
    public string? LeftLabel  { get => GetValue(LeftLabelProperty);  set => SetValue(LeftLabelProperty, value); }
    public string? RightLabel { get => GetValue(RightLabelProperty); set => SetValue(RightLabelProperty, value); }

    /// <summary>Raised when the user lets go of the lever (pointer released, capture lost or key released).</summary>
    public event EventHandler? Released;

    public SpringLever()
    {
        InitializeComponent();
        InitializeRendering();
    }
}
```

### 14.3 The page that links the two: `SkeumorphismPage.axaml`

The card composes the scene. The lever is stacked to the right of the tank, with air between the two, and the outlet of the tank passes under its base plate.

```xml
<suki:GlassCard Width="320" Padding="16" TextElement.Foreground="{DynamicResource SukiText}">
    <StackPanel>
        <!-- Header: title + level badge -->
        <Grid ColumnDefinitions="*,Auto">
            <TextBlock VerticalAlignment="Center" FontSize="17" FontWeight="DemiBold" Text="Tank" />
            <suki:GlassCard Grid.Column="1" Padding="10,3" CornerRadius="10">
                <StackPanel Orientation="Horizontal" Spacing="6">
                    <Border Width="7" Height="7" VerticalAlignment="Center" CornerRadius="4"
                            Background="{DynamicResource SukiPrimaryColor}" />
                    <TextBlock FontSize="11" FontWeight="DemiBold" Text="{Binding LevelText}" />
                </StackPanel>
            </suki:GlassCard>
        </Grid>

        <!-- Scene: the outlet of the tank passes under the lever -->
        <Panel Width="288" Height="200" Margin="0,16,0,0">
            <controls:Tank Level="{Binding Level}"
                           Inflow="{Binding Inflow}"
                           Outflow="{Binding Outflow}" />
            <controls:SpringLever Margin="0,46,0,0"
                                  HorizontalAlignment="Right"
                                  VerticalAlignment="Top"
                                  LeftLabel="Fill"
                                  RightLabel="Drain"
                                  AutomationProperties.Name="Fill and drain lever"
                                  Value="{Binding LeverValue}"
                                  Released="Lever_OnReleased" />
        </Panel>
    </StackPanel>
</suki:GlassCard>
```

```csharp
public partial class SkeumorphismPage : UserControl
{
    public SkeumorphismPage() => InitializeComponent();

    private void Lever_OnReleased(object? sender, EventArgs e) =>
        (DataContext as SkeumorphismViewModel)?.CloseValves();
}
```

There is no exact field: a jog lever has no value to retain (Law 10).

### 14.4 All the logic: `SkeumorphismViewModel.cs`

```csharp
/// <summary>
/// Simulated tank with a fill valve and a drain valve, driven by a spring lever.
/// All the logic lives here (response curve, anti-spam, stop on release, automatic stop when empty or full):
/// the Tank only displays values and the SpringLever only reports the user's intent.
/// </summary>
public partial class SkeumorphismViewModel : ObservableObject
{
    private const double MaxRatePerSecond = 0.12;    // fraction of the tank per second at full opening
    private const double MinFlowChange = 0.03;       // anti-spam threshold on valve commands

    private readonly DispatcherTimer _deviceTimer;
    private DateTime _lastTick = DateTime.UtcNow;

    /// <summary>Lever intent: -1 = fill .. 1 = drain.</summary>
    [ObservableProperty] private double _leverValue;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(LevelText))] private double _level = 0.7;
    [ObservableProperty] private double _inflow;
    [ObservableProperty] private double _outflow;

    public string LevelText => $"{Level * 100:0} %";

    public SkeumorphismViewModel()
    {
        _deviceTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(30), DispatcherPriority.Background, (_, _) => SimulateDevice());
        _deviceTimer.Start();
    }

    /// <summary>Safety first: the valves close as soon as the user lets go of the lever.</summary>
    public void CloseValves()
    {
        Inflow = 0;
        Outflow = 0;
    }

    partial void OnLeverValueChanged(double value)
    {
        // Refuse commands that cannot be honoured: nothing to drain, or no room left.
        if (value == 0 || (value > 0 && Level <= 0) || (value < 0 && Level >= 1))
        {
            CloseValves();
            return;
        }

        var flow = value * Math.Abs(value);           // quadratic response: fine control near the dead zone
        var current = Outflow - Inflow;
        if (Math.Abs(flow - current) < MinFlowChange && Math.Abs(flow) < 1) return;

        Outflow = Math.Max(0, flow);
        Inflow = Math.Max(0, -flow);
    }

    private void SimulateDevice()
    {
        var now = DateTime.UtcNow;
        var dt = (now - _lastTick).TotalSeconds;
        _lastTick = now;

        if (Inflow == 0 && Outflow == 0) return;

        Level = Math.Clamp(Level + MaxRatePerSecond * (Inflow - Outflow) * dt, 0, 1);

        // The device closes its valves by itself at the limits.
        if (Level <= 0) Outflow = 0;
        if (Level >= 1) Inflow = 0;
    }
}
```

With a real device, `SimulateDevice` disappears: `Level`, `Inflow` and `Outflow` are fed by the device, and `OnLeverValueChanged` / `CloseValves` send the commands (`Task.Run` for the command, `Dispatcher.UIThread.Post` to return to the interface).

### 14.5 The tank drawing: `Tank.axaml`

288 × 200 frame. The layers follow §6.3:

| Layer | Content | Recipe |
|---|---|---|
| 1. Glow | `Aura`, accent `BoxShadow` set in code, height = level | §5.11 |
| 2. Hidden parts | inlet (left → flange, fade `0.02 → 0.24`), outlet (bottom → bend → under the right foot at y = 192 → fade to the right), slanted legs and pads | §5.8, §5.14 |
| 3. Glass back | body 100 × 130, `CornerRadius="34,34,16,16"`: wide, round shoulders | §5.6 |
| 4. Content | inner `ClipToBounds` `Border`, with the falling stream, the liquid anchored at the bottom and the particles | §5.7 |
| 5. Walls | outer and inner rims, vertical reflection, dome reflection, 25/50/75 % graduations | §5.6 |
| 6. Fixed metal | screwed flange on top (under which the end of the inlet hides), outlet fitting, leg brackets without screws | §5.2, §5.14 |

No hoops on the glass: they hid the liquid. The tokens are prefixed `Tk` and declared in both `ThemeDictionaries`. The tubes use `TkTubeRim`, `TkTubeBg` and `TkTubeCore` (§3.2).

### 14.6 The tank mechanics: `Tank.Rendering.cs`

```csharp
private void Tick()
{
    _tick++;

    // The flows fade in and out, they never switch abruptly.
    _shownIn = Approach(_shownIn, Inflow);
    _shownOut = Approach(_shownOut, Outflow);

    this.Get<Path>("OutletFlow").Opacity = 0.85 * _shownOut;
    this.Get<Path>("InletFlow").Opacity = 0.85 * _shownIn;
    var fall = this.Get<Line>("FallStream");
    fall.Opacity = 0.8 * _shownIn;
    fall.StrokeThickness = 1.5 + 2.5 * _shownIn;
    fall.EndPoint = new Point(FallX, SurfaceY + 2);

    UpdatePipe(_outletSpecks, _outletCanvas, OutletRoute, _shownOut, onExit: null);
    UpdatePipe(_inletSpecks, _inletCanvas, InletRoute, _shownIn, onExit: SpawnFallSpeck);
    UpdateFall();     // particles falling from the inlet to the surface
    UpdateTank();     // suspended matter, always below the surface
}

// Particles carried by a pipe flow: random spawn, speed, lateral offset and wobble, so they never form a queue.
private void UpdatePipe(List<PipeSpeck> specks, Canvas canvas, Route route, double flow, Action? onExit)
{
    if (flow > 0.02)
    {
        var expected = 0.9 * flow;                           // Poisson-like spawning
        while (_rng.NextDouble() < expected)
        {
            specks.Add(CreatePipeSpeck(canvas));
            expected -= 1;
        }
    }

    for (var i = specks.Count - 1; i >= 0; i--)
    {
        var p = specks[i];
        p.S += p.Speed * (0.25 + 3.2 * flow);
        p.Offset = Math.Clamp(p.Offset + (_rng.NextDouble() - 0.5) * 0.35, -2.6, 2.6);   // brownian drift

        var exited = p.S >= route.Length;
        if (flow < 0.02) p.Fade -= 0.05;                     // flow stopped: fade out in place
        if (exited || p.Fade <= 0)
        {
            canvas.Children.Remove(p.E);
            specks.RemoveAt(i);
            if (exited) onExit?.Invoke();                    // inlet: the particle falls into the tank
            continue;
        }

        var (pos, normal) = route.At(p.S);
        var lateral = p.Offset + 0.9 * Math.Sin(p.S / p.WobbleLength + p.Phase);
        p.E.Opacity = p.Alpha * Math.Min(1, p.S / 8) * p.Fade;
        Canvas.SetLeft(p.E, pos.X + normal.X * lateral - p.E.Width / 2);
        Canvas.SetTop(p.E, pos.Y + normal.Y * lateral - p.E.Height / 2);
    }
}
```

`Route` flattens the geometry of the XAML `Path`s (lines and a `Q` curve sampled in 12 segments) and gives, for a distance `s`, a position and a normal. It is declared with **exactly** the same points as the XAML:

```csharp
private static readonly Route OutletRoute = new Route(new Point(96, 160))
    .LineTo(new Point(96, 180)).QuadTo(new Point(96, 192), new Point(108, 192)).LineTo(new Point(288, 192));
```

Particles created in code are recolored in `ApplyTheme()` at each theme change, and the timer stops when the tank leaves the visual tree.

### 14.7 The lever: `SpringLever.axaml` and `SpringLever.Rendering.cs`

110 × 104 frame, pivot at (55,82), radius 60, travel ±35°, dead zone 4°. From the back to the front: arc rail and center mark, accent-filled arc, glow under the knob, lever (rod and knob, `RenderTransformOrigin="50%,100%"`), boss with chrome axle, then screwed base plate (§5.13) and end labels above each end of the rail (§6.4). The mechanics are those of §8.3.

### 14.8 What this example demonstrates

| Law / rule | Where |
|---|---|
| 1. Suki card, exposed object | `GlassCard`, glass level badge, inlet and outlet melted into the card, feet and base plate that belong to the devices |
| 2. Tinted metal | blue-slate ramp on the flange, the legs, the pads, the boss and the base plate |
| 3. Energy is the accent | liquid, halo, flow in the pipes, filling stream, arc and glow of the lever |
| 4. A single light | radial origins at 35 %/30 %, specular lines, `0 3 6` shadows |
| 5. Glass becomes object | layered tank, three-thickness glass tubes |
| 6. Vector and sober | no bitmap, large radii, counted details (screwed flange, screwed base plate, graduations) |
| 7. Look like the real hardware | tank silhouette: wide, shoulders, flange, low outlet, feet |
| 8. Two themes | same keys in `Dark` and `Light`, opaque light tubes in the light theme |
| 9. Behavior | relative anchoring, two directions, dead zone, quadratic response, spring, unpredictable particles, fades |
| 10. Suki precision | no exact field, since there is no value to retain |
| §6.5 Black box | two controls with one responsibility each, universal APIs, logic in the ViewModel |
