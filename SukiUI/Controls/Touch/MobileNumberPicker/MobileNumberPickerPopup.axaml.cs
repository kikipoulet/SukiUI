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

    private ISukiDialogManager? _dialogManager;
    
    public MobileNumberPickerPopup(MobileNumberPicker _mobile, ISukiDialogManager manager)
    {
        _dialogManager = manager;
        _mobileNumberPicker = _mobile;
        InitializeComponent();
        SetCurrentValue(_mobile.Value);
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private MobileNumberPicker? _mobileNumberPicker;
    
    public int CurrentValue = 0;

    private bool _isScrolling;
    private Point _startingPosition;

    private void OnPickerPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_mobileNumberPicker is null || sender is not InputElement input)
            return;
        _isScrolling = true;
        _startingPosition = e.GetPosition(this.FindControl<TextBlock>("CurrentValueText"));
        e.Pointer.Capture(input);
    }

    private void OnPickerPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isScrolling)
            return;
        _isScrolling = false;
        var difference = (_startingPosition.Y - e.GetPosition(this.FindControl<TextBlock>("CurrentValueText")).Y) / 20;
        SetCurrentValue((int)(CurrentValue + difference));
        e.Pointer.Capture(null);
    }

    private void OnPickerPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isScrolling && _mobileNumberPicker is not null)
        {
            var difference = (_startingPosition.Y - e.GetPosition(this.FindControl<TextBlock>("CurrentValueText")).Y) / 20;
            var temporaryValue = (int)(CurrentValue + difference);

            if (temporaryValue > _mobileNumberPicker.Maximum)
            {
                _startingPosition = e.GetPosition(this.FindControl<TextBlock>("CurrentValueText"));
                temporaryValue = _mobileNumberPicker.Maximum;
                CurrentValue = temporaryValue ;
            }


            if (temporaryValue < _mobileNumberPicker.Minimum)
            {
                temporaryValue = _mobileNumberPicker.Minimum;
                _startingPosition = e.GetPosition(this.FindControl<TextBlock>("CurrentValueText"));
                CurrentValue = temporaryValue;
            }
                

            
            
            SetTextValues(temporaryValue);
        }
    }

    private void SetTextValues(int temporaryValue)
    {
        if (_mobileNumberPicker is null)
            return;
        this.FindControl<TextBlock>("CurrentValueText").Text = temporaryValue.ToString();
        
        if(temporaryValue -1 < _mobileNumberPicker.Minimum)
            this.FindControl<TextBlock>("CurrentValueTextMinus1").Text = "";
        else
            this.FindControl<TextBlock>("CurrentValueTextMinus1").Text = (temporaryValue -1).ToString();
        
        if(temporaryValue + 1 > _mobileNumberPicker.Maximum)
            this.FindControl<TextBlock>("CurrentValueTextPlus1").Text = "";
        else
            this.FindControl<TextBlock>("CurrentValueTextPlus1").Text = (temporaryValue +1).ToString();
        
        if(temporaryValue +2 > _mobileNumberPicker.Maximum)
            this.FindControl<TextBlock>("CurrentValueTextPlus2").Text = "";
        else
            this.FindControl<TextBlock>("CurrentValueTextPlus2").Text = (temporaryValue +2).ToString();
        
        if(temporaryValue -2 < _mobileNumberPicker.Minimum)
            this.FindControl<TextBlock>("CurrentValueTextMinus2").Text = "";
        else
            this.FindControl<TextBlock>("CurrentValueTextMinus2").Text = (temporaryValue -2).ToString();
    }

    private void DoneClick(object sender, RoutedEventArgs e)
    {
        _dialogManager?.DismissDialog();
    }

    private void plus(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(CurrentValue + 1);
    }

    private void minus(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(CurrentValue - 1);
    }

    private void SetCurrentValue(int value)
    {
        if (_mobileNumberPicker is null)
            return;
        CurrentValue = Math.Clamp(value, _mobileNumberPicker.Minimum, _mobileNumberPicker.Maximum);
        _mobileNumberPicker.Value = CurrentValue;
        SetTextValues(CurrentValue);
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e) => _isScrolling = false;
}
