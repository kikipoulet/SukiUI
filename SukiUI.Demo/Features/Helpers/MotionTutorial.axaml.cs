using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using SukiUI.Content;
using SukiUI.Controls;
using SukiUI.Demo.Controls;
using SukiUI.Motion;

namespace SukiUI.Demo.Features.Helpers
{
    // Inside the SukiUI namespace, the simple name "Motion" is the sibling namespace
    // SukiUI.Motion: this alias names the entry class.
    using Motion = SukiUI.Motion.Motion;

    /// <summary>
    /// The SukiUI.Motion tutorial: one card per lesson, each with a short explanation, a live
    /// demo and the exact code the demo runs.
    /// </summary>
    public partial class MotionTutorial : UserControl
    {
        private const double LaneWidth = 320;
        private const double PuckSize = 40;
        private const double Travel = LaneWidth - PuckSize - 16;

        public MotionTutorial()
        {
            InitializeComponent();

            Section("The basics");
            FirstAnimation();
            Curves();
            RenderProperties();
            AnyProperty();

            Section("Composing motions");
            Sequences();
            WaitForTheEnd();
            Interruptions();
            Impulses();
            Drive();
            Cascade();

            Section("Behaviors");
            InlineBehavior();
            PropertyAndTemplatePart();
            ReusableBehavior();
            Profiles();
            Popups();

            Section("Rules to remember");
            Rules();
        }

        // ---- 1. first animation ---------------------------------------------------------

        private void FirstAnimation()
        {
            var puck = Puck();
            Card("Your first animation",
                "Motion.Of(control) starts a motion on any control: pick a property, say where it goes, play it. Without a curve, the motion uses Spring.Smooth — no bounce, about half a second.",
                Column(
                    Center(puck),
                    Buttons(
                        ("Fade out", () => _ = Motion.Of(puck)
                            .Animate(Visual.OpacityProperty)
                            .To(0.2)
                            .PlayAsync()),
                        ("Fade in", () => _ = Motion.Of(puck)
                            .Animate(Visual.OpacityProperty)
                            .To(1.0)
                            .PlayAsync()))),
                """
                _ = Motion.Of(puck)
                    .Animate(Visual.OpacityProperty)
                    .To(0.2)
                    .PlayAsync();
                """);
        }

        // ---- 2. curves ------------------------------------------------------------------

        private void Curves()
        {
            var linear = Puck();
            var eased = Puck();
            var spring = Puck();
            var preset = Puck();
            var presets = new ComboBox
            {
                ItemsSource = new[] { "Smooth", "Snappy", "Bouncy", "Gentle" },
                SelectedIndex = 2,
                Width = 140,
            };
            bool right = false;

            void Play()
            {
                right = !right;
                double to = right ? Travel : 0.0;
                _ = Motion.Of(linear)
                    .Animate(SukiMotion.TranslateXProperty).To(to).Over(TimeSpan.FromMilliseconds(500))
                    .PlayAsync();
                _ = Motion.Of(eased)
                    .Animate(SukiMotion.TranslateXProperty).To(to).Over(TimeSpan.FromMilliseconds(500)).WithEasing(new CubicEaseInOut())
                    .PlayAsync();
                _ = Motion.Of(spring)
                    .Animate(SukiMotion.TranslateXProperty).To(to).WithSpring(new Spring(12.0, 6.0))
                    .PlayAsync();
                _ = Motion.Of(preset)
                    .Animate(SukiMotion.TranslateXProperty).To(to).WithSpring(PresetAt(presets.SelectedIndex))
                    .PlayAsync();
            }

            Card("Duration or spring",
                "Two kinds of curve. Over(duration) reaches the target in exactly that time, linear unless you add any Avalonia easing. WithSpring(...) is real physics: a damped spring that settles on the target — omega (how fast) and decay (how damped), or one of the presets.",
                Column(
                    Labeled("Over(500 ms)", Lane(linear)),
                    Labeled("Over + CubicEaseInOut", Lane(eased)),
                    Labeled("WithSpring(new Spring(12, 6))", Lane(spring)),
                    Labeled("WithSpring(preset)", Lane(preset), presets),
                    Buttons(("Play", Play))),
                """
                Motion.Of(puck).Animate(SukiMotion.TranslateXProperty)
                    .To(to).Over(TimeSpan.FromMilliseconds(500));

                Motion.Of(puck).Animate(SukiMotion.TranslateXProperty)
                    .To(to).Over(TimeSpan.FromMilliseconds(500)).WithEasing(new CubicEaseInOut());

                Motion.Of(puck).Animate(SukiMotion.TranslateXProperty)
                    .To(to).WithSpring(new Spring(omega: 12.0, decay: 6.0));

                // Presets: Spring.Smooth, Spring.Snappy, Spring.Bouncy, Spring.Gentle
                Motion.Of(puck).Animate(SukiMotion.TranslateXProperty)
                    .To(to).WithSpring(Spring.Bouncy);
                """);
        }

        private static Spring PresetAt(int index) => index switch
        {
            0 => Spring.Smooth,
            1 => Spring.Snappy,
            3 => Spring.Gentle,
            _ => Spring.Bouncy,
        };

