# SukiUI


<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/suki_photo.jpg" width="200" height="200"></img> Suki is the name of my dog :-)

<br/>

UI Theme and Controls for AvaloniaUI 

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/GlobalApp.gif"></img>

### Planning - Ideas :
- Cleaner code (because the library is very young)
- More Controls (and make actual richer)
- Dark Theme
- out of the box Mobile Theme !

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

# Mobile (Testing Preview)

Screen video of an empty android app in debug mode with avalonia mobile, trying to use Suki UI on mobile. 
Controls need lots of small modifications (mostly sizing) but can look great on mobile.

---> The goal is to make asap a style file that adapt every control to mobile to be used out of the box

https://user-images.githubusercontent.com/19242427/162712044-ab5e4259-3bee-4d70-b164-c368ae141c0d.mp4

# Usage

## Desktop Page

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/DesktopMenu.gif"></img>

``` 
<Window 
  ...
  Classes="NakedWindow" 
  xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
>

  <suki:DesktopPage LogoColor="#2f54eb" LogoKind="Xaml">
    <suki:DesktopPage.MenuItems>
      <MenuItem Header="File">
        <MenuItem Header="File" />
        <MenuItem Header="Edit" />
        <MenuItem Header="Help" />
      </MenuItem>
      <MenuItem Header="Edit" />
      <MenuItem Header="Help" />
    </suki:DesktopPage.MenuItems>
    
    <Grid>
          <TextBlock>Content<TextBlock/>
    <Grid/>
    <suki:DesktopPage/>
<Window/>

``` 

## Side Menu

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/SideMenu.gif"></img>

- YourUsercontrol.axaml
``` 
<Grid Name="myGrid"></Grid>
``` 

- YourUserControl.axaml.cs
``` 
            InitializeComponent();

            this.FindControl<Grid>("myGrid").Children.Add(new SideMenu()
            {
                DataContext = new SideMenuModel()
                {
                    CurrentPage = new Grid() { Background = Brushes.WhiteSmoke },
                    
                    HeaderContent = new TextBlock(){Text = "Jean ValJean"},
                    
                    MenuItems = new List<SideMenuItem>()
                    {
                        new SideMenuItem()
                        {
                            Icon = Material.Icons.MaterialIconKind.CircleOutline,
                            Header = "Dashboard",
                            Content = new TextBlock(){Text = "Dashboard Page"}
                        },
                        
                        ...
                    }
                }
            }); 
``` 

## Mobile Page

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/MobileMenu.gif"></img>

Menu for a mobile app to switch between pages and go back

``` 
<UserControl 
  ...
  xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
>

  <suki:MobilePage  Header="Test" Name="MobileMenu" >
    <Grid >
      <Button Click="AddPage" VerticalAlignment="Center" HorizontalAlignment="Center">
        <TextBlock>Go To A Next Page</TextBlock>
      </Button>
    </Grid>
  </suki:MobilePage>
<UserControl/>

``` 

``` 

public void NavigateToNewPage(){
     
     var menu = this.FindControl<MobilePage>("MobileMenu");
     menu.NewPage("Header", new RecursivePage());
     
}

``` 


## ToggleSwitch

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/ToggleSwitch.gif"></img>

```
 <ToggleSwitch OffContent="No" OnContent="Yes" />
```

## Stepper

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Stepper.gif"></img>


``` 
xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
...

<suki:Stepper Name="myStep" />
```

```
this.FindControl<Stepper>("myStep").Steps = new List<string>() { "one", "two", "thre", "four", "five" };
this.FindControl<Stepper>("myStep").Index = 2;
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

## Loading

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Loading.gif"></img> 

 ``` 
xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
...

<suki:Loading></suki:Loading>
``` 

 ## PropertyGrid

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/PropertyGrid.gif"></img> 

 ``` 
xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
...

<suki:PropertyGrid Name="propertyGrid" />

...

this.FindControl<PropertyGrid>("propertyGrid").Item = new Person()
{
     Name = "Billy",
     Partner = new Person()
     {
          Name = "Charles"
     }
};
``` 

 
## Buttons

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Buttons.gif"></img>
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


## Notification


<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Notification.gif"></img>
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
 
 
 ## Tabs
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Tab.png"></img>
 ```
  <TabControl>
       <TabItem Header="Tab 1" />
       <TabItem Header="Tab 2" />
       <TabItem Header="Tab 3" />
  </TabControl>
  
  <TabControl Classes="FlatTabControl">
       <TabItem Classes="FlatTabItem" Header="Tab 1" />
       <TabItem Classes="FlatTabItem" Header="Tab 2" />
       <TabItem Classes="FlatTabItem" Header="Tab 3" />
  </TabControl>
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

## MessageBox

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/MessageBox.png"></img>
```
 SukiUI.MessageBox.MessageBox.Info(this, "Title", "This is an information message that need to be read.");
 MessageBox.Success(this, "Title", "This is an Success message that need to be read.");
 MessageBox.Error(this, "Title", "This is an Success message that need to be read.");
```


## ComboBox

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/ComboBox.gif"></img>

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


## ProgressBar

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/ProgressBar.png"></img>
```
<ProgressBar  Value="60" />
``` 

## Card and Hoverable

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Hoverable.gif"></img>
``` 
<Border Classes="Card"></Border>
<Border Classes="Card Hoverable"></Border>
```
 
 ## DataGrid
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/DataGrid.gif"></img>
 ```
 <DataGrid Name="myDataGrid" AutoGenerateColumns="True" IsReadOnly="True" />
 ```
 ```
 this.FindControl<DataGrid>("myDataGrid").Items = new List<Person>();
 ```

 
 ## Slider
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Slider.gif"></img>
 ``` 
 <Slider IsSnapToTickEnabled="True" Maximum="100" Minimum="0" TickFrequency="1" Value="50"></Slider>
 ``` 


 ## TextBox
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/TextBox.gif"></img>
 ``` 
 <TextBox Classes="Prefix" Margin="5" Text="avaloniaui.net" Watermark="https://" />
 <TextBox Classes="Suffix" Margin="5" Text="avaloniaui" Watermark="@gmail.com" />
 <TextBox Margin="5" Text="Elem" />
 <TextBox Classes="FlatTextBox" Text="Elem" />
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

## Calendar
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Calendar.png"></img>
 ```
 <Calendar></Calendar>
 ``` 
 
 ## Expander
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Expander.gif"></img>

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

## TreeView 

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/TreeView.gif"></img>
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



 
