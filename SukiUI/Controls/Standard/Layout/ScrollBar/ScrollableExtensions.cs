using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Rendering.Composition;
using SukiUI.Helpers;

namespace SukiUI.Theme
{
    public static class Scrollable
    {
        public static void MakeScrollable(CompositionVisual? compositionVisual) =>
            CompositionAnimationHelper.MakeScrollable(compositionVisual);

        // A single shared handler: it only ever needs the sender, so every AnimatedScroll host can reuse it,
        // and a stable delegate instance is what makes the "-=" below actually unsubscribe.
        private static readonly EventHandler<VisualTreeAttachmentEventArgs> AttachedHandler =
            (sender, _) => CompositionAnimationHelper.MakeScrollable(ElementComposition.GetElementVisual((Visual)sender!));

        internal static void SetAnimatedScrollHandler(Visual element, bool enabled)
        {
            element.AttachedToVisualTree -= AttachedHandler;
            if (enabled)
                element.AttachedToVisualTree += AttachedHandler;
        }
    }

    public static class StackPanelExtensions
    {
        public static readonly AttachedProperty<bool> AnimatedScrollProperty =
            AvaloniaProperty.RegisterAttached<StackPanel, bool>("AnimatedScroll", typeof(StackPanel), defaultValue: false);

        static StackPanelExtensions()
        {
            AnimatedScrollProperty.Changed.AddClassHandler<StackPanel>(
                (element, _) => Scrollable.SetAnimatedScrollHandler(element, GetAnimatedScroll(element)));
        }

        public static bool GetAnimatedScroll(StackPanel wrap) => wrap.GetValue(AnimatedScrollProperty);

        public static void SetAnimatedScroll(StackPanel wrap, bool value) => wrap.SetValue(AnimatedScrollProperty, value);
    }

    public static class WrapPanelExtensions
    {
        public static readonly AttachedProperty<bool> AnimatedScrollProperty =
            AvaloniaProperty.RegisterAttached<WrapPanel, bool>("AnimatedScroll", typeof(WrapPanel), defaultValue: false);

        static WrapPanelExtensions()
        {
            AnimatedScrollProperty.Changed.AddClassHandler<WrapPanel>(
                (element, _) => Scrollable.SetAnimatedScrollHandler(element, GetAnimatedScroll(element)));
        }

        public static bool GetAnimatedScroll(WrapPanel wrap) => wrap.GetValue(AnimatedScrollProperty);

        public static void SetAnimatedScroll(WrapPanel wrap, bool value) => wrap.SetValue(AnimatedScrollProperty, value);
    }

    public static class ItemsPresenterExtensions
    {
        public static readonly AttachedProperty<bool> AnimatedScrollProperty =
            AvaloniaProperty.RegisterAttached<ItemsPresenter, bool>("AnimatedScroll", typeof(ItemsPresenter), defaultValue: false);

        static ItemsPresenterExtensions()
        {
            AnimatedScrollProperty.Changed.AddClassHandler<ItemsPresenter>(
                (element, _) => Scrollable.SetAnimatedScrollHandler(element, GetAnimatedScroll(element)));
        }

        public static bool GetAnimatedScroll(ItemsPresenter wrap) => wrap.GetValue(AnimatedScrollProperty);

        public static void SetAnimatedScroll(ItemsPresenter wrap, bool value) => wrap.SetValue(AnimatedScrollProperty, value);
    }

    public static class ItemsControlExtensions
    {
        public static readonly AttachedProperty<bool> AnimatedScrollProperty =
            AvaloniaProperty.RegisterAttached<ItemsControl, bool>("AnimatedScroll", typeof(ItemsControl), defaultValue: false);

        static ItemsControlExtensions()
        {
            AnimatedScrollProperty.Changed.AddClassHandler<ItemsControl>(
                (element, _) => Scrollable.SetAnimatedScrollHandler(element, GetAnimatedScroll(element)));
        }

        public static bool GetAnimatedScroll(ItemsControl wrap) => wrap.GetValue(AnimatedScrollProperty);

        public static void SetAnimatedScroll(ItemsControl wrap, bool value) => wrap.SetValue(AnimatedScrollProperty, value);
    }
}
