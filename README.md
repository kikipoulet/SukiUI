<div id="header" align="center">
 <kbd>
<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/suki_photo.jpg" width="200" height="200"></img> 
  </kbd>
<br/>
Suki is the name of my dog :-)
</div>
<br/>

# Suki UI

### UI Theme and additional Controls for AvaloniaUI ! <img src="https://www.avaloniaui.net/assets/Logo.svg"></img>




## Overview

SukiUI theme and controls in SukiUI 3.0.0 Nuget Package.

### Desktop

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/DesktopDemo.gif"></img>

### Mobile

Done with Avalonia 0.11 <br/>
SukiUI with Avalonia 0.11 is not available yet because I want to make more controls for mobile use before.

https://user-images.githubusercontent.com/19242427/187209227-03598bf8-c958-4577-b787-fa2ee48779d1.mp4
 

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


</br>

# Controls Documentation
</br>

## Menu - Page Layout
<details>
  <summary>Click to see more</summary>
     
### Desktop Page

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/DesktopMenuAvecItems.gif"></img>

``` 
<Window 
  ...
  Classes="NakedWindow" 
  xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
>

 <suki:DesktopPage
        Header="Suki UI Testing - New Project"
        LogoColor="#2f54eb"
        LogoKind="Xaml"
        MenuVisibility="True"
        Name="myPage">
        <suki:DesktopPage.MenuItems>
            <MenuItem Header="File">
                <MenuItem Header="File" />
                <MenuItem Header="Edit" />
                <MenuItem Header="Help" />
            </MenuItem>
            <MenuItem Header="Edit" />
            <MenuItem Header="Help" />
        </suki:DesktopPage.MenuItems>
    
    <Grid> Content </Grid>
    </suki:DesktopPage>
</Window>

``` 

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/DesktopMenuSansItems.gif"></img>

``` 
<Window 
  ...
  Classes="NakedWindow" 
  xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
>

<suki:DesktopPage
        Header="Suki UI Testing - New Project"
        LogoColor="#2f54eb"
        LogoKind="Xaml"
        MenuVisibility="False"
        Name="myPage">

	<Grid> Content </Grid>
</suki:DesktopPage>
</Window>

``` 

- The DesktopPage Control can show a dialog inside the window, go to Interactivity -> Dialog to get more informations

### Side Menu

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/SideMenu3.gif"></img>

Xaml Code Method
</br>
<details>
  <summary>Click to see more</summary>
  
  ``` 
  <suki:DesktopPage
        LogoColor="#2f54eb"
        LogoKind="Xaml"
        MenuVisibility="False"
        Title="Suki UI Testing - New Project">
	
  <suki:SideMenu>
      <suki:SideMenu.DataContext>
        <suki:SideMenuModel>
          
          <suki:SideMenuModel.HeaderContent>
            <!-- Header Content -->
          </suki:SideMenuModel.HeaderContent>
          
          <suki:SideMenuModel.MenuItems>	  
            <suki:SideMenuItem Header="DashBoard" Icon="CircleOutline">
              <suki:SideMenuItem.Content>
                <!-- Dashboard Content -->
              </suki:SideMenuItem.Content>
            </suki:SideMenuItem>
	    
	    <!-- Other SideMenuItems ... -->
	    
          </suki:SideMenuModel.MenuItems>
	  
	  <suki:SideMenuModel.FooterMenuItems>
	  	<!-- SideMenuItems -->
	  </suki:SideMenuModel.FooterMenuItems>
          
        </suki:SideMenuModel>
      </suki:SideMenu.DataContext>
    </suki:SideMenu>
    
  </suki:DesktopPage>
  ``` 
  
</details

  
Code-Behind method
  </br>
  
<details>
  <summary>Click to see more</summary>

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
  
</details>



</details>

## Button - Input
<details>
  <summary>Click to see more</summary>

### ToggleSwitch

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/ToggleSwitch3.gif"></img>

```
 <ToggleSwitch OffContent="No" OnContent="Yes" />
```


