using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;


namespace SukiUI.Animations
{
    public class SquishyDragExtensions
    {
        
        private class State
        {
            public Point? StartPoint;
            public ScaleTransform Scale = new(1, 1);
            public SkewTransform Skew = new(0, 0);
            public TranslateTransform Translate = new(0, 0);

            public double Intensity = 1;
            public double SquishDepth = 1;
            public bool EnableTilt = true;
            public bool EnableX = true;
            public bool EnableY = true;
            public bool ScaleByXY = true;
        }
        
        private static readonly AttachedProperty<State> StateProperty = AvaloniaProperty.RegisterAttached<Control, State>(
            "State",
            typeof(Control)
        );
        
        
        
        public static readonly AttachedProperty<bool> EnableXProperty = AvaloniaProperty.RegisterAttached<Control, bool>("EnableX", typeof(Control), true);               


        public static bool GetEnableX(Control control)
        {
            return control.GetValue(EnableXProperty);
        }

        public static void SetEnableX(Control control, bool value)
        {
            control.SetValue(EnableXProperty, value);
            control.GetValue(StateProperty).EnableX = value;
        }
        
        public static readonly AttachedProperty<bool> EnableYProperty = AvaloniaProperty.RegisterAttached<Control, bool>("EnableY", typeof(Control), true);                    


        public static bool GetEnableY(Control control)
        {
            return control.GetValue(EnableYProperty);
        }

        public static void SetEnableY(Control control, bool value)
        {
            control.SetValue(EnableYProperty, value);
            control.GetValue(StateProperty).EnableY = value;
        }
        
        public static readonly AttachedProperty<bool> EnableTiltProperty =
            AvaloniaProperty.RegisterAttached<Control, bool>(
                "EnableTilt", typeof(Control), true);

        public static bool GetEnableTilt(Control control)
        {
            return control.GetValue(EnableTiltProperty);
        }

        public static void SetEnableTilt(Control control, bool value)
        {
            control.SetValue(EnableTiltProperty, value);
            control.GetValue(StateProperty).EnableTilt = value;
        }
        
        
        public static readonly AttachedProperty<bool> ScaleByXYAxisProperty = AvaloniaProperty.RegisterAttached<Control, bool>("ScaleByXYAxis", typeof(Control), false);

        public static bool GetScaleByXYAxis(Control control)
        {
            return control.GetValue(ScaleByXYAxisProperty);
        }

        public static void SetScaleByXYAxis(Control control, bool value)
        {
            control.SetValue(ScaleByXYAxisProperty, value);
            control.GetValue(StateProperty).ScaleByXY = value;
        }
        
        public static readonly AttachedProperty<double> IntensityProperty = AvaloniaProperty.RegisterAttached<Control, double>("Intensity", typeof(Control), 1.0);

        public static readonly AttachedProperty<double> SquishDepthProperty = AvaloniaProperty.RegisterAttached<Control, double>("SquishDepth", typeof(Control), 1.0);

        public static double GetIntensity(Control control)
        {
            return control.GetValue(IntensityProperty);
        }

        public static void SetIntensity(Control control, double value)
        {
            control.SetValue(IntensityProperty, value);
            control.GetValue(StateProperty).Intensity = value;
        }

        public static double GetSquishDepth(Control control)
        {
            return control.GetValue(SquishDepthProperty);
        }

        public static void SetSquishDepth(Control control, double value)
        {
            control.SetValue(SquishDepthProperty, value);
            control.GetValue(StateProperty).SquishDepth = value;
        }
        
        
        
        
        public static readonly AttachedProperty<bool> EnableProperty = AvaloniaProperty.RegisterAttached<Control, bool>("Enable", typeof(Control), defaultValue: false);    
 

        public static bool GetEnable(Control control)
        {
            return control.GetValue(EnableProperty);
        }

        public static void SetEnable(Control control, bool value)
        {
            control.SetValue(EnableProperty, value);

            if (value)
            {
                var state = new State();
                control.SetValue(StateProperty, state);
                
                control.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
                control.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Bubble , handledEventsToo: true);
                control.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Bubble , handledEventsToo: true);
                
