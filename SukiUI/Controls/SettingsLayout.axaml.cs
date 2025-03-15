using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SukiUI.Controls;

public class SettingsLayoutItem
public class SettingsLayoutItem : Control
{
    public string Header { get; set; }
    public Control Content { get; set; }
    public static readonly DirectProperty<SettingsLayoutItem, string?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<SettingsLayoutItem, string?>(
            nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);

    public string? Header
    {
        get { return _header; }
        set { SetAndRaise(HeaderProperty, ref _header, value); }
    }

    private string? _header;

    public static readonly DirectProperty<SettingsLayoutItem, Control?> ContentProperty =
    AvaloniaProperty.RegisterDirect<SettingsLayoutItem, Control?>(
        nameof(Content),
        o => o.Content,
        (o, v) => o.Content = v);

    public Control? Content
    {
        get { return _content; }
        set { SetAndRaise(ContentProperty, ref _content, value); }
    }

    private Control? _content;
}

public partial class SettingsLayout : UserControl
{
    public static readonly DirectProperty<SettingsLayout, double> MinWidthWhetherStackShowProperty =
        AvaloniaProperty.RegisterDirect<SettingsLayout, double>(
            nameof(MinWidthWhetherStackSummaryShow), o => o.MinWidthWhetherStackSummaryShow,
            (o, v) => o.MinWidthWhetherStackSummaryShow = v, 1100);

    public static readonly StyledProperty<double> StackSummaryWidthProperty =
        AvaloniaProperty.Register<SettingsLayout, double>(nameof(StackSummaryWidth), 400);

    public SettingsLayout()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateItems();
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private double _minWidthWhetherStackSummaryShow = 1100;

    /// <summary>
    /// Get or set a value that represents the minimum width for displaying the StackSummary in the SettingsLayout. 
    /// If the width of the SettingsLayout is less than this value, the StackSummary will not be displayed.
    /// The default value is 1100, and the minimum configurable value is 1.
    /// </summary>
    public double MinWidthWhetherStackSummaryShow
    {
        get=> _minWidthWhetherStackSummaryShow;
        set
        {
            if (value < 1)
            {
                return;
            }
            SetAndRaise(MinWidthWhetherStackShowProperty, ref _minWidthWhetherStackSummaryShow, value);
        }
    }

    /// <summary>
    /// Get or set the width of the StackSummary. The default value is 400, and the minimum configurable value is 0.
    /// </summary>
    public double StackSummaryWidth
    {
        get => GetValue(StackSummaryWidthProperty);
        set
        {
            if (value < 0)
            {
                return;
            }
            SetValue(StackSummaryWidthProperty, value);
        }
    }

    private ObservableCollection<SettingsLayoutItem> _items;

    public static readonly DirectProperty<SettingsLayout, ObservableCollection<SettingsLayoutItem>> StepsProperty =
        AvaloniaProperty.RegisterDirect<SettingsLayout, ObservableCollection<SettingsLayoutItem>>(nameof(Items),
            l => l.Items,
            (numpicker, v) => { numpicker.Items = v; }, defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public ObservableCollection<SettingsLayoutItem> Items
    {
        get { return _items; }
        set { SetAndRaise(StepsProperty, ref _items, value); }
    }

    private void UpdateItems()
    {
        if (Items is null)
        {
            return;
        }

        var stackSummary = (StackPanel)this.GetTemplateChildren().First(n => n.Name == "StackSummary");
        var myScroll = (ScrollViewer)this.GetTemplateChildren().First(n => n.Name == "MyScroll");

        if (myScroll.Content is not StackPanel stackItems)
            return;

        stackItems.Children.Clear();
        stackSummary.Children.Clear();

        var radios = new List<RadioButton>();
        var borders = new List<Border>();

        stackItems.Children.Add(new Border() { Height = 8 });

        foreach (var settingsLayoutItem in Items)
        {
            var border = new Border
            {
                Child = new GroupBox()
                {
                    Margin = new Thickness(10, 20),
                    Header = new TextBlock() { Text = settingsLayoutItem.Header },
                    Content = new Border()
                    {
                        Margin = new Thickness(35, 12),
                        Child = settingsLayoutItem.Content
                    }
                }
            };

            borders.Add(border);
            stackItems.Children.Add(border);

            var summaryButton = new RadioButton()
            {
                Content = new TextBlock() { Text = settingsLayoutItem.Header, FontSize = 17 },
                Classes = {  "MenuChip" }
            };
            summaryButton.Click += async (sender, args) =>
            {
                if (isAnimatingScroll)
                    return;
                var x = border.TranslatePoint(new Point(), stackItems);

                if (x.HasValue)
                    await AnimateScroll(x.Value.Y); // myScroll.Offset = new Vector(0, x.Value.Y);
            };
            radios.Add(summaryButton);
            stackSummary.Children.Add(summaryButton);
        }

        stackSummary.Children.Add(new Border() { Height = 300 });

        myScroll.ScrollChanged += (sender, args) =>
        {
            if (isAnimatingScroll)
                return;

            var OffsetY = myScroll.Offset.Y;

            var l = borders.Select(b => Math.Abs(b.TranslatePoint(new Point(), stackItems).Value.Y - OffsetY)).ToList();
            radios[l.IndexOf(l.Min())].IsChecked = true;
        };
    }

    private Mutex mut = new Mutex();

    private double LastDesiredSize = -1;

    private async void DockPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var stack = this.GetTemplateChildren().First(n => n.Name == "StackSummary");
        var desiredSize = e.NewSize.Width > MinWidthWhetherStackSummaryShow ? StackSummaryWidth : 0;

        if (LastDesiredSize == desiredSize)
            return;

        LastDesiredSize = desiredSize;

        if (stack.Width != desiredSize && (stack.Width == 0 || stack.Width == StackSummaryWidth))
            stack.Animate<double>(WidthProperty, stack.Width, desiredSize, TimeSpan.FromMilliseconds(800));
    }

    private bool isAnimatingWidth = false;
    private bool isAnimatingMargin = false;
    private bool isAnimatingScroll = false;

    private async Task AnimateScroll(double desiredScroll)
    {
        isAnimatingScroll = true;
        var myscroll = (ScrollViewer)this.GetTemplateChildren().First(n => n.Name == "MyScroll");

        var animationTask = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(800),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(1),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame()
                {
                    Setters = { new Setter { Property = ScrollViewer.OffsetProperty, Value = myscroll.Offset } },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = ScrollViewer.OffsetProperty,
                            Value = new Vector(myscroll.Offset.X, desiredScroll - 30)
                        }
                    },
                    KeyTime = TimeSpan.FromMilliseconds(800)
                }
            }
        }.RunAsync(myscroll);

        var abortTask = Task.Run(async () =>
        {
            await Task.Delay(1000);
            isAnimatingScroll = false;
        });

        await Task.WhenAll(animationTask, abortTask);
    }
}