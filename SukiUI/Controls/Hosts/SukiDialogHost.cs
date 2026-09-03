
using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Rendering.Composition;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SukiUI.Animations;
using SukiUI.Controls.GlassMorphism;
using SukiUI.Dialogs;
using SukiUI.Helpers;

namespace SukiUI.Controls
{
    public class SukiDialogHost : TemplatedControl
    {
        // Size-based spring calibration: dialog "mass" grows with its area, so bigger
        // dialogs get a slower, more damped spring and a smaller scale travel, while
        // small ones are allowed to be toy-like.
        private const double SmallDialogArea = 48_000.0;  // ~280x170
        private const double LargeDialogArea = 346_000.0; // ~700x495
        // The emergence always rises from below (fixed 100px vertical); the pointer only
        // steers the horizontal offset (direction, capped at 80px). Closing sinks straight
        // back down by the same vertical distance.
        private const double EmergenceVertical = 100.0;
        private const double EmergenceHorizontalMax = 50.0;

        // Pinned-dialog shake: a real spring given an initial velocity (an impulse, not a
        // keyframe wiggle), pushed away from the press vertically and decaying back to
        // rest — slow, soft, with several small oscillations.
        private const double ShakeOmega = 15.0;
        private const double ShakeDecay = 9.0; // zeta = 0.30: ~4 visible swings, 37% decay each
        private const double ShakeImpulse = 320.0; // px/s away from the press (~22px first swing)

        private Border? _dialogBackground;
        private ContentControl? _dialogContent;
        private Border? _dialogSurface;
        private ISukiDialogManager? _attachedManager;
        private bool _isAttachedToLogicalTree;
        private CancellationTokenSource? _dismissCts;

        // Last known pointer position (top-level coords), tracked so a dialog opening can
        // emerge from where the invoking click happened.
        private Point? _lastPointerPosition;
        private TopLevel? _pointerTrackingTopLevel;
        private EventHandler<PointerEventArgs>? _topLevelPointerMoved;

        // Open/close transitions built per opening, kept so a shake can detach and restore
        // them around its direct writes (single writer on the RenderTransform at a time).
        private Transitions? _openTransitions;

        // The glass overlay fades on its own fast clock (a DoubleTransition on
        // BlurBackground.OverlayOpacity), decoupled from the content's choreography:
        // melting the frost together with the content's fade read as the dialog
        // blackening, and the frost must be fully in before the content's opening
        // blur has finished collapsing.
        private DispatcherTimer? _shakeTimer;
        private double _shakeY, _shakeV, _shakeScale;
        private DateTime _shakeLastTick;
        public static readonly StyledProperty<ISukiDialogManager> ManagerProperty = AvaloniaProperty.Register<SukiDialogHost, ISukiDialogManager>(nameof(Manager));

        public ISukiDialogManager Manager
        {
            get => GetValue(ManagerProperty);
            set => SetValue(ManagerProperty, value);
        }
        
        public static readonly StyledProperty<object?> DialogProperty = AvaloniaProperty.Register<SukiDialogHost, object?>(nameof(Dialog));

        internal object? Dialog
        {
            get => GetValue(DialogProperty);
            set => SetValue(DialogProperty, value);
        }

        public static readonly StyledProperty<bool> IsDialogOpenProperty = AvaloniaProperty.Register<SukiDialogHost, bool>(nameof(IsDialogOpen));

