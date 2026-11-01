using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Styling;
using SukiUI.Helpers;

namespace SukiUI.Controls.Touch
{
    public partial class TouchNavigationStack : UserControl
    {
        public static readonly StyledProperty<object?> InitialContentProperty =
            AvaloniaProperty.Register<TouchNavigationStack, object?>(nameof(InitialContent));

        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<TouchNavigationStack, string>(nameof(Title));

        private CancellationTokenSource? _contentTransitionCts;
        private Control? _currentPage;
        private string? _currentTitle;
        private object? _originalContent;
        private CancellationTokenSource? _revealCts;

        public TouchNavigationStack()
        {
            InitializeComponent();
        }

        public object? InitialContent
        {
            get => GetValue(InitialContentProperty);
            set => SetValue(InitialContentProperty, value);
        }

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == InitialContentProperty)
            {
                _originalContent = change.NewValue;
                if (_currentPage is null)
                    UpdateContent();
            }
            else if (change.Property == TitleProperty && _currentPage is null)
            {
                SetTitle(Title);
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            _originalContent = InitialContent;
            UpdateContent();
            _revealCts?.Cancel();
            _revealCts?.Dispose();
            _revealCts = new CancellationTokenSource();
            _ = RevealAsync(_revealCts.Token);
        }

        private async Task RevealAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(700, cancellationToken);
                await this.Get<DockPanel>("DP").AnimateAsync(OpacityProperty, 0d, 1d,
                    TimeSpan.FromMilliseconds(500), cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        // Affiche une nouvelle page (contrôle Avalonia), avec un titre
        public void Push(Control page, string title)
        {
            _currentPage = page;
            _currentTitle = title;
            UpdateContent();
        }

        // Revient à la page d'origine
        public void Pop()
        {
            _currentPage = null;
            _currentTitle = null;
            UpdateContent();
        }

        private void UpdateContent()
        {
            var headerPanel = this.FindControl<StackPanel>("HeaderPanel");
            var backButton = this.FindControl<Button>("BackButton");
            var contentPresenter = this.FindControl<ContentControl>("ContentP");
            if (headerPanel is null || backButton is null || contentPresenter is null)
                return;

            if (_currentPage != null)
            {
                // Affiche le header (titre + bouton retour)
                headerPanel.IsVisible = true;


                StartContentTransition(contentPresenter, _currentPage, () =>
                {
                    if (backButton.Width != 160)
                        backButton.Animate(WidthProperty).From(0).To(160).RunAsync();

                    SetTitle(_currentTitle ?? Title);
                });
            }
            else
            {
                // Cache le header et affiche le contenu d'origine
                headerPanel.IsVisible = true;
                StartContentTransition(contentPresenter, _originalContent, () =>
                {
                    backButton.Animate(WidthProperty).From(160).To(0).RunAsync();
                    SetTitle(_currentTitle ?? Title);
                });
            }
        }

        private void SetTitle(string currentTitle)
        {
            var titleTextfrom = this.FindControl<TextBlock>("TitleTextFrom");
            var titleTextto = this.FindControl<TextBlock>("TitleTextTo");
            if (titleTextfrom is null || titleTextto is null)
                return;

            titleTextfrom.Text = titleTextto.Text;

            titleTextfrom.Opacity = 1;
            titleTextto.Opacity = 0;

            titleTextto.Text = currentTitle;

            titleTextto.Animate(OpacityProperty).From(0).To(1).RunAsync();
            titleTextfrom.Animate(OpacityProperty).From(1).To(0).RunAsync();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _revealCts?.Cancel();
            _revealCts?.Dispose();
            _revealCts = null;
            _contentTransitionCts?.Cancel();
            _contentTransitionCts?.Dispose();
            _contentTransitionCts = null;
            base.OnDetachedFromVisualTree(e);
        }

        private void StartContentTransition(ContentControl content, object? nextContent, Action completed)
        {
            _contentTransitionCts?.Cancel();
            _contentTransitionCts?.Dispose();
            _contentTransitionCts = new CancellationTokenSource();
            _ = SetContentPresenterAsync(content, nextContent, completed, _contentTransitionCts.Token);
        }

        private static async Task SetContentPresenterAsync(ContentControl content, object? nextContent,
            Action completed,
            CancellationToken cancellationToken)
        {
            try
            {
                await content.AnimateAsync(OpacityProperty, 1d, 0d, TimeSpan.FromMilliseconds(250), cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                content.Content = nextContent;

                completed();
                await content.AnimateAsync(OpacityProperty, 0d, 1d, TimeSpan.FromMilliseconds(250), cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void BackButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Pop();
        }
    }


    public static class AnimationExtensions
    {
        public static Task AnimateAsync<T>(
            this Animatable control,
            AvaloniaProperty property,
            T from,
            T to,
            TimeSpan? duration = null,
            CancellationToken cancellationToken = default)
        {
            var animation = new Animation
            {
                Duration = duration ?? TimeSpan.FromMilliseconds(500),
                FillMode = FillMode.Forward,
                Easing = new CubicEaseInOut(),
                IterationCount = new IterationCount(1),
                PlaybackDirection = PlaybackDirection.Normal,
                Children =
                {
                    new KeyFrame
                    {
                        Setters = { new Setter { Property = property, Value = from } },
                        KeyTime = TimeSpan.FromSeconds(0)
                    },
                    new KeyFrame
                    {
                        Setters = { new Setter { Property = property, Value = to } },
                        KeyTime = duration ?? TimeSpan.FromMilliseconds(500)
                    }
                }
            };

            return animation.RunAsync(control, cancellationToken);
        }
    }
}