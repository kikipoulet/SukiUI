# 安装

::: info 
写完本篇时，Avalonia 的版本是 `11.2.0-beta1` ，SukiUI 的版本是 `6.0.0-beta8`
:::

SukiUI 可以通过以下两种方式安装:
- [Nuget](https://www.nuget.org/packages/SukiUI) **推荐**
- 从 [Github Action](https://github.com/kikipoulet/SukiUI/actions/workflows/build.yml) 获取 CI 构建后的 `.dll`

## 准备工作

以下是 `Avalonia Template` 项目创建后，默认的包列表:

![](/getting-started/introduction-default-package-list.webp "default package list")

在这些包中，`Avalonia.Themes.Fluent` 可以被删除

::: tip
有关 `Avalonia Template`: [设置编辑器](https://docs.avaloniaui.net/zh-Hans/docs/get-started/set-up-an-editor)
:::

## 安装最新的 SukiUI

对于大多数用户来说，通过 Nuget 安装是不错的选择；但如果你想体验最新构建的功能，请从 Github Action 中下载构建

### 通过 Nuget 安装

```
dotnet add package SukiUI --version 6.0.0
```

::: tip
访问 [SukiUI on Nuget](https://www.nuget.org/packages/SukiUI) 获取更多信息
:::

### 通过 Github Action 安装

1. 访问 [SukiUI CI](https://github.com/kikipoulet/SukiUI/actions/workflows/build.yml)
2. 选择最后一个`workflow`
![](/getting-started/introduction-workflow.webp "workflow")

3. 下载
![](/getting-started/introduction-artifact.webp "artifact")

4. 在项目中添加引用
![](/getting-started/introduction-reference.webp "reference")

5. 选择刚刚下载的 `SukiUI.dll` 并添加

::: tip
最终，你的包列表应为：

![](/getting-started/introduction-final-package-list.webp "package list")
:::