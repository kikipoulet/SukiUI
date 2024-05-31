# Stack Page

`StackPage` simply remembers everything it's `Content` property is set to, up to the Limit (default 5
items).

Clicking on the items in the header will unwind the stack to that item.

`StackPage` will also unwind the stack automatically if you set the Content to an object that is
already in it's stack.

## Show

<img src="/controls/layout/stackpage.gif"/>

## Example

```xml
<controls:SukiStackPage Limit="5">
    <!-- Content -->
</controls:SukiStackPage>
```

Push a new page:

```csharp
this.Get<StackPage>("StackSettings").Push("Network", b);
```

## See Also

[Demo: SukiUI.Demo/Features/ControlsLibrary/StackPage/StackPageView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/StackPage/StackPageView.axaml)

[API: Controls/SukiStackPage.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/SukiStackPage.axaml.cs)