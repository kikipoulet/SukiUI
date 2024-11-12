# 背景

SukiUI 的背景渲染器可利用 GPU 加速和着色器来绘制复杂的运行时背景效果，且这些效果会随着主题变化。

本页提到的所有属性都可以通过 `SukiWindow` 中的 `Background{Property}` 属性访问。如果你希望在其他上下文中托管一个 `SukiBackground` 控件，那么这些属性应该采用类似的命名方式，但不需要 `Background` 前缀。

## 样式

通过 `BackgroundStyle` 可以更改默认的样式，包括 `Gradient`（渐变）、`Flat`（纯色）和 `Bubble`（气泡）。你也可以通过用 SKSL 编写的自定义着色器来创建自己的背景。

这里提供两种编写自定义着色器的方法：

#### BackgroundShaderFile

通过将嵌入的资源文件包含在你的应用中，并使用正确的文件扩展名（例如 `MyShader.sksl`），只需将 `BackgroundShaderFile` 属性设置为文件名（不带扩展名）。SukiUI 会自动在你的嵌入资源中搜索该文件并加载着色器。

SukiUI 也有一些着色器可以通过这种方式访问，如 `Cells` 和 `Waves`。

#### BackgroundShaderCode

可以不嵌入文件，直接将表示着色器的 SKSL 字符串赋值给此属性，就可以创建运行时效果并渲染。你也可以与该属性建立绑定以在运行时动态更改着色器代码，任何更改都会立即更新渲染结果。

#### 注意

当选择要渲染的背景样式时，如果同时有多个属性被设置，SukiUI 将按照以下顺序处理：`BackgroundShaderFile` -> `BackgroundShaderCode` -> `BackgroundStyle`

## 自定义样式

在 SukiUI，必须编写 [SKSL](https://github.com/google/skia/blob/main/src/sksl/README.md) 以渲染自定义背景。

这可能在一开始显得有些复杂，但语言本身很简单，你需要编写一个入口函数，该函数返回一个 `vec4`，表示每个像素的颜色。借助 GPU 的并行计算，可以非常快速地执行大量相对复杂的数学运算。

在编译着色器之前，SukiUI 会在这些效果中提供一组可用的变量（uniforms），供你在着色器中使用：

- `float iTime` - 背景开始渲染以来的时间刻度，仅在启用动画时变化。
- `float iDark` - 应用是处于`light`主题还是`dark`主题，0 表示`light`，1 表示`dark`。
- `float iAlpha` - 背景的透明度，主要由背景控件的透明度控制。
- `vec3 iResolution` - 背景的分辨率，以像素为单位，且只有`x` 和 `y`是有效的，`z` 始终为 0。
- `vec3 iPrimary` - 当前主题色的（不完全）表示。
- `vec3 iAccent` - 当前强调主题色的（不完全）表示。
- `vec3 iBase` - 预先计算的主题背景色，如果应用在`light`主题下则偏白色。

这些变量会自动包含在你提供的任何着色器文件或代码中，可以像使用全局变量一样使用它们。它们会随着每一帧更新，并且应用程序状态的任何更改都会反映在这些变量中。

最简单的着色器示例是我们的 `Flat` 样式，它只返回每个像素的基础背景颜色：

```glsl
vec4 main(vec2 fragCoord) {
    return vec4(iBase, iAlpha);
}
```

## 过渡效果

启用 `BackgroundTransitionsEnabled` 后，任何背景样式的更改都可以通过简单的透明度切换来实现“淡入淡出”，这也是 `iAlpha` 属性的主要用途。

可以设置 `BackgroundTransitionTime` 来定义过渡时间，单位为秒（默认为 1 秒）。

## 动画

SukiUI 的背景渲染器支持动画（以原生帧率运行），且通过 `BackgroundAnimationEnabled` 启用。除 `Flat` 以外，所有默认的 SukiUI 背景都支持动画。

::: warning
由于需要重绘整个[可视树](https://docs.avaloniaui.net/zh-Hans/docs/concepts/control-trees#%E5%8F%AF%E8%A7%86%E6%A0%91)，启用动画会对性能产生显著影响，因此建议仅在必要时使用。在一些测试中，我们发现启用动画时 CPU 使用率大约增加 5%，而 GPU 使用率增加约 20%。
:::

## 软件渲染可能

如果 [SkiaSharp](https://github.com/mono/SkiaSharp)（Avalonia 的渲染引擎）无法找到任何可用的硬件加速，SukiUI 将退回到软件渲染解决方案，即在 CPU 上渲染 `Flat` 背景样式。