        // ---- 3. render properties ---------------------------------------------------------

        private void RenderProperties()
        {
            var card = Tile("Card", 120, 70);
            var flags = new Dictionary<string, bool>();

            bool Flip(string key) => flags[key] = !flags.GetValueOrDefault(key);

            void Toggle(string key, MotionProperty<double> property, double on, double off) =>
                _ = Motion.Of(card)
                    .Animate(property).To(Flip(key) ? on : off).WithSpring(Spring.Snappy)
                    .PlayAsync();

            Card("Render properties",
                "SukiMotion adds the render properties Avalonia only exposes through objects: scale, translate, rotate and skew all compose in one transform (they never fight over RenderTransform), and blur and shadow share the Effect slot — dropped entirely at rest, so a finished blur costs no shader.",
                Column(
                    Center(card, height: 170),
                    Buttons(
                        ("Scale", () => Toggle("scale", SukiMotion.ScaleProperty, 1.4, 1.0)),
                        ("TranslateY", () => Toggle("y", SukiMotion.TranslateYProperty, -40, 0)),
                        ("Rotate", () => Toggle("rotate", SukiMotion.RotateProperty, 45, 0)),
                        ("SkewX", () => Toggle("skew", SukiMotion.SkewXProperty, 20, 0)),
                        ("Blur", () => _ = Motion.Of(card)
                            .Animate(SukiMotion.BlurProperty).To(Flip("blur") ? 8.0 : 0.0).Over(TimeSpan.FromMilliseconds(300))
                            .PlayAsync()),
                        ("Shadow", () =>
                        {
                            bool on = Flip("shadow");
                            _ = Motion.Of(card)
                                .Animate(SukiMotion.ShadowOpacityProperty).To(on ? 0.6 : 0.0).Over(TimeSpan.FromMilliseconds(300))
                                .And()
                                .Animate(SukiMotion.ShadowBlurProperty).To(on ? 24.0 : 0.0).Over(TimeSpan.FromMilliseconds(300))
                                .And()
                                .Animate(SukiMotion.ShadowOffsetYProperty).To(on ? 10.0 : 0.0).Over(TimeSpan.FromMilliseconds(300))
                                .PlayAsync();
                        }),
                        ("Reset", () =>
                        {
                            flags.Clear();
                            _ = Motion.Of(card)
                                .Set(SukiMotion.ScaleProperty).To(1.0)
                                .And().Set(SukiMotion.TranslateYProperty).To(0.0)
                                .And().Set(SukiMotion.RotateProperty).To(0.0)
                                .And().Set(SukiMotion.SkewXProperty).To(0.0)
                                .And().Set(SukiMotion.BlurProperty).To(0.0)
                                .And().Set(SukiMotion.ShadowOpacityProperty).To(0.0)
                                .PlayAsync();
                        }))),
                """
                // ScaleProperty, ScaleX/Y, TranslateX/Y, RotateProperty, SkewX/Y
                _ = Motion.Of(card)
                    .Animate(SukiMotion.RotateProperty).To(45.0).WithSpring(Spring.Snappy)
                    .PlayAsync();

                // BlurProperty, ShadowOpacity/Blur/OffsetX/OffsetY (one Effect slot: last writer owns it)
                _ = Motion.Of(card)
                    .Animate(SukiMotion.BlurProperty).To(8.0).Over(TimeSpan.FromMilliseconds(300))
                    .PlayAsync();

                // Set: at once, no animation — it always wins over a running motion.
                _ = Motion.Of(card).Set(SukiMotion.RotateProperty).To(0.0).PlayAsync();
                """);
        }

        // ---- 4. any property --------------------------------------------------------------

        private void AnyProperty()
        {
           
            var swatch = new GlowSwatch
            {
                Width = 120,
                Height = 70,
                CornerRadius = new CornerRadius(8),
                BorderBrush = Brushes.White,
                Glow = Colors.SlateBlue,
            };
            bool big = false;
            
            Card("Any property, any type",
                "Every Avalonia property animates the same way — the built-in ones and your own custom ones (here GlowSwatch.Glow, a Color). Supported types: double, float, int, Color, Point, Vector, Size, Thickness, CornerRadius; a spring moves each component on its own.",
                Column(
                    Center(swatch, height: 120),
                    Buttons(("Morph", () =>
                    {
                        big = !big;
                        _ = Motion.Of(swatch)
                            .Animate(Layoutable.WidthProperty).To(big ? 260.0 : 120.0).WithSpring(Spring.Snappy)
                            .And()
                            .Animate(Border.CornerRadiusProperty).To(new CornerRadius(big ? 35 : 8)).WithSpring(Spring.Snappy)
                            .And()
                            .Animate(Border.BorderThicknessProperty).To(new Thickness(big ? 4 : 0)).Over(TimeSpan.FromMilliseconds(300))
                            .And()
                            .Animate(GlowSwatch.GlowProperty).To(big ? Colors.DarkOrange : Colors.SlateBlue).Over(TimeSpan.FromMilliseconds(400))
                            .PlayAsync();
                    }))),
                """
                _ = Motion.Of(swatch)
                    .Animate(Layoutable.WidthProperty).To(260.0).WithSpring(Spring.Snappy)           // double
                    .And()
                    .Animate(Border.CornerRadiusProperty).To(new CornerRadius(35)).WithSpring(Spring.Snappy)
                    .And()
                    .Animate(Border.BorderThicknessProperty).To(new Thickness(4)).Over(TimeSpan.FromMilliseconds(300))
                    .And()
                    .Animate(GlowSwatch.GlowProperty).To(Colors.DarkOrange).Over(TimeSpan.FromMilliseconds(400)) // your own Color property
                    .PlayAsync();
                """);
        }

