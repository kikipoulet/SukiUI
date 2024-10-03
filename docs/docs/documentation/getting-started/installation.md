# Installation

::: info
When this page was written, the latest Avalonia version was `11.2.0-beta1` with SukiUI version `6.0.0-beta8`
:::

SukiUI can be installed in two ways:
- [Nuget](https://www.nuget.org/packages/SukiUI) **Recommended**
- CI Artifacts from [Github Action](https://github.com/kikipoulet/SukiUI/actions/workflows/build.yml)

## Prepare your application

The following picture shows packages already installed with the default `Avalonia Template`:

![](/getting-started/introduction-default-package-list.webp "default package list")

In these packages, `Avalonia.Themes.Fluent` will no longer be necessary and can be removed.

::: tip
About `Avalonia Template`: [Set up an editor](https://docs.avaloniaui.net/docs/get-started/set-up-an-editor)
:::

## Install latest SukiUI package

The Nuget installation is suitable for most users, but if you want to use the latest build, you can download the dll from Github Action after the automatic build.

### Via Nuget

```
dotnet add package SukiUI --version 6.0.0
```

You're done !

::: tip
Visit [SukiUI on Nuget](https://www.nuget.org/packages/SukiUI) for more information
:::

### Via Github Action

<details>
  <summary>Github Action Guide</summary>

1. Visit [SukiUI CI](https://github.com/kikipoulet/SukiUI/actions/workflows/build.yml)
2. Select the latest workflow
![](/getting-started/introduction-workflow.webp "workflow")

3. Download the artifact
![](/getting-started/introduction-artifact.webp "artifact")

4. Add reference
![](/getting-started/introduction-reference.webp "reference")

5. Select `SukiUI.dll` you downloaded

::: tip
The package list should be:

![](/getting-started/introduction-final-package-list.webp "package list")
:::
 </details>
