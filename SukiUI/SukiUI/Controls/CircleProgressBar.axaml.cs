using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls
{
    public partial class CircleProgressBar : UserControl
    {
        public CircleProgressBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<CircleProgressBar, int>(nameof(Value), defaultValue: 50);

        public int Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, (int)(value*3.6)); }
        }

        public static readonly StyledProperty<int> HeightProperty =
        AvaloniaProperty.Register<CircleProgressBar, int>(nameof(Height), defaultValue: 150);

        public int Height
        {
            get { return GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly StyledProperty<int> WidthProperty =
        AvaloniaProperty.Register<CircleProgressBar, int>(nameof(Width), defaultValue: 150);

        public int Width
        {
            get { return GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly StyledProperty<int> StrokeWidthProperty =
        AvaloniaProperty.Register<CircleProgressBar, int>(nameof(StrokeWidth), defaultValue: 10);

        public int StrokeWidth
        {
            get { return GetValue(StrokeWidthProperty); }
            set { SetValue(StrokeWidthProperty, value); }
        }

        
        
    }
}
