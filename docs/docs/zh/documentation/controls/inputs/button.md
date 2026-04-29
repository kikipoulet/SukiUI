# Button

SukiUI 提供了多种样式的按钮，并支持添加主题色

## 样式

### Standard

<img src="https://sleekshot.app/api/download/wWleWLZYWqR6" width="200px" />

```xml
<Button Content="Button" ></Button>
```

### Flat

<img src="https://sleekshot.app/api/download/tYN4eE9SLoot" width="200px" />

```xml
<Button Content="Button" Classes="Flat" ></Button>
```

### Rounded

<img src="https://sleekshot.app/api/download/zKfpsqmZzaHV" width="200px" />

```xml
<Button Content="Button" Classes="Flat Rounded" ></Button>
```

### Outlined

<img src="https://sleekshot.app/api/download/uVA8CTxZ989L" width="200px" />

```xml
<Button Content="Button" Classes="Outlined" ></Button>
```

### Basic

<img src="https://sleekshot.app/api/download/KTKtsjlVKsth" width="200px" />

```xml
<Button Content="Button" Classes="Basic" ></Button>
```

### Flat Accent

<img src="https://sleekshot.app/api/download/vGFvTPZG1E8i" width="200px" />

```xml
<Button Content="Button" Classes="Flat Accent" ></Button>
```

### Large

<img src="https://sleekshot.app/api/download/LiWhO4edwZi3" width="200px" />

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