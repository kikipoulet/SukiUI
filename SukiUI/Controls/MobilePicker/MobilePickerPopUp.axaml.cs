using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace SukiUI.Controls.MobilePicker;

public partial class MobilePickerPopUp : UserControl
{
    public MobilePickerPopUp()
    {
        InitializeComponent();
        
  
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void DoneClick(object sender, RoutedEventArgs e)
    {
        InteractiveContainer.CloseDialog();
        var model = ((MobilePickerPopUpVM)DataContext);

        model.mobilePicker.SelectedItem = model.SelectedItem;
    }
}

public class MobilePickerPopUpVM: ReactiveObject
{
    private ObservableCollection<string> _items = new ObservableCollection<string>(){};

    public ObservableCollection<string> Items
    {
        get => _items;
        set => this.RaiseAndSetIfChanged(ref _items, value);
    }
    
    private string _selecteditem = null;

    public string SelectedItem
    {
        get => _selecteditem;
        set => this.RaiseAndSetIfChanged(ref _selecteditem, value);
    }

    public MobilePicker mobilePicker;
}