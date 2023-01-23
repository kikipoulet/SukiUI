using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls.TouchInput.TouchNumericPad;

public partial class NumericPadPopUp : UserControl
{
    public NumericPadPopUp()
    {
        InitializeComponent();
        _textBlock = this.FindControl<TextBlock>("TextNombre");
    }

    public TouchNumericPad rootControl = null;
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private TextBlock _textBlock;

    public string CurrentText = "";

    private void AddNumber(object sender, RoutedEventArgs e)
    {
        CurrentText += ((TextBlock)((Button)sender).Content).Text;
        _textBlock.Text = CurrentText;
    }

    private void RemoveChar(object sender, RoutedEventArgs e)
    {
        try
        {
            CurrentText = CurrentText.Remove(CurrentText.Length - 1);
            _textBlock.Text = CurrentText;
        }catch(Exception exc){}
    }

    private void Close(object sender, RoutedEventArgs e)
    {
        if(rootControl != null)
            rootControl.Value = Double.Parse(CurrentText, CultureInfo.InvariantCulture);
        
        InteractiveContainer.CloseDialog();
    }
}