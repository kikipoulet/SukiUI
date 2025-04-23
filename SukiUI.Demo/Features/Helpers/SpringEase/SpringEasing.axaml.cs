using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using SukiUI.Animations;
using SukiUI.Converters;

namespace SukiUI.Demo.Features.Helpers
{
    public partial class SpringEasing : UserControl
    {

        public ObservableCollection<ObservableValue> ObservableValues { get; set; } =
            new ObservableCollection<ObservableValue>();
    
        public SpringEasing()
        {
            InitializeComponent();

            var chart = this.Get<CartesianChart>("MyChart");
            
            chart.Series =  [
                new LineSeries<ObservableValue>(ObservableValues)
                {
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0.2 
                }
            ];
            
            chart.XAxes = new List<Axis>
            {
                new Axis
                {
                    Labels = new string[] { },
                    IsVisible = false 
                }
            };

            chart.YAxes = new List<Axis>
            {
                new Axis
                {
                    Labels = new List<string>(){"0","1"},
                    LabelsPaint = new SolidColorPaint(SKColors.Gray,2), TextSize = 22,
                    SeparatorsPaint =  new SolidColorPaint(SKColors.LightSlateGray) 
                    { 
                        StrokeThickness = 1, 
                        PathEffect = new DashEffect(new float[] { 3, 3 }) 
                    } 
                    
                }
            };
            
            UpdateChart(new SukiSpringEase());
        }

        private void ParmetersChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {

            if (!IsInitialized)
                return;
            
          var neweasing = new SukiSpringEase()
          {
              Damping = (double)this.Get<NumericUpDown>("DampingBox").Value,
              Mass = (double)this.Get<NumericUpDown>("MassBox").Value,
              Stiffness = (double)this.Get<NumericUpDown>("StiffnessBox").Value,
          };

          if (neweasing.Damping == 0 || neweasing.Mass == 0 || neweasing.Stiffness == 0)
              return;
            
            
          UpdateChart(neweasing);
          this.Get<EasingDemoBoxControl>("DemoBox").Ease = neweasing;

        }

        private void UpdateChart(SukiSpringEase easing)
        {
            if (ObservableValues.Count == 0)
            {
                double start = 0;
                while (start <= 1)
                {
                    ObservableValues.Add(new ObservableValue(easing.Ease(start)));
                    start += 0.01;
                }
            }
            else
            {
                double start = 0;
                int i = 0;
                while (start <= 1)
                {
                    ObservableValues[i].Value = easing.Ease(start);
                    start += 0.01;
                    i++;
                }
            }
        }

        private void SetBase(object? sender, RoutedEventArgs e)
        {
            this.Get<NumericUpDown>("MassBox").Value = 1;
            this.Get<NumericUpDown>("DampingBox").Value = 10;
            this.Get<NumericUpDown>("StiffnessBox").Value = 50;
        }

        private void SetSmooth(object? sender, RoutedEventArgs e)
        {
            this.Get<NumericUpDown>("MassBox").Value = 1;
            this.Get<NumericUpDown>("DampingBox").Value = 10;
            this.Get<NumericUpDown>("StiffnessBox").Value = 30;
        }
    }
}