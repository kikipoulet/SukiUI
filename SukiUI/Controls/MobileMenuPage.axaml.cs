using Avalonia.Controls;
using Avalonia.Markup.Xaml;

// using DialogHostAvalonia.Positioners;

namespace SukiUI.Controls
{
    public partial class MobileMenuPage : UserControl
    {
        public MobileMenuPage()
        {
            InitializeComponent();

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
