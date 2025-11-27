using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Dialogs;

namespace SukiUI.Controls.Touch.MobileNumberPicker;


public partial class MobileNumberPickerPopup : UserControl
{
    public MobileNumberPickerPopup()
    {
        InitializeComponent();
    }

    private ISukiDialogManager DialogManager;
    
    public MobileNumberPickerPopup(MobileNumberPicker _mobile, ISukiDialogManager manager)
    {
        DialogManager = manager;
        _MobileNumberPicker = _mobile;
        InitializeComponent();
        SetTextValues(_mobile.Value);
        CurrentValue = _mobile.Value;
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public MobileNumberPicker _MobileNumberPicker = null;
    
    public int CurrentValue = 0;

    private bool isScrolling = false;
    private Point StartingPosition;

    private void PointerPressed(object sender, PointerPressedEventArgs e)
    {
        isScrolling = true;
        StartingPosition = e.GetPosition(this.FindControl<TextBlock>("CurrentValueText"));
    }

    private void PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        isScrolling = false;
        var difference = (StartingPosition.Y - e.GetPosition(this.FindControl<TextBlock>("CurrentValueText")).Y ) /20;
       
         _MobileNumberPicker.Value = ((int)(CurrentValue + difference)) + modifier;
       CurrentValue = ((int)(CurrentValue + difference)) + modifier;
    }

    private int modifier = 0;

    private void PointerMoved(object sender, PointerEventArgs e)
    {
        if (isScrolling)
        {
            var difference = (StartingPosition.Y - e.GetPosition(this.FindControl<TextBlock>("CurrentValueText")).Y ) /20;
            var temporaryValue = (int)(CurrentValue + difference) + modifier;

            if (temporaryValue > _MobileNumberPicker.Maximum)
            {
                StartingPosition = e.GetPosition(this.FindControl<TextBlock>("CurrentValueText")); 
                temporaryValue = _MobileNumberPicker.Maximum;
                CurrentValue = temporaryValue ;
            }


            if (temporaryValue < _MobileNumberPicker.Minimum)
            {
                temporaryValue = _MobileNumberPicker.Minimum;
                StartingPosition = e.GetPosition(this.FindControl<TextBlock>("CurrentValueText"));
                CurrentValue = temporaryValue;
            }
                

            
            
            SetTextValues(temporaryValue);
        }
    }

    private void SetTextValues(int temporaryValue)
    {
      
        
        this.FindControl<TextBlock>("CurrentValueText").Text = temporaryValue.ToString();
        
        if(temporaryValue -1 < _MobileNumberPicker.Minimum)
            this.FindControl<TextBlock>("CurrentValueTextMinus1").Text = "";
        else
            this.FindControl<TextBlock>("CurrentValueTextMinus1").Text = (temporaryValue -1).ToString();
        
        if(temporaryValue + 1 > _MobileNumberPicker.Maximum)
            this.FindControl<TextBlock>("CurrentValueTextPlus1").Text = "";
        else
            this.FindControl<TextBlock>("CurrentValueTextPlus1").Text = (temporaryValue +1).ToString();
        
        if(temporaryValue +2 > _MobileNumberPicker.Maximum)
            this.FindControl<TextBlock>("CurrentValueTextPlus2").Text = "";
        else
            this.FindControl<TextBlock>("CurrentValueTextPlus2").Text = (temporaryValue +2).ToString();
        
        if(temporaryValue -2 < _MobileNumberPicker.Minimum)
            this.FindControl<TextBlock>("CurrentValueTextMinus2").Text = "";
        else
            this.FindControl<TextBlock>("CurrentValueTextMinus2").Text = (temporaryValue -2).ToString();
    }

    private void DoneClick(object sender, RoutedEventArgs e)
    {
        DialogManager.DismissDialog();
    }

    private void plus(object sender, RoutedEventArgs e)
    {
        SetTextValues(((int)CurrentValue) + 1);
        CurrentValue = ((int)(CurrentValue ) + 1);
        _MobileNumberPicker.Value = ((int)(CurrentValue )) + 1;
    }

    private void minus(object sender, RoutedEventArgs e)
    {
       
        SetTextValues(((int)CurrentValue) -1);
        CurrentValue = ((int)(CurrentValue) -1);
        _MobileNumberPicker.Value = ((int)(CurrentValue )) -1;
    }
}