                var transformGroup = new TransformGroup
                {
                    Children = new Transforms
                    {
                        state.Scale,
                        state.Skew,
                        state.Translate
                    }
                };

                control.RenderTransform = transformGroup;

                control.PointerPressed += OnPointerPressed;
                control.PointerMoved += OnPointerMoved;
                control.PointerReleased += OnPointerReleased;
            }
            else
            {
                control.PointerPressed -= OnPointerPressed;
                control.PointerMoved -= OnPointerMoved;
                control.PointerReleased -= OnPointerReleased;
            }
        }
        
        
       private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not Control control) return;
            var state = control.GetValue(StateProperty);
            state.StartPoint = e.GetPosition(control);
        }

        private static void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (sender is not Control control) return;
            var state = control.GetValue(StateProperty);
            if (state.StartPoint is null) return;

            var current = e.GetPosition(control);
            var dx = current.X - state.StartPoint.Value.X;
            var dy = current.Y - state.StartPoint.Value.Y;

            double skewFactor = 0.02 * state.Intensity;
            double translateFactor = 0.05 * state.Intensity;
            double scaleFactor = 0.0001 * state.Intensity;

            if (state.EnableTilt)
            {
                var Xangle = dx * skewFactor;
                if (Xangle > 3 * state.SquishDepth)
                    Xangle = 3 * state.SquishDepth;
                if (Xangle < -3 * state.SquishDepth)
                    Xangle = -3 * state.SquishDepth;

                var Yangle = -dy * skewFactor;
                if (Yangle > 3 * state.SquishDepth)
                    Yangle = 3 * state.SquishDepth;
                if (Yangle < -3 * state.SquishDepth)
                    Yangle = -3 * state.SquishDepth;

                state.Skew.AngleX = (Yangle < 0 ? Xangle : -Xangle) * Math.Abs(Yangle) * 0.3;
                state.Skew.AngleY = (Xangle < 0 ? Yangle : -Yangle) * Math.Abs(Xangle) * 0.3;
            }

            if(state.EnableX)
                state.Translate.X = dx * translateFactor;
            if(state.EnableY)
                state.Translate.Y = dy * translateFactor;

            if (state.ScaleByXY)
            {
                if(state.EnableX)
                    state.Scale.ScaleX = 1 - dx * (scaleFactor *1.5);
                if(state.EnableY)
                    state.Scale.ScaleY = 1 - dy * (scaleFactor * 1.5);
            }
            else
            {
                if (state.EnableX)
                    state.Scale.ScaleX = 1 - Math.Abs(dx) * scaleFactor;
                if (state.EnableY)
                    state.Scale.ScaleY = 1 - Math.Abs(dy) * scaleFactor;
            }
        }

        private static void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (sender is not Control control) return;
            var state = control.GetValue(StateProperty);

            state.StartPoint = null;

            double startSkewX = state.Skew.AngleX;
            double startSkewY = state.Skew.AngleY;
            double startTransX = state.Translate.X;
            double startTransY = state.Translate.Y;
            double startScaleX = state.Scale.ScaleX;
            double startScaleY = state.Scale.ScaleY;

            var easing = new SukiSpringEase
            {
                Mass = 1,
                Damping = 7,
                Stiffness = 50
            };

            const int durationMs = 700;
            var startTime = DateTime.Now;

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            timer.Tick += (_, _) =>
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                double t = Math.Min(elapsed / durationMs, 1.0);
                double ease = easing.Ease(t);

                state.Skew.AngleX = startSkewX * (1 - ease);
                state.Skew.AngleY = startSkewY * (1 - ease);
                state.Translate.X = startTransX * (1 - ease);
                state.Translate.Y = startTransY * (1 - ease);
                state.Scale.ScaleX = startScaleX + (1 - startScaleX) * ease;
                state.Scale.ScaleY = startScaleY + (1 - startScaleY) * ease;

                if (t >= 1.0)
                {
                    state.Skew.AngleX = 0;
                    state.Skew.AngleY = 0;
                    state.Translate.X = 0;
                    state.Translate.Y = 0;
                    state.Scale.ScaleX = 1;
                    state.Scale.ScaleY = 1;
                    timer.Stop();
                }
            };

            timer.Start();
        }

    }
}