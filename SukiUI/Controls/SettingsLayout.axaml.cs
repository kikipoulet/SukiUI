using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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

        List<RadioButton> radios = new List<RadioButton>();
        List<Border> borders = new List<Border>();
        
        stackItems.Children.Add(new Border(){Height = 8});
        
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
            
            borders.Add(border);
            stackItems.Children.Add(border);

            var summaryButton = new RadioButton()
            {
                Content = settingsLayoutItem.Header
            };
            summaryButton.Click += (sender, args) =>
            {
                if (isAnimatingScroll)
                    return;
                var x = border.TranslatePoint(new Point(), stackItems);
                
                if(x.HasValue)
                   AnimateScroll(x.Value.Y);  // myScroll.Offset = new Vector(0, x.Value.Y);
            };
            radios.Add(summaryButton);
            stackSummary.Children.Add(summaryButton);
        }
        
        stackSummary.Children.Add(new Border(){Height = 300});

        myScroll.ScrollChanged += (sender, args) =>
        {
            if (isAnimatingScroll)
                return;

            var OffsetY = myScroll.Offset.Y;
            
            var l = borders.Select(b => Math.Abs(b.TranslatePoint(new Point(), stackItems).Value.Y - OffsetY)).ToList();
            radios[l.IndexOf(l.Min())].IsChecked = true;
        };
    }

    private void SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (isAnimatingWidth)
            return;

        var currentwidth = this.FindControl<StackPanel>("StackSummary").Width;
      var desiredSize = e.NewSize.Width > 1000 ? 400 : 0;
      
      if(desiredSize != currentwidth)
        if ( (currentwidth == 0 || currentwidth == 400))
          AnimateSummaryWidth(this.FindControl<StackPanel>("StackSummary").Width, desiredSize);

      
      if (e.NewSize.Width <= 1000 && e.NewSize.Width > 850)
         AnimateItemsMargin(new Thickness(100, 0));
      else if (e.NewSize.Width <= 850 && e.NewSize.Width > 700)
          AnimateItemsMargin(new Thickness(50, 0));
      else
          AnimateItemsMargin( new Thickness(-10,0));
    }

    private bool isAnimatingWidth = false;
    private bool isAnimatingMargin = false;
    private bool isAnimatingScroll = false;
    
    private void AnimateItemsMargin(Thickness desiredSize)
    {

        if (isAnimatingMargin)
            return;
        
        var stackItems = this.FindControl<StackPanel>("StackItems");
        if (stackItems.Margin.Left == desiredSize.Left)
            return;
        
        isAnimatingMargin = true;
        
        new Animation
        {
            Duration = TimeSpan.FromMilliseconds(800), FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(1), PlaybackDirection = PlaybackDirection.Normal, 
            Children =
            {
                new KeyFrame()
                {
                    Setters = { new Setter { Property = MarginProperty, Value = stackItems.Margin } },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters = { new Setter { Property = MarginProperty, Value = desiredSize } },
                    KeyTime = TimeSpan.FromMilliseconds(800)
                }
            }
        }.RunAsync(stackItems);

        Task.Run(() =>
        {
            Thread.Sleep(1000);
            isAnimatingMargin = false;
        });
    }

    private void AnimateSummaryWidth(double current, double desiredSize)
    {
        isAnimatingWidth = true;
        
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

        Task.Run(() =>
        {
            Thread.Sleep(1000);
            isAnimatingWidth = false;
        });
    }

    private void AnimateScroll( double desiredScroll)
    {
       
        isAnimatingScroll = true;
        var myscroll = this.FindControl<ScrollViewer>("MyScroll");
        
        new Animation
        {
            Duration = TimeSpan.FromMilliseconds(800), FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(1), PlaybackDirection = PlaybackDirection.Normal, 
            Children =
            {
                new KeyFrame()
                {
                    Setters = { new Setter { Property = ScrollViewer.OffsetProperty, Value = myscroll.Offset } },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters = { new Setter { Property = ScrollViewer.OffsetProperty, Value = new Vector(myscroll.Offset.X, desiredScroll) } },
                    KeyTime = TimeSpan.FromMilliseconds(800)
                }
            }
        }.RunAsync(this.FindControl<ScrollViewer>("MyScroll"));

        Task.Run(() =>
        {
            Thread.Sleep(1000);
            isAnimatingScroll = false;
        });
    }
}