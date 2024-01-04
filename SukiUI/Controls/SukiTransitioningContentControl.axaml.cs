using System;
using System.Reactive.Linq;
using System.Threading;
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
                //to.Opacity = 1;
                ((InputElement)to).IsHitTestVisible = true;
                ((InputElement)from).IsHitTestVisible = false;
                //from.Opacity = 0;
                _isFirstBufferActive = !_isFirstBufferActive;
            }, DispatcherPriority.Background);
            //Task.Run(Swap);
        }
    }
}