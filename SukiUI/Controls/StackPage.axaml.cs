using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;

namespace SukiUI.Controls;

public partial class StackPage : UserControl
{
    public StackPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<StackPage, string>(nameof(Header), defaultValue: "Header");

    public string Header
    {
        get { return GetValue(HeaderProperty); }
        set
        {
            SetValue(HeaderProperty, value); 
            
        }
    }
    
    public static readonly StyledProperty<Control> ContentProperty =
        AvaloniaProperty.Register<StackPage, Control>(nameof(Content), defaultValue: new Grid());

    public Control Content
    {
        get { return GetValue(ContentProperty); }
        set
        {
            SetValue(ContentProperty, value); 
            UpdateHeaders();
        }
    }

    public List<Tuple<string, Control>> Pages { get; set; } = new List<Tuple<string, Control>>();

    public void Push(string header, Control page)
    {
        Pages.Add(new Tuple<string, Control>(header, page));
        UpdateHeaders();
    }
    private void UpdateHeaders()
    {
        var stackHeaders = this.FindControl<StackPanel>("StackHeader");
        var currentControl = this.FindControl<TransitioningContentControl>("CurrentPage");
        
        stackHeaders.Children.Clear();

        if (!Pages.Any())
        {
            AddStrongHeader(Header);
            currentControl.Content = Content;
            return;
        }

        AddLowHeader(Header,00);

        var i = 1;
        foreach (var tuple in Pages.Take(Pages.Count - 1))
        {
            AddChevron();
            AddLowHeader(tuple.Item1,i);
            i++;
        }
        
        AddChevron();
        AddStrongHeader(Pages.Last().Item1);
        currentControl.Content = Pages.Last().Item2;
    }


    private void AddChevron()
    {
        var stackHeaders = this.FindControl<StackPanel>("StackHeader");
        var lowcolor = new SolidColorBrush( (Color)Application.Current.FindResource("SukiLowText"));
        
        stackHeaders.Children.Add(new MaterialIcon()
        {
            Kind = MaterialIconKind.ChevronRight,
            Foreground = lowcolor,
            Height = 24, Width = 24, Margin = new Thickness(15,-3,15,0),
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
        });
    }
    
    private void AddLowHeader(string s, int n =0)
    {
        
        var stackHeaders = this.FindControl<StackPanel>("StackHeader");
        var lowcolor = new SolidColorBrush( (Color)Application.Current.FindResource("SukiLowText"));

        var button = new TextBlock()
        {
            Classes = { "h2" }, Text = s, Foreground = lowcolor
        };
       

        button.PointerReleased += (sender, args) =>
        {
            Pages = Pages.Take(n).ToList();
            UpdateHeaders();
        };
        
        button.PointerEntered += (sender, args) =>
        {
            button.RenderTransform = new ScaleTransform() { ScaleX = 1.02, ScaleY = 1.02 };
        };
        
        button.PointerExited += (sender, args) =>
        {
            button.RenderTransform = new ScaleTransform() { ScaleX = 1, ScaleY = 1 };
        };
        
        stackHeaders.Children.Add(button);
    }

    private void AddStrongHeader(string s)
    {
        var stackHeaders = this.FindControl<StackPanel>("StackHeader");
        stackHeaders.Children.Add(new TextBlock()
        {
            Classes = { "h2" }, Text = s
        });
    }
}