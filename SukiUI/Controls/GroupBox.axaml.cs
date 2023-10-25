using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls
{
    public partial class GroupBox : UserControl
    {
        public GroupBox()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<Control> HeaderProperty =
            AvaloniaProperty.Register<GroupBox, Control>(nameof(Header), defaultValue: new TextBlock(){Text ="Header"});

        public Control Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly StyledProperty<string?> TextProperty =
            TextBlock.TextProperty.AddOwner<GroupBox>(new(
                defaultBindingMode: BindingMode.TwoWay,
                enableDataValidation: true));

        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}