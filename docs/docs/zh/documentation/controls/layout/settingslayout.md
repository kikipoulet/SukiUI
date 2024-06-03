# Settings Layout

可以通过 `SettingsLayout` 方便地呈现 App 的设置页。同时，它也会随着窗口宽度的变化而更新。

## 展示

<img src="/controls/layout/settingslayout.gif" />

## 示例

```xml
<suki:SettingsLayout>
    <suki:SettingsLayout.Items>
        <objectModel:ObservableCollection x:TypeArguments="suki:SettingsLayoutItem">
            <suki:SettingsLayoutItem Header="Settings Part1">
                <suki:SettingsLayoutItem.Content>
                    <Border Background="LightGray" Height="300" />
                </suki:SettingsLayoutItem.Content>
            </suki:SettingsLayoutItem>

            <suki:SettingsLayoutItem Header="Settings Part 2">
                <suki:SettingsLayoutItem.Content>
                    <Border Background="LightGray" Height="300" />
                </suki:SettingsLayoutItem.Content>
            </suki:SettingsLayoutItem>

            <suki:SettingsLayoutItem Header="Settings Part 3">
                <suki:SettingsLayoutItem.Content>
                    <Border Background="LightGray" Height="300" />
                </suki:SettingsLayoutItem.Content>
            </suki:SettingsLayoutItem>
        </objectModel:ObservableCollection>
    </suki:SettingsLayout.Items>
</suki:SettingsLayout>
```

## 参阅

[Demo: SukiUI.Demo/Features/Theming/ThemingView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/Theming/ThemingView.axaml)

[API: Controls/SettingsLayout.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/SettingsLayout.axaml.cs)