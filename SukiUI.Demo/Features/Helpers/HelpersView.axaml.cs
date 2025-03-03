using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Controls;
using SukiUI.Helpers;

namespace SukiUI.Demo.Features.Helpers
{
    public partial class HelpersView : UserControl
    {
        public HelpersView()
        {
            InitializeComponent();
        }

        private void MoveBorderWidth(object? sender, RoutedEventArgs e)
        {
            this.Get<Border>("MyBorder").Animate(WidthProperty).From(50).To(200).Start();
        }



        private CancellationTokenSource token;
        private void InputElement_OnPointerEntered(object? sender, PointerEventArgs e)
        {
            if(token is not null)
                token.Cancel();
            
            (sender as GlassCard).Animate<double>(WidthProperty, (sender as GlassCard).Width, 200);
        }

        private void InputElement_OnPointerExited(object? sender, PointerEventArgs e)
        {
            if(token is not null)
                token.Cancel();
            
            (sender as GlassCard).Animate<double>(WidthProperty, (sender as GlassCard).Width, 50);
        }
    }
}