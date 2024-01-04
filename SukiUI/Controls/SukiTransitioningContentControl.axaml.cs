using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;

namespace SukiUI.Controls
{
    // TODO: This needs fairly significant work to make a bit more bomb proof
    // There are probably some more gains that can be made in terms of performance.
    // Unfortunately we're still bound by the arrange of controls having to happen on the main thread.
    public class SukiTransitioningContentControl : ContentControl
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

        private bool _isFirstBufferActive;

        private Visual _firstBuffer = null!;
        private Visual _secondBuffer = null!;

        private IDisposable? _disposable;

        private static readonly Animation FadeIn;
        private static readonly Animation FadeOut;

        static SukiTransitioningContentControl()
        {
            FadeIn = new Animation
            {
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

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            if (e.NameScope.Get<ContentControl>("PART_FirstBufferControl") is { } fBuff)
                _firstBuffer = fBuff;
            if (e.NameScope.Get<ContentControl>("PART_SecondBufferControl") is { } sBuff)
                _secondBuffer = sBuff;

            _disposable = this.GetObservable(ContentProperty)
                .ObserveOn(new AvaloniaSynchronizationContext())
                .Subscribe(onNext: OnContentChanged);
        }
        
        public void OnContentChanged(object? content)
        {
            if (content is null) return;
            // Setting to higher priorities seems to help overall.
            if (_isFirstBufferActive) SecondBuffer = content;
            else FirstBuffer = content;
            var from = _isFirstBufferActive ? _firstBuffer : _secondBuffer;
            var to = _isFirstBufferActive ? _secondBuffer : _firstBuffer;
            FadeOut.RunAsync(from);
            FadeIn.RunAsync(to);
            ((InputElement)to).IsHitTestVisible = true;
            ((InputElement)from).IsHitTestVisible = false;
            _isFirstBufferActive = !_isFirstBufferActive;
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            _disposable?.Dispose();
        }
    }
}