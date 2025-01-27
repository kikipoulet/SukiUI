# SukiSideMenu

`SukiSideMenu` 是 SukiUI 主要的导航控件，类似其他库中的 `NavigationView`

## 展示

<img src="/controls/navigation/sukisidemenu.webp" height="200px" width="200px"/>

## 示例

```xml
<suki:SukiSideMenu IsSearchEnabled="True">
    <suki:SukiSideMenu.Items>
        <suki:SukiSideMenuItem Header="Page Title" Classes="Compact">
            <suki:SukiSideMenuItem.Icon>
                <!-- Icon -->
            </suki:SukiSideMenuItem.Icon>
            <suki:SukiSideMenuItem.PageContent>
                <!-- Page Content (必需) -->
            </suki:SukiSideMenuItem.PageContent>
        </suki:SukiSideMenuItem>
        
        <!-- 其他页面 -->

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
 
如果 `suki:SukiSideMenuItem.PageContent` 内没有内容，类似 `System.InvalidOperationException: The control SukiUI.Controls.SukiSideMenuItem already has a visual parent StackPanel while trying to add it as a child of ContentPresenter (Name = PART_FirstBufferControl)` 的异常会被触发。

:::

## 参阅

[Demo: SukiUI.Demo/SukiUIDemoView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/SukiUIDemoView.axaml)


[API: Controls/SukiSideMenu.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/SukiSideMenu.axaml.cs)