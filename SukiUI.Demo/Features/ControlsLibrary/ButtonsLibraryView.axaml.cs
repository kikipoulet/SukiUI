using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.VisualTree;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class ButtonsLibraryView : UserControl
{
    public ButtonsLibraryView()
    {
        InitializeComponent();

        var w = this.Get<WrapPanel>("WrapButtons");
            
        w.AttachedToVisualTree += (sender, args) =>
        {
            CompositionVisual compositionVisual = ElementComposition.GetElementVisual(w);
            if (compositionVisual == null)
                return;
        
            Compositor compositor = compositionVisual.Compositor;

            var animationGroup = compositor.CreateAnimationGroup();
            Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = "Offset";

            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(300);

            ImplicitAnimationCollection implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
            animationGroup.Add(offsetAnimation);
            implicitAnimationCollection["Offset"] = animationGroup;
            compositionVisual.ImplicitAnimations = implicitAnimationCollection;
        };
    }

   
}

