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
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace SukiUI.Controls;

public partial class InteractiveContainer : UserControl
{
    public InteractiveContainer()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static readonly StyledProperty<Control> PageContentProperty = AvaloniaProperty.Register<InteractiveContainer, Control>(nameof(PageContent), defaultValue: new Grid());

    public Control PageContent
    {
        get { return GetValue(PageContentProperty); }
        set { SetValue(PageContentProperty, value); this.FindControl<ContentControl>("GeneralContent").Content = value; }
    }


    private static InteractiveContainer GetInteractiveContainerInstance()
    {
        InteractiveContainer container = null;
        try
        {
            container = ((ISingleViewApplicationLifetime)Application.Current.ApplicationLifetime).MainView.GetVisualDescendants().OfType<InteractiveContainer>().First();
                
        }
        catch (Exception exc)
        {
            
            try
            {
                container = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow.GetVisualDescendants().OfType<InteractiveContainer>().First();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "You are trying to use a InteractiveContainer functionality without declaring one !");
            }
                
        }

        return container;
    }

    
    public static void ShowToast(Control Message, int seconds)
    {
        var container = GetInteractiveContainerInstance();
        ContentControl ct = container.FindControl<ContentControl>("ToastContent");
        Border bd = container.FindControl<Border>("ToastBorder");
        
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            bd.Opacity = 1;
            ct.Content = Message;
            bd.Margin = new Thickness(0, 100, 0, 0);
        });
            
        Task.Run((() =>
        {
            Thread.Sleep(seconds * 1000);
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                bd.Opacity = 0;
                bd.Margin = new Thickness(0, 125, 0, 0);
            });
        }));
    }

    public static void CloseDialog()
    {
        var container = GetInteractiveContainerInstance();

        Border DialogBorder = container.FindControl<Border>("borderDialog");
        

        DialogBorder.Opacity = 0;
        DialogBorder.IsHitTestVisible = false;
        IsDialogOpen = false;
        container.FindControl<Grid>("gridDialog").Opacity = 0;
        container.FindControl<Grid>("gridDialog").IsHitTestVisible = false;
    }

    public static void WaitUntilDialogIsClosed()
    {
        while (IsDialogOpen)
            Thread.Sleep(200);
    }

    public static bool IsDialogOpen { get; set; } = false;
    
    public static void ShowDialog(Control content, bool showAtBottom = false)
    {
        var container = GetInteractiveContainerInstance();
        IsDialogOpen = true;
        Border DialogBorder = container.FindControl<Border>("borderDialog");

        container.FindControl<ContentControl>("DialogContent").Content = content;
        
        if (showAtBottom)
        {
            DialogBorder.VerticalAlignment = VerticalAlignment.Bottom;
            DialogBorder.Margin = new Thickness(0,0,0,20);
        }
        else
        {
            DialogBorder.VerticalAlignment = VerticalAlignment.Center;
            DialogBorder.Margin = new Thickness(0,0,0,0);
        }
        
        DialogBorder.Opacity = 1;
        DialogBorder.IsHitTestVisible = true;
       
       container.FindControl<Grid>("gridDialog").Opacity = 0.56;
       container.FindControl<Grid>("gridDialog").IsHitTestVisible = true;

    }
}