        internal bool IsDialogOpen
        {
            get => GetValue(IsDialogOpenProperty);
            set => SetValue(IsDialogOpenProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            DetachTemplateEvents();
            if (e.NameScope.Find<Border>("PART_DialogBackground") is { } dialogBackground)
            {
                _dialogBackground = dialogBackground;
                dialogBackground.PointerPressed += DialogBackgroundOnPointerPressed;
                dialogBackground.Loaded += DialogBackgroundOnLoaded;
            }
            if (e.NameScope.Find<ContentControl>("PART_DialogContent") is { } dialogContent)
            {
                _dialogContent = dialogContent;
                // Rest pose before any open (no transitions on a fresh template, so this
                // lands instantly and invisibly).
                dialogContent.RenderTransform = TransformOperations.Parse("translate(0px, 0px) scale(0.72)");
                dialogContent.Opacity = 0.0;
            }
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            _isAttachedToLogicalTree = true;
            if (Manager is { } manager)
                AttachManagerEvents(manager);
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _isAttachedToLogicalTree = false;
            StopShake();
            DetachTemplateEvents();
            DetachManagerEvents();
            base.OnDetachedFromLogicalTree(e);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            // The TopLevel is only reliably reachable once the VISUAL tree is attached —
            // at logical-attach time GetTopLevel may still return null.
            WirePointerTracking();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            UnwirePointerTracking();
        }

        private void WirePointerTracking()
        {
            if (_pointerTrackingTopLevel != null || TopLevel.GetTopLevel(this) is not { } topLevel)
                return;
            _pointerTrackingTopLevel = topLevel;
            // Track every move in the window (handledEventsToo: presses swallowed by
            // controls still update the position). A click is always preceded by a move to
            // where it happens, so this is where dialogs emerge from.
            _topLevelPointerMoved = (_, e) => _lastPointerPosition = e.GetPosition(topLevel);
            topLevel.AddHandler(InputElement.PointerMovedEvent, _topLevelPointerMoved,
                Avalonia.Interactivity.RoutingStrategies.Bubble, handledEventsToo: true);
        }

        private void UnwirePointerTracking()
        {
            if (_pointerTrackingTopLevel is { } topLevel && _topLevelPointerMoved != null)
                topLevel.RemoveHandler(InputElement.PointerMovedEvent, _topLevelPointerMoved);
            _pointerTrackingTopLevel = null;
            _topLevelPointerMoved = null;
        }

        private void DialogBackgroundOnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (Dialog is ISukiDialog { CanDismissWithBackgroundClick: false })
            {
                // The dialog is pinned: physically push back — a vertical spring impulse
                // away from the press (a press below the center bumps it upward), decaying
                // to rest through a few small, soft oscillations.
                var click = e.GetPosition(this);
                double direction = click.Y - Bounds.Height / 2.0 >= 0.0 ? -1.0 : 1.0;
                StartShake(direction * ShakeImpulse);
                return;
            }
            BackgroundRequestClose();
        }

