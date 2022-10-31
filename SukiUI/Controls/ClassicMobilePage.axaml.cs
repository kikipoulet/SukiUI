using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls
{
    public partial class ClassicMobilePage : UserControl
    {
        public ClassicMobilePage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<string> HeaderProperty =
            AvaloniaProperty.Register<CircleProgressBar, string>(nameof(Header), defaultValue: "Header");

        public string Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value ); }
        }
    }
}