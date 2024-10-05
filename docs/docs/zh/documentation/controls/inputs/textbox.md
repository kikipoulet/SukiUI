# TextBox

收集用户提供的信息的基础控件

## 示例

### Simple

<img src="https://sleekshot.app/api/download/8nnHYLrgchCe"/>

```xml
<TextBox Text="Hello" />
```

### Clear Button

<img src="https://sleekshot.app/api/download/tNkEf1yb0lml"/>

```xml
<TextBox theme:TextBoxExtensions.AddDeleteButton="True"  Text="Hello" />
```

### Prefix

<img src="https://sleekshot.app/api/download/354ntrKtfvXo"/>

```xml
<TextBox theme:TextBoxExtensions.Prefix="https://" Text="www.google.com" />
```

### Watermark

<img src="https://sleekshot.app/api/download/Y3odALgSfPCT"/>

```xml
<TextBox Watermark="Watermark" Text="" />
```

## 参阅

[Demo: SukiUI.Demo/Features/Dashboard/DashboardView.axaml](https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/Dashboard/DashboardView.axaml)