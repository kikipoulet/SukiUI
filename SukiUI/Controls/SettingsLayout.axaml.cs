using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using DynamicData;

namespace SukiUI.Controls;

public class SettingsLayoutItem
{
    public string Header { get; set; }
    public Control Content { get; set; }
}

public partial class SettingsLayout : UserControl
{
    public SettingsLayout()
    {
        InitializeComponent();
       
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        
        base.OnLoaded(e);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private ObservableCollection<SettingsLayoutItem> _items;

    public static readonly DirectProperty<SettingsLayout, ObservableCollection<SettingsLayoutItem>> StepsProperty =
        AvaloniaProperty.RegisterDirect<SettingsLayout, ObservableCollection<SettingsLayoutItem>>(nameof(Items), l => l.Items,
            (numpicker, v) =>
            {
                numpicker.Items = v;
              
            }, defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    public ObservableCollection<SettingsLayoutItem> Items
    {
        get { return _items; }
        set
        {
            SetAndRaise(StepsProperty, ref _items, value);
            UpdateItems(); 
        }
    }

    private void UpdateItems()
    {
        var stackItems = this.FindControl<StackPanel>("StackItems");
        var stackSummary = this.FindControl<StackPanel>("StackSummary");
        var myScroll = this.FindControl<ScrollViewer>("MyScroll");
        
        if (stackItems == null)
            return;
        
        stackItems.Children.Clear();
        stackSummary.Children.Clear();
        
        foreach (var settingsLayoutItem in Items)
        {
            Border border = new Border();
            border.Child = new GroupBox()
            {
                Margin = new Thickness(10,20),
                Header = settingsLayoutItem.Header,
                Content = new Border()
                {
                    Margin = new Thickness(35,12),
                    Child = settingsLayoutItem.Content
                }
            };
            stackItems.Children.Add(border);

            var summaryButton = new RadioButton()
            {
                Content = settingsLayoutItem.Header
            };
            summaryButton.Click += (sender, args) =>
            {
                var x = border.TranslatePoint(new Point(), stackItems);
                
                if(x.HasValue)
                    myScroll.Offset = new Vector(0, x.Value.Y);
            };
            stackSummary.Children.Add(summaryButton);
        }
        
        stackSummary.Children.Add(new Border(){Height = 300});
    }

    private void SizeChanged(object sender, SizeChangedEventArgs e)
    {
      var currentwidth = this.FindControl<StackPanel>("StackSummary").Width;
      var desiredSize = e.NewSize.Width > 1000 ? 400 : 0;
      if (desiredSize != currentwidth && (currentwidth == 0 || currentwidth == 400))
          AnimateSummaryWidth(this.FindControl<StackPanel>("StackSummary").Width, desiredSize);
    }

    private void AnimateSummaryWidth(double current, double desiredSize)
    {
        new Animation
        {
            Duration = TimeSpan.FromMilliseconds(800), FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(1), PlaybackDirection = PlaybackDirection.Normal, 
            Children =
            {
                new KeyFrame()
                {
                    Setters = { new Setter { Property = WidthProperty, Value = current } },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters = { new Setter { Property = WidthProperty, Value = desiredSize } },
                    KeyTime = TimeSpan.FromMilliseconds(800)
                }
            }
        }.RunAsync(this.FindControl<StackPanel>("StackSummary"));
    }
}