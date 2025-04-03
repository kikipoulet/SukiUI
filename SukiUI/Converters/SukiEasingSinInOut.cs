using Avalonia.Animation.Easings;

namespace SukiUI.Converters
{

    public enum EasingIntensity
    {
        Soft,Normal,Strong
    }
    
    // There should be better implementation btw, as I'm not a mathematician I asked chatgpt for functions close to what I wanted.
    public class SukiEasingSmoothOutBounced: Easing
    {
        public EasingIntensity BounceIntensity { get; set; }= EasingIntensity.Normal;
       public override double Ease(double progress)
        {

            double warpedProgress = Math.Sqrt(progress);

            double baseEaseValue;
            if (warpedProgress < 0.5)
            {
                baseEaseValue = 4.0 * warpedProgress * warpedProgress * warpedProgress;
            }
            else
            {
                double factor = -2.0 * warpedProgress + 2.0;
                baseEaseValue = 1.0 - Math.Pow(factor, 3) / 2.0;
            }   

     
            double c1 = BounceIntensity switch
            {
                EasingIntensity.Soft   => 0.65,
                EasingIntensity.Normal => 1.0, 
                EasingIntensity.Strong => 1.3,  
                _                      => 1.0
            };
       
            double c3 = c1 + 1.0;

            double p = baseEaseValue - 1.0;
        
            return 1.0 + c3 * p * p * p + c1 * p * p;
        }
    }
    
    public class SukiEasingOutBounced: Easing
    {
        public EasingIntensity BounceIntensity { get; set; }= EasingIntensity.Normal;
     
         public override double Ease(double progress)
        {

            double c1 = BounceIntensity switch
            {
                EasingIntensity.Soft   => 0.65,
                EasingIntensity.Normal => 1.0,
                EasingIntensity.Strong => 1.3,
                _                      => 1.0
            };

            double c3 = c1 + 1.0; 
            double c4 = c1 * 0.5; 
        
            double p = progress - 1.0;
        
            return 1.0 + c4 * p * p * p * p + c3 * p * p * p + c1 * p * p;
        }
    
    }
    
    public class SukiSmoothEasingOut : Easing
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