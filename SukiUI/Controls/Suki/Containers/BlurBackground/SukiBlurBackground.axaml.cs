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

        // Pass-through for BlurBackground.OverlayOpacity: dialogs set it to 0 in their
        // template so the glass never flashes at its default full strength during the
        // frames before the host takes control of the fade.
        public static readonly StyledProperty<double> OverlayOpacityProperty =
            AvaloniaProperty.Register<SukiBlurBackground, double>(nameof(OverlayOpacity), 1d,
                coerce: (_, value) => Math.Clamp(value, 0d, 1d));

        public double OverlayOpacity
        {
            get => GetValue(OverlayOpacityProperty);
            set => SetValue(OverlayOpacityProperty, value);
        }

        public SukiBlurBackground()
        {
            InitializeComponent();
            UpdateBlurProperties();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsDynamicProperty || change.Property == IntensityFactorProperty ||
                change.Property == OverlayOpacityProperty)
                UpdateBlurProperties();
        }

        private void UpdateBlurProperties()
        {
            if (this.FindControl<BlurBackground>("BB") is not { } blur)
                return;
            blur.IsDynamic = IsDynamic;
            blur.IntensityFactor = IntensityFactor;
            blur.OverlayOpacity = OverlayOpacity;
        }
    }
}
