
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using Avalonia.Animation.Easings;


namespace SukiUI.Animations
{
    public class SquishyOverExtensions
    {
       private class State
        {
            public ScaleTransform Scale = new(1, 1);
            public SkewTransform Skew = new(0, 0);
            public TranslateTransform Translate = new(0, 0);

            public double Intensity = 1;
            public double SquishDepth = 1;
            public bool EnableTilt = true;
            
            public DispatcherTimer? ResetTimer;
            public SquishTransformOperation Target = new SquishTransformOperation();
        }

        private class SquishTransformOperation
        {
            public double ScaleX = 1;
            public double ScaleY = 1;
            public double TranslateX = 0;
            public double TranslateY = 0;
            public double AngleX = 0;
            public double AngleY = 0;
        }

        private static readonly AttachedProperty<State> StateProperty = AvaloniaProperty.RegisterAttached<Control, State>(
            "State",
            typeof(Control) 
        );
        
        public static readonly AttachedProperty<bool> EnableTiltProperty =
            AvaloniaProperty.RegisterAttached<Control, bool>(
                "EnableTilt", typeof(Control), true);

        public static bool GetEnableTilt(Control control) => control.GetValue(EnableTiltProperty);
        public static void SetEnableTilt(Control control, bool value)
        {
             control.SetValue(EnableTiltProperty, value);
             control.GetValue(StateProperty).EnableTilt = value;
        }

        public static readonly AttachedProperty<double> IntensityProperty =
            AvaloniaProperty.RegisterAttached<Control, double>(
                "Intensity", typeof(Control), 1.0);

        public static double GetIntensity(Control control) => control.GetValue(IntensityProperty);
        public static void SetIntensity(Control control, double value)
        {
            control.SetValue(IntensityProperty, value);
            control.GetValue(StateProperty).Intensity = value;
        }

        public static readonly AttachedProperty<double> SquishDepthProperty =
            AvaloniaProperty.RegisterAttached<Control, double>(
                "SquishDepth", typeof(Control), 1.0);

        public static double GetSquishDepth(Control control) => control.GetValue(SquishDepthProperty);
        public static void SetSquishDepth(Control control, double value)
        {
            control.SetValue(SquishDepthProperty, value);
            control.GetValue(StateProperty).SquishDepth = value;
        }


        public static readonly AttachedProperty<bool> EnableProperty =
            AvaloniaProperty.RegisterAttached<Control, bool>(
                "Enable", typeof(Control), defaultValue: false);

        public static bool GetEnable(Control control) => control.GetValue(EnableProperty);

