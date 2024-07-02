# ContextMenu

Add a right-click triggered context menu for a control.

## Show

<img src="/controls/inputs/contextmenus.gif"/>

## Example

```xml
<controls:GlassCard>
    <controls:GlassCard.ContextMenu>
        <ContextMenu>
            <MenuItem Command="{Binding OptionClickedCommand}"
                      CommandParameter="{x:False}"
                      Header="Option" />

            <MenuItem Header="Disabled Option" IsEnabled="False" />

            <MenuItem Command="{Binding OptionClickedCommand}"
                      CommandParameter="{x:True}"
                      Header="Option With Icon">
                <MenuItem.Icon>
                    <PathIcon />
                </MenuItem.Icon>
            </MenuItem>

            <MenuItem Header="Disabled With Icon" IsEnabled="False">
                <MenuItem.Icon>
                    <PathIcon />
                </MenuItem.Icon>
            </MenuItem>

            <MenuItem Header="Separator Next" />
            <MenuItem Header="-" />

            <MenuItem Header="Submenu">
                <MenuItem Header="Sub-Submenu">
                    <MenuItem Command="{Binding}" Header="Nested Option" />
                    <MenuItem Header="Disabled Nested Option" IsEnabled="False" />
                </MenuItem>
            </MenuItem>
        </ContextMenu>
    </controls:GlassCard.ContextMenu>
</controls:GlassCard>
```

## See Also

[Demo: SukiUI.Demo/Features/ControlsLibrary/ContextMenusView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/ContextMenusView.axaml)