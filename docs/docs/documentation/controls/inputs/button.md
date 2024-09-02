# Button

SukiUI has a handful of button styles, available in both the standard primary color, but also in the theme's accent color.

<br/>

## Styles

### Standard

<img src="https://sleekshot.app/api/download/wWleWLZYWqR6" />

```xml
<Button Content="Button" ></Button>
```

### Flat

<img src="https://sleekshot.app/api/download/tYN4eE9SLoot" />

```xml
<Button Content="Button" Classes="Flat" ></Button>
```

### Rounded

<img src="https://sleekshot.app/api/download/zKfpsqmZzaHV" />

```xml
<Button Content="Button" Classes="Flat Rounded" ></Button>
```

### Outlined

<img src="https://sleekshot.app/api/download/uVA8CTxZ989L" />

```xml
<Button Content="Button" Classes="Outlined" ></Button>
```

### Basic

<img src="https://sleekshot.app/api/download/KTKtsjlVKsth" />

```xml
<Button Content="Button" Classes="Basic" ></Button>
```


### Flat Accent

<img src="https://sleekshot.app/api/download/vGFvTPZG1E8i" />

```xml
<Button Content="Button" Classes="Flat Accent" ></Button>
```

### Large

<img src="https://sleekshot.app/api/download/LiWhO4edwZi3" />

```xml
<Button Content="Button" Classes="Flat Large" ></Button>
```


<br/>

## Busy/Loading Button

<img src="/controls/inputs/button-busy.gif" height="300px" width="300px"/>

### Xaml

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


## See Also

[Demo: SukiUI.Demo/Features/ControlsLibrary/ButtonsView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/ButtonsView.axaml)
