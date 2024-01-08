using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace SukiUI.Theme
{

    public static class Scrollable
    {
        public static void MakeScrollable(CompositionVisual compositionVisual)
        {
            if (compositionVisual == null)
                return;
        
            Compositor compositor = compositionVisual.Compositor;

            var animationGroup = compositor.CreateAnimationGroup();
            Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = "Offset";

            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(250);

            ImplicitAnimationCollection implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
            animationGroup.Add(offsetAnimation);
            implicitAnimationCollection["Offset"] = animationGroup;
            compositionVisual.ImplicitAnimations = implicitAnimationCollection;
        }
    }
    public static class WrapPanelExtensions
    {
        public static readonly AttachedProperty<bool> AnimatedScrollProperty =
            AvaloniaProperty.RegisterAttached<WrapPanel, bool>("AnimatedScroll", typeof(WrapPanel), defaultValue: false);

        public static bool GetAnimatedScroll(WrapPanel wrap)
        {
            return wrap.GetValue(AnimatedScrollProperty);
        }

        public static void SetAnimatedScroll(WrapPanel wrap, bool value)
        {
            if(value)
                wrap.AttachedToVisualTree += (sender, args) => Scrollable.MakeScrollable(ElementComposition.GetElementVisual(wrap));

            wrap.SetValue(AnimatedScrollProperty, value);
        }

    }
    
    public static class StackPanelExtensions
    {
        public static readonly AttachedProperty<bool> AnimatedScrollProperty =
            AvaloniaProperty.RegisterAttached<StackPanel, bool>("AnimatedScroll", typeof(StackPanel), defaultValue: false);

        public static bool GetAnimatedScroll(StackPanel Stack)
        {
            return Stack.GetValue(AnimatedScrollProperty);
        }

        public static void SetAnimatedScroll(StackPanel Stack, bool value)
        {
            if(value)
                Stack.AttachedToVisualTree += (sender, args) => Scrollable.MakeScrollable(ElementComposition.GetElementVisual(Stack));

            Stack.SetValue(AnimatedScrollProperty, value);
        }
    }
    
    public static class ItemsControlExtensions
    {
        public static readonly AttachedProperty<bool> AnimatedScrollProperty =
            AvaloniaProperty.RegisterAttached<ItemsControl, bool>("AnimatedScroll", typeof(ItemsControl), defaultValue: false);

        public static bool GetAnimatedScroll(ItemsControl Stack)
        {
            return Stack.GetValue(AnimatedScrollProperty);
        }

        public static void SetAnimatedScroll(ItemsControl Stack, bool value)
        {
            if(value)
                Stack.AttachedToVisualTree += (sender, args) => Scrollable.MakeScrollable(ElementComposition.GetElementVisual(Stack));

            Stack.SetValue(AnimatedScrollProperty, value);
        }
    }
  
  
}