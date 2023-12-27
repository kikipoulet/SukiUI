using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace SukiUI.Controls;

public partial class InteractiveContainer : UserControl
{
    private readonly IDataTemplate? _viewLocator;
    
    public InteractiveContainer()
    {
        InitializeComponent();
        _viewLocator = Application.Current?.DataTemplates.FirstOrDefault();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static readonly StyledProperty<bool> ShowAtBottomProperty = AvaloniaProperty.Register<InteractiveContainer, bool>(nameof(InteractiveContainer), defaultValue: false);
    public bool ShowAtBottom
    {
        get => GetValue(ShowAtBottomProperty);
        set => SetValue(ShowAtBottomProperty, value );
    }
    
    public static readonly StyledProperty<bool> IsDialogOpenProperty = AvaloniaProperty.Register<InteractiveContainer, bool>(nameof(InteractiveContainer), defaultValue: false);
    public bool IsDialogOpen
    {
        get => GetValue(IsDialogOpenProperty);
        set => SetValue(IsDialogOpenProperty, value );
    }
    
    public static readonly StyledProperty<bool> IsToastOpenProperty = AvaloniaProperty.Register<InteractiveContainer, bool>(nameof(InteractiveContainer), defaultValue: false);
    public bool IsToastOpen
    {
        get => GetValue(IsToastOpenProperty);
        set => SetValue(IsToastOpenProperty, value );
    }
    
    public static readonly StyledProperty<Control> DialogContentProperty = AvaloniaProperty.Register<InteractiveContainer, Control>(nameof(InteractiveContainer), defaultValue: new Grid());
    public Control DialogContent
    {
        get => GetValue(DialogContentProperty);
        set => SetValue(DialogContentProperty, value );
    }
    
    public static readonly StyledProperty<Control> ToastContentProperty = AvaloniaProperty.Register<InteractiveContainer, Control>(nameof(InteractiveContainer), defaultValue: new Grid());
    public Control ToastContent
    {
        get => GetValue(ToastContentProperty);
        set => SetValue(ToastContentProperty, value );
    }

    public static readonly StyledProperty<bool> AllowBackgroundCloseProperty = AvaloniaProperty.Register<InteractiveContainer, bool>("AllowBackgroundClose");
    public bool AllowBackgroundClose
    {
        get => GetValue(AllowBackgroundCloseProperty);
        set => SetValue(AllowBackgroundCloseProperty, value);
    }



    private static InteractiveContainer GetInteractiveContainerInstance()
    {
        InteractiveContainer container;
        try
        {
            container = ((ISingleViewApplicationLifetime)Application.Current.ApplicationLifetime).MainView.GetVisualDescendants().OfType<InteractiveContainer>().First();
                
        }
        catch (Exception)
        {
            
            try
            {
                container = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow.GetVisualDescendants().OfType<InteractiveContainer>().First();
            }
            catch (Exception)
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

        container.ToastContent = Message;
        container.IsToastOpen = true;

            
        Task.Run((() =>
        {
            Thread.Sleep(seconds * 1000);
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                container.IsToastOpen = false;
            });
        }));
    }

    public static void CloseDialog()
    {
        GetInteractiveContainerInstance().IsDialogOpen = false;
    }

    internal static void BackgroundRequestClose()
    {
        var inst = GetInteractiveContainerInstance();
        if (!inst.AllowBackgroundClose) return;
        inst.IsDialogOpen = false;
    }

    public static void WaitUntilDialogIsClosed()
    {
        InteractiveContainer container = null;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            container = GetInteractiveContainerInstance();
        });
        bool flag = true;

        do
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                flag = container.IsDialogOpen;
            });
            
            Thread.Sleep(200);
        } while (flag);
           
    }


    /// <summary>
    /// Shows a dialog in the <see cref="InteractiveContainer"/>
    /// Can display ViewModels if provided, if a suitable ViewLocator has been registered with Avalonia.
    /// </summary>
    /// <param name="content">Content to display.</param>
    /// <param name="showAtBottom"></param>
    /// <param name="showCardBehind"></param>
    /// <param name="allowBackgroundClose">Allows the dialog to be closed by clicking outside of it.</param>
    public static void ShowDialog(object? content, bool showAtBottom = false, bool showCardBehind = true, bool allowBackgroundClose = false)
    {
        var container = GetInteractiveContainerInstance();

        Control? control;

        if (content is Control c)
            control = c;
        else
            control = container._viewLocator?.Build(content) 
                      ?? new TextBlock { Text = "No Suitable View Locator Available." } ;
        
        container.IsDialogOpen = true;
        container.DialogContent = control;
        container.ShowAtBottom = showAtBottom;
        container.AllowBackgroundClose = allowBackgroundClose;

        container.GetTemplateChildren().First(n => n.Name == "BorderDialog1").Opacity = showCardBehind ? 1 : 0;
    }
}