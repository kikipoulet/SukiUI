# SukiSideMenu

`SukiSideMenu` is the main navigation control used in SukiUI, similar to `NavigationView` in other libraries.

## Show

<img src="/controls/navigation/sukisidemenu.webp" height="200px" width="200px"/>

## Example

```xml
<suki:SukiSideMenu IsSearchEnabled="True">
    <suki:SukiSideMenu.Items>
        <suki:SukiSideMenuItem Header="Page Title" Classes="Compact" >
            <suki:SukiSideMenuItem.Icon>
                <!-- Icon -->
            </suki:SukiSideMenuItem.Icon>
            <suki:SukiSideMenuItem.PageContent>
                <!-- Page Content (Required) -->
            </suki:SukiSideMenuItem.PageContent>
        </suki:SukiSideMenuItem>

        <!-- Other Pages -->

    </suki:SukiSideMenu.Items>


    <suki:SukiSideMenu.HeaderContent>
        <!-- Header Content -->
    </suki:SukiSideMenu.HeaderContent>

    <suki:SukiSideMenu.FooterContent>
        <!-- Footer Content -->
    </suki:SukiSideMenu.FooterContent>
</suki:SukiSideMenu>
```

::: warning

If there is no content inside `suki:SukiSideMenuItem.PageContent`, it will trigger an exception similar to `System.InvalidOperationException: The control SukiUI.Controls.SukiSideMenuItem already has a visual parent StackPanel while trying to add it as a child of ContentPresenter (Name = PART_FirstBufferControl)`.

:::

## See Also

[Demo: SukiUI.Demo/SukiUIDemoView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/SukiUIDemoView.axaml)

[API: Controls/SukiSideMenu.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/SukiSideMenu.axaml.cs)