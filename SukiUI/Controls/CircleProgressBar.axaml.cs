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

        private double _value = 50;

        public double Value
        {
            get => _value;
            set
            {
                _value = (int)(value * 3.6);
                SetValue(ValueProperty, _value);
            }
        }

        /// <summary>
        /// Defines the <see cref="Value"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<CircleProgressBar, double>(nameof(Value), defaultValue: 50, coerce: (o, d) => d * 3.6);

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