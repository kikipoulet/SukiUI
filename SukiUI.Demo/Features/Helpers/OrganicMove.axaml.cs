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
        
        private void MoveOrganic(object? sender, RoutedEventArgs e)
        {
            var card = this.Get<GlassCard>("GlassMove");
            card.MoveToOrganic(new Thickness( card.Margin.Left == 0 ? 300 : 0, 0, 0, 0), TimeSpan.FromMilliseconds(2000));
        }
    }
}