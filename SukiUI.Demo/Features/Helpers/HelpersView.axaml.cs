using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using SukiUI.Controls;
using SukiUI.Helpers;
using System.Runtime.CompilerServices;

namespace SukiUI.Demo.Features.Helpers
{
    public partial class HelpersView : UserControl
    {
        private bool _animationProfileLabCreated;

        public HelpersView()
        {
            InitializeComponent();
            SyntaxHelpersItem.PageContent = RuntimeFeature.IsDynamicCodeCompiled
                ? new SyntaxHelpers()
                : new TextBlock
                {
                    Text = "ConditionalXAML is not available in Native AOT builds.",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(32),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };
        }

        private void OnMenuSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_animationProfileLabCreated ||
                !e.AddedItems.Contains(AnimationProfileItem))
            {
                return;
            }

            AnimationProfileItem.PageContent = new AnimationProfileLab();
            _animationProfileLabCreated = true;
        }

        private CancellationTokenSource token;


       
    }
}