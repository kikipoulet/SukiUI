using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
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

            this.GetObservable(ContentProperty)
                .ObserveOn(new AvaloniaSynchronizationContext())
                .Subscribe(onNext: OnContentChanged);
        }

        private readonly Task[] _tasks = new Task[2];
        
        public void OnContentChanged(object? content)
        {
            if (content is null) return;
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (_isFirstBufferActive) SecondBuffer = content;
                else FirstBuffer = content;
                var from = _isFirstBufferActive ? _firstBuffer : _secondBuffer;
                var to = _isFirstBufferActive ? _secondBuffer : _firstBuffer;
                _tasks[0] = FadeOut.RunAsync(from);
                _tasks[1] = FadeIn.RunAsync(to);
                await Task.WhenAll(_tasks);
                ((InputElement)to).IsHitTestVisible = true;
                ((InputElement)from).IsHitTestVisible = false;
                _isFirstBufferActive = !_isFirstBufferActive;
            }, DispatcherPriority.MaxValue);
            // Setting to higher priorities seems to help overall.
        }
    }
}