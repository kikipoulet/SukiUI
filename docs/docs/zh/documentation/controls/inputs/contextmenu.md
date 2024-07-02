# ContextMenu

为控件添加一个右键触发的上下文菜单

## 展示

<img src="/controls/inputs/contextmenus.gif"/>

## 示例

```xml
<controls:GlassCard>
    <controls:GlassCard.ContextMenu>
        <ContextMenu>
            <MenuItem Command="{Binding}"
                      CommandParameter="{x:False}"
                      Header="Option" />

            <MenuItem Header="Disabled Option" IsEnabled="False" />

            <MenuItem Command="{Binding}"
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

## 参阅

[Demo: SukiUI.Demo/Features/ControlsLibrary/ContextMenusView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/ContextMenusView.axaml)