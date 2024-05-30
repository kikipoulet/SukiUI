# Stack Page

`StackPage` 内包含多个 `Content`，其数量可以设限

在标题栏上点击任何一个标题都可以跳转至该页面

`StackPage` 也可以跳转到已存在的 `Content`

## 展示

<img src="/controls/layout/stackpage.gif"/>

## 示例

```xml
<controls:SukiStackPage Limit="5">
    <!-- Content -->
</controls:SukiStackPage>
```

添加新页面:

```csharp
this.Get<StackPage>("StackSettings").Push("Network", b);
```

## 参阅

[Demo: SukiUI.Demo/Features/ControlsLibrary/StackPage/StackPageView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/StackPage/StackPageView.axaml)

[API: Controls/SukiStackPage.axaml.cs](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI/Controls/SukiStackPage.axaml.cs)