### Buttons

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Buttons3.gif"></img>
```
 <Button Classes="Primary">
     <TextBlock>Primary</TextBlock>
 </Button>
 
 <Button Classes="Accent">
     <TextBlock>Accent</TextBlock>
 </Button>

 <Button>
     <TextBlock>Neutral</TextBlock>
 </Button>

 <Button Classes="Outlined">
     <TextBlock>Outlined</TextBlock>
 </Button>
```
 
 ### Slider
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Slider3.gif"></img>
 ``` 
 <Slider IsSnapToTickEnabled="True" Maximum="100" Minimum="0" TickFrequency="1" Value="50"></Slider>
 ``` 


 ### TextBox
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/TextBoxBottom.gif"></img>
 ``` 
 <TextBox Classes="Prefix" Margin="5" Text="avaloniaui.net" Watermark="https://" />
 <TextBox Classes="Suffix" Margin="5" Text="avaloniaui" Watermark="@gmail.com" />
 <TextBox Margin="5" Text="Elem" />
 <TextBox Classes="BottomBar" Margin="5" Text="BottomBar" />
 <TextBox Classes="FlatTextBox" Text="Elem" />
 ``` 


### ComboBox

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

 ### NumericUpDown
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/NumericUpDown.png"></img>
 ``` 
 <NumericUpDown></NumericUpDown>
 ```
 
 ### RadioButton 
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/RadioButton.png"></img>
 ```
 <StackPanel Orientation="Vertical">
           <RadioButton Margin="5">Item 1</RadioButton>
           <RadioButton Margin="5">Item 2</RadioButton>
           <RadioButton Margin="5">Item 3</RadioButton>
</StackPanel>
 ```


</details>

## Progression Visuals
<details>
  <summary>Click to see more</summary>
     
### Stepper

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


### CircleProgressBar

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/CircleProgressBar3.gif"></img>
``` 
xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
...

<suki:CircleProgressBar Height="130" StrokeWidth="11" Value="20" Width="130">
             <TextBlock Classes="h3">20</TextBlock>
</suki:CircleProgressBar>
``` 

Animation coming asap : https://github.com/AvaloniaUI/Avalonia/issues/8659

### Loading

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Loading3.gif"></img> 

 ``` 
xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
...

<suki:Loading></suki:Loading>
``` 
     
     
### ProgressBar

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/ProgressBar3.gif"></img>
```
<ProgressBar  Value="60" />
``` 
     
</details>





 ## Data Presentation
<details>
  <summary>Click to see more</summary>

### PropertyGrid

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/PropertyGrid3.gif"></img> 

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

 
 ### DataGrid
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/DataGrid.gif"></img>
 ```
 <DataGrid Name="myDataGrid" AutoGenerateColumns="True" IsReadOnly="True" />
 ```
 ```
 this.FindControl<DataGrid>("myDataGrid").Items = new List<Person>();
 ```

### ListBox

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/ListBox.png"></img>
```
 <ListBox>
      <TextBlock>item 1</TextBlock>
      <TextBlock>item 2</TextBlock>
      <TextBlock>item 3</TextBlock>
 </ListBox>
 ```


### TreeView 

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


     
 ### GroupBox

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
     
     
</details>

 ## Interactivity
<details>
  <summary>Click to see more</summary>

### Notification


<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Notification3.gif"></img>
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

### Dialog

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Dialog3.gif"></img>

Working when using DesktopPage control 

Method 1 :
```
 // This static method will search the first DesktopPage control in your app and display the dialog
 	SukiUI.Controls.DesktopPage.ShowDialogS(  new MyUserControl()  );
 
 // Close the dialog anywhere in your app
 	SukiUI.Controls.DesktopPage.CloseDialogS();

```

Method 2 :
```
 // Call the method directly from the DesktopPage Control

 FindControl<DesktopPage>("MyDesktopPage").ShowDialog(  new TextBlock() { Text = "This is an example !" }  );

```

This is done with the DialogHost library ( https://github.com/AvaloniaUtils/DialogHost.Avalonia ), thanks to them !

 ### Expander
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Expander.gif"></img>

 ```
 <Expander Header="Click To Expand">
           <TextBlock>Expanded</TextBlock>
 </Expander>
 
 ``` 

### MessageBox

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/MessageBox3.gif"></img>
```
 SukiUI.MessageBox.MessageBox.Info(this, "Title", "This is an information message that need to be read.");

```



     
</details>


 
 

 


 ## Others
<details>
  <summary>Click to see more</summary>

### Tabs
 
 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Tabs3.gif"></img>
 ```
  <TabControl>
       <TabItem Header="Tab 1" />
       <TabItem Header="Tab 2" />
       <TabItem Header="Tab 3" />
  </TabControl>
  
 ``` 

### Card and Hoverable

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Hoverable.gif"></img>
``` 
<Border Classes="Card"></Border>
<Border Classes="Card Hoverable"></Border>
```


 
 ### TextBlock
 
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


 

 



### ContextMenu

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


</details>
 
