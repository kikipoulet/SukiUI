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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using SukiUI.Content;

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
        AvaloniaProperty.Register<SukiHost, Control>(nameof(DialogContent));

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
    
    private static Window? _mainWindow;
    private static readonly Dictionary<Window, SukiHost> Instances = new();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (VisualRoot is not Window w) return;
        Instances.Add(w, this);
        _mainWindow ??= w;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (VisualRoot is not Window window)
            throw new InvalidOperationException("SukiHost must be hosted inside a Window or SukiWindow");

        e.NameScope.Get<Border>("PART_DialogBackground").PointerPressed += (_, _) => BackgroundRequestClose(this);
        
        var b = e.NameScope.Get<Border>("PART_DialogBackground");
        b.Loaded += (sender, args) =>
        {
            var v = ElementComposition.GetElementVisual(b);
            CompositionAnimationHelper.MakeOpacityAnimated(v, 400);
        }; 
        
     
    }
    

    // TODO: Dialog API desperately needs to support a result or on-close callback.
    // TODO: Toasts and dialogs should be dragged out into their own discrete service and provided by a higher level service locator.
    // TODO: Currently not possible to switch the toast side at runtime, in reality there should be multiple anchors and toasts can be displayed on them arbitrarily.
    // Giving devs direct access to this object like this is messy and there really needs to be a standard abstraction above all these features.
    // This goes for other APIs like the background and theming.

    /// <summary>
    /// Shows a dialog in the <see cref="SukiHost"/>
    /// Can display ViewModels if provided, if a suitable ViewLocator has been registered with Avalonia.
    /// </summary>
    /// <param name="window">The window who's SukiHost should be used to display the toast.</param>
    /// <param name="content">Content to display.</param>
    /// <param name="showCardBehind">Whether or not to show a card behind the content.</param>
    /// <param name="allowBackgroundClose">Allows the dialog to be closed by clicking outside of it.</param>
    /// <exception cref="InvalidOperationException">Thrown if there is no SukiHost associated with the specified window.</exception>
    public static void ShowDialog(Window window, object? content, bool showCardBehind = true,
        bool allowBackgroundClose = false)
    {
        if (!Instances.TryGetValue(window, out var host))
            throw new InvalidOperationException("No SukiHost present in this window");
        var control = content as Control ?? ViewLocator.TryBuild(content);
        host.IsDialogOpen = true;
        host.DialogContent = control;
        host.AllowBackgroundClose = allowBackgroundClose;
        host.GetTemplateChildren().First(n => n.Name == "BorderDialog1").Opacity = showCardBehind ? 1 : 0;
    }
    
    /// <summary>
    /// <inheritdoc cref="ShowDialog(Avalonia.Controls.Window,object?,bool,bool)"/>
    /// </summary>
    /// <param name="content">Content to display.</param>
    /// <param name="showCardBehind">Whether or not to show a card behind the content.</param>
    /// <param name="allowBackgroundClose">Allows the dialog to be closed by clicking outside of it.</param>
    public static void ShowDialog(object? content, bool showCardBehind = true, bool allowBackgroundClose = false) =>
        ShowDialog(_mainWindow, content, showCardBehind, allowBackgroundClose);

    public static void ShowMessageBox(MessageBoxModel model, bool allowbackgroundclose = true)
    {
        
        
        SukiHost.ShowDialog(new MessageBox(){
            _onActionCallback = model.ActionButton,
            Title = model.Title, Content = model.Content, ShowActionButton = model.ActionButtonContent != null, 
            ActionButtonContent = model.ActionButtonContent, 
            Icon = model.Type switch
            {
                NotificationType.Information => Icons.InformationOutline,
                NotificationType.Success => Icons.Check,
                NotificationType.Warning => Icons.AlertOutline,
                NotificationType.Error => Icons.AlertOutline,
                _ => Icons.InformationOutline
            }
            , Foreground = model.Type switch
            {
                NotificationType.Information => SukiHost.GetGradient(Color.FromRgb(47,84,235)),
                NotificationType.Success => SukiHost.GetGradient(Color.FromRgb(82,196,26)),
                NotificationType.Warning => SukiHost.GetGradient(Color.FromRgb(240,140,22)),
                NotificationType.Error => SukiHost.GetGradient(Color.FromRgb(245,34,45)),
                _ => SukiHost.GetGradient(Color.FromRgb(89,126,255))
            }}, false, allowbackgroundclose);

    }

    private static LinearGradientBrush GetGradient(Color c1)
    {
        return new LinearGradientBrush()
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(){Color = c1, Offset = 0},
                new GradientStop(){Color = Color.FromArgb(140, c1.R,c1.G,c1.B), Offset = 1}
            }
        };
    }
    
    /// <summary>
    /// Attempts to close a dialog if one is shown in a specific window.
    /// </summary>
    public static void CloseDialog(Window window)
    {
        if (!Instances.TryGetValue(window, out var host))
            throw new InvalidOperationException("No SukiHost present in this window");
        host.IsDialogOpen = false;
       
    }

    /// <summary>
    /// Attempts to close a dialog if one is shown in the earliest of any opened windows.
    /// </summary>
    public static void CloseDialog() => CloseDialog(_mainWindow);
    
    /// <summary>
    /// Used to close the open dialog when the background is clicked, if this is allowed.
    /// </summary>
    private static void BackgroundRequestClose(SukiHost host)
    {
        if (!host.AllowBackgroundClose) return;
        host.IsDialogOpen = false;
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        if (VisualRoot is not Window w) return;
        Instances.Remove(w);
        _mainWindow = Instances.FirstOrDefault().Key;
    }
}