        // ---- 5. sequences -----------------------------------------------------------------

        private void Sequences()
        {
            var puck = Puck();

            Card("Then and And",
                "And() plays the next action together with the previous one (same step). Then() starts a new step once every action of the current one has finished. A chain reads top to bottom, exactly in the order it plays.",
                Column(
                    Lane(puck, height: 100),
                    Buttons(
                        ("Play", () => _ = Motion.Of(puck)
                            .Animate(SukiMotion.ScaleProperty).To(2.0).WithSpring(Spring.Bouncy)
                            .Then()
                            .Animate(SukiMotion.TranslateXProperty).To(200.0).Over(TimeSpan.FromMilliseconds(250)).WithEasing(new CubicEaseOut())
                            .And()
                            .Animate(SukiMotion.RotateProperty).To(180.0).Over(TimeSpan.FromMilliseconds(250))
                            .PlayAsync()),
                        ("Back", () => _ = Motion.Of(puck)
                            .Animate(SukiMotion.TranslateXProperty).To(0.0).WithSpring(Spring.Smooth)
                            .And()
                            .Animate(SukiMotion.RotateProperty).To(0.0).WithSpring(Spring.Smooth)
                            .Then()
                            .Animate(SukiMotion.ScaleProperty).To(1.0).WithSpring(Spring.Smooth)
                            .PlayAsync()))),
                """
                // Scale up to 2, then slide 200 px while turning half a turn.
                _ = Motion.Of(puck)
                    .Animate(SukiMotion.ScaleProperty).To(2.0).WithSpring(Spring.Bouncy)
                    .Then()
                    .Animate(SukiMotion.TranslateXProperty).To(200.0).Over(TimeSpan.FromMilliseconds(250)).WithEasing(new CubicEaseOut())
                    .And()
                    .Animate(SukiMotion.RotateProperty).To(180.0).Over(TimeSpan.FromMilliseconds(250))
                    .PlayAsync();
                """);
        }

        // ---- 6. wait for the end -------------------------------------------------------------

        private void WaitForTheEnd()
        {
            var puck = Puck();
            var status = Note("Press Hide, then Interrupt before it ends.");

            async void Hide()
            {
                status.Text = "Fading out…";
                bool completed = await Motion.Of(puck)
                    .Animate(Visual.OpacityProperty).To(0.0).Over(TimeSpan.FromMilliseconds(1500))
                    .PlayAsync();
                if (completed)
                    puck.IsVisible = false;
                status.Text = completed ? "PlayAsync returned true: the puck is now hidden." : "PlayAsync returned false: interrupted, the puck stays.";
            }

            void Show()
            {
                puck.IsVisible = true;
                _ = Motion.Of(puck)
                    .Animate(Visual.OpacityProperty).From(0.0).To(1.0).Over(TimeSpan.FromMilliseconds(300))
                    .PlayAsync();
                status.Text = "Shown: From(0.0) is written before the first frame — no flash.";
            }

            Card("Wait for the end",
                "PlayAsync() returns true when the motion went to the end, false when something interrupted it. Hide or remove an element only on true. From(...) writes the start value immediately, so showing an element and fading it in in the same call never flashes.",
                Column(
                    Center(puck),
                    Buttons(("Hide", Hide), ("Interrupt", () => Motion.Stop(puck)), ("Show", Show)),
                    status),
                """
                bool completed = await Motion.Of(puck)
                    .Animate(Visual.OpacityProperty).To(0.0).Over(TimeSpan.FromMilliseconds(1500))
                    .PlayAsync();
                if (completed)
                    puck.IsVisible = false;   // only once the fade really ended

                Motion.Stop(puck);            // interrupts it: PlayAsync returns false

                puck.IsVisible = true;
                _ = Motion.Of(puck)
                    .Animate(Visual.OpacityProperty).From(0.0).To(1.0).Over(TimeSpan.FromMilliseconds(300))
                    .PlayAsync();
                """);
        }

        // ---- 7. interruptions -------------------------------------------------------------------

