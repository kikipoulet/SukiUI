using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace SukiUI.Controls
{
    // TODO: This needs fairly significant work to make a bit more bomb proof
    // There are probably some more gains that can be made in terms of performance.
    // Unfortunately we're still bound by the arrange of controls having to happen on the main thread.
    public class SukiTransitioningContentControl : TemplatedControl
    {
        internal static readonly StyledProperty<object?> FirstBufferProperty =
            AvaloniaProperty.Register<SukiTransitioningContentControl, object?>(nameof(FirstBuffer));

        internal object? FirstBuffer
        {
            get => GetValue(FirstBufferProperty);
            set => SetValue(FirstBufferProperty, value);
        }

        internal static readonly StyledProperty<object?> SecondBufferProperty =
            AvaloniaProperty.Register<SukiTransitioningContentControl, object?>(nameof(SecondBuffer));

        internal object? SecondBuffer
        {
            get => GetValue(SecondBufferProperty);
            set => SetValue(SecondBufferProperty, value);
        }

        public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<SukiTransitioningContentControl, object?>(nameof(Content));

        public object? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the previous content remains composed while the next content fades in.
        /// </summary>
        public static readonly StyledProperty<bool> KeepPreviousContentDuringTransitionProperty =
            AvaloniaProperty.Register<SukiTransitioningContentControl, bool>(
                nameof(KeepPreviousContentDuringTransition), defaultValue: true);

        /// <summary>
        /// When <see langword="false"/>, releases the previous content before animating the next content.
        /// This avoids rendering two expensive visual trees during navigation.
        /// </summary>
        public bool KeepPreviousContentDuringTransition
        {
            get => GetValue(KeepPreviousContentDuringTransitionProperty);
            set => SetValue(KeepPreviousContentDuringTransitionProperty, value);
        }

        private bool _isFirstBufferActive;

        private ContentPresenter? _firstBuffer = null;
        private ContentPresenter? _secondBuffer = null;

        private static readonly Animation FadeIn;
        private static readonly Animation FadeOut;
        
        private object? _contentBeforeApplied;
        private bool _hasContentBeforeApplied;

        static SukiTransitioningContentControl()
        {
            FadeIn = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(400),
                Children =
                {
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = OpacityProperty,
                                Value = 0d
                            }
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = OpacityProperty,
                                Value = 1d
                            }
                        },
                        Cue = new Cue(1d)
                    }
                },
                FillMode = FillMode.Forward
            };
            FadeOut = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(400),
                Children =
                {
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = OpacityProperty,
                                Value = 1d
                            }
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = OpacityProperty,
                                Value = 0d
                            }
                        },
                        Cue = new Cue(1d)
                    }
                },
                FillMode = FillMode.Forward
            };
            FadeIn.Duration = FadeOut.Duration = TimeSpan.FromMilliseconds(250);
        }

        private CancellationTokenSource? _animCancellationToken;
        

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if(change.Property == ContentProperty)
                PushContent(change.NewValue);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            if (e.NameScope.Get<ContentPresenter>("PART_FirstBufferControl") is { } fBuff)
                _firstBuffer = fBuff;
            if (e.NameScope.Get<ContentPresenter>("PART_SecondBufferControl") is { } sBuff)
                _secondBuffer = sBuff;
            if (_hasContentBeforeApplied)
            {
                PushContent(_contentBeforeApplied);
                _contentBeforeApplied = null;
                _hasContentBeforeApplied = false;
            }
        }

        private void PushContent(object? content)
        {
            if (_firstBuffer is null || _secondBuffer is null)
            {
                _contentBeforeApplied = content;
                _hasContentBeforeApplied = true;
                return;
            }

            CancelAnimation();
            var cancellation = new CancellationTokenSource();
            _animCancellationToken = cancellation;

            var from = _isFirstBufferActive ? _firstBuffer : _secondBuffer;
            var to = _isFirstBufferActive ? _secondBuffer : _firstBuffer;
            var fromIsFirstBuffer = _isFirstBufferActive;

            if (_isFirstBufferActive)
                SecondBuffer = content;
            else
                FirstBuffer = content;
            _isFirstBufferActive = !_isFirstBufferActive;

            if (!KeepPreviousContentDuringTransition)
            {
                if (fromIsFirstBuffer)
                    FirstBuffer = null;
                else
                    SecondBuffer = null;
            }

            from.IsHitTestVisible = false;
            to.IsHitTestVisible = false;
            _ = RunTransitionAsync(from, to, fromIsFirstBuffer, KeepPreviousContentDuringTransition, cancellation);
        }

        private async Task RunTransitionAsync(ContentPresenter from, ContentPresenter to, bool fromIsFirstBuffer,
            bool keepPreviousContent, CancellationTokenSource cancellation)
        {
            // Snapshot the token as a value struct before any await so that accessing it
            // after the CancellationTokenSource has been disposed (in CancelAnimation) is safe.
            var token = cancellation.Token;
            try
            {
                if (keepPreviousContent)
                {
                    await Task.WhenAll(
                        FadeOut.RunAsync(from, token),
                        FadeIn.RunAsync(to, token));
                }
                else
                {
                    await FadeIn.RunAsync(to, token);
                }
                token.ThrowIfCancellationRequested();

                if (keepPreviousContent)
                {
                    if (fromIsFirstBuffer)
                        FirstBuffer = null;
                    else
                        SecondBuffer = null;
                }
                to.IsHitTestVisible = true;
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void CancelAnimation()
        {
            if (_animCancellationToken is null) return;
            _animCancellationToken.Cancel();
            _animCancellationToken.Dispose();
            _animCancellationToken = null;
            // Restore hit-testing on whichever buffer is now active so it remains interactive.
            var active = _isFirstBufferActive ? _firstBuffer : _secondBuffer;
            if (active != null) active.IsHitTestVisible = true;
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            CancelAnimation();
        }
    }
}
