# Settings Layout

You can present `settings` through a `SettingsLayout`, which will update with the width of the window.

## Show

<img src="/controls/layout/settingslayout.gif" />

## Example

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

## See Also

[Demo: SukiUI.Demo/Features/Theming/ThemingView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/Theming/ThemingView.axaml)

[API: Controls/SettingsLayout.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/SettingsLayout.axaml.cs)