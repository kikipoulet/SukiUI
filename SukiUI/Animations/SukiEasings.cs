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

    /// <summary>
    /// True spring-physics easing for the press (tension) phase.
    /// Uses a damped harmonic oscillator: 1 - e^(-damping·t) · cos(frequency·t).
    /// Higher damping = less oscillation (snappy), higher frequency = faster response.
    /// </summary>
    public class SukiEaseElasticIn : Easing
    {
        public double Damping { get; set; } = 10.0;
        public double Frequency { get; set; } = 25.0;

        public override double Ease(double progress)
        {
            if (progress <= 0) return 0;
            if (progress >= 1) return 1;

            // EaseIn = 1 - EaseOut(1 - t)
            double t = 1.0 - progress;
            double raw = 1.0 - Math.Exp(-Damping * t) * Math.Cos(Frequency * t);
            double rawAt1 = 1.0 - Math.Exp(-Damping) * Math.Cos(Frequency);

            if (Math.Abs(rawAt1) < 1e-10)
                return progress;

            return 1.0 - raw / rawAt1;
        }
    }

    /// <summary>
    /// True spring-physics easing for the release phase.
    /// Uses a damped harmonic oscillator: 1 - e^(-damping·t) · cos(frequency·t).
    /// Lower damping = more visible bounces, lower frequency = smoother oscillation.
    /// </summary>
    public class SukiEaseElasticOut : Easing
    {
        public double Damping { get; set; } = 8.0;
        public double Frequency { get; set; } = 20.0;

        public override double Ease(double progress)
        {
            if (progress <= 0) return 0;
            if (progress >= 1) return 1;

            double raw = 1.0 - Math.Exp(-Damping * progress) * Math.Cos(Frequency * progress);
            double rawAt1 = 1.0 - Math.Exp(-Damping) * Math.Cos(Frequency);

            if (Math.Abs(rawAt1) < 1e-10)
                return progress;

            return raw / rawAt1;
        }
    }

    /// <summary>
    /// Full spring-physics easing (iOS / Material 3 Expressive family): damped harmonic
    /// oscillator released from rest - zero initial velocity (no first-frame jump, unlike
    /// SukiEaseElasticOut), natural overshoot governed by the damping ratio, settles at 1.
    /// x(t) = 1 - e^(-zeta*wn*t) * (cos(wd*t) + (zeta*wn/wd) * sin(wd*t)),  wd = wn*sqrt(1-zeta^2).
    /// Use DampingRatio ~0.8-0.9 for position (tiny overshoot), >= 1 for critically damped.
    /// </summary>
    public class SukiEaseSpring : Easing
    {
        public double DampingRatio { get; set; } = 0.8;
        public double NaturalFrequency { get; set; } = 9.0;

        public override double Ease(double progress)
        {
            if (progress <= 0) return 0;
            if (progress >= 1) return 1;

            double wn = NaturalFrequency;
            if (DampingRatio >= 1.0)
            {
                // Critically damped: 1 - e^(-wn*t) * (1 + wn*t), normalized at t=1.
                double RawCriticallyDamped(double t) => 1.0 - Math.Exp(-wn * t) * (1.0 + wn * t);
                double rawAt1 = RawCriticallyDamped(1.0);
                return Math.Abs(rawAt1) < 1e-10 ? progress : RawCriticallyDamped(progress) / rawAt1;
            }

            double lambda = DampingRatio * wn;
            double wd = wn * Math.Sqrt(1.0 - DampingRatio * DampingRatio);
            double Raw(double t) => 1.0 - Math.Exp(-lambda * t) * (Math.Cos(wd * t) + (lambda / wd) * Math.Sin(wd * t));
            double at1 = Raw(1.0);
            return Math.Abs(at1) < 1e-10 ? progress : Raw(progress) / at1;
        }
    }
}