        private void Interruptions()
        {
            var puck = Puck();
            var lane = Lane(puck);
            lane.Cursor = new Cursor(StandardCursorType.Hand);
            lane.PointerPressed += (_, e) =>
            {
                double x = Math.Clamp(e.GetPosition(lane).X - 8 - PuckSize / 2, 0, Travel);
                _ = Motion.Of(puck)
                    .Animate(SukiMotion.TranslateXProperty).To(x).WithSpring(Spring.Bouncy)
                    .PlayAsync();
            };

            Card("Interruptions keep their momentum",
                "Click anywhere in the lane, several times, quickly. A new motion on a property interrupts the running one and starts from the pose on screen — a spring also carries the live velocity, so the puck never jumps nor stops dead: it turns around.",
                Column(lane, Note("Click the lane.")),
                """
                lane.PointerPressed += (_, e) =>
                {
                    double x = e.GetPosition(lane).X - 28;
                    _ = Motion.Of(puck)
                        .Animate(SukiMotion.TranslateXProperty).To(x).WithSpring(Spring.Bouncy)
                        .PlayAsync();
                };
                """);
        }

        // ---- 8. impulses ------------------------------------------------------------------------

        private void Impulses()
        {
            var badge = Tile("♥", 60, 60);
            var card = Tile("Card", 120, 60);

            Card("Impulses",
                "AddVelocity(...) strikes a spring: it adds a kick to the live velocity and the spring settles back on its target. A pulse is a scale kick around 1.0; a shake is a sideways kick around 0. Kicks add up when they come fast.",
                Column(
                    Row(Center(badge, height: 100), Center(card, height: 100)),
                    Buttons(
                        ("Pulse", () => _ = Motion.Of(badge)
                            .Animate(SukiMotion.ScaleProperty).To(1.0).WithSpring(Spring.Bouncy).AddVelocity(6.0)
                            .PlayAsync()),
                        ("Shake", () => _ = Motion.Of(card)
                            .Animate(SukiMotion.TranslateXProperty).To(0.0).WithSpring(new Spring(30.0, 6.0)).AddVelocity(600.0)
                            .PlayAsync()))),
                """
                // Pulse: a kick on the scale, which springs back to 1.0.
                _ = Motion.Of(badge)
                    .Animate(SukiMotion.ScaleProperty).To(1.0).WithSpring(Spring.Bouncy).AddVelocity(6.0)
                    .PlayAsync();

                // Shake: a sideways kick on a stiff, lightly damped spring around 0.
                _ = Motion.Of(card)
                    .Animate(SukiMotion.TranslateXProperty).To(0.0).WithSpring(new Spring(30.0, 6.0)).AddVelocity(600.0)
                    .PlayAsync();
                """);
        }

        // ---- 9. drive ---------------------------------------------------------------------------

        private void Drive()
        {
            var puck = Puck();
            bool right = false;

            static double Speed(DriveContext<Border, NoProfile, MotionPlayArgs> ctx) =>
                Math.Abs(ctx.VelocityOf(SukiMotion.TranslateXProperty));

            Card("Drive: values derived from a motion",
                "Drive(property).From(source).With(...) writes a property every frame from the live value or velocity of other properties, for as long as they move — then it stops, costing nothing. Here the blur and the squash & stretch follow the puck's speed.",
                Column(
                    Lane(puck),
                    Buttons(("Throw", () =>
                    {
                        right = !right;
                        _ = Motion.Of(puck)
                            .Animate(SukiMotion.TranslateXProperty).To(right ? Travel : 0.0).WithSpring(Spring.Smooth)
                            .And()
                            .Drive(SukiMotion.BlurProperty)
                                .From(SukiMotion.TranslateXProperty)
                                .With(ctx => Math.Min(Speed(ctx) * 0.006, 8.0))
                            .And()
                            .Drive(SukiMotion.ScaleXProperty)
                                .From(SukiMotion.TranslateXProperty)
                                .With(ctx => 1.0 + Math.Min(Speed(ctx) * 0.0006, 0.5))
                            .And()
                            .Drive(SukiMotion.ScaleYProperty)
                                .From(SukiMotion.TranslateXProperty)
                                .With(ctx => 1.0 - Math.Min(Speed(ctx) * 0.0006, 0.5))
                            .PlayAsync();
                    }))),
                """
                _ = Motion.Of(puck)
                    .Animate(SukiMotion.TranslateXProperty).To(260.0).WithSpring(Spring.Smooth)
                    .And()
                    .Drive(SukiMotion.BlurProperty)
                        .From(SukiMotion.TranslateXProperty)
                        .With(ctx => Math.Min(Math.Abs(ctx.VelocityOf(SukiMotion.TranslateXProperty)) * 0.006, 8.0))
                    .And()
                    .Drive(SukiMotion.ScaleXProperty)
                        .From(SukiMotion.TranslateXProperty)
                        .With(ctx => 1.0 + Math.Min(Math.Abs(ctx.VelocityOf(SukiMotion.TranslateXProperty)) * 0.0006, 0.5))
                    .PlayAsync();

                // ctx.ValueOf(property) reads the source's value instead (e.g. a blur that follows an opacity).
                """);
        }

        // ---- 10. cascade ------------------------------------------------------------------------

