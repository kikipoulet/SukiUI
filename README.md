# SukiUI

UI Theme for AvaloniaUI - inspired and using Citrus and Default theme 

Goal : have a simple consistent flat UI desktop theme for Avalonia other than Fluent design.

Planning - Ideas :
1. Cleaner code (because the library is very young)
2. More Controls -> be as close as possible to paid UI libraries
3. Color theme ? 

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/1.png"></img>
<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/3.png"></img>

MessageBox and Notification
<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/2.png"></img>


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

``` 
<Border Classes="Card"></Border>
<Border Classes="Card Hoverable"></Border>
```

## ListBox

```
 <ListBox>
      <TextBlock>item 1</TextBlock>
      <TextBlock>item 2</TextBlock>
      <TextBlock>item 3</TextBlock>
 </ListBox>
 ```
 
## Buttons

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

```
 SukiUI.MessageBox.MessageBox.Info(this, "Title", "This is an information message that need to be read.");
 MessageBox.Success(this, "Title", "This is an Success message that need to be read.");
 MessageBox.Error(this, "Title", "This is an Success message that need to be read.");
```

## Notification

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

```
 <ToggleSwitch OffContent="No" OnContent="Yes" />
```

## ProgressBar

```
<ProgressBar  Value="60" />
``` 

## Menu

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
 
 ```
 <DataGrid AutoGenerateColumns="True" IsReadOnly="True" />
 ``` 
 
 ## Calendar
 
 ```
 <Calendar></Calendar>
 ``` 
 
 ## Expender
 
 ```
 <Expander Header="Click To Expand">
           <TextBlock>Expanded</TextBlock>
 </Expander>
 
 <Expander Classes="Naked" Header="Click To Expand Naked">
           <TextBlock>Expanded</TextBlock>
 </Expander>
 ``` 
 
 ## NumericUpDown
 
 ``` 
 <NumericUpDown></NumericUpDown>
 ```
 
 ## RadioButton 
 
 ```
 <StackPanel Orientation="Vertical">
           <RadioButton Margin="5">Item 1</RadioButton>
           <RadioButton Margin="5">Item 2</RadioButton>
           <RadioButton Margin="5">Item 3</RadioButton>
</StackPanel>
 ```
 
 ## Slider
 
 ``` 
 <Slider></Slider>
 ``` 
 
 ## Tabs
 
 ```
 <TabControl>
        <TabItem Header="Tab 1" />
        <TabItem Header="Tab 2" />
        <TabItem Header="Tab 3" />
 </TabControl>
 ``` 
 
 ## TextBox
 
 ``` 
 <TextBox Text="Element" />
 ``` 
 
 ## TextBlock
 
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

``` 
xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
...

<suki:CircleProgressBar Height="150" StrokeWidth="12" Value="50" Width="150">
             <TextBlock Classes="h3">50 %</TextBlock>
</suki:CircleProgressBar>
``` 

## ContextMenu

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
                        
 ``` 
xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
...

<suki:Loading></suki:Loading>
``` 
 
 
