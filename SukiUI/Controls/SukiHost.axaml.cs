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

    private static SukiHost? _instance;
    private static SukiHost Instance => EnsureInstance();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _instance = this;
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
    /// <param name="showCardBehind">Whether or not to show a card behind the content.</param>
    /// <param name="allowBackgroundClose">Allows the dialog to be closed by clicking outside of it.</param>
    public static void ShowDialog(object? content, bool showCardBehind = true, bool allowBackgroundClose = false)
    {
        var control = content as Control ?? ViewLocator.TryBuild(content);
        Instance.IsDialogOpen = true;
        Instance.DialogContent = control;
        Instance.AllowBackgroundClose = allowBackgroundClose;

        Instance.GetTemplateChildren().First(n => n.Name == "BorderDialog1").Opacity = showCardBehind ? 1 : 0;
    }

    /// <summary>
    /// Attempts to close a dialog if one is shown.
    /// </summary>
    public static void CloseDialog()
    {
        Instance.IsDialogOpen = false;
    }

    private static void BackgroundRequestClose()
    {
        if (!Instance.AllowBackgroundClose) return;
        Instance.IsDialogOpen = false;
    }

    /// <summary>
    /// Shows a toast in the SukiHost - The default location is in the bottom right.
    /// This can be changed
    /// </summary>
    /// <param name="title">The title to display in the toast.</param>
    /// <param name="content">The content of the toast, this can be any control or ViewModel.</param>
    /// <param name="duration">Duration for this toast to be active. Default is 2 seconds.</param>
    /// <param name="onClicked">A callback that will be fired if the Toast is cleared by clicking.</param>
    public static void ShowToast(string title, object content, TimeSpan? duration = null, Action? onClicked = null) =>
        ShowToast(new SukiToastModel(
            title,
            content as Control ?? ViewLocator.TryBuild(content),
            duration ?? TimeSpan.FromSeconds(2),
            onClicked));

    /// <summary>
    /// <inheritdoc cref="ShowToast(string,object,System.Nullable{System.TimeSpan},System.Action?)"/>
    /// </summary>
    /// <param name="model">A pre-constructed <see cref="SukiToastModel"/>.</param>
    public static void ShowToast(SukiToastModel model)
    {
        var toast = SukiToastPool.Get();

        toast.Initialize(model);
        Dispatcher.UIThread.Invoke(() =>
        {
            Instance.ToastsCollection.Add(toast);
            toast.Animate<Double>(OpacityProperty, 0,1,TimeSpan.FromMilliseconds(500));
            toast.Animate<Thickness>(MarginProperty, new Thickness(0,10,0,-10),new Thickness(),TimeSpan.FromMilliseconds(500));
        });

    }

    /// <summary>
    /// Clears a specific toast from display (if it is still currently being displayed).
    /// </summary>
    /// <param name="toast">The toast to clear.</param>
    public static void ClearToast(SukiToast toast)
    {

        var wasRemoved = Dispatcher.UIThread.Invoke(() => Instance.ToastsCollection.Remove(toast));
        if (!wasRemoved) return;
        SukiToastPool.Return(toast);

    }
    
    /// <summary>
    /// Clears all active toasts immediately.
    /// </summary>
    public static void ClearAllToasts()
    {
        SukiToastPool.Return(Instance.ToastsCollection);
        Dispatcher.UIThread.Invoke(() => Instance.ToastsCollection.Clear());
    }

    private static SukiHost EnsureInstance()
    {
        if (_instance is null)
            throw new InvalidOperationException("SukiHost must be active somewhere in the VisualTree");
        return _instance;
    }
}