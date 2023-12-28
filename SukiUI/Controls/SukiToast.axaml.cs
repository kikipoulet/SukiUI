using System;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using SukiUI.Models;

namespace SukiUI.Controls;

public class SukiToast : ContentControl
{
    protected override Type StyleKeyOverride => typeof(SukiToast);

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SukiToast, string>(nameof(Title));

    private readonly Timer _timer = new();

    public SukiToast()
    {
        _timer.Elapsed += TimerOnElapsed;
    }

    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        _timer.Stop();
        SukiHost.RequestHideToast(this);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        e.NameScope.Get<GlassCard>("PART_ToastCard").PointerPressed += (_,_) => SukiHost.RequestHideToast(this);
    }
    
    public void Initialize(SukiToastModel model)
    {
        Title = model.Title;
        Content = model.Content;
        _timer.Interval = model.Lifetime.TotalMilliseconds;
        _timer.Start();
    }
}