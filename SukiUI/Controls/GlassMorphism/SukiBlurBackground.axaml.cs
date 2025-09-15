using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace SukiUI.Controls.GlassMorphism
{
    public partial class SukiBlurBackground : UserControl
    {
        public static readonly StyledProperty<bool> IsDynamicProperty = AvaloniaProperty.Register<SukiBlurBackground, bool>(
            nameof(IsDynamic), false);

        public bool IsDynamic
        {
            get => GetValue(IsDynamicProperty);
            set
            {
                SetValue(IsDynamicProperty, value);
                this.Get<BlurBackground>("BB").IsDynamic = value;  
            }
        }

        public SukiBlurBackground()
        {
            InitializeComponent();
        }
        
        
    }
}