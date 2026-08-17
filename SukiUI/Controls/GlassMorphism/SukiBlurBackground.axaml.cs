using Avalonia;
using Avalonia.Controls;

namespace SukiUI.Controls.GlassMorphism
{
    public partial class SukiBlurBackground : UserControl
    {
        public static readonly StyledProperty<bool> IsDynamicProperty = AvaloniaProperty.Register<SukiBlurBackground, bool>(
            nameof(IsDynamic), false);

        public bool IsDynamic
        {
            get => GetValue(IsDynamicProperty);
            set => SetValue(IsDynamicProperty, value);
        }
        
        public static readonly StyledProperty<double> IntensityFactorProperty =
            AvaloniaProperty.Register<SukiBlurBackground, double>(nameof(IntensityFactor), 1d,
                coerce: (_, value) => Math.Max(0, value));

        public double IntensityFactor
        {
            get => GetValue(IntensityFactorProperty);
            set => SetValue(IntensityFactorProperty, value);
        }
        
        public SukiBlurBackground()
        {
            InitializeComponent();
            UpdateBlurProperties();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsDynamicProperty || change.Property == IntensityFactorProperty)
                UpdateBlurProperties();
        }

        private void UpdateBlurProperties()
        {
            if (this.FindControl<BlurBackground>("BB") is not { } blur)
                return;
            blur.IsDynamic = IsDynamic;
            blur.IntensityFactor = IntensityFactor;
        }
    }
}
