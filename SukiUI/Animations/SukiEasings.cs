using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;

namespace SukiUI.Animations
{

    public enum EasingIntensity
    {
        Soft,Normal,Strong
    }


    public class SukiEaseInBackOutBack : Easing
    {
        public EasingIntensity BounceIntensity { get; set; } = EasingIntensity.Normal;

        public override double Ease(double progress)
        {
            double Pi = Math.PI;
            double c1 = BounceIntensity switch
            {
                EasingIntensity.Soft => 0.9,
                EasingIntensity.Normal => 1.15,
                EasingIntensity.Strong => 1.5,
                _ => 1.0
            };

            double c2 = c1 * 1.525;

            if (progress < 0.5)
            {
                double term = 2 * progress;
                return (Math.Pow(term, 2) * ((c2 + 1) * term - c2)) / 2.0;
            }
            else
            {
                double term = 2 * progress - 2;
                return (Math.Pow(term, 2) * ((c2 + 1) * term + c2) + 2) / 2.0;
            }
        }
    }

    public class SukiEaseInOutBack: Easing
    {
        public EasingIntensity BounceIntensity { get; set; }= EasingIntensity.Normal;
        
        public override double Ease(double progress)
        {
            double c1 = BounceIntensity switch
            {
                EasingIntensity.Soft   => 0.9,
                EasingIntensity.Normal => 1.15,
                EasingIntensity.Strong => 1.5,
                _                      => 1.0
            };
            
            double c3 = c1 + 1;

            double t = progress;
            double smoothedStart = t * t * (2 - t); 
            double p = smoothedStart;

            return 1 + c3 * Math.Pow(p - 1, 3) + c1 * Math.Pow(p - 1, 2);
        }

    }
    
    public class SukiEaseOutBack: Easing
    {
        public EasingIntensity BounceIntensity { get; set; }= EasingIntensity.Normal;
     
         public override double Ease(double progress)
        {
            double c1 = BounceIntensity switch
            {
                EasingIntensity.Soft   => 0.9,
                EasingIntensity.Normal => 1.15,
                EasingIntensity.Strong => 1.5,
                _                      => 1.0
            };
        
            double  c3 = c1 + 1;
            
            return 1 + c3 * Math.Pow(progress - 1, 3) + c1 * Math.Pow(progress - 1, 2);
   
        }
    
    }
    
    public class SukiEaseOut : Easing
    {
        public override double Ease(double progress)
        {
            double warpedProgress = Math.Sqrt(progress);
            return 1.0 - Math.Pow(1.0 - warpedProgress, 3);
        }
    }
    
    public class SukiEaseInOut : Easing
    {
        public override double Ease(double progress)
        {
            double warpedProgress = Math.Sqrt(progress);
        
            if (warpedProgress < 0.5)
            {
                return 4.0 * warpedProgress * warpedProgress * warpedProgress;
            }
            else
            {
                double factor = -2.0 * warpedProgress + 2.0;
                return 1.0 - Math.Pow(factor, 3) / 2.0;
            }
        }
    }
}