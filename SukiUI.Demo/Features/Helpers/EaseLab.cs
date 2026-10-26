using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using SukiUI.Controls;
using SukiUI.Motion;

namespace SukiUI.Demo.Features.Helpers
{
    // Inside the SukiUI namespace, the simple name "Motion" is the sibling namespace
    // SukiUI.Motion: this alias names the entry class.
    using Motion = SukiUI.Motion.Motion;

    /// <summary>
    /// The SukiEase lab: pick any preset, chain the same modifiers you write in code, see the
    /// curve change, and watch it move something sideways and scale something.
    /// </summary>
    public sealed class EaseLab : UserControl
    {
        private const double PuckSize = 34;
        private const int Milliseconds = 800;
        private static readonly FontFamily Mono = new("Cascadia Code, Consolas, Menlo, monospace");

        private SukiEasePreset _preset = SukiEase.Presets.First(p => p.Path == "Signature.Springy");
        private bool _reverse, _inOut, _wall, _clamp;
        private double _softer, _stronger;                              // amounts: 0 leaves the modifier out
        private bool _right;

        private readonly EaseGraph _graph = new();
        private readonly StackPanel _list = new() { Spacing = 2 };
        private readonly TextBox _search = new() { PlaceholderText = "Search the catalog…" };
        private readonly TextBlock _title = new() { FontSize = 20, FontWeight = FontWeight.SemiBold };
        private readonly TextBox _code = new()
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            FontFamily = Mono,
            FontSize = 12.5,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
        };
        private readonly Dictionary<SukiEasePreset, (Border Row, EaseThumbnail Thumb, TextBlock Name)> _rows = new();
        private readonly List<Action> _syncControls = new();
        private readonly List<Action> _restyle = new();                  // what follows the state of the modifiers
        private readonly WrapPanel _recipe = new();
        private readonly Border _puck = Puck(), _tile = ScaleTile();
        private readonly Border _track = Track();
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(15) };
        private Stopwatch? _clock;

        public EaseLab()
        {
            _graph.Ghost = _preset.Ease;
            _timer.Tick += (_, _) => Tick();

            var page = new StackPanel { Margin = new Thickness(50), Spacing = 20 };
            page.Children.Add(new TextBlock { Classes = { "h1" }, Text = "SukiEase", Margin = new Thickness(0, 20, 0, 0) });
            page.Children.Add(Note("The catalog of known easings. Pick one from the list on the left — the classics, the springs, "
                                   + "or the curves of the design systems — then chain modifiers on it, as you would in code."));

            var grid = new Grid { ColumnSpacing = 20 };
            grid.ColumnDefinitions.Add(new ColumnDefinition(310, GridUnitType.Pixel));
            grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            var browser = Browser();
            var workshop = Workshop();
            Grid.SetColumn(workshop, 1);
            grid.Children.Add(browser);
            grid.Children.Add(workshop);
            page.Children.Add(grid);
            Content = page;

            Rebuild();
            FillList();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            Highlight();
        }

        // ---- the ease, built from the choices on screen ------------------------------------------

        private SukiEase Build()
        {
            var ease = _preset.Ease;
            if (_reverse) ease = ease.Reverse();
            if (_inOut) ease = ease.InOut();
            if (_softer > 0) ease = ease.Softer(_softer);
            if (_stronger > 0) ease = ease.Stronger(_stronger);
            if (_wall) ease = ease.BounceOffTarget();
            if (_clamp) ease = ease.NoOvershoot();
            return ease;
        }

        private void Rebuild()
        {
            var ease = Build();
            _graph.Ease = ease;
            _graph.Ghost = _preset.Ease;
            _graph.Milliseconds = Milliseconds;
            _graph.Progress = null;
            _graph.Refresh();

            _title.Text = _preset.Name;
            FillRecipe();
            foreach (var restyle in _restyle) restyle();
            _code.Text = Code(ease);
        }

        /// <summary>The chain as you would write it: the preset, then each modifier that is on.</summary>
        private void FillRecipe()
        {
            string Amount(double v) => v.ToString("0.#", CultureInfo.InvariantCulture);
            _recipe.Children.Clear();
            _recipe.Children.Add(Chip($"SukiEase.{_preset.Path}", primary: true));
            if (_reverse) _recipe.Children.Add(Chip(".Reverse()"));
            if (_inOut) _recipe.Children.Add(Chip(".InOut()"));
            if (_softer > 0) _recipe.Children.Add(Chip($".Softer({Amount(_softer)})"));
            if (_stronger > 0) _recipe.Children.Add(Chip($".Stronger({Amount(_stronger)})"));
            if (_wall) _recipe.Children.Add(Chip(".BounceOffTarget()"));
            if (_clamp) _recipe.Children.Add(Chip(".NoOvershoot()"));
        }

        private string Code(SukiEase ease)
        {
            var text = new StringBuilder("var ease = SukiEase.").Append(ease).Append(";\n\n");
            text.Append("Motion.Of(card)\n");
            text.Append("    .Animate(SukiMotion.ScaleProperty).To(1.5)\n");
            text.Append("    .Over(TimeSpan.FromMilliseconds(").Append(Milliseconds.ToString(CultureInfo.InvariantCulture)).Append("))\n");
            text.Append("    .WithEasing(ease)\n");
            text.Append("    .PlayAsync();");
            return text.ToString();
        }

        // ---- the preset browser ------------------------------------------------------------------

        private Control Browser()
        {
            _search.TextChanged += (_, _) => FillList();
            var scroll = new ScrollViewer { MaxHeight = 880, Content = _list, Margin = new Thickness(-6, 0) };
            var card = new GlassCard
            {
                VerticalAlignment = VerticalAlignment.Top,
                Content = new StackPanel { Spacing = 12, Children = { _search, scroll } },
            };
            return card;
        }

        private void FillList()
        {
            _list.Children.Clear();
            _rows.Clear();
            string filter = _search.Text?.Trim() ?? "";
            string? group = null;
            foreach (var preset in SukiEase.Presets)
            {
                if (filter.Length > 0 && !preset.Path.Contains(filter, StringComparison.OrdinalIgnoreCase)) continue;
                if (preset.Group != group)
                {
                    group = preset.Group;
                    var header = new TextBlock
                    {
                        Text = group.Replace('.', ' ').ToUpperInvariant().Replace(" ", "  ›  "),
                        FontSize = 10.5,
                        FontWeight = FontWeight.SemiBold,
                        Margin = new Thickness(8, 14, 0, 4),
                    };
                    Brush(header, TextBlock.ForegroundProperty, "SukiLowText");
                    _list.Children.Add(header);
                }
                _list.Children.Add(PresetRow(preset));
            }
            Highlight();
        }

        private Control PresetRow(SukiEasePreset preset)
        {
            var thumb = new EaseThumbnail(preset.Ease);
            var name = new TextBlock { Text = preset.Name, VerticalAlignment = VerticalAlignment.Center, FontSize = 13 };
            var row = new Border
            {
                CornerRadius = new CornerRadius(9),
                Padding = new Thickness(8, 5),
                Background = Brushes.Transparent,
                Cursor = new Cursor(StandardCursorType.Hand),
                Child = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Children = { thumb, name } },
            };
            row.PointerEntered += (_, _) => { if (_preset != preset) row.Background = Overlay(0.06); };
            row.PointerExited += (_, _) => { if (_preset != preset) row.Background = Brushes.Transparent; };
            row.PointerPressed += (_, _) => Select(preset);
            _rows[preset] = (row, thumb, name);
            return row;
        }

        private void Highlight()
        {
            foreach (var (preset, (row, thumb, name)) in _rows)
            {
                bool on = preset == _preset;
                row.Background = on ? Overlay(0.16, primary: true) : Brushes.Transparent;
                name.FontWeight = on ? FontWeight.SemiBold : FontWeight.Normal;
                thumb.Highlighted = on;
                thumb.InvalidateVisual();
            }
        }

        private IBrush Overlay(double opacity, bool primary = false) =>
            this.TryFindResource(primary ? "SukiPrimaryColor" : "SukiLowText", ActualThemeVariant, out var value) && value is Color color
                ? new SolidColorBrush(color, opacity)
                : new SolidColorBrush(Colors.Gray, opacity);

        private void Select(SukiEasePreset preset)
        {
            _preset = preset;
            _reverse = _inOut = _wall = _clamp = false;
            _softer = _stronger = 0;
            foreach (var sync in _syncControls) sync();
            Highlight();
            Rebuild();
            Play();
        }

        // ---- the workshop ------------------------------------------------------------------------

        private Control Workshop()
        {
            var column = new StackPanel { Spacing = 20 };

            // the curve, and what it does to a movement and to a scale
            var header = new DockPanel();
            var play = new Button { Content = "▶  Play", Classes = { "Flat" } };
            play.Click += (_, _) => Play();
            DockPanel.SetDock(play, Avalonia.Controls.Dock.Right);
            header.Children.Add(play);
            header.Children.Add(new StackPanel { Spacing = 6, Children = { _title } });

            column.Children.Add(new GlassCard
            {
                Content = new StackPanel { Spacing = 16, Children = { header, _graph, Stage() } },
            });

            // the modifiers
            column.Children.Add(new GlassCard { Content = Modifiers() });

            // the code
            var copy = new Button { Content = "Copy", HorizontalAlignment = HorizontalAlignment.Right };
            copy.Click += async (_, _) =>
            {
                if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
                    await clipboard.SetTextAsync(_code.Text ?? "");
            };
            column.Children.Add(new GlassCard
            {
                Content = new StackPanel
                {
                    Spacing = 8,
                    Children = { Label("The code that plays it"), _code, copy },
                },
            });
            return column;
        }

        // ---- the modifiers -----------------------------------------------------------------------

        private Control Modifiers()
        {
            var reset = new Button { Content = "Reset", Classes = { "Flat" }, VerticalAlignment = VerticalAlignment.Center };
            reset.Click += (_, _) => Select(_preset);

            var heading = new DockPanel();
            DockPanel.SetDock(reset, Avalonia.Controls.Dock.Right);
            heading.Children.Add(reset);
            heading.Children.Add(new StackPanel
            {
                Spacing = 2,
                Children =
                {
                    new TextBlock { Text = "Modifiers", FontSize = 16, FontWeight = FontWeight.SemiBold },
                    Note("Each one is a method you chain after the preset."),
                },
            });

            // the recipe, in a well of its own: what is written here is what the code below plays
            var recipe = new Border { CornerRadius = new CornerRadius(12), Padding = new Thickness(12, 10, 12, 4), Child = _recipe };
            recipe.Bind(Border.BackgroundProperty, recipe.GetResourceObservable("SukiLowText", r => r is Color c ? new SolidColorBrush(c, 0.08) : r));

            var tiles = new Grid { ColumnSpacing = 12, RowSpacing = 12 };
            tiles.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            tiles.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            for (int row = 0; row < 3; row++) tiles.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var controls = new[]
            {
                ToggleTile("Reverse()", "An ease-out becomes the matching ease-in, and back.", () => _reverse, v => _reverse = v),
                ToggleTile("InOut()", "The curve, then its mirror image.", () => _inOut, v => _inOut = v),
                AmountTile("Softer(amount)", "Closer to the straight line, keeping its character.", () => _softer, v => _softer = v),
                AmountTile("Stronger(amount)", "Speed raised to a power: the character marked more.", () => _stronger, v => _stronger = v),
                ToggleTile("BounceOffTarget()", "What goes beyond the target is mirrored back inside.", () => _wall, v => _wall = v),
                ToggleTile("NoOvershoot()", "Stays between 0 and 1.", () => _clamp, v => _clamp = v),
            };
            for (int i = 0; i < controls.Length; i++)
            {
                Grid.SetRow(controls[i], i / 2);
                Grid.SetColumn(controls[i], i % 2);
                tiles.Children.Add(controls[i]);
            }

            return new StackPanel { Spacing = 14, Children = { heading, recipe, tiles } };
        }

        /// <summary>A tile with a switch: on, it is outlined in the primary color.</summary>
        private Control ToggleTile(string method, string caption, Func<bool> get, Action<bool> set)
        {
            var toggle = new ToggleSwitch { OnContent = "", OffContent = "", IsChecked = get(), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) };
            toggle.IsCheckedChanged += (_, _) =>
            {
                set(toggle.IsChecked == true);
                Rebuild();
            };
            _syncControls.Add(() => toggle.IsChecked = get());
            return Tile(method, caption, toggle, null, get);
        }

        /// <summary>A tile with a slider from 0 (the modifier is left out) to 1: it shows its value and is outlined when it is on.</summary>
        private Control AmountTile(string method, string caption, Func<double> get, Action<double> set)
        {
            var slider = new Slider { Minimum = 0, Maximum = 1, TickFrequency = 0.1, IsSnapToTickEnabled = true, Value = get() };
            var value = new TextBlock { VerticalAlignment = VerticalAlignment.Center, FontFamily = Mono, FontSize = 12.5, FontWeight = FontWeight.SemiBold };
            Brush(value, TextBlock.ForegroundProperty, "SukiPrimaryColor");
            void Show() => value.Text = get() <= 0 ? "off" : get().ToString("0.#", CultureInfo.InvariantCulture);
            Show();

            bool syncing = false;
            slider.PropertyChanged += (_, e) =>
            {
                if (e.Property != RangeBase.ValueProperty || syncing) return;
                set(slider.Value);
                Show();
                Rebuild();
            };
            _syncControls.Add(() =>
            {
                syncing = true;
                slider.Value = get();
                Show();
                syncing = false;
            });
            return Tile(method, caption, value, slider, () => get() > 0);
        }

        private Control Tile(string method, string caption, Control right, Control? body, Func<bool> on)
        {
            var title = new TextBlock { Text = method, FontFamily = Mono, FontSize = 13, FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center };
            var head = new DockPanel();
            DockPanel.SetDock(right, Avalonia.Controls.Dock.Right);
            head.Children.Add(right);
            head.Children.Add(title);

            var text = new TextBlock { Text = caption, FontSize = 11.5, TextWrapping = TextWrapping.Wrap };
            Brush(text, TextBlock.ForegroundProperty, "SukiLowText");
            var content = new StackPanel { Spacing = 4, Children = { head, text } };
            if (body is not null) content.Children.Add(body);

            var frame = new Border { CornerRadius = new CornerRadius(12), BorderThickness = new Thickness(1) };
            Brush(frame, Border.BorderBrushProperty, "SukiControlBorderBrush");
            var outline = new Border { CornerRadius = new CornerRadius(12), BorderThickness = new Thickness(1.5), IsVisible = false, IsHitTestVisible = false };
            Brush(outline, Border.BorderBrushProperty, "SukiPrimaryColor");
            outline.Bind(Border.BackgroundProperty, outline.GetResourceObservable("SukiPrimaryColor", r => r is Color c ? new SolidColorBrush(c, 0.09) : r));
            _restyle.Add(() => outline.IsVisible = on());

            return new Panel { Children = { frame, outline, new Border { Padding = new Thickness(14, 10, 14, 12), Child = content } } };
        }

        // ---- the stage: a movement, and a scale ------------------------------------------------------

        private Control Stage()
        {
            _track.Child = new Panel { Height = PuckSize, Children = { _puck } };
            _track.VerticalAlignment = VerticalAlignment.Center;
            var moves = Frame("Moves", "Motion · TranslateXProperty", new Panel { Height = TileHeight * 1.5, Children = { _track } });

            _tile.HorizontalAlignment = HorizontalAlignment.Center;
            _tile.VerticalAlignment = VerticalAlignment.Center;
            var scales = Frame("Scales", "Motion · ScaleProperty", new Panel { Height = TileHeight * 1.5, Children = { _tile } });

            var stage = new Grid { ColumnSpacing = 14 };
            stage.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            stage.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            Grid.SetColumn(scales, 1);
            stage.Children.Add(moves);
            stage.Children.Add(scales);
            return stage;
        }

        private static Border Frame(string title, string caption, Control content)
        {
            var name = new TextBlock { Text = title, FontWeight = FontWeight.SemiBold };
            var note = new TextBlock { Text = caption, FontSize = 10.5 };
            Brush(note, TextBlock.ForegroundProperty, "SukiLowText");
            var frame = new Border
            {
                CornerRadius = new CornerRadius(12),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14, 10),
                Child = new StackPanel { Spacing = 8, Children = { new StackPanel { Children = { name, note } }, content } },
            };
            Brush(frame, Border.BorderBrushProperty, "SukiControlBorderBrush");
            return frame;
        }

        // ---- playing -----------------------------------------------------------------------------

        private void Play()
        {
            var ease = Build();
            var duration = TimeSpan.FromMilliseconds(Milliseconds);
            _right = !_right;

            double travel = Math.Max(0, _track.Bounds.Width - _track.Padding.Left - _track.Padding.Right - PuckSize);
            _ = Motion.Of(_puck)
                .Animate(SukiMotion.TranslateXProperty).To(_right ? travel : 0.0)
                .Over(duration).WithEasing(ease)
                .PlayAsync();

            _ = Motion.Of(_tile)
                .Animate(SukiMotion.ScaleProperty).To(_right ? 1.5 : 1.0)
                .Over(duration).WithEasing(ease)
                .PlayAsync();

            _clock = Stopwatch.StartNew();
            _graph.Progress = 0;
            _timer.Start();
        }

        private void Tick()
        {
            double t = _clock is null ? 1 : _clock.Elapsed.TotalMilliseconds / Milliseconds;
            _graph.Progress = Math.Min(1.0, t);
            _graph.Refresh();
            if (t >= 1.0) _timer.Stop();
        }

        // ---- small pieces ------------------------------------------------------------------------

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

        private const double TileWidth = 90, TileHeight = 56;

        private static Border ScaleTile()
        {
            var tile = new Border
            {
                Width = TileWidth,
                Height = TileHeight,
                CornerRadius = new CornerRadius(12),
                Child = new TextBlock
                {
                    Text = "Ease",
                    Foreground = Brushes.White,
                    FontWeight = FontWeight.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };
            Brush(tile, Border.BackgroundProperty, "SukiPrimaryColor");
            return tile;
        }

        private static Border Track()
        {
            var track = new Border
            {
                Height = PuckSize + 12,
                CornerRadius = new CornerRadius((PuckSize + 12) / 2),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(6),
            };
            Brush(track, Border.BorderBrushProperty, "SukiControlBorderBrush");
            return track;
        }

        /// <summary>One link of the chain, in code type: the preset in the primary color, then the modifiers.</summary>
        private static Border Chip(string text, bool primary = false)
        {
            var label = new TextBlock { Text = text, FontFamily = Mono, FontSize = 12.5, FontWeight = primary ? FontWeight.SemiBold : FontWeight.Normal };
            var chip = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(9, 3), Margin = new Thickness(0, 0, 6, 6), Child = label };
            if (primary)
            {
                Brush(label, TextBlock.ForegroundProperty, "SukiPrimaryColor");
                chip.Bind(Border.BackgroundProperty, chip.GetResourceObservable("SukiPrimaryColor", r => r is Color c ? new SolidColorBrush(c, 0.14) : r));
            }
            else
            {
                chip.BorderThickness = new Thickness(1);
                Brush(chip, Border.BorderBrushProperty, "SukiControlBorderBrush");
            }
            return chip;
        }

        private static TextBlock Label(string text)
        {
            var label = new TextBlock { Text = text, FontSize = 12.5, FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center };
            Brush(label, TextBlock.ForegroundProperty, "SukiLowText");
            return label;
        }

        private static TextBlock Note(string text)
        {
            var note = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
            Brush(note, TextBlock.ForegroundProperty, "SukiLowText");
            return note;
        }

        /// <summary>Binds a brush property to a theme resource (brush or color) so it follows the theme.</summary>
        private static void Brush(Control target, AvaloniaProperty property, string key) =>
            target.Bind(property, target.GetResourceObservable(key, r => r is Color c ? new SolidColorBrush(c) : r));
    }
}
