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
    public InteractiveContainer()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static readonly StyledProperty<bool> ShowAtBottomProperty = AvaloniaProperty.Register<InteractiveContainer, bool>(nameof(InteractiveContainer), defaultValue: false);

    public bool ShowAtBottom
    {
        get { return GetValue(ShowAtBottomProperty); }
        set
        {
            
            SetValue(ShowAtBottomProperty, value );
        }
    }
    
    public static readonly StyledProperty<bool> IsDialogOpenProperty = AvaloniaProperty.Register<InteractiveContainer, bool>(nameof(InteractiveContainer), defaultValue: false);

    public bool IsDialogOpen
    {
        get { return GetValue(IsDialogOpenProperty); }
        set
        {
            
            SetValue(IsDialogOpenProperty, value );
        }
    }
    
    public static readonly StyledProperty<bool> IsToastOpenProperty = AvaloniaProperty.Register<InteractiveContainer, bool>(nameof(InteractiveContainer), defaultValue: false);

    public bool IsToastOpen
    {
        get { return GetValue(IsToastOpenProperty); }
        set
        {
            
            SetValue(IsToastOpenProperty, value );
        }
    }
    
    public static readonly StyledProperty<Control> DialogContentProperty = AvaloniaProperty.Register<InteractiveContainer, Control>(nameof(InteractiveContainer), defaultValue: new Grid());

    public Control DialogContent
    {
        get { return GetValue(DialogContentProperty); }
        set
        {
            
            SetValue(DialogContentProperty, value );
        }
    }
    
    public static readonly StyledProperty<Control> ToastContentProperty = AvaloniaProperty.Register<InteractiveContainer, Control>(nameof(InteractiveContainer), defaultValue: new Grid());

    public Control ToastContent
    {
        get { return GetValue(ToastContentProperty); }
        set
        {
            
            SetValue(ToastContentProperty, value );
        }
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
    public static void ShowDialog(object? content, bool showAtBottom = false, bool showCardBehind = true)
    {
        var container = GetInteractiveContainerInstance();

        Control? control;

        if (content is Control c)
        {
            control = c;
        }
        else
        {
            control = Application.Current?.DataTemplates.FirstOrDefault()?.Build(content);
            control ??= new TextBlock { Text = "No Suitable View Locator Available." };
        }
        
        container.IsDialogOpen = true;
        container.DialogContent = control;
        container.ShowAtBottom = showAtBottom;

        container.GetTemplateChildren().First(n => n.Name == "Glass1").Opacity = showCardBehind ? 1 : 0;
        container.GetTemplateChildren().First(n => n.Name == "Glass2").Opacity = showCardBehind ? 1 : 0;
        container.GetTemplateChildren().First(n => n.Name == "Glass3").Opacity = showCardBehind ? 1 : 0;
          
    }
}