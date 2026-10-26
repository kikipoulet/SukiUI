using System.Collections.Specialized;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System.Threading;

namespace SukiUI.Controls
{
    public partial class SettingsLayout : UserControl
    {
        public static readonly DirectProperty<SettingsLayout, IEnumerable<SettingsLayoutItem>> ItemsProperty =
            AvaloniaProperty.RegisterDirect<SettingsLayout, IEnumerable<SettingsLayoutItem>>(
                nameof(Items),
                o => o.Items,
                (o, v) => o.Items = v);

        public static readonly DirectProperty<SettingsLayout, double> MinWidthWhetherStackShowProperty =
            AvaloniaProperty.RegisterDirect<SettingsLayout, double>(
                nameof(MinWidthWhetherStackSummaryShow), o => o.MinWidthWhetherStackSummaryShow,
                (o, v) => o.MinWidthWhetherStackSummaryShow = v, 1100);

        public static readonly StyledProperty<double> StackSummaryWidthProperty =
            AvaloniaProperty.Register<SettingsLayout, double>(nameof(StackSummaryWidth), 400);

        private readonly List<Border> _borders = [];
        private readonly List<Border> _contentHosts = [];
        private readonly List<RadioButton> _radios = [];

        private IEnumerable<SettingsLayoutItem> _bounds = [];
        private INotifyCollectionChanged? _itemsCollection;

        private double _lastDesiredSize = -1;

        private double _minWidthWhetherStackSummaryShow = 1100;
        private ScrollViewer? _scrollViewer;
        private StackPanel? _stackItems;
        private StackPanel? _stackSummary;

        private bool isAnimatingScroll = false;
        private CancellationTokenSource? _scrollAnimationCancellation;

        public SettingsLayout()
        {
            InitializeComponent();
        }

