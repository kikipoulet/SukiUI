<div id="header" align="center">
 <kbd>
<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/suki_photo.jpg" width="200" height="200"></img> 
  </kbd>
<br/>
Suki is the name of my dog :-)
</div>
<br/>

# ✨ SukiUI

### Desktop UI Library for AvaloniaUI ! <img src="https://www.avaloniaui.net/img/logo/avalonia-white-purple.svg"></img>

<br/>

⚠️ Mobile Controls will be removed in a future version and moved to a new mobile library named [Cheryl UI](https://github.com/kikipoulet/CherylUI)
<br/><br/>
A lot of work has been done on mobile controls to create a 'serious' mobile solution for Avalonia in [Cheryl UI](https://github.com/kikipoulet/CherylUI). SukiUI will now focus only on desktop controls.
<br/><br/>
<br/>

<details>
  <summary>🎉 New 5.1.0 Release Notes</summary>
 <br/>
  - Some new animations and slight style changes <br/>
  - Switch animation smoother and reacting when long press before switching when released  <br/>
  <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/SwitchNewAnimation.gif"></img>  <br/><br/>
  - Trying to add features to existing controls to add rich interactions trough AttachedProperties and extensions methods.
  <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/FeaturfulControls.gif"></img>  <br/><br/>

  New code involved :<br/>

  ```
  ButtonSignIn.ShowProgress();   // Use ShowProgress Method on a Button to show Loading Circle
  ButtonSignIn.HideProgress();   // Hide the Loading Circle

  PasswordTextBox.Error("Wrong Password");   // Trigger the error animation with a custom message
 
  ```

  Moreover, I want to create "quick animations" that can be triggered on any control via extension methods. For new there are : 

  <br/>

  ```
  AnyControl.Vibrate(TimeSpan.FromSeconds(1));   // Make the control vibrate during 1 second
  AnyControl.Jump();   // Make the control do a double jump animation

  AnyControl.Animate<double>(WidthProperty, 100, 200);    // animate the width of a control from 100 to 200
 
  ```
<br/>
I want to focus the development of the library on these kind of interactions. I consider the style of the library almost definitive, and now it is important to make the controls rich and featureful. These kind of interactions are now the user projection of the quality of the software, so it is important to me to make it acessible for the desktop developers and included in the library. <br/> <br/>
So, please do not hesitate to suggest a micro interaction like this first example.

</details>

<details>
  <summary> New Control : SettingsLayout</summary>
 
   Orginazing a nice and elegant settings page has always been a kind of nightmare/mystery to me. How to organize these little TextBoxes and Switches in so much window space ? How to handle the window resizing ? ..
 
 This is why I ended up with this control to try to solve that layout problem while trying to be elegant and responsive in the style of SukiUI. [SettingsLayout Documentation](https://github.com/kikipoulet/SukiUI/wiki/3.-Controls#settingslayout)

 <img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/settingslayoutPage.gif"></img>

</details>

You have an idea ? A simple critic of the library ? Do not hesitate to send me an email to share your personal feedback.

## 👐 Demo

### Desktop Controls

Some desktop design "sketch" using SukiUI 5.0.0 :

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/Resume.gif"></img>

<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/ResumeDark.gif"></img>

[Desktop Controls Documentation](https://github.com/kikipoulet/SukiUI/wiki/3.-Controls)

### Mobile Controls

<kbd>
<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/MobileOverview.gif" style="float:left" ></img>
</kbd>
<kbd>
<img src="https://raw.githubusercontent.com/kikipoulet/SukiUI/main/Images/dashboard.gif" ></img> 
</kbd>


[Mobile Controls Documentation](https://github.com/kikipoulet/SukiUI/wiki/4.-Mobile-Controls)


## 📦 Usage

[Installation](https://github.com/kikipoulet/SukiUI/wiki/1.-Installation)


</br>

[Controls Documentation](https://github.com/kikipoulet/SukiUI/wiki/2.-Controls) 


