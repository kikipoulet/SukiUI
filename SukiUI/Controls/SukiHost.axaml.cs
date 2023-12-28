using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using SukiUI.Helpers;

namespace SukiUI.Controls;

/// <summary>
/// Hosts both Dialogs and Notifications
/// </summary>
public class SukiHost : ContentControl
{
    protected override Type StyleKeyOverride => typeof(SukiHost);
    
    public static readonly StyledProperty<bool> IsDialogOpenProperty = AvaloniaProperty.Register<SukiHost, bool>(nameof(IsDialogOpen), defaultValue: false);
    public bool IsDialogOpen
    {
        get => GetValue(IsDialogOpenProperty);
        set => SetValue(IsDialogOpenProperty, value );
    }
    
    public static readonly StyledProperty<Control> DialogContentProperty = AvaloniaProperty.Register<SukiHost, Control>(nameof(DialogContent), defaultValue: new Grid());
    public Control DialogContent
    {
        get => GetValue(DialogContentProperty);
        set => SetValue(DialogContentProperty, value );
    }
    
    public static readonly StyledProperty<bool> AllowBackgroundCloseProperty = AvaloniaProperty.Register<SukiHost, bool>(nameof(AllowBackgroundClose), defaultValue: true);
    public bool AllowBackgroundClose
    {
        get => GetValue(AllowBackgroundCloseProperty);
        set => SetValue(AllowBackgroundCloseProperty, value);
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
        e.NameScope.Get<Border>("PART_DialogBackground").PointerPressed += (_, _) => BackgroundRequestClose();
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
}