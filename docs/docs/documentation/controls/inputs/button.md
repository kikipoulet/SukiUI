# Button

SukiUI has a handful of button styles, available in both the standard primary color, but also in the theme's accent color.

## Show

<img src="/controls/inputs/button.webp" height="300px" width="300px"/>

- Busy

<img src="/controls/inputs/button-busy.gif" height="300px" width="300px"/>

## Example

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

## See Also

[Demo: SukiUI.Demo/Features/ControlsLibrary/ButtonsView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/ButtonsView.axaml)