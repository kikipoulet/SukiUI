using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Threading;
using SukiUI.Enums;
using SukiUI.Helpers;
using SukiUI.Models;

namespace SukiUI.Controls;

/// <summary>
/// Hosts both Dialogs and Notifications
/// </summary>
public class SukiHost : ContentControl
{
    protected override Type StyleKeyOverride => typeof(SukiHost);

    public static readonly StyledProperty<bool> IsDialogOpenProperty =
        AvaloniaProperty.Register<SukiHost, bool>(nameof(IsDialogOpen), defaultValue: false);

    public bool IsDialogOpen
    {
        get => GetValue(IsDialogOpenProperty);
        set => SetValue(IsDialogOpenProperty, value);
    }

    public static readonly StyledProperty<Control> DialogContentProperty =
        AvaloniaProperty.Register<SukiHost, Control>(nameof(DialogContent), defaultValue: new Grid());

    public Control DialogContent
    {
        get => GetValue(DialogContentProperty);
        set => SetValue(DialogContentProperty, value);
    }

    public static readonly StyledProperty<bool> AllowBackgroundCloseProperty =
        AvaloniaProperty.Register<SukiHost, bool>(nameof(AllowBackgroundClose), defaultValue: true);

    public bool AllowBackgroundClose
    {
        get => GetValue(AllowBackgroundCloseProperty);
        set => SetValue(AllowBackgroundCloseProperty, value);
    }

    public static readonly AttachedProperty<ToastLocation> ToastLocationProperty =
        AvaloniaProperty.RegisterAttached<SukiHost, SukiWindow, ToastLocation>("ToastLocation",
            defaultValue: ToastLocation.BottomRight);

    public static void SetToastLocation(Control element, ToastLocation value) =>
        element.SetValue(ToastLocationProperty, value);

    public static ToastLocation GetToastLocation(Control element) =>
        element.GetValue(ToastLocationProperty);

    public static readonly StyledProperty<AvaloniaList<SukiToast>> ToastsCollectionProperty =
        AvaloniaProperty.Register<SukiHost, AvaloniaList<SukiToast>>(nameof(ToastsCollection),
            defaultValue: new AvaloniaList<SukiToast>());

    public AvaloniaList<SukiToast> ToastsCollection
    {
        get => GetValue(ToastsCollectionProperty);
        set => SetValue(ToastsCollectionProperty, value);
    }

    private static SukiHost? Instance { get; set; }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Instance = this;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var toastLoc = GetToastLocation((SukiWindow)VisualRoot);
        e.NameScope.Get<Border>("PART_DialogBackground").PointerPressed += (_, _) => BackgroundRequestClose();
        e.NameScope.Get<ItemsControl>("PART_ToastPresenter").HorizontalAlignment =
            toastLoc == ToastLocation.BottomLeft
                ? HorizontalAlignment.Left
                : HorizontalAlignment.Right;
    }

    /// <summary>
    /// Shows a dialog in the <see cref="InteractiveContainer"/>
    /// Can display ViewModels if provided, if a suitable ViewLocator has been registered with Avalonia.
    /// </summary>
    /// <param name="content">Content to display.</param>
    /// <param name="showAtBottom"></param>
    /// <param name="showCardBehind"></param>
    /// <param name="allowBackgroundClose">Allows the dialog to be closed by clicking outside of it.</param>
    public static void ShowDialog(object? content, bool showAtBottom = false, bool showCardBehind = true,
        bool allowBackgroundClose = false)
    {
        if (Instance is null)
            throw new InvalidOperationException("SukiHost must be active somewhere in the VisualTree");

        var control = content as Control ?? ViewLocator.TryBuild(content);
        Instance.IsDialogOpen = true;
        Instance.DialogContent = control;
        Instance.AllowBackgroundClose = allowBackgroundClose;

        Instance.GetTemplateChildren().First(n => n.Name == "BorderDialog1").Opacity = showCardBehind ? 1 : 0;
    }

    public static void CloseDialog()
    {
        if (Instance is null)
            throw new InvalidOperationException("SukiHost must be active somewhere in the VisualTree");
        Instance.IsDialogOpen = false;
    }

    internal static void BackgroundRequestClose()
    {
        if (Instance is null)
            throw new InvalidOperationException("SukiHost must be active somewhere in the VisualTree");
        if (!Instance.AllowBackgroundClose) return;
        Instance.IsDialogOpen = false;
    }

    public static void ShowToast(string title, object content, TimeSpan? duration = null)
    {
        if (Instance is null)
            throw new InvalidOperationException("SukiHost must be active somewhere in the VisualTree");
        var toast = SukiToastPool.Get();
        toast.Initialize(new SukiToastModel(title, content, duration ?? TimeSpan.FromSeconds(4)));
        Dispatcher.UIThread.Invoke(() =>
        {
            Instance.ToastsCollection.Add(toast);
            toast.Animate<Double>(OpacityProperty, 0,1,TimeSpan.FromMilliseconds(500));
            toast.Animate<Thickness>(MarginProperty, new Thickness(0,10,0,-10),new Thickness(),TimeSpan.FromMilliseconds(500));
        });
    }

    public static void RequestHideToast(SukiToast toast)
    {
        if (Instance is null)
            throw new InvalidOperationException("SukiHost must be active somewhere in the VisualTree");
        Task.Run(() =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                toast.Animate<Double>(OpacityProperty, 1, 0, TimeSpan.FromMilliseconds(500));
                toast.Animate<Thickness>(MarginProperty, new Thickness(), new Thickness(0, 10, 0, -10),
                    TimeSpan.FromMilliseconds(500));
            });

            Thread.Sleep(500);

            Dispatcher.UIThread.Invoke(() => Instance.ToastsCollection.Remove(toast));
            SukiToastPool.Return(toast);
        });
    }
}