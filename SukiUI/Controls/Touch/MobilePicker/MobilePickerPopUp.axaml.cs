using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Dialogs;

namespace SukiUI.Controls.Touch.MobilePicker;

public partial class MobilePickerPopUp : UserControl
{
    private ISukiDialogManager? dialogManager;

    public MobilePickerPopUp()
    {
        InitializeComponent();
    }

    public MobilePickerPopUp(ISukiDialogManager manager)
    {
        InitializeComponent();
        dialogManager = manager;

    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void DoneClick(object sender, RoutedEventArgs e)
    {
        dialogManager?.DismissDialog();
        if (DataContext is MobilePickerPopUpViewModel { MobilePicker: { } picker } model)
            picker.SelectedItem = model.SelectedItem;

    }
}
