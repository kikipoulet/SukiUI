using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Controls;

namespace SukiUI.Demo.Features.Helpers
{
    public partial class OrganicMove : UserControl
    {
        public OrganicMove()
        {
            InitializeComponent();
        }
        
        private void GoToTopRight(object? sender, RoutedEventArgs e)
        {
            var card = this.Get<GlassCard>("GlassMove");
            card.MoveToOrganic(new Thickness( 300 , 0, 0, 0), TimeSpan.FromMilliseconds(2000));
        }
        private void GoToTopLeft(object? sender, RoutedEventArgs e)
        {
            var card = this.Get<GlassCard>("GlassMove");
            card.MoveToOrganic(new Thickness( 0 , 0, 0, 0), TimeSpan.FromMilliseconds(2000));
        }
        
        private void GoToBotLeft(object? sender, RoutedEventArgs e)
        {
            var card = this.Get<GlassCard>("GlassMove");
            card.MoveToOrganic(new Thickness( 0 , 300, 0, 0), TimeSpan.FromMilliseconds(2000));
        }
        
        private void GoToBotRight(object? sender, RoutedEventArgs e)
        {
            var card = this.Get<GlassCard>("GlassMove");
            card.MoveToOrganic(new Thickness( 300 , 300, 0, 0), TimeSpan.FromMilliseconds(2000));
        }
    }
}