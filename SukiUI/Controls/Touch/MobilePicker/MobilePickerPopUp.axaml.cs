using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Dialogs;
using SukiUI.Helpers;

namespace SukiUI.Controls.Touch.MobilePicker;

public partial class MobilePickerPopUp : UserControl
{
    private ISukiDialogManager dialogManager;
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
       dialogManager.DismissDialog();
        var model = ((MobilePickerPopUpVM)DataContext);

        model.mobilePicker.SelectedItem = model.SelectedItem;
        
    }
}

public class MobilePickerPopUpVM: SukiObservableObject
{
    private ObservableCollection<string> _items = new ObservableCollection<string>(){};

    public ObservableCollection<string> Items
    {
        get => _items;
        set =>  SetAndRaise(ref _items, value);
    }
    
    private string _selecteditem = null;

    public string SelectedItem
    {
        get => _selecteditem;
        set => SetAndRaise(ref _selecteditem, value);
    }
    
    private string _title = null;

    public string Title
    {
        get => _title;
        set => SetAndRaise(ref _title, value);
    }
    
    private string _subtitle = null;

    public string SubTitle
    {
        get => _subtitle;
        set => SetAndRaise(ref _subtitle, value);
    }

    public MobilePicker mobilePicker { get; set; }
}