# Backgrounds

SukiUI includes a background renderer which utilises GPU acceleration and shaders to draw complex runtime background effects that update with theme changes. 

All properties mentioned on this page can be accessed out of the box via the `Background{Property}` properties present in `SukiWindow`. If you wish to host a `SukiBackground` control of your own in another context then these properties should be similarly named without the `Background` prefix.

## Styles

Changing style to one of the defaults is done via `BackgroundStyle` - Available out of the box are `Gradient`, `Flat` and `Bubble`. It is possible however to use custom shaders (written in SKSL) to create your own backgrounds. 

There are two methods to use your own shaders written in this language:

#### BackgroundShaderFile

By including an embedded resource in your application with the correct file extension (E.G. `MyShader.sksl`) it is possibly to simply set the `BackgroundShaderFile` property to the name of the file (without the extension). SukiUI will search your embedded resources for the file and load the shader for use automatically.

SukiUI includes a few shaders that are not included in the default enum but are nonetheless accessible this way including `Cells` and `Waves`.

#### BackgroundShaderCode

Simply assigning a string representing your shader to this property will create the runtime effect and render it without the need for embedded files, it is possible to bind to this property and dynamically alter the shader code at runtime if you so desire, any change will be reflected immediately.

#### Notes

When choosing which background style to render, if more than one property is assigned to a non-null value at once, then SukiUI will prioritize the properties in the following order `BackgroundShaderFile` -> `BackgroundShaderCode` -> `BackgroundStyle`

## Custom Styles

In order to render custom backgrounds via the properties mentioned before, it is necessary to write [SKSL](https://github.com/google/skia/blob/main/src/sksl/README.md). 

This might seem daunting at first but it is a fairly simple language in which you write an entry point function that returns a `vec4` representing the colour of each pixel. With GPU parallelization it is possible to execute large numbers of fairly complex mathematical calculations very quickly.

SukiUI automatically includes a set of uniforms in these effects before shader compilation for you to use in your shaders, these are:

- `float iTime` - A representation of the current ticks since the background began rendering. Only changed when animation is enabled.
- `float iDark` - Whether the application is in light or dark mode - 0 = light, 1 = dark.
- `float iAlpha` - The alpha at which the background is being drawn, largely controlled by opacity of the background control.
- `vec3 iResolution` - The resolution of the background in pixels, `x` and `y` are the only elements used, `z` is always 0.
- `vec3 iPrimary` - A (not exact) representation of the current active primary theme color.
- `vec3 iAccent` - A (not exact) representation of the current active accent theme color.
- `vec3 iBase` - A pre-calculated background colour for the theme - or an off-white if the app is in light mode.

These uniforms are included automatically in any shader file or code you supply and can be used as if they were globally scoped variables. They are updated per-frame and any change in the application state will be reflected in these uniforms.

The simplest example of a shader is our `Flat` style which simply returns the base background colour for every pixel.

```glsl
vec4 main(vec2 fragCoord) {
    return vec4(iBase, iAlpha);
}
```

## Transitions

Enabling `BackgroundTransitionsEnabled` will cause any change in the background style to "fade" between the two with a simple opacity switch, this is where the `iAlpha` property is mostly used. 

`BackgroundTransitionTime` can also be set to define the time in seconds for the transition (default: 1 second).

## Animation

The background renderer does support animation via `BackgroundAnimationEnabled` and will run at native framerates, all default SukiUI backgrounds support animation out of the box (except for `Flat`).

::: warning
Due to the need for an invalidation and repaint of the entire [visual tree](https://docs.avaloniaui.net/docs/concepts/control-trees#visual-tree), animations can have a significant impact on performance and it is recommended that you only use it if absolutely necessary. In limited testing we see a roughly 5% bump in CPU and 20% bump in GPU utilization _at all times_ when animation is enabled.
:::

## Software fallback

In the case [SkiaSharp](https://github.com/mono/SkiaSharp) (Avalonia's underlying rendering engine) is unable to find any usable hardware acceleration, SukiUI will fall back to a software rendering solution which is simply the `Flat` background style rendered on the CPU.