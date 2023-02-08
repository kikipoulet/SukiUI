using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace SukiUI.Controls.TouchInput.TouchKeyboard;

public partial class TouchKeyboardPopUp : UserControl
{
    private TouchKeyboard rootControl;
    public TouchKeyboardPopUp(TouchKeyboard root, string text = "")
    {
        rootControl = root;
        CurrentText = text;
        InitializeComponent();
        _textBlock.Text = CurrentText;
    }

    public TouchKeyboardPopUp()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _textBlock = this.FindControl<TextBlock>("TextKeyboard");
    }
    

    private void Close(object sender, RoutedEventArgs e)
    {
        if(rootControl != null)
            rootControl.Text = CurrentText;

        InteractiveContainer.CloseDialog();
    }
    
    public string CurrentText = "";
    private TextBlock _textBlock;
    
    private void AddChar(object sender, RoutedEventArgs e)
    {
        CurrentText += ((TextBlock)((Button)sender).Content).Text;
        _textBlock.Text = CurrentText;
    }

    private void ButtonCapsOff_OnClick(object sender, RoutedEventArgs e)
    {
        this.FindControl<Button>("ButtonCapsOff").IsVisible = false;
        this.FindControl<Button>("ButtonCapsOn").IsVisible = true;
        
        foreach (var button in this.GetVisualDescendants().OfType<Button>())
        {
            try
            {
                TextBlock t = (TextBlock)button.Content;
                if ("azertyuiopqsdfghjklmwxcvbn".Contains(t.Text))
                    t.Text = t.Text.ToUpper();
            }catch(Exception exc){}
        }
    }

    private void ButtonCapsOn_OnClick(object sender, RoutedEventArgs e)
    {
        this.FindControl<Button>("ButtonCapsOff").IsVisible = true;
        this.FindControl<Button>("ButtonCapsOn").IsVisible =false;

        foreach (var button in this.GetVisualDescendants().OfType<Button>())
        {
            try
            {
                TextBlock t = (TextBlock)button.Content;
                if ("azertyuiopqsdfghjklmwxcvbn".ToUpper().Contains(t.Text))
                    t.Text = t.Text.ToLower();
            }catch(Exception exc){}
        }
    }

    private void ButtonBackspace(object sender, RoutedEventArgs e)
    {
        try
        {
            CurrentText = CurrentText.Remove(CurrentText.Length - 1);
            _textBlock.Text = CurrentText;
        }catch(Exception exc){}
    }
}