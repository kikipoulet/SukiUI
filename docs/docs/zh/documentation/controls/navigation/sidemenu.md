# SukiSideMenu

`SukiSideMenu` 是 SukiUI 主要的导航控件，类似其他库中的 `NavigationView`

## 展示

<img src="/controls/navigation/sukisidemenu.webp" height="200px" width="200px"/>

## 示例

```xml
<suki:SukiSideMenu>
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

    <suki:SukiSideMenu.HeaderContent>
        <!-- Header Content -->
    </suki:SukiSideMenu.HeaderContent>

    <suki:SukiSideMenu.FooterContent>
        <!-- Footer Content -->
    </suki:SukiSideMenu.FooterContent>
</suki:SukiSideMenu>
```

## 参阅

[Demo: SukiUI.Demo/SukiUIDemoView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/SukiUIDemoView.axaml)


[API: Controls/SukiSideMenu.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/SukiSideMenu.axaml.cs)