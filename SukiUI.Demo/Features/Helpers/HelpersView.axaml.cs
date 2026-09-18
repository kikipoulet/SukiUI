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

      



        private CancellationTokenSource token;


       
    }
}