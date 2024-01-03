using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Extensions;
using System;
using System.Globalization;

namespace SukiUI.Controls.TouchInput.TouchNumericPad;

public partial class NumericPadPopUp : UserControl
{
    public NumericPadPopUp()
    {
        InitializeComponent();
        _textBlock = this.FindRequiredControl<TextBlock>("TextNombre");
    }

    public TouchNumericPad? rootControl = null;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private readonly TextBlock _textBlock;

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
        }
        catch (Exception) { }
    }

    private void Close(object sender, RoutedEventArgs e)
    {
        try
        {
            if (rootControl != null)
                rootControl.Value = Double.Parse(CurrentText, CultureInfo.InvariantCulture);
        }
        catch
        {
        }

        SukiHost.CloseDialog();
    }
}