# Button

SukiUI 提供了多种样式的按钮，并支持添加主题色

## 样式

### Standard

![button-standard](/controls/inputs/button-standard.png)

```xml
<Button Content="Button" ></Button>
```

### Flat

![button-flat](/controls/inputs/button-flat.png)

```xml
<Button Content="Button" Classes="Flat" ></Button>
```

### Rounded

![button-flat-rounded](/controls/inputs/button-flat-rounded.png)

```xml
<Button Content="Button" Classes="Flat Rounded" ></Button>
```

### Outlined

![button-outlined](/controls/inputs/button-outlined.png)

```xml
<Button Content="Button" Classes="Outlined" ></Button>
```

### Basic

![button-basic](/controls/inputs/button-basic.png)

```xml
<Button Content="Button" Classes="Basic" ></Button>
```

### Flat Accent

![button-flat-accent](/controls/inputs/button-flat-accent.png)

```xml
<Button Content="Button" Classes="Flat Accent" ></Button>
```

### Large

![button-large](/controls/inputs/button-large.png)

```xml
<Button Content="Button" Classes="Flat Large" ></Button>
```

## Busy/Loading 状态

<img src="/controls/inputs/button-busy.gif" width="300px"/>

### Axaml

```xml
...
xmlns:theme="clr-namespace:SukiUI.Theme;assembly=SukiUI"
...

<Button theme:ButtonExtensions.ShowProgress="true"></Button>
```

### C#

```Csharp
   MyButton.ShowProgress();

   MyButton.HideProgress();
```

## 参阅

[Demo: SukiUI.Demo/Features/ControlsLibrary/ButtonsView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/ButtonsView.axaml)