        private void DialogBackgroundOnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_dialogBackground is null)
                return;
            var visual = ElementComposition.GetElementVisual(_dialogBackground);
            if (visual is not null)
                CompositionAnimationHelper.MakeOpacityAnimated(visual, 400);
        }

        private void DetachTemplateEvents()
        {
            if (_dialogBackground is null)
                return;
            _dialogBackground.PointerPressed -= DialogBackgroundOnPointerPressed;
            _dialogBackground.Loaded -= DialogBackgroundOnLoaded;
            _dialogBackground = null;
            _dialogContent = null;
            _dialogSurface = null;
        }

        private void BackgroundRequestClose()
        {
            if (Dialog is not ISukiDialog { CanDismissWithBackgroundClick: true } sukiDialog) return;
            if (!sukiDialog.CanDismissWithBackgroundClick) return;
            Manager.TryDismissDialog(sukiDialog);
        }

        private static void OnManagerPropertyChanged(AvaloniaObject sender,
            AvaloniaPropertyChangedEventArgs propChanged)
        {
            if (sender is not SukiDialogHost host)
                throw new NullReferenceException("Dependency object is not of valid type " + nameof(SukiDialogHost));
            host.DetachManagerEvents();
            if (!host._isAttachedToLogicalTree)
                return;
            if (propChanged.NewValue is ISukiDialogManager manager)
                host.AttachManagerEvents(manager);
        }

        private void AttachManagerEvents(ISukiDialogManager newManager)
        {
            if (ReferenceEquals(_attachedManager, newManager))
                return;
            DetachManagerEvents();
            _attachedManager = newManager;
            newManager.OnDialogShown += ManagerOnDialogShown;
            newManager.OnDialogDismissed += ManagerOnDialogDismissed;
        }
        
        private void DetachManagerEvents()
        {
            if (_attachedManager is null)
                return;
            _attachedManager.OnDialogShown -= ManagerOnDialogShown;
            _attachedManager.OnDialogDismissed -= ManagerOnDialogDismissed;
            _attachedManager = null;
            _dismissCts?.Cancel();
            _dismissCts?.Dispose();
            _dismissCts = null;
        }

        private void ManagerOnDialogShown(object sender, SukiDialogManagerEventArgs args)
        {
            // Cancel any pending clear from a prior dismissal so a reused pooled instance
            // is not nulled out by the old timer.
            _dismissCts?.Cancel();
            _dismissCts?.Dispose();
            _dismissCts = null;
            Dialog = args.Dialog;
            IsDialogOpen = true;
            WirePointerTracking(); // last-chance, idempotent: needed for the emergence offset
            // One dispatcher pass later the content has been laid out: its measured size
            // calibrates the spring, and the pointer is still at the invoking click.
            Dispatcher.UIThread.Post(PlayOpenAnimation, DispatcherPriority.Loaded);
        }

        private void ManagerOnDialogDismissed(object sender, SukiDialogManagerEventArgs args)
        {
            IsDialogOpen = false;
            PlayCloseAnimation();
            _dismissCts?.Cancel();
            _dismissCts?.Dispose();
            var cts = new CancellationTokenSource();
            _dismissCts = cts;
            Task.Delay(500, cts.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;
                if (Dialog != args.Dialog) return;
                Dialog = null;
                _dismissCts?.Dispose();
                _dismissCts = null;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Opens the dialog with a fixed rise from below, steered horizontally toward the
        /// click that summoned it (the pointer is still at that spot, so no gesture channel
        /// is needed). The spring is calibrated against the dialog's measured area: small
        /// ones replay the button's release spring exactly, bigger ones get more mass —
        /// slower, more damped, no rebound.
        /// </summary>
        private void PlayOpenAnimation()
        {
            if (_dialogContent is not { } content)
                return;
            EnsureDialogSurface(content);

            double width = content.Bounds.Width > 0 ? content.Bounds.Width : content.DesiredSize.Width;
            double height = content.Bounds.Height > 0 ? content.Bounds.Height : content.DesiredSize.Height;
            double sizeT = Math.Clamp(
                (width * height - SmallDialogArea) / (LargeDialogArea - SmallDialogArea), 0.0, 1.0);

            // Small dialogs replay the button's release spring — same real-time frequency
            // (omega / duration = 16 rad/s, the button's own value, which needs the full
            // 650ms window to deploy) but more damped than the button itself (zeta 0.53,
            // i.e. a 14% rebound: the dialog is a bigger object, the full yo-yo would be
            // too much). The damping ramp is curved (sizeT^1.6): mid-size dialogs keep
            // noticeably more bounce than a linear ramp would leave them, and only truly
            // large dialogs go overdamped — no rebound at all, never longer than small.
            double sizeCurve = Math.Pow(sizeT, 1.6);
            double transformDurationMs = Lerp(650.0, 400.0, sizeCurve);
            double omega = Lerp(10.4, 5.8, sizeT); // omega / duration => 16.0 rad/s at the small end
            double zeta = Lerp(0.53, 1.05, sizeCurve);
            var spring = new SukiSpringEaseOut { Omega = omega, Decay = 2.0 * zeta * omega };
            var transformDuration = TimeSpan.FromMilliseconds(transformDurationMs);
            double fromScale = Lerp(0.72, 0.86, sizeT);

            // Initial pose with the transitions detached, then re-arm and head for the
            // target: the transitions animate from whatever pose is current, which is the
            // emerged-from-the-click one.
            content.Transitions = null;
            SetDialogPose(content, EmergenceOffset(), fromScale, 0.0, 40.0);
            FadeGlass(content, 1.0);
            _openTransitions = BuildTransitions(spring, transformDuration);
            content.Transitions = _openTransitions;
            SetDialogPose(content, (0.0, 0.0), 1.0, 1.0, 0.0);
        }

        /// <summary>
        /// Closes downward: the dialog sinks below its resting place, regardless of where
        /// the dismissal interaction (background press, close button, Escape) happened.
        /// </summary>
        private void PlayCloseAnimation()
        {
            if (_dialogContent is not { } content)
                return;
            // A shake leaves the transitions detached (it owns the transform while it
            // runs): restore them so the close actually animates instead of snapping.
            StopShake();
            content.Transitions ??= _openTransitions;
            FadeGlass(content, 0.0);
            SetDialogPose(content, (0.0, EmergenceVertical), 0.8, 0.0, 40.0);
        }

        /// <summary>
        /// The glass fades on its own clock at both ends of the dialog's life (the
        /// content's choreography stays out of it: melting the frost together with
        /// the content's fade read as the dialog blackening, and the frost must be
        /// fully in before the content's opening blur has finished collapsing).
        /// The transition is attached ONCE per glass lifetime — replacing a live
        /// Transitions collection and writing the target in the same frame makes
        /// the write land before the transition is armed and the value snaps.
        /// </summary>
        private const int GlassFadeMilliseconds = 220;

        private static void FadeGlass(ContentControl content, double to)
        {
            if (FindGlassOverlay(content) is not { } glass)
                return;
            if (glass.Transitions is null)
            {
                glass.Transitions = new Transitions
                {
                    new DoubleTransition
                    {
                        Property = BlurBackground.OverlayOpacityProperty,
                        Duration = TimeSpan.FromMilliseconds(GlassFadeMilliseconds)
                    }
                };
                // Same-frame attach + write snaps; give the transition one frame to arm.
                Dispatcher.UIThread.Post(() => glass.OverlayOpacity = to, DispatcherPriority.Loaded);
            }
            else
            {
                glass.OverlayOpacity = to;
            }
        }

        /// <summary>
        /// Struck-spring shake: the horizontal offset starts from the pose actually on
        /// screen (a press during the opening is handled gracefully) with an initial
        /// velocity, and integrates back to rest. The transitions are detached while the
        /// shake writes the transform directly, then re-armed.
        /// </summary>
        private void StartShake(double initialVelocity)
        {
            if (_dialogContent is not { } content)
                return;
            StopShake();
            var (ty, scale) = ReadCurrentTransform(content);
            _shakeY = ty;
            _shakeV = initialVelocity;
            _shakeScale = scale;
            _shakeLastTick = DateTime.Now;
            content.Transitions = null;

            var timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            timer.Tick += (_, _) => ShakeTick(content);
            _shakeTimer = timer;
            timer.Start();
        }

        private void ShakeTick(ContentControl content)
        {
            double dt = Math.Min((DateTime.Now - _shakeLastTick).TotalSeconds, 0.05);
            _shakeLastTick = DateTime.Now;

            int steps = Math.Max(1, (int)Math.Ceiling(dt / 0.008));
            double h = dt / steps;
            for (int i = 0; i < steps; i++)
            {
                double accel = -ShakeOmega * ShakeOmega * _shakeY - ShakeDecay * _shakeV;
                _shakeV += accel * h;
                _shakeY += _shakeV * h;
            }

            if (Math.Abs(_shakeY) < 0.5 && Math.Abs(_shakeV) < 10.0)
            {
                // Settled: hand the transform back to the transitions and restate the open
                // rest pose — if the shake interrupted an opening mid-flight, the remaining
                // travel resumes as a proper transition instead of snapping.
                StopShake();
                content.Transitions = _openTransitions;
                SetDialogPose(content, (0.0, 0.0), 1.0, 1.0, 0.0);
                return;
            }

            content.RenderTransform = TransformOperations.Parse(FormattableString.Invariant(
                $"translate(0px, {_shakeY:0.##}px) scale({_shakeScale:0.###})"));
        }

        private void StopShake()
        {
            _shakeTimer?.Stop();
            _shakeTimer = null;
        }

        private static (double Ty, double Scale) ReadCurrentTransform(ContentControl content)
        {
            var matrix = content.RenderTransform is { } transform ? transform.Value : Matrix.Identity;
            return (matrix.M32, matrix.M11);
        }

        private (double Dx, double Dy) EmergenceOffset()
        {
            double dx = 0.0;
            if (GetPointerPositionInHost() is { } click)
            {
                // Only the horizontal direction of the gesture survives; the vertical part
                // is always a fixed rise from below.
                dx = Math.Clamp(click.X - Bounds.Width / 2.0, -EmergenceHorizontalMax, EmergenceHorizontalMax);
            }
            return (dx, EmergenceVertical);
        }

        private Point? GetPointerPositionInHost()
        {
            if (_lastPointerPosition is not { } position || _pointerTrackingTopLevel is not { } topLevel)
                return null;
            return topLevel.TranslatePoint(position, this);
        }

        /// <summary>
        /// The dialog's depth-of-field surface (PART_DialogSurface, in the SukiDialog's
        /// ControlTheme — NOT this host's template, hence the visual-tree walk). The
        /// DoF blur lives there, never on the content control itself: the glass overlay
        /// renders through a custom draw op, and under an ancestor Effect it lands in
        /// the effect buffer (transparent backdrop) — its opaque restore pass then
        /// smears into black. Value first, transition attached after, so the initial
        /// pose lands instantly and every later write animates (same-frame attach+write
        /// snaps). Re-checked on every open: a re-applied template brings a fresh,
        /// uninitialized surface.
        /// </summary>
        private void EnsureDialogSurface(ContentControl content)
        {
            var surface = content.GetVisualDescendants()
                .OfType<Border>()
                .FirstOrDefault(b => b.Name == "PART_DialogSurface");
            if (surface is null || ReferenceEquals(surface, _dialogSurface))
                return;
            _dialogSurface = surface;
            surface.Effect = new BlurEffect { Radius = 40 };
            surface.Transitions = new Transitions
            {
                new EffectTransition { Property = Visual.EffectProperty, Duration = TimeSpan.FromMilliseconds(250) }
            };
        }

        private void SetDialogPose(
            ContentControl content, (double Dx, double Dy) offset, double scale, double opacity, double blur)
        {
            content.RenderTransform = TransformOperations.Parse(FormattableString.Invariant(
                $"translate({offset.Dx:0.##}px, {offset.Dy:0.##}px) scale({scale:0.###})"));
            content.Opacity = opacity;
            // Depth of field goes to the content surface, never the content control
            // itself: an Effect on the glass's ancestor pulls the custom op into the
            // effect buffer and breaks the overlay (see OnApplyTemplate).
            if (_dialogSurface is { } surface)
                surface.Effect = new BlurEffect { Radius = blur };
        }

        /// <summary>
        /// The dialog's glass overlay, if its template carries one (SukiDialog does).
        /// Found lazily per opening since the dialog content changes with each show.
        /// </summary>
        // Visual-tree walk, not logical: the glass lives inside the SukiDialog's
        // ControlTemplate, and template children are not logical descendants — the
        // logical lookup silently returned null here (glass stuck at its template
        // opacity, never driven).
        private static BlurBackground? FindGlassOverlay(ContentControl content) =>
            content.GetVisualDescendants().OfType<BlurBackground>().FirstOrDefault();

        private static Transitions BuildTransitions(Easing spring, TimeSpan transformDuration) => new()
        {
            new DoubleTransition { Property = Visual.OpacityProperty, Duration = TimeSpan.FromMilliseconds(300) },
            new TransformOperationsTransition
            {
                Property = Visual.RenderTransformProperty,
                Duration = transformDuration,
                Easing = spring
            }
        };

        private static double Lerp(double from, double to, double t) => from + (to - from) * t;

        static SukiDialogHost()
        {
            ManagerProperty.Changed.Subscribe(
                new Avalonia.Reactive.AnonymousObserver<AvaloniaPropertyChangedEventArgs<ISukiDialogManager>>(x =>
                    OnManagerPropertyChanged(x.Sender, x)));
        }
    }
}
