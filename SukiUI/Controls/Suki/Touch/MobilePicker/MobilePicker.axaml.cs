using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using SukiUI.Dialogs;

namespace SukiUI.Controls.Touch.MobilePicker;

public partial class MobilePicker : UserControl
{
    public MobilePicker()
    {
        SetCurrentValue(ItemsProperty, new ObservableCollection<string>());
        SetCurrentValue(PopupScaleProperty, new ScaleTransform());
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public ISukiDialogManager DialogManager
    {
        get => GetValue(DialogManagerProperty);
        set => SetValue(DialogManagerProperty, value);
    }
    public static readonly StyledProperty<ISukiDialogManager> DialogManagerProperty =
        AvaloniaProperty.Register<MobilePicker, ISukiDialogManager>(
            nameof(DialogManager));
    

    private string? _subtitle;
    public string? SubTitle
    {
        get => _subtitle;
        set => SetAndRaise(SubTitleProperty, ref _subtitle, value );
    }
    public static readonly DirectProperty<MobilePicker, string?> SubTitleProperty =
        AvaloniaProperty.RegisterDirect<MobilePicker, string?>(
            nameof(SubTitle),
            o => o.SubTitle,
            (o, v) => o.SubTitle = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    
    private object? _title;
    public object? Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value );
    }
    public static readonly DirectProperty<MobilePicker, object?> TitleProperty =
        AvaloniaProperty.RegisterDirect<MobilePicker, object?>(
            nameof(Title),
            o => o.Title,
            (o, v) => o.Title = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    private string? _selectedItem;
    public string? SelectedItem
    {
        get => _selectedItem;
        set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value );
    }
    public static readonly DirectProperty<MobilePicker, string?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<MobilePicker, string?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    
    public static readonly StyledProperty<ObservableCollection<string>> ItemsProperty =
        AvaloniaProperty.Register<MobilePicker, ObservableCollection<string>>(nameof(Items));

    public ObservableCollection<string> Items
    {
        get { return GetValue(ItemsProperty)!; }
        set { SetValue(ItemsProperty, value ); }
    }

    private void OpenPopup(object sender, RoutedEventArgs e)
    {
        if (DialogManager is null)
            return;
        var control = new MobilePickerPopUp(DialogManager);

        if (control.DataContext is not MobilePickerPopUpViewModel vm)
            return;
        vm.Items = Items;
        vm.SelectedItem = SelectedItem;
 
        vm.SubTitle = SubTitle;
        vm.Title = Title;
        vm.MobilePicker = this;
        
        if (control.FindControl<Border>("rootBorder") is { } rootBorder)
        {
            rootBorder.RenderTransform = PopupScale;
            rootBorder.Width = PopupWidth;
        }

        DialogManager.CreateDialog().WithContent(control).Dismiss().ByClickingBackground().TryShow();

 
       
    }
    
            
    public static readonly StyledProperty<ScaleTransform> PopupScaleProperty =
        AvaloniaProperty.Register<MobilePicker, ScaleTransform>(nameof(PopupScale));

    public ScaleTransform PopupScale
    {
        get { return GetValue(PopupScaleProperty)!; }
        set
        {
            
            SetValue(PopupScaleProperty, value );
        }
    }
    

    
    public static readonly StyledProperty<int> PopupWidthProperty =
        AvaloniaProperty.Register<MobilePicker, int>(nameof(PopupWidth), defaultValue: 300,
            coerce: (_, value) => Math.Max(0, value));

    public int PopupWidth
    {
        get { return GetValue(PopupWidthProperty); }
        set
        {
            
            SetValue(PopupWidthProperty, value );
        }
    }

    private void InputElement_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        OpenPopup(sender, e);
    }
}
