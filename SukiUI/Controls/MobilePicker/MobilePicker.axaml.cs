using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls.MobilePicker;

public partial class MobilePicker : UserControl
{
    public MobilePicker()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static readonly StyledProperty<string> SelectedItemProperty =
        AvaloniaProperty.Register<MobilePicker, string>(nameof(SelectedItem), defaultValue: "default");

    public string SelectedItem
    {
        get { return GetValue(SelectedItemProperty); }
        set { SetValue(SelectedItemProperty, value ); }
    }
    
    public static readonly StyledProperty<ObservableCollection<string>> ItemsProperty =
        AvaloniaProperty.Register<MobilePicker, ObservableCollection<string>>(nameof(Items), defaultValue: new ObservableCollection<string>());

    public ObservableCollection<string> Items
    {
        get { return GetValue(ItemsProperty); }
        set { SetValue(ItemsProperty, value ); }
    }

    private void OpenPopup(object sender, RoutedEventArgs e)
    {
        var control = new MobilePickerPopUp();

        var vm = ((MobilePickerPopUpVM)control.DataContext);
        vm.Items = Items;
        vm.SelectedItem = SelectedItem;
        vm.mobilePicker = this;
        
        MobileMenuPage.ShowDialogS(control , true);
 
       
    }
}