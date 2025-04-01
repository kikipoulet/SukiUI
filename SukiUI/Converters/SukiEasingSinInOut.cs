using Avalonia.Animation.Easings;

namespace SukiUI.Converters
{
    public class SukiEasingOut: Easing
    {
        public override double Ease(double progress)
        {
            progress = Math.Max(0.0, Math.Min(1.0, progress));
            
            const double c1 = 1.01; 
            const double c3 = c1 + 1.0;
            double p = progress - 1.0; 

            return 1.0 + c3 * p * p * p + c1 * p * p;
        }
    }
}