        private void Cascade()
        {
            var list = new StackPanel { Spacing = 6, Width = 280 };
            for (int i = 1; i <= 6; i++)
                list.Children.Add(Tile($"Item {i}", 280, 32));
            var stagger = new Slider { Minimum = 20, Maximum = 160, Value = 60, Width = 160 };

            Card("Cascade",
                "Cascade(items, item => …) plays the same animations on every item, one after the other. Inside the lambda, Animate(property) targets the item. WithStagger sets the delay between two items (it can depend on the item count), OnlyWhenFewerThan skips long lists.",
                Column(
                    Center(list, height: 250),
                    Labeled("Stagger", stagger),
                    Buttons(("Replay", () => _ = Motion.Of(list)
                        .Cascade(ctx => ctx.Control.Children, item => item
                            .Animate(Visual.OpacityProperty).From(0.0).To(1.0).Over(TimeSpan.FromMilliseconds(300))
                            .And()
                            .Animate(SukiMotion.TranslateYProperty).From(16.0).To(0.0).Over(TimeSpan.FromMilliseconds(300)).WithEasing(new CubicEaseOut())
                            .And()
                            .Animate(SukiMotion.BlurProperty).From(6.0).To(0.0).Over(TimeSpan.FromMilliseconds(200)))
                        .WithStagger(TimeSpan.FromMilliseconds(stagger.Value))
                        .PlayAsync()))),
                """
                _ = Motion.Of(list)
                    .Cascade(ctx => ctx.Control.Children, item => item
                        .Animate(Visual.OpacityProperty).From(0.0).To(1.0).Over(TimeSpan.FromMilliseconds(300))
                        .And()
                        .Animate(SukiMotion.TranslateYProperty).From(16.0).To(0.0).Over(TimeSpan.FromMilliseconds(300)).WithEasing(new CubicEaseOut())
                        .And()
                        .Animate(SukiMotion.BlurProperty).From(6.0).To(0.0).Over(TimeSpan.FromMilliseconds(200)))
                    .WithStagger(TimeSpan.FromMilliseconds(60))
                    .OnlyWhenFewerThan(20)      // optional: a long list shows at once
                    .PlayAsync();
                """);
        }

        // ---- 11. inline behavior ----------------------------------------------------------------

        private void InlineBehavior()
        {
            var tile = Tile("Press me", 180, 90);

            Motion.Configure(tile, motion =>
            {
                motion.On(InputElement.PointerEnteredEvent)
                      .Where(ctx => !ctx.Args.Properties.IsLeftButtonPressed)
                      .Animate(SukiMotion.ScaleProperty)
                          .To(1.05)
                          .Over(TimeSpan.FromMilliseconds(150))
                          .WithEasing(new CubicEaseOut())
                          .RetargetIfMoving();

                motion.On(InputElement.PointerExitedEvent)
                      .Where(ctx => !ctx.Args.Properties.IsLeftButtonPressed)
                      .Animate(SukiMotion.ScaleProperty)
                          .To(1.0)
                          .Over(TimeSpan.FromMilliseconds(150))
                          .WithEasing(new CubicEaseOut())
                          .RetargetIfMoving();

                motion.On(InputElement.PointerPressedEvent)
                      .Do(ctx => ctx.Args.Pointer.Capture(ctx.Control))
                      .And()
                      .Animate(SukiMotion.ScaleProperty)
                          .To(0.92)
                          .Over(TimeSpan.FromMilliseconds(120))
                          .MustFinish()
                      .Then()
                      .Animate(SukiMotion.ScaleProperty)
                          .To(0.85)
                          .Over(TimeSpan.FromSeconds(1.5));

                motion.On(InputElement.PointerReleasedEvent)
                      .Or(InputElement.PointerCaptureLostEvent)
                      .Animate(SukiMotion.ScaleProperty)
                          .To(ctx => ctx.Control.IsPointerOver ? 1.05 : 1.0)
                          .WithSpring(Spring.Bouncy);
            });

            Card("A behavior on a control",
                "Motion.Configure(control, motion => …) wires a whole behavior in one place: motion.On(event) takes any typed Avalonia event (ctx.Args is typed by it), Where filters it, and what follows is what it plays. Try a very short click, a long press, and leaving while the tile still bounces.",
                Column(Center(tile, height: 140), Note(
                    "MustFinish(): the descent always plays to the end — a release during it waits, so even the shortest click shows the press.\n" +
                    "RetargetIfMoving(): hovering while the release spring bounces only moves its target — the bounce is never cut.\n" +
                    "Where(...): no hover reaction while the button is held.")),
                """
                Motion.Configure(tile, motion =>
                {
                    // Hover (not while held): grow a little — without cutting a bounce short.
                    motion.On(InputElement.PointerEnteredEvent)
                          .Where(ctx => !ctx.Args.Properties.IsLeftButtonPressed)
                          .Animate(SukiMotion.ScaleProperty)
                              .To(1.05)
                              .Over(TimeSpan.FromMilliseconds(150))
                              .WithEasing(new CubicEaseOut())
                              .RetargetIfMoving();

                    // (PointerExitedEvent: the same, back to 1.0)

                    // Press: capture the pointer, sink — the descent always plays to the end —
                    // then keep sinking slowly while held.
                    motion.On(InputElement.PointerPressedEvent)
                          .Do(ctx => ctx.Args.Pointer.Capture(ctx.Control))
                          .And()
                          .Animate(SukiMotion.ScaleProperty).To(0.92).Over(TimeSpan.FromMilliseconds(120)).MustFinish()
                          .Then()
                          .Animate(SukiMotion.ScaleProperty).To(0.85).Over(TimeSpan.FromSeconds(1.5));

                    // Release: spring back (to the hover size if the pointer is still over).
                    motion.On(InputElement.PointerReleasedEvent)
                          .Or(InputElement.PointerCaptureLostEvent)
                          .Animate(SukiMotion.ScaleProperty)
                              .To(ctx => ctx.Control.IsPointerOver ? 1.05 : 1.0)
                              .WithSpring(Spring.Bouncy);
                });
                """);
        }

