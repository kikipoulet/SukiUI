# SukiUI

UI Theme for AvaloniaUI - inspired and using Citrus and Default theme 

Goal : have a simple consistent flat UI desktop theme for Avalonia other than Fluent design.

Planning - Ideas :
1. Cleaner code (because the library is very young)
2. More Controls -> be as close as possible to paid UI libraries
3. Color theme ? 

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Global1.png"></img>
<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Global3.png"></img>
<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Global2.png"></img>


# Installation

- Install SukiUI Nuget Package
- Reference SukiUI in your App.axaml file

```
<Application ...>
     <Application.Styles>
        ...
        <StyleInclude Source="avares://SukiUI/Theme/Index.xaml"/>
        ...
    </Application.Styles>
</Application>
```

# Usage

## Card and Hoverable

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/HoverableCard.png"></img>
``` 
<Border Classes="Card"></Border>
<Border Classes="Card Hoverable"></Border>
```

## ListBox

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/ListBox.png"></img>
```
 <ListBox>
      <TextBlock>item 1</TextBlock>
      <TextBlock>item 2</TextBlock>
      <TextBlock>item 3</TextBlock>
 </ListBox>
 ```
 
## Buttons

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Buttons.png"></img>
```
<Button Classes="Primary">
    <TextBlock>Primary</TextBlock>
</Button>
<Button Classes="Secondary">
    <TextBlock>Secondary</TextBlock>
</Button>
<Button>
    <TextBlock>Neutral</TextBlock>
</Button>
<Button Classes="Success">
    <TextBlock>Success</TextBlock>
</Button>
<Button Classes="Danger">
    <TextBlock>Danger</TextBlock>
</Button>
```

## MessageBox

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/MessageBox.png"></img>
```
 SukiUI.MessageBox.MessageBox.Info(this, "Title", "This is an information message that need to be read.");
 MessageBox.Success(this, "Title", "This is an Success message that need to be read.");
 MessageBox.Error(this, "Title", "This is an Success message that need to be read.");
```

## Notification


<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Notification.png"></img>
```
 WindowNotificationManager notificationManager;

public MainWindow()
{
    InitializeComponent();
    notificationManager = new WindowNotificationManager(this); 
}

private void ShowNotification(object sender, RoutedEventArgs e)
{
    var notif = new Avalonia.Controls.Notifications.Notification("title","message");
    notificationManager.Show(notif);
}
```

## ComboBox

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/ComboBox1.png"></img>
<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/ComboBox2.png"></img>
```
 <ComboBox PlaceholderText="Select an item">
    <ComboBoxItem>
       <TextBlock>Main Item 1</TextBlock>
    </ComboBoxItem>
    <ComboBoxItem>
        <TextBlock>Main Item 2</TextBlock>
    </ComboBoxItem>
</ComboBox>
```

## ToggleSwitch

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Switch1.png"></img>
<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Switch2.png"></img>
```
 <ToggleSwitch OffContent="No" OnContent="Yes" />
```

## ProgressBar

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/ProgressBar.png"></img>
```
<ProgressBar  Value="60" />
``` 

## Menu

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Menu.png"></img>
``` 
<Menu>
     <MenuItem Header="File">
             <MenuItem Header="File" />
             <MenuItem Header="Edit" />
             <MenuItem Header="Help" />
     </MenuItem>
     <MenuItem Header="Edit" />
     <MenuItem Header="Help" />
</Menu>
 ``` 
 
 ## DataGrid
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/DataGrid.png"></img>
 ```
 <DataGrid AutoGenerateColumns="True" IsReadOnly="True" />
 ``` 
 
 ## Calendar
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Calendar.png"></img>
 ```
 <Calendar></Calendar>
 ``` 
 
 ## Expender
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Expander1.png"></img>
  <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Expander2.png"></img>
 ```
 <Expander Header="Click To Expand">
           <TextBlock>Expanded</TextBlock>
 </Expander>
 
 ``` 
 
 ## NumericUpDown
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/NumericUpDown.png"></img>
 ``` 
 <NumericUpDown></NumericUpDown>
 ```
 
 ## RadioButton 
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/RadioButton.png"></img>
 ```
 <StackPanel Orientation="Vertical">
           <RadioButton Margin="5">Item 1</RadioButton>
           <RadioButton Margin="5">Item 2</RadioButton>
           <RadioButton Margin="5">Item 3</RadioButton>
</StackPanel>
 ```
 
 ## Slider
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Slider.png"></img>
 ``` 
 <Slider></Slider>
 ``` 
 
 ## Tabs
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Tab.png"></img>
 ```
 <TabControl>
        <TabItem Header="Tab 1" />
        <TabItem Header="Tab 2" />
        <TabItem Header="Tab 3" />
 </TabControl>
 ``` 
 
 ## TextBox
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/TextBox.png"></img>
 ``` 
 <TextBox Text="Element" />
 <TextBox Classes="FlatTextBox" Text="Element" />
 ``` 
 
 ## TextBlock
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/TextBlock.png"></img>
 ``` 
 <StackPanel>
      <TextBlock Classes="h1">h1</TextBlock>
      <TextBlock Classes="h2">h2</TextBlock>
      <TextBlock Classes="h3">h3</TextBlock>
      <TextBlock Classes="h4">h4</TextBlock>
      <TextBlock>Normal</TextBlock>
      <TextBlock Classes="Accent">Accent</TextBlock>
</StackPanel>
``` 

## TreeView 

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/TreeView.png"></img>
``` 
<TreeView>
      <TreeViewItem Header="blub">
          <TreeViewItem Header="blub" />
          <TreeViewItem Header="blub" />
      </TreeViewItem>
      <TreeViewItem Header="blub" />
      <TreeViewItem Header="blub" />
</TreeView>
```

## CircleProgressBar

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/CircleProgressBar.png"></img>
``` 
xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
...

<suki:CircleProgressBar Height="150" StrokeWidth="12" Value="50" Width="150">
             <TextBlock Classes="h3">50 %</TextBlock>
</suki:CircleProgressBar>
``` 

## ContextMenu

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/ContextMenu.png"></img>
```
<Border.ContextMenu>
     <ContextMenu>
           <MenuItem Header="Menu item 1" />
           <MenuItem Header="Menu item 2" />
           <Separator />
           <MenuItem Header="Menu item 3" />
     </ContextMenu>
</Border.ContextMenu>
``` 

## Loading

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Loading.png"></img> 

 ``` 
xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
...

<suki:Loading></suki:Loading>
``` 

## GroupBox

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/GroupBox.png"></img> 

 ``` 
xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
...

<suki:GroupBox Header="Test Header">
    <Grid Height="100" Width="150">
          <TextBlock VerticalAlignment="Center" HorizontalAlignment="Center">Test Content</TextBlock>
    </Grid>
</suki:GroupBox>
``` 
 
 ## GroupBox

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/PropertyGrid.png"></img> 

 ``` 
xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
...

<suki:PropertyGrid Name="propertyGrid" />

...

this.FindControl<PropertyGrid>("propertyGrid").Item = new Object();
``` 
 
