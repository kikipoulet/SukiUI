using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using SukiUI.Controls.TouchInput.TouchNumericPad;

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
    

    
    
    private string _selectedItem;
    public string SelectedItem
    {
        get => _selectedItem;
        set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value );
    }
    public static readonly DirectProperty<MobilePicker, string> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<MobilePicker, string>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    
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

        control.Height = PopupHeight;
        control.Width = PopupWidth;
        control.FindControl<Border>("rootBorder").RenderTransform = PopupScale;
        
        InteractiveContainer.ShowDialog(control , true);
 
       
    }
    
            
    public static readonly StyledProperty<ScaleTransform> PopupScaleProperty = AvaloniaProperty.Register<MobilePicker, ScaleTransform>(nameof(MobilePicker), defaultValue: new ScaleTransform());

    public ScaleTransform PopupScale
    {
        get { return GetValue(PopupScaleProperty); }
        set
        {
            
            SetValue(PopupScaleProperty, value );
        }
    }
    
    public static readonly StyledProperty<int> PopupHeightProperty = AvaloniaProperty.Register<MobilePicker, int>(nameof(MobilePicker), defaultValue: 200);

    public int PopupHeight
    {
        get { return GetValue(PopupHeightProperty); }
        set
        {
            
            SetValue(PopupHeightProperty, value );
        }
    }
    
    public static readonly StyledProperty<int> PopupWidthProperty = AvaloniaProperty.Register<MobilePicker, int>(nameof(MobilePicker), defaultValue: 300);

    public int PopupWidth
    {
        get { return GetValue(PopupWidthProperty); }
        set
        {
            
            SetValue(PopupWidthProperty, value );
        }
    }

}