        // ---- 12. property change and template part --------------------------------------------

        private void PropertyAndTemplatePart()
        {
            var toggle = new ToggleButton
            {
                Content = new PathIcon { Data = Icons.ChevronRight, Width = 14, Height = 14 },
                Width = 44,
                Height = 44,
            };

            Motion.Configure(toggle, motion =>
            {
                var content = motion.TemplatePart("PART_ContentPresenter");
                motion.OnChanged(ToggleButton.IsCheckedProperty)
                      .Animate(content, SukiMotion.RotateProperty)
                          .To(ctx => ctx.Control.IsChecked == true ? 90.0 : 0.0)
                          .WithSpring(Spring.Snappy);
            });

            Card("React to a property, animate a template part",
                "motion.OnChanged(property) reacts to any property change. motion.TemplatePart(name) targets a named part of the control's template — looked up when each animation starts, so a re-applied template is picked up by itself.",
                Column(Center(toggle, height: 90), Note("Toggle the button: its chevron turns.")),
                """
                Motion.Configure(toggle, motion =>
                {
                    var content = motion.TemplatePart("PART_ContentPresenter");

                    motion.OnChanged(ToggleButton.IsCheckedProperty)
                          .Animate(content, SukiMotion.RotateProperty)
                              .To(ctx => ctx.Control.IsChecked == true ? 90.0 : 0.0)
                              .WithSpring(Spring.Snappy);
                });
                """);
        }

        // ---- 13. reusable behavior --------------------------------------------------------------

        private void ReusableBehavior()
        {
            var chips = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Center };
            var wobble = new WobbleMotion();
            foreach (var name in new[] { "Hover", "each", "of", "these" })
            {
                var chip = Tile(name, 90, 40);
                chip.Margin = new Thickness(6);
                Motion.Attach(chip, wobble);
                chips.Children.Add(chip);
            }

            Card("A reusable behavior",
                "Derive from MotionBehavior<TControl> and declare it once in Configure. RegisterEnable gives the class a XAML Enable property — set it on an element or from a style (the badges at the top of this page use both) — and Motion.Attach(control, behavior) attaches it from C#. It is exactly how SukiUI's own press, toggle, typing and popup motions are written.",
                Column(Center(chips, height: 80)),
                """
                public sealed class WobbleMotion : MotionBehavior<InputElement>
                {
                    public static readonly AttachedProperty<bool> EnableProperty =
                        RegisterEnable<WobbleMotion, InputElement>();

                    public static bool GetEnable(InputElement element) => element.GetValue(EnableProperty);
                    public static void SetEnable(InputElement element, bool value) => element.SetValue(EnableProperty, value);

                    protected override void Configure(MotionBuilder<InputElement> motion)
                    {
                        // Hover: an angular kick on a lightly damped spring around 0°.
                        motion.On(InputElement.PointerEnteredEvent)
                              .Animate(SukiMotion.RotateProperty)
                                  .To(0.0)
                                  .WithSpring(new Spring(20.0, 5.0))
                                  .AddVelocity(240.0);

                        // Removed: the element goes back upright.
                        motion.OnDisabled()
                              .Set(SukiMotion.RotateProperty).To(0.0);
                    }
                }
                """,
                xaml: """
                <!-- XAML: on an element… -->
                <Border local:WobbleMotion.Enable="True" />

                <!-- …or from a style -->
                <Style Selector="Border.chip">
                    <Setter Property="local:WobbleMotion.Enable" Value="True" />
                </Style>

                <!-- C#: attach an instance from code, dispose to remove it
                     IDisposable wobbling = Motion.Attach(chip, new WobbleMotion()); -->
                """);
        }

        // ---- 14. profiles ------------------------------------------------------------------------

