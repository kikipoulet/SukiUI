# AutoCompleteBox

为 `TextBox` 添加补全功能，当你需要提示建议输入项时这将有用

## 展示

<img src="/controls/inputs/autocompletebox.gif" height="300px" width="300px"/>

## 示例

```xml
<AutoCompleteBox>
    <AutoCompleteBox.ItemsSource>
        <!-- Suggested strings -->
        <objectModel:ObservableCollection x:TypeArguments="system:String">
            <system:String>USA 1</system:String>
            <system:String>USA 2</system:String>
            <system:String>USA 3</system:String>
            <system:String>France</system:String>
            <system:String>England</system:String>
            <system:String>Germany</system:String>
            <system:String>Belgium</system:String>
            <system:String>China</system:String>
        </objectModel:ObservableCollection>
    </AutoCompleteBox.ItemsSource>
</AutoCompleteBox>
```

## 参阅

[Demo: SukiUI.Demo/Features/ControlsLibrary/CollectionsView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/CollectionsView.axaml) 