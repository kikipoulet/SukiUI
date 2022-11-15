using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace SukiUI.Controls
{
    public partial class MobileMenuPage : UserControl
    {
        public MobileMenuPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void ShowDialog(Control content, bool showAtBottom = false)
        {
            var model = (MobileMenuPageViewModel)this.DataContext;

            model.DialogAtBottom = showAtBottom;
            
            model.DialogChild = content;
            model.IsDialogOpen = false;
            model.IsDialogOpen = true;
        }
        
        public static void ShowDialogS(Control content, bool showAtBottom = false)
        {
            try
            {
                ((ISingleViewApplicationLifetime)Application.Current.ApplicationLifetime).MainView
                    .GetVisualDescendants().OfType<MobileMenuPage>().First().ShowDialog(content, showAtBottom);
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Unable to open dialog in the Mobile view, trying desktop.");
                try
                {
                    ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow
                        .GetVisualDescendants().OfType<MobileMenuPage>().First().ShowDialog(content, showAtBottom);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to show a dialog. " + ex.Message);
                }
                
            }
        }

        
    }
}
