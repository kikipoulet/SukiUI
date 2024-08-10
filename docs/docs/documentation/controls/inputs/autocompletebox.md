# AutoCompleteBox

Autocomplete function of `TextBox`. It would be helpful when you need to enter advice or help text.

## Show

<img src="/controls/inputs/autocompletebox.gif" height="300px" width="300px"/>

## Example

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

## See Also

[Demo: SukiUI.Demo/Features/ControlsLibrary/CollectionsView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/CollectionsView.axaml)