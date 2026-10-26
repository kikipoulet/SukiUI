using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls
{
    public partial class CircleProgressBar : UserControl
    {
        static CircleProgressBar()
        {
            WidthProperty.OverrideDefaultValue<CircleProgressBar>(150);
            HeightProperty.OverrideDefaultValue<CircleProgressBar>(150);
        }

        public CircleProgressBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public double Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Value"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<CircleProgressBar, double>(nameof(Value), defaultValue: 50,
                coerce: (_, value) => Math.Clamp(value, 0, 100));

        internal static readonly StyledProperty<double> SweepAngleProperty =
            AvaloniaProperty.Register<CircleProgressBar, double>(nameof(SweepAngle), defaultValue: 180);

        internal double SweepAngle
        {
            get => GetValue(SweepAngleProperty);
            private set => SetValue(SweepAngleProperty, value);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ValueProperty)
                SweepAngle = change.GetNewValue<double>() * 3.6;
        }

        public static readonly StyledProperty<double> StrokeWidthProperty =
            AvaloniaProperty.Register<CircleProgressBar, double>(nameof(StrokeWidth), defaultValue: 10);

        public double StrokeWidth
        {
            get { return GetValue(StrokeWidthProperty); }
            set { SetValue(StrokeWidthProperty, value); }
        }

        public static readonly StyledProperty<bool> IsIndeterminateProperty =
            AvaloniaProperty.Register<CircleProgressBar, bool>(nameof(IsIndeterminate), false);

        public bool IsIndeterminate
        {
            get => GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }
    }
}