        public static void SetEnable(Control control, bool value)
        {
            var existingState = control.GetValue(StateProperty);

            if (value)
            {
                if (existingState == null) 
                {
                    var state = new State
                    {
                        Intensity = GetIntensity(control),
                        SquishDepth = GetSquishDepth(control),
                        EnableTilt = GetEnableTilt(control)
                    };
                    control.SetValue(StateProperty, state);
                    
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
                    control.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative); 

                    control.PointerExited += OnPointerExited;
                    control.PointerEntered += OnPointerEntered;
                    
                    control.AddHandler(InputElement.PointerEnteredEvent, OnPointerEntered, RoutingStrategies.Bubble);
                    control.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Bubble);
                    control.AddHandler(InputElement.PointerExitedEvent, OnPointerExited, RoutingStrategies.Bubble);
                }
            }
            else
            {
                if (existingState != null)
                {
                    control.RemoveHandler(InputElement.PointerEnteredEvent, OnPointerEntered);
                    control.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
                    control.RemoveHandler(InputElement.PointerExitedEvent, OnPointerExited);
                    
                    existingState.ResetTimer?.Stop();
                    
                    control.RenderTransform = null; 
                    control.ClearValue(StateProperty);
                }
            }
        }

        private static void OnPointerEntered(object? sender, PointerEventArgs e)
        {
            
            if (sender is not Control control) return;
            var state = control.GetValue(StateProperty);
            if (state == null) return;
            
            state.ResetTimer?.Stop();
            state.ResetTimer = null;
            
            state.Target = GetTransformations(control, state, e.GetPosition(control));
            
            LaunchAnimation(state,700, new SukiSpringEase() 
            {
                Mass = 1,
                Damping = 9, 
                Stiffness = 50
            });
        }
        
        private static double Lerp(double from, double to, double t)
        {
            return from + (to - from) * t;
        }

        private static void LaunchAnimation(State state, double duration, Easing sukiSpringEase)
        {
            double durationMs = duration; 
            var startTime = DateTime.Now;

            var timer = new DispatcherTimer(DispatcherPriority.Render) 
            {
                Interval = TimeSpan.FromMilliseconds(16) 
            };
            
            double BaseTranslateX = state.Translate.X;
            double BaseTranslateY = state.Translate.Y;
            double BaseScaleX = state.Scale.ScaleX;
            double BaseScaleY = state.Scale.ScaleY;
            double BaseAngleX = state.Skew.AngleX;
            double BaseAngleY = state.Skew.AngleY;

            timer.Tick += (_, _) =>
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                double t = Math.Min(elapsed / durationMs, 1.0);
                double ease =sukiSpringEase.Ease(t); 
                
                state.Skew.AngleX = Lerp(BaseAngleX, state.Target.AngleX , ease);
                state.Skew.AngleY = Lerp(BaseAngleY, state.Target.AngleY , ease);
                state.Translate.X = Lerp(BaseTranslateX, state.Target.TranslateX , ease);
                state.Translate.Y = Lerp(BaseTranslateY, state.Target.TranslateY , ease);
                state.Scale.ScaleX = Lerp(BaseScaleX, state.Target.ScaleX , ease);
                state.Scale.ScaleY = Lerp(BaseScaleY, state.Target.ScaleY , ease);
                
                if (t >= 1.0)
                {
                    timer.Stop();
                    state.ResetTimer = null;
                }
            };

            state.ResetTimer = timer; 
            timer.Start();
        }

        private static void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (sender is not Control control) return;
            var state = control.GetValue(StateProperty);
            if (state == null) return; 
            
            var operations = GetTransformations(control, state, e.GetPosition(control));

            if (state.ResetTimer == null)
            {
                state.Scale.ScaleY = operations.ScaleY;
                state.Scale.ScaleX = operations.ScaleX;
                state.Translate.X = operations.TranslateX;
                state.Translate.Y = operations.TranslateY;
                state.Skew.AngleX = operations.AngleX;
                state.Skew.AngleY = operations.AngleY;
            }
            else
            {
                state.Target = operations;
            }
        }
        

        private static SquishTransformOperation GetTransformations(Control control, State state, Point currentPosition)
        {
            SquishTransformOperation operation = new();
            
             if (control.Bounds.Width <= 0 || control.Bounds.Height <= 0) 
                 return operation;


            var center = new Point(control.Bounds.Width / 2, control.Bounds.Height / 2);
            
            var dx = currentPosition.X - center.X;
            var dy = currentPosition.Y - center.Y;
            
            var normalizedDx = (control.Bounds.Width > 0) ? dx / (control.Bounds.Width / 2) : 0;
            var normalizedDy = (control.Bounds.Height > 0) ? dy / (control.Bounds.Height / 2) : 0;

            double baseIntensity = 10 * state.Intensity; 
            double skewFactor = 0.005 * baseIntensity;     
            double translateFactor = 0.025 * baseIntensity;
            double scaleFactor = 0.0001 * baseIntensity;   


            operation.TranslateX= normalizedDx * (control.Bounds.Width / 2) * translateFactor;
            operation.TranslateY = normalizedDy * (control.Bounds.Height / 2) * translateFactor;

            operation.ScaleX = 1 - Math.Abs(dx) * scaleFactor;
            operation.ScaleY = 1 - Math.Abs(dy) * scaleFactor;
            
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

              operation.AngleX = (Yangle < 0 ? Xangle : -Xangle) * Math.Abs(Yangle) * 0.3;
              operation.AngleY = (Xangle < 0 ? Yangle : -Yangle) * Math.Abs(Xangle) * 0.3;
            }
            else
            {
                 state.Skew.AngleX = 0;
                 state.Skew.AngleY = 0;
            }

            return operation;
        }


        private static void OnPointerExited(object? sender, PointerEventArgs e)
        {
            if (sender is not Control control) return;
            var state = control.GetValue(StateProperty);
            if (state == null) return; 

            state.ResetTimer?.Stop();
            
            state.Target.TranslateY = 0;
            state.Target.TranslateX = 0;
            state.Target.AngleX = 0;
            state.Target.AngleY = 0;
            state.Target.ScaleX = 1;
            state.Target.ScaleY = 1;
            
            
            LaunchAnimation(state,700, new SukiSpringEase() 
            {
                Mass = 1,
                Damping = 7, 
                Stiffness = 50
            });
            
        }

       
    }  
}
