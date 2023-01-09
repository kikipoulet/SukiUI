using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
// using DialogHostAvalonia.Positioners;

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
            
            model.DialogChild = content;
            model.IsDialogOpen = false;
            model.IsDialogOpen = true;

            var gridBackground = this.FindControl<Grid>("gridDialog");
            var borderDialog = this.FindControl<Border>("borderDialog");

            if (showAtBottom)
            {
                borderDialog.VerticalAlignment = VerticalAlignment.Bottom;
                borderDialog.Margin = new Thickness(0,0,0,20);
            }
            else
            {
                borderDialog.VerticalAlignment = VerticalAlignment.Center;
                borderDialog.Margin = new Thickness(0,0,0,0);
            }
                

            borderDialog.Opacity = 1;
            gridBackground.Opacity = 0.56;
            
            
            
        }
        
        public void ShowToast(Control Message, int seconds)
        {
            var model = (MobileMenuPageViewModel)this.DataContext;

            Dispatcher.UIThread.InvokeAsync(() =>
            {


               

                model.ToastOpacity = 1;
                model.ContentToast = Message;
                model.ToastMargin = new Thickness(0, 100, 0, 0);
            });
            
            Task.Run((() =>
            {
                Thread.Sleep(seconds * 1000);
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    model.ToastOpacity = 0;
                    model.ToastMargin = new Thickness(0, 125, 0, 0);
                });
            }));
        }
        

        public void CloseDialog()
        {
            var model = (MobileMenuPageViewModel)this.DataContext;

            
            model.IsDialogOpen = false;
            
            
            var gridBackground = this.FindControl<Grid>("gridDialog");
            var borderDialog = this.FindControl<Border>("borderDialog");

            borderDialog.Opacity = 0;
            gridBackground.Opacity = 0;
            borderDialog.Margin = new Thickness(0,15,0,0);
        }
        
        public static void ShowToastS(Control Content, int seconds)
        {

            MobileMenuPage mbmp = null;
            
            try
            {
                mbmp = ((ISingleViewApplicationLifetime)Application.Current.ApplicationLifetime).MainView.GetVisualDescendants().OfType<MobileMenuPage>().First();
                
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Unable to open dialog in the Mobile view, trying desktop.");
                try
                {
                    mbmp = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow.GetVisualDescendants().OfType<MobileMenuPage>().First();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to show a dialog. " + ex.Message);
                }
                
            }
            
            mbmp.ShowToast(Content, seconds);
        }

        public static void WaitUntilDialogClosed()
        {
            MobileMenuPage mbmp = null;
            try
            {
                mbmp = ((ISingleViewApplicationLifetime)Application.Current.ApplicationLifetime).MainView.GetVisualDescendants().OfType<MobileMenuPage>().First();
                
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Unable to open dialog in the Mobile view, trying desktop.");
                try
                {
                    mbmp = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow.GetVisualDescendants().OfType<MobileMenuPage>().First();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to show a dialog. " + ex.Message);
                }
                
            }

            MobileMenuPageViewModel model = null; 
                
               Dispatcher.UIThread.InvokeAsync((() => model = (MobileMenuPageViewModel)mbmp.DataContext));

            bool flag = true;

            do
            {
                Dispatcher.UIThread.InvokeAsync(() => flag = model.IsDialogOpen);
                Thread.Sleep(200);
            } while (flag);
                
        }
        
        public static void ShowDialogS(Control content, bool showAtBottom = false)
        {

            MobileMenuPage mbmp = null;
            
            try
            {
                mbmp = ((ISingleViewApplicationLifetime)Application.Current.ApplicationLifetime).MainView.GetVisualDescendants().OfType<MobileMenuPage>().First();
                
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Unable to open dialog in the Mobile view, trying desktop.");
                try
                {
                    mbmp = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow.GetVisualDescendants().OfType<MobileMenuPage>().First();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to show a dialog. " + ex.Message);
                }
                
            }
            
            var model = (MobileMenuPageViewModel)mbmp.DataContext;
         /*  if (showAtBottom)
                model.DialogPosition = new AlignmentDialogPopupPositioner(){ VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(10,10,0,10)};
            else
                model.DialogPosition = new AlignmentDialogPopupPositioner(){VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center}; */

            mbmp.ShowDialog(content, showAtBottom);
        }
        
        public static void CloseDialogS()
        {
            try
            {
                ((ISingleViewApplicationLifetime)Application.Current.ApplicationLifetime).MainView.GetVisualDescendants().OfType<MobileMenuPage>().First().CloseDialog();
            }
            catch (Exception exc)
            {
                try
                {
                    ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow
                        .GetVisualDescendants().OfType<MobileMenuPage>().First().CloseDialog();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        
    }
}
