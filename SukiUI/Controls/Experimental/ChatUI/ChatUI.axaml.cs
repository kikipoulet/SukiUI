using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

namespace SukiUI.Controls.Experimental
{
    public partial class ChatUI : UserControl
    {
        public ChatUI()
        {
            InitializeComponent();
        }

        private ScrollViewer ChatScroll;
        
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            Messages.CollectionChanged += ValueOnCollectionChanged;
            ChatScroll =  e.NameScope.Get<ScrollViewer>("ChatScrollViewer");
        }

        public static readonly StyledProperty<ObservableCollection<ChatMessage>> MessagesProperty =
            AvaloniaProperty.Register<ChatUI, ObservableCollection<ChatMessage>>(nameof(Messages), 
                defaultValue: new ObservableCollection<ChatMessage>());

        public ObservableCollection<ChatMessage> Messages
        {
            get { return GetValue(MessagesProperty); }
            set {
            {
                SetValue(MessagesProperty, value);
                value.CollectionChanged += ValueOnCollectionChanged;
            } }
        }

      

        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<ChatUI, string>(nameof(Text));
        
        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        
        public static readonly StyledProperty<IImage?> UserImageSourceProperty =
            AvaloniaProperty.Register<ChatUI, IImage?>(nameof(UserImageSource));
        
        public IImage? UserImageSource
        {
            get => GetValue(UserImageSourceProperty);
            set => SetValue(UserImageSourceProperty, value);
        }
        
        public static readonly StyledProperty<IImage?> FriendImageSourceProperty =
            AvaloniaProperty.Register<ChatUI, IImage?>(nameof(FriendImageSource));
        
        public IImage? FriendImageSource
        {
            get => GetValue(FriendImageSourceProperty);
            set => SetValue(FriendImageSourceProperty, value);
        }
        
        
        private void SendMessage(object sender, RoutedEventArgs e)
        {
            Messages.Add(new ChatMessage()
            {
                Content = Text, IsUser = true, IsWriting = false
            });

            Text = "";
            
           
        }

        private CancellationTokenSource AnimationToken;
        
        private void ValueOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            AnimationToken?.Cancel();
            AnimationToken = new CancellationTokenSource();
             new Animation
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
                        Setters = { new Setter { Property = ScrollViewer.OffsetProperty, Value = ChatScroll.Offset } },
                        KeyTime = TimeSpan.FromSeconds(0)
                    },
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = ScrollViewer.OffsetProperty,
                                Value = new Vector(ChatScroll.Offset.X, ChatScroll.Offset.Y + 500)
                            }
                        },
                        KeyTime = TimeSpan.FromMilliseconds(800)
                    }
                }
            }.RunAsync(ChatScroll, AnimationToken.Token);
        }
    }
    
    public class ContentToControlConverter : IValueConverter
    {
        public static ContentToControlConverter Instance = new ContentToControlConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
                return new TextBlock() { Text = (string?)value };

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}