        private void Profiles()
        {
            var tile = Tile("Press me", 160, 70);
            Motion.Attach(tile, new SquishMotion());
            var snappy = new ToggleSwitch { Content = "Snappy profile" };
            snappy.IsCheckedChanged += (_, _) =>
                SquishProfile.Current = snappy.IsChecked == true ? SquishProfile.Snappy : SquishProfile.Soft;

            Card("Profiles",
                "A behavior can take a profile — any type you like, here a record holding a scale and a Spring. GetProfile runs each time an event fires, so ctx.Profile is the profile of that gesture: switch it live and the next press uses it. Without a profile, use MotionBehavior<TControl> and plain values.",
                Column(Center(tile, height: 110), Center(snappy, height: 40)),
                """
                public sealed record SquishProfile(double PressedScale, Spring Release)
                {
                    public static readonly SquishProfile Soft = new(0.9, Spring.Gentle);
                    public static readonly SquishProfile Snappy = new(0.75, Spring.Bouncy);
                    public static SquishProfile Current { get; set; } = Soft;
                }

                public sealed class SquishMotion : MotionBehavior<InputElement, SquishProfile>
                {
                    // EnableProperty, GetEnable, SetEnable: as in the lesson above.

                    protected override SquishProfile GetProfile(InputElement control) => SquishProfile.Current;

                    protected override void Configure(MotionBuilder<InputElement, SquishProfile> motion)
                    {
                        motion.On(InputElement.PointerPressedEvent)
                              .Do(ctx => ctx.Args.Pointer.Capture(ctx.Control))
                              .And()
                              .Animate(SukiMotion.ScaleYProperty).To(ctx => ctx.Profile.PressedScale).Over(TimeSpan.FromMilliseconds(100))
                              .And()
                              .Animate(SukiMotion.ScaleXProperty).To(ctx => 2.0 - ctx.Profile.PressedScale).Over(TimeSpan.FromMilliseconds(100));

                        motion.On(InputElement.PointerReleasedEvent)
                              .Or(InputElement.PointerCaptureLostEvent)
                              .Animate(SukiMotion.ScaleYProperty).To(1.0).WithSpring(ctx => ctx.Profile.Release)
                              .And()
                              .Animate(SukiMotion.ScaleXProperty).To(1.0).WithSpring(ctx => ctx.Profile.Release);
                    }
                }
                """);
        }

        // ---- 15. popups ---------------------------------------------------------------------------

        private void Popups()
        {
            var combo = new ComboBox
            {
                ItemsSource = new[] { "Springs", "Motion blur", "Item cascade", "Held close" },
                SelectedIndex = 0,
                Width = 200,
            };

            Card("Popups",
                "motion.Popup() animates the popup of a templated control. The section owns the real popup: it opens it once the start pose is written, closes it only at the end of the closing motion, resumes from the live pose when reopened mid-collapse, and keeps the host in sync when the popup closes behind its back. This ComboBox runs SukiPopupMotion.",
                Column(Center(combo, height: 70)),
                """
                protected override void Configure(MotionBuilder<TemplatedControl, SukiPopupProfile> motion)
                {
                    var popup = motion.Popup()
                        .PopupPart("PART_SukiPopup")
                        .RootPart("PART_LayoutTransform")
                        .ItemsPresenterPart("PART_ItemsPresenter")
                        .OpenWhen(ComboBox.IsDropDownOpenProperty)
                        .CloseOnPointerPressedOutside()
                        .CloseInstantlyWhenWindowDeactivated();

                    popup.OnOpening()
                         .Animate(popup.Root, SukiMotion.ScaleYProperty).From(0.72).To(1.0).WithSpring(Spring.Snappy)
                         .And()
                         .Animate(popup.Root, Visual.OpacityProperty).From(0.0).To(1.0).Over(TimeSpan.FromMilliseconds(150))
                         .And()
                         .Cascade(popup.Items, item => item
                             .Animate(Visual.OpacityProperty).From(0.0).To(1.0).Over(TimeSpan.FromMilliseconds(200)))
                         .CompleteIfInterrupted();

                    popup.OnClosing()
                         .Animate(popup.Root, SukiMotion.ScaleYProperty).To(0.9).WithSpring(Spring.Snappy)
                         .And()
                         .Animate(popup.Root, Visual.OpacityProperty).To(0.0).Over(TimeSpan.FromMilliseconds(120));
                }
                """);
        }

        // ---- 16. rules --------------------------------------------------------------------------

        private void Rules()
        {
            Card("The rules of the engine",
                "Everything the engine decides on its own fits in this list.",
                Note(
                    "1. One motion per property: the latest wins, starting from the pose on screen — a spring also carries the live velocity.\n" +
                    "2. MustFinish(): that step always plays to the end; another reaction arriving meanwhile waits (only the latest is kept). The same reaction triggered again restarts.\n" +
                    "3. RetargetIfMoving(): if a spring is moving (or waiting), only its target changes — pose and velocity are kept.\n" +
                    "4. An interrupted motion stops as a whole, and its PlayAsync returns false.\n" +
                    "5. From(...) is written only when the property is at rest; in flight, the motion continues from where it is.\n" +
                    "6. Set(...) is immediate and always wins, even over a MustFinish step.\n" +
                    "7. A motion on a control that is in no window completes at once (final values written, PlayAsync returns true). For an element just added and not shown yet, UsingClockOf(shownElement) borrows the frames of a visual that is.\n" +
                    "8. Nothing that is at rest costs anything: no timer, no frame callback, no effect left in the slot."),
                """
                // The whole API in one place:
                Motion.Of(control)                      // a one-off motion → PlayAsync()
                Motion.Configure(control, motion => …)  // an inline behavior → IDisposable
                Motion.Attach(control, behavior)        // a reusable behavior from C# → IDisposable
                Motion.Stop(control)                    // interrupts what runs on it
                Motion.Simulate(control, routedEvent)   // fires a behavior's reaction (tests, benchmarks)
                MotionBehavior<TControl[, TProfile]>    // a reusable behavior, RegisterEnable for XAML
                """);
        }

        // ---- building blocks ----------------------------------------------------------------------

        private void Section(string title) => Lessons.Children.Add(new TextBlock
        {
            Text = title,
            Classes = { "h3" },
            Margin = new Thickness(0, 30, 0, 0),
        });

        private void Card(string title, string text, Control demo, string code, string? xaml = null)
        {
            var demoPanel = new Border
            {
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                ClipToBounds = true,
                Child = demo,
            };
            Brush(demoPanel, Border.BorderBrushProperty, "SukiControlBorderBrush");

            var content = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock { Text = title, FontSize = 15, FontWeight = FontWeight.DemiBold },
                    Note(text),
                    demoPanel,
                    CodeBlock(code, "csharp"),
                },
            };
            if (xaml is not null)
                content.Children.Add(CodeBlock(xaml, "xml"));

            Lessons.Children.Add(new GlassCard { Margin = new Thickness(0, 0, 10, 0), Content = content });
        }

        /// <summary>A read-only, syntax-highlighted snippet (the demo's TextMate editor), sized to
        /// its text: no inner vertical scrolling, the mouse wheel keeps scrolling the page.</summary>
        private static Control CodeBlock(string code, string language)
        {
            const double padding = 12;
            var editor = new CodeEditor
            {
                Language = language,
                Text = code,
                IsReadOnly = true,
                ShowLineNumbers = false,
                FontSize = 12.5,
                Padding = new Thickness(16, padding),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            };

            int lines = code.Split('\n').Length;
            void Fit()
            {
                double line = editor.TextArea.TextView.DefaultLineHeight;
                if (double.IsNaN(line) || line <= 0)
                    line = editor.FontSize * 1.35;
                editor.Height = lines * line + 2 * padding;
            }
            Fit();
            editor.Loaded += (_, _) => Fit();

            return new Border
            {
                CornerRadius = new CornerRadius(12),
                ClipToBounds = true,
                Child = editor,
            };
        }

        private static TextBlock Note(string text)
        {
            var note = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, FontSize = 13, LineHeight = 20 };
            Brush(note, TextBlock.ForegroundProperty, "SukiLowText");
            return note;
        }

        private static Border Puck()
        {
            var puck = new Border
            {
                Width = PuckSize,
                Height = PuckSize,
                CornerRadius = new CornerRadius(PuckSize / 2),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Brush(puck, Border.BackgroundProperty, "SukiPrimaryColor");
            return puck;
        }

        private static Border Tile(string text, double width, double height)
        {
            var tile = new Border
            {
                Width = width,
                Height = height,
                CornerRadius = new CornerRadius(10),
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.White,
                    FontWeight = FontWeight.DemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };
            Brush(tile, Border.BackgroundProperty, "SukiPrimaryColor");
            return tile;
        }

        private static Border Lane(Control puck, double height = 56)
        {
            var lane = new Border
            {
                Width = LaneWidth,
                Height = height,
                CornerRadius = new CornerRadius(height / 2),
                BorderThickness = new Thickness(1),
                Background = Brushes.Transparent,
                Padding = new Thickness(8, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new Panel { Children = { puck } },
            };
            Brush(lane, Border.BorderBrushProperty, "SukiControlBorderBrush");
            return lane;
        }

        private static Control Center(Control content, double height = 70) => new Panel
        {
            Height = height,
            Children =
            {
                content.Also(c =>
                {
                    c.HorizontalAlignment = HorizontalAlignment.Center;
                    c.VerticalAlignment = VerticalAlignment.Center;
                }),
            },
        };

        private static StackPanel Column(params Control[] children)
        {
            var column = new StackPanel { Spacing = 12 };
            column.Children.AddRange(children);
            return column;
        }

        private static Grid Row(params Control[] children)
        {
            var row = new Grid();
            for (int i = 0; i < children.Length; i++)
            {
                row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                Grid.SetColumn(children[i], i);
                row.Children.Add(children[i]);
            }
            return row;
        }

        private static StackPanel Labeled(string label, params Control[] controls)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
            var text = new TextBlock { Text = label, Width = 200, FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
            Brush(text, TextBlock.ForegroundProperty, "SukiLowText");
            row.Children.Add(text);
            row.Children.AddRange(controls);
            return row;
        }

        private static WrapPanel Buttons(params (string Text, Action OnClick)[] buttons)
        {
            var panel = new WrapPanel();
            foreach (var (text, onClick) in buttons)
            {
                var button = new Button { Content = text, Margin = new Thickness(0, 0, 8, 8) };
                button.Click += (_, _) => onClick();
                panel.Children.Add(button);
            }
            return panel;
        }

        /// <summary>Binds a brush property to a theme resource (brush or color) so it follows the theme.</summary>
        private static void Brush(Control target, AvaloniaProperty property, string key) =>
            target.Bind(property, target.GetResourceObservable(key, r => r is Color c ? new SolidColorBrush(c) : r));
    }

    internal static class ControlExtensions
    {
        public static T Also<T>(this T value, Action<T> configure)
        {
            configure(value);
            return value;
        }
    }
}