        public IEnumerable<SettingsLayoutItem> Items
        {
            get => _bounds;
            set => SetAndRaise(ItemsProperty, ref _bounds, value);
        }

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

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            DetachItemsCollection();
            _scrollAnimationCancellation?.Cancel();
            _scrollAnimationCancellation?.Dispose();
            _scrollAnimationCancellation = null;
            isAnimatingScroll = false;
            if (_scrollViewer is not null)
                _scrollViewer.ScrollChanged -= MyScrollOnScrollChanged;
            base.OnDetachedFromLogicalTree(e);
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            AttachItemsCollection();
            UpdateItems();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            if (_scrollViewer is not null)
                _scrollViewer.ScrollChanged -= MyScrollOnScrollChanged;
            _stackSummary = e.NameScope.Find<StackPanel>("StackSummary");
            _scrollViewer = e.NameScope.Find<ScrollViewer>("MyScroll");
            _stackItems = _scrollViewer?.Content as StackPanel;
            AttachItemsCollection();
            UpdateItems();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ItemsProperty)
            {
                AttachItemsCollection();
                UpdateItems();
            }
        }

        private void UpdateItems()
        {
            if (_stackSummary is null || _stackItems is null || _scrollViewer is null)
                return;

            _scrollViewer.ScrollChanged -= MyScrollOnScrollChanged;
            foreach (var contentHost in _contentHosts)
                contentHost.Child = null;
            _contentHosts.Clear();
            _stackSummary.Children.Clear();
            _stackItems.Children.Clear();
            _radios.Clear();
            _borders.Clear();

            _stackItems.Children.Add(new Border { Height = 8 });

            foreach (var settingsLayoutItem in Items ?? [])
            {
                if (settingsLayoutItem.Header is null)
                {
                    continue;
                }

                var header = new TextBlock();
                header.Bind(TextBlock.TextProperty, settingsLayoutItem.GetObservable(SettingsLayoutItem.HeaderProperty));


                var contentHost = new Border
                {
                    Margin = new Thickness(35, 12),
                    Child = settingsLayoutItem.Content
                };
                _contentHosts.Add(contentHost);

                var gb = new GroupBox
                {
                    Margin = new Thickness(10, 20),
                    Header = header,
                    Content = contentHost
                };

                if (Classes.Contains("Touch"))
                    gb.Classes.Add("Touch");

                var border = new Border
                {
                    Child = gb
                };

                _borders.Add(border);
                _stackItems.Children.Add(border);

                var textBlock = new TextBlock { };
                textBlock.Bind(TextBlock.TextProperty, settingsLayoutItem.GetObservable(SettingsLayoutItem.HeaderProperty));

                var summaryButton = new RadioButton
                {
                    Content = textBlock,
                    Classes = { "MenuChip" }
                };
                summaryButton.Click += async (sender, args) =>
                {
                    if (isAnimatingScroll)
                        return;
                    var x = border.TranslatePoint(new Point(), _stackItems);

                    if (x.HasValue)
                        await AnimateScroll(x.Value.Y); // myScroll.Offset = new Vector(0, x.Value.Y);
                };
                _radios.Add(summaryButton);
                _stackSummary.Children.Add(summaryButton);
            }

            _stackSummary.Children.Add(new Border { Height = 300 });
            _scrollViewer.ScrollChanged += MyScrollOnScrollChanged;
        }

        private void MyScrollOnScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (isAnimatingScroll || _scrollViewer is null || _stackItems is null || _borders.Count == 0)
                return;

            var nearestIndex = -1;
            var nearestDistance = double.MaxValue;
            for (var i = 0; i < _borders.Count; i++)
            {
                var point = _borders[i].TranslatePoint(new Point(), _stackItems);
                if (point is null) continue;
                var distance = Math.Abs(point.Value.Y - _scrollViewer.Offset.Y);
                if (distance >= nearestDistance) continue;
                nearestDistance = distance;
                nearestIndex = i;
            }

            if (nearestIndex >= 0 && nearestIndex < _radios.Count)
                _radios[nearestIndex].IsChecked = true;
        }

        private void AttachItemsCollection()
        {
            DetachItemsCollection();
            _itemsCollection = Items as INotifyCollectionChanged;
            if (_itemsCollection is not null)
                _itemsCollection.CollectionChanged += ItemsCollectionOnCollectionChanged;
        }

        private void DetachItemsCollection()
        {
            if (_itemsCollection is not null)
                _itemsCollection.CollectionChanged -= ItemsCollectionOnCollectionChanged;
            _itemsCollection = null;
        }

        private void ItemsCollectionOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateItems();
        }

        private void DockPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var stack = _stackSummary;
            if (stack is null) return;
            var desiredSize = e.NewSize.Width > MinWidthWhetherStackSummaryShow ? StackSummaryWidth : 0;

            if (_lastDesiredSize == desiredSize)
                return;

            _lastDesiredSize = desiredSize;

            if (stack.Width != desiredSize && (stack.Width == 0 || stack.Width == StackSummaryWidth))
                stack.Animate<double>(WidthProperty, stack.Width, desiredSize, TimeSpan.FromMilliseconds(800));
        }

        private async Task AnimateScroll(double desiredScroll)
        {
            var myscroll = _scrollViewer;
            if (myscroll is null) return;

            _scrollAnimationCancellation?.Cancel();
            _scrollAnimationCancellation?.Dispose();
            var cancellation = new CancellationTokenSource();
            _scrollAnimationCancellation = cancellation;
            isAnimatingScroll = true;

            var animation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(800),
                FillMode = FillMode.Forward,
                Easing = new CubicEaseInOut(),
                IterationCount = new IterationCount(1),
                PlaybackDirection = PlaybackDirection.Normal,
                Children =
                {
                    new KeyFrame
                    {
                        Setters = { new Setter { Property = ScrollViewer.OffsetProperty, Value = myscroll.Offset } },
                        KeyTime = TimeSpan.FromSeconds(0)
                    },
                    new KeyFrame
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
            };

            try
            {
                await animation.RunAsync(myscroll, cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                // Detaching cancels the active transition.
            }
            finally
            {
                if (ReferenceEquals(_scrollAnimationCancellation, cancellation))
                {
                    _scrollAnimationCancellation = null;
                    isAnimatingScroll = false;
                }

                cancellation.Dispose();
            }
        }
    }
}
