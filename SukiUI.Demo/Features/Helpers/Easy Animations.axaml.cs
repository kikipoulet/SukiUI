using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Helpers;

namespace SukiUI.Demo.Features.Helpers
{
    public partial class Easy_Animations : UserControl
    {
        public Easy_Animations()
        {
            InitializeComponent();
        }
        
        private void MoveBorderWidth(object? sender, RoutedEventArgs e)
        {
            this.Get<Border>("MyBorder").Animate(WidthProperty).From(50).To(200).Start();
        }
    }
}