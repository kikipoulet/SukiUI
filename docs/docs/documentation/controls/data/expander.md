# Expander

`Expander` is a content area which can be collapsed and expanded. It has styles for all `ExpandDirections`, using custom animation behaviour and controls internally to correctly animate showing/hiding any size content.

## Show

<img src="/controls/data/expander.gif" />

## Example

```xml
<Expander ExpandDirection="Down" Header="Down Expander">
    <TextBlock>Some Down Content</TextBlock>
</Expander>

<Expander ExpandDirection="Up" Header="Up Expander">
    <TextBlock>Some Up Content</TextBlock>
</Expander>

<Expander ExpandDirection="Right" Header="Right Expander">
    <TextBlock>Some Right Content</TextBlock>
</Expander>

<Expander ExpandDirection="Left" Header="Left Expander">
    <TextBlock>Some Left Content</TextBlock>
</Expander>
```

## See Also

[Demo: SukiUI.Demo/Features/ControlsLibrary/ExpanderView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/ExpanderView.axaml)