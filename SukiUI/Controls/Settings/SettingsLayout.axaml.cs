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

namespace SukiUI.Controls;

public partial class SettingsLayout : UserControl
{
    public static readonly DirectProperty<SettingsLayout, IEnumerable<SettingsLayoutItem>> ItemsProperty =
    AvaloniaProperty.RegisterDirect<SettingsLayout, IEnumerable<SettingsLayoutItem>>(
        nameof(Items),
        o => o.Items);

    private IEnumerable<SettingsLayoutItem> _bounds;

    public IEnumerable<SettingsLayoutItem> Items
    {
        get { return _bounds; }
        set { SetAndRaise(ItemsProperty, ref _bounds, value); }
    }

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
        get => _minWidthWhetherStackSummaryShow;
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

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateItems();
    }

    private void UpdateItems()
    {
        if (Items is null)
        {
            return;
        }

        var stackSummary = this.GetTemplateChildren().First(n => n.Name == "StackSummary") as StackPanel;
        var myScroll = this.GetTemplateChildren().First(n => n.Name == "MyScroll") as ScrollViewer;

        if (myScroll?.Content is not StackPanel stackItems)
            return;

        if (stackSummary is not StackPanel)
            return;

        var radios = new List<RadioButton>();
        var borders = new List<Border>();

        stackItems.Children.Add(new Border() { Height = 8 });

        foreach (var settingsLayoutItem in Items)
        {
            if (settingsLayoutItem.Header is null)
            {
                continue;
            }

            var header = new TextBlock();
            header.Bind(TextBlock.TextProperty, new Binding(nameof(SettingsLayoutItem.Header))
            {
                Source = settingsLayoutItem
            });

            var border = new Border
            {
                Child = new GroupBox()
                {
                    Margin = new Thickness(10, 20),
                    Header = header,
                    Content = new Border()
                    {
                        Margin = new Thickness(35, 12),
                        Child = settingsLayoutItem.Content
                    }
                }
            };

            borders.Add(border);
            stackItems.Children.Add(border);

            var textBlock = new TextBlock { FontSize = 17 };
            textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(SettingsLayoutItem.Header))
            {
                Source = settingsLayoutItem
            });

            var summaryButton = new RadioButton()
            {
                Content = textBlock,
                Classes = { "MenuChip" }
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

    private double LastDesiredSize = -1;

    private void DockPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var stack = this.GetTemplateChildren().First(n => n.Name == "StackSummary");
        var desiredSize = e.NewSize.Width > MinWidthWhetherStackSummaryShow ? StackSummaryWidth : 0;

        if (LastDesiredSize == desiredSize)
            return;

        LastDesiredSize = desiredSize;

        if (stack.Width != desiredSize && (stack.Width == 0 || stack.Width == StackSummaryWidth))
            stack.Animate<double>(WidthProperty, stack.Width, desiredSize, TimeSpan.FromMilliseconds(800));
    }

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