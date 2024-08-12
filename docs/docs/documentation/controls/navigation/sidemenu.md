# SukiSideMenu

`SukiSideMenu` is the main navigation control used in SukiUI, similar to `NavigationView` in other libraries.

## Show

<img src="/controls/navigation/sukisidemenu.webp" height="200px" width="200px"/>

## Example

```xml
<suki:SukiSideMenu 
    IsSearchEnabled="True"  // Enabled Search function
    >
    <suki:SukiSideMenu.Items>
        <suki:SukiSideMenuItem Header="Statistics">
            <suki:SukiSideMenuItem.Icon>
                <!-- Icon -->
            </suki:SukiSideMenuItem.Icon>
            <suki:SukiSideMenuItem.PageContent>
                <!-- Page Content -->
            </suki:SukiSideMenuItem.PageContent>
        </suki:SukiSideMenuItem>
    </suki:SukiSideMenu.Items>

    <suki:SukiSideMenu.ItemTemplate>
        <DataTemplate>
            <suki:SukiSideMenuItem 
                Classes="Compact"  // Enabled Compact SideMenuItem style
                Header="{Binding DisplayName}">
                <suki:SukiSideMenuItem.Icon>
                    <avalonia:MaterialIcon Kind="{Binding Icon}" />
                </suki:SukiSideMenuItem.Icon>
            </suki:SukiSideMenuItem>
        </DataTemplate>
    </suki:SukiSideMenu.ItemTemplate>

    <suki:SukiSideMenu.HeaderContent>
        <!-- Header Content -->
    </suki:SukiSideMenu.HeaderContent>

    <suki:SukiSideMenu.FooterContent>
        <!-- Footer Content -->
    </suki:SukiSideMenu.FooterContent>
</suki:SukiSideMenu>
```

## See Also

[Demo: SukiUI.Demo/SukiUIDemoView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/SukiUIDemoView.axaml)


[API: Controls/SukiSideMenu.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/SukiSideMenu.axaml.cs)