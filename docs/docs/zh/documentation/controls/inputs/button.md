# Button

SukiUI 提供了多种样式的按钮，并支持添加主题色

## 展示

<img src="/controls/inputs/button.webp" height="300px" width="300px"/>

- Busy

<img src="/controls/inputs/button-busy.gif" height="300px" width="300px"/>

## 示例

```xml
<Button>
    <!-- Content -->
</Button>
```

### Busy

```xml
...
xmlns:theme="clr-namespace:SukiUI.Theme;assembly=SukiUI"
...

<Button theme:ButtonExtensions.ShowProgress="true">
    <!-- Content -->
</Button>
```

## 参阅

[Demo: SukiUI.Demo/Features/ControlsLibrary/ButtonsView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/ButtonsView.axaml)