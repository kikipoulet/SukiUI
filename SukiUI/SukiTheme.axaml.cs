using System.Globalization;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using SukiUI.CompactnessTheme;
using SukiUI.Enums;
using SukiUI.Extensions;
using SukiUI.Locale;
using SukiUI.Models;

namespace SukiUI
{
    public partial class SukiTheme : Styles
    {
        public static readonly StyledProperty<string?> LocaleProperty =
            AvaloniaProperty.Register<SukiTheme, string?>(nameof(Locale));

        public static readonly StyledProperty<SukiCompactness> CompactnessProperty =
            AvaloniaProperty.Register<SukiTheme, SukiCompactness>(nameof(Compactness), SukiCompactness.Normal);

        public static readonly StyledProperty<SukiColor> ThemeColorProperty =
            AvaloniaProperty.Register<SukiTheme, SukiColor>(nameof(ThemeColor), defaultBindingMode: BindingMode.OneTime,
                defaultValue: SukiColor.Blue);

        public static readonly StyledProperty<bool> IsRightToLeftProperty =
            AvaloniaProperty.Register<SukiTheme, bool>(nameof(IsRightToLeft), defaultBindingMode: BindingMode.OneTime,
                defaultValue: false);

        // Static Members...

        /// <summary>
        /// The default Color Themes included with SukiUI.
        /// </summary>
        public static readonly IReadOnlyDictionary<SukiColor, SukiColorTheme> DefaultColorThemes;

        // Localization

        private static readonly Dictionary<string, ResourceDictionary> LocaleToResource = new()
        {
            { "en-US", new en_US() },
            { "nl-NL", new nl_NL() },
            { "pt-PT", new pt_PT() },
            { "zh-CN", new zh_CN() },
            { "de-DE", new de_DE() },
            { "es-ES", new es_ES() },
            { "fr-FR", new fr_FR() },
            { "it-IT", new it_IT() },
            { "ru-RU", new ru_RU() },
            { "ja-JP", new ja_JP() }
        };

        private static readonly ResourceDictionary DefaultResource = new en_US();
        private readonly AvaloniaList<SukiColorTheme> _allThemes = [];

        private readonly Application _app;

        private readonly HashSet<SukiColorTheme> _colorThemeHashset = [];

        private bool _isApplyingColorTheme;
        private int _colorThemeVersion;

        static SukiTheme()
        {
            var defaultThemes = new[]
            {
                new DefaultSukiColorTheme(SukiColor.Orange, Color.Parse("#d48806"), Color.Parse("#176CE8")),
                new DefaultSukiColorTheme(SukiColor.Red, Color.Parse("#D03A2F"), Color.Parse("#2FC5D0")),
                new DefaultSukiColorTheme(SukiColor.Green, Color.Parse("#537834"), Color.Parse("#B24DB0")),
                new DefaultSukiColorTheme(SukiColor.Blue, Color.Parse("#0A59F7"), Color.Parse("#F7A80A"))
            };
            DefaultColorThemes = defaultThemes.ToDictionary(x => x.ThemeColor, y => (SukiColorTheme)y);
        }

        public SukiTheme()
        {
            AvaloniaXamlLoader.Load(this);
            _app = Application.Current!;
            // Covers every base-theme change: ChangeBaseTheme/SwitchBaseTheme, direct RequestedThemeVariant
            // assignments, and OS light/dark switches while following the system theme.
            // Resources are refreshed first so OnBaseThemeChanged subscribers observe the new palette.
            _app.ActualThemeVariantChanged += (_, _) =>
            {
                // While a color theme is being applied, its variant resources are refreshed once it completes.
                if (!_isApplyingColorTheme)
                    RefreshBaseThemeColorResources();
                OnBaseThemeChanged?.Invoke(_app.ActualThemeVariant);
            };
            foreach (var theme in DefaultColorThemes)
                AddColorTheme(theme.Value);

            UpdateFlowDirectionResources(IsRightToLeft);
            SetColorThemeResourcesOnColorThemeChanged();
            ApplyLocale(Locale);
            ApplyCompactness(Compactness);
        }

        /// <summary>
        /// Used to assign the ColorTheme at launch
        /// </summary>
        public SukiColor ThemeColor
        {
            get => GetValue(ThemeColorProperty);
            set => SetValue(ThemeColorProperty, value);
        }

        public bool IsRightToLeft
        {
            get => GetValue(IsRightToLeftProperty);
            set => SetValue(IsRightToLeftProperty, value);
        }

        /// <summary>
        /// Called whenever the application's <see cref="SukiColorTheme"/> is changed.
        /// Useful where controls cannot use "DynamicResource"
        /// </summary>
        public Action<SukiColorTheme>? OnColorThemeChanged { get; set; }

        /// <summary>
        /// Called whenever the application's <see cref="ThemeVariant"/> is changed.
        /// Useful where controls need to change based on light/dark.
        /// </summary>
        public Action<ThemeVariant>? OnBaseThemeChanged { get; set; }

        /// <summary>
        /// Currently active <see cref="SukiColorTheme"/>
        /// If you want to change this please use <see cref="ChangeColorTheme(SukiUI.Models.SukiColorTheme)"/>
        /// </summary>
        public SukiColorTheme? ActiveColorTheme { get; private set; }

        /// <summary>
        /// All available Color Themes.
        /// </summary>
        public IAvaloniaReadOnlyList<SukiColorTheme> ColorThemes => _allThemes;

        /// <summary>
        /// Currently active <see cref="ThemeVariant"/>
        /// If you want to change this please use <see cref="ChangeBaseTheme"/> or <see cref="SwitchBaseTheme"/>
        /// </summary>
        public ThemeVariant ActiveBaseTheme => _app.ActualThemeVariant;

        public string? Locale
        {
            get => GetValue(LocaleProperty);
            set => SetValue(LocaleProperty, value);
        }

        /// <summary>
        /// How tight the UI is (paddings, margins, fonts, fixed sizes). Each control declares its values per level
        /// in a <c>*.Compactness.axaml</c> file next to its template; <see cref="SukiCompactness.Normal"/> is the historical look.
        /// </summary>
        public SukiCompactness Compactness
        {
            get => GetValue(CompactnessProperty);
            set => SetValue(CompactnessProperty, value);
        }

        /// <summary>
        /// Called whenever <see cref="Compactness"/> changes. Useful where controls cannot use "DynamicResource".
        /// </summary>
        public Action<SukiCompactness>? OnCompactnessChanged { get; set; }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == IsRightToLeftProperty)
            {
                UpdateFlowDirectionResources(change.GetNewValue<bool>());
            }
            else if (change.Property == ThemeColorProperty)
            {
                SetColorThemeResourcesOnColorThemeChanged();
            }
            else if (change.Property == LocaleProperty)
            {
                ApplyLocale(change.GetNewValue<string?>());
            }
            else if (change.Property == CompactnessProperty)
            {
                var compactness = change.GetNewValue<SukiCompactness>();
                ApplyCompactness(compactness);
                OnCompactnessChanged?.Invoke(compactness);
            }

            base.OnPropertyChanged(change);
        }

        /// <summary>
        /// Change the theme to one of the default themes.
        /// </summary>
        /// <param name="sukiColor">The <see cref="SukiColor"/> to change to.</param>
        /// <remarks>When called off the UI thread, the change is posted to the UI thread.</remarks>
        public void ChangeColorTheme(SukiColor sukiColor)
        {
            if (PostToUIThreadIfNeeded(() => ChangeColorTheme(sukiColor))) return;
            ThemeColor = sukiColor;
        }

        /// <summary>
        /// Tries to change the theme to a specific theme, this can be either a default or a custom defined one.
        /// </summary>
        /// <param name="sukiColorTheme"></param>
        /// <remarks>When called off the UI thread, the change is posted to the UI thread.</remarks>
        public void ChangeColorTheme(SukiColorTheme sukiColorTheme)
        {
            if (PostToUIThreadIfNeeded(() => ChangeColorTheme(sukiColorTheme))) return;
            if (sukiColorTheme is DefaultSukiColorTheme defaultTheme && ThemeColor != defaultTheme.ThemeColor)
            {
                ThemeColor = defaultTheme.ThemeColor;
                return;
            }

            SetColorTheme(sukiColorTheme);
        }

        /// <summary>
        /// Blindly switches to the "next" theme available in the <see cref="ColorThemes"/> collection.
        /// </summary>
        /// <remarks>When called off the UI thread, the change is posted to the UI thread.</remarks>
        public void SwitchColorTheme()
        {
            if (PostToUIThreadIfNeeded(SwitchColorTheme)) return;
            var index = -1;
            for (var i = 0; i < ColorThemes.Count; i++)
            {
                if (ColorThemes[i] != ActiveColorTheme) continue;
                index = i;
                break;
            }

            if (index == -1) return;
            var newIndex = (index + 1) % ColorThemes.Count;
            var newColorTheme = ColorThemes[newIndex];
            ChangeColorTheme(newColorTheme);
        }

        /// <summary>
        /// Add a new <see cref="SukiColorTheme"/> to the ones available, without making it active.
        /// </summary>
        /// <param name="sukiColorTheme">New <see cref="SukiColorTheme"/> to add.</param>
        public void AddColorTheme(SukiColorTheme sukiColorTheme)
        {
            if (_colorThemeHashset.Contains(sukiColorTheme))
                throw new InvalidOperationException("This color theme has already been added.");
            _colorThemeHashset.Add(sukiColorTheme);
            _allThemes.Add(sukiColorTheme);
        }

        /// <summary>
        /// Adds multiple new <see cref="SukiColorTheme"/> to the ones available, without making any active.
        /// </summary>
        /// <param name="sukiColorThemes">A collection of new <see cref="SukiColorTheme"/> to add.</param>
        public void AddColorThemes(IEnumerable<SukiColorTheme> sukiColorThemes)
        {
            foreach (var colorTheme in sukiColorThemes)
                AddColorTheme(colorTheme);
        }

        /// <summary>
        /// Tries to change the base theme to the one provided, if it is different.
        /// </summary>
        /// <param name="baseTheme"><see cref="ThemeVariant"/> to change to.</param>
        /// <remarks>When called off the UI thread, the change is posted to the UI thread.</remarks>
        public void ChangeBaseTheme(ThemeVariant baseTheme)
        {
            if (PostToUIThreadIfNeeded(() => ChangeBaseTheme(baseTheme))) return;
            if (_app.ActualThemeVariant == baseTheme) return;
            _app.RequestedThemeVariant = baseTheme;
        }

        /// <summary>
        /// Simply switches from Light -> Dark and visa versa.
        /// </summary>
        /// <remarks>When called off the UI thread, the change is posted to the UI thread.</remarks>
        public void SwitchBaseTheme()
        {
            if (PostToUIThreadIfNeeded(SwitchBaseTheme)) return;
            if (Application.Current is null) return;
            var newBase = Application.Current.ActualThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
            Application.Current.RequestedThemeVariant = newBase;
        }

        private void UpdateFlowDirectionResources(bool rightToLeft)
        {
            var primary = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            var opposite = rightToLeft ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;

            Resources["FlowDirectionPrimary"] = primary;
            Resources["FlowDirectionOpposite"] = opposite;
        }

        /// <summary>
        /// Initializes the color theme resources whenever the property is changed.
        /// In an ideal world people wouldn't use the property
        /// </summary>
        private void SetColorThemeResourcesOnColorThemeChanged()
        {
            if (!DefaultColorThemes.TryGetValue(ThemeColor, out var colorTheme))
                throw new Exception($"{ThemeColor} has no defined color theme.");
            SetColorTheme(colorTheme);
        }

        // Per base color: the color itself, 11 opacity steps and the 2 base-theme variants.
        private const int ResourcesPerColorWithOpacities = 14;

        private void SetColorTheme(SukiColorTheme colorTheme)
        {
            var baseTheme = ActiveBaseTheme;
            var resources = new List<KeyValuePair<object, object?>>(2 * ResourcesPerColorWithOpacities + 2);
            AddColorWithOpacities(resources, "SukiPrimaryColor", colorTheme.Primary);
            resources.Add(new("SukiPrimaryDarkColor", colorTheme.PrimaryDark));
            AddColorWithOpacities(resources, "SukiAccentColor", colorTheme.Accent);
            resources.Add(new("SukiAccentDarkColor", colorTheme.AccentDark));

            var version = ++_colorThemeVersion;
            var wasApplying = _isApplyingColorTheme;
            _isApplyingColorTheme = true;
            try
            {
                SetResources(_app.Resources, resources);
            }
            finally
            {
                _isApplyingColorTheme = wasApplying;
            }

            // A handler reached from the writes above (e.g. Shadcn's OnBaseThemeChanged) applied a newer
            // color theme; it is already active and consistent, so this superseded call must not touch it.
            if (version != _colorThemeVersion) return;

            ActiveColorTheme = colorTheme;

            // A resource setter or change handler may switch the base theme while the palette is written;
            // the variant resources computed above are then stale.
            if (ActiveBaseTheme != baseTheme)
                RefreshBaseThemeColorResources();

            OnColorThemeChanged?.Invoke(ActiveColorTheme);
        }

        /// <summary>
        /// Rewrites only the resources that depend on the base theme; the color theme itself is unchanged,
        /// so <see cref="OnColorThemeChanged"/> is not raised.
        /// </summary>
        private void RefreshBaseThemeColorResources()
        {
            if (ActiveColorTheme is not { } colorTheme)
            {
                SetColorThemeResourcesOnColorThemeChanged();
                return;
            }

            var resources = new List<KeyValuePair<object, object?>>(4);
            AddBaseThemeVariants(resources, "SukiPrimaryColor", colorTheme.Primary);
            AddBaseThemeVariants(resources, "SukiAccentColor", colorTheme.Accent);
            SetResources(_app.Resources, resources);
        }

        private void AddColorWithOpacities(List<KeyValuePair<object, object?>> resources, string baseName, Color baseColor)
        {
            resources.Add(new(baseName, baseColor));
            resources.Add(new($"{baseName}75", baseColor.WithAlpha(0.75)));
            resources.Add(new($"{baseName}50", baseColor.WithAlpha(0.50)));
            resources.Add(new($"{baseName}25", baseColor.WithAlpha(0.25)));
            resources.Add(new($"{baseName}20", baseColor.WithAlpha(0.2)));
            resources.Add(new($"{baseName}15", baseColor.WithAlpha(0.15)));
            resources.Add(new($"{baseName}10", baseColor.WithAlpha(0.10)));
            resources.Add(new($"{baseName}7", baseColor.WithAlpha(0.07)));
            resources.Add(new($"{baseName}5", baseColor.WithAlpha(0.05)));
            resources.Add(new($"{baseName}3", baseColor.WithAlpha(0.03)));
            resources.Add(new($"{baseName}1", baseColor.WithAlpha(0.005)));
            resources.Add(new($"{baseName}0", baseColor.WithAlpha(0.00)));
            AddBaseThemeVariants(resources, baseName, baseColor);
        }

        private void AddBaseThemeVariants(List<KeyValuePair<object, object?>> resources, string baseName, Color baseColor)
        {
            var isDark = ActiveBaseTheme == ThemeVariant.Dark;
            resources.Add(new($"{baseName}120", isDark ? Lighten(baseColor, 0.7) : baseColor));
            resources.Add(new($"{baseName}150", isDark ? Lighten(baseColor, 1) : baseColor));
        }

        /// <summary>
        /// Writes <paramref name="resources"/> into <paramref name="target"/>. A plain <see cref="ResourceDictionary"/>
        /// is updated in a single batch that raises one resource change notification; any other
        /// <see cref="IResourceDictionary"/> is updated key by key through its indexer.
        /// </summary>
        /// <remarks>
        /// Subclasses deliberately take the indexer path: they can re-implement the dictionary indexer to observe
        /// or intercept writes, and <see cref="ResourceDictionary.SetItems"/> would bypass that.
        /// Callers pass fully computed values, so a failure while building them cannot leave a partial update behind.
        /// </remarks>
        private static void SetResources(IResourceDictionary target, IEnumerable<KeyValuePair<object, object?>> resources)
        {
            if (target.GetType() == typeof(ResourceDictionary))
            {
                ((ResourceDictionary)target).SetItems(resources);
                return;
            }

            foreach (var resource in resources)
                target[resource.Key] = resource.Value;
        }

        /// <summary>
        /// Posts <paramref name="action"/> to the UI thread when called from another thread.
        /// </summary>
        /// <returns><see langword="true"/> if the action was posted and the caller should return.</returns>
        private static bool PostToUIThreadIfNeeded(Action action)
        {
            if (Dispatcher.UIThread.CheckAccess()) return false;
            Dispatcher.UIThread.Post(action);
            return true;
        }

        public static Color Lighten(Color color, double amount)
        {
            amount = Clamp(amount, 0.0, 1.0);

            byte lighten(byte component)
            {
                var result = (int)(component + (255 - component) * amount);
                return (byte)Clamp(result, 0, 255);
            }

            return Color.FromArgb(
                color.A,
                lighten(color.R),
                lighten(color.G),
                lighten(color.B)
            );
        }

        public static double Clamp(double value, double min, double max)
        {
            return value < min ? min : value > max ? max : value;
        }

        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }

        /// <summary>
        /// Retrieves an instance tied to a specific instance of an application.
        /// </summary>
        /// <returns>A <see cref="SukiTheme"/> instance that can be used to change themes.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no SukiTheme has been defined in App.axaml.</exception>
        public static SukiTheme GetInstance(Application app)
        {
            var theme = app.Styles.FirstOrDefault(style => style is SukiTheme);
            if (theme is not SukiTheme sukiTheme)
                throw new InvalidOperationException(
                    "No SukiTheme instance available. Ensure SukiTheme has been set in Application.Styles in App.axaml.");
            return sukiTheme;
        }

        /// <summary>
        /// Retrieves an instance tied to the currently active application.
        /// </summary>
        /// <returns>A <see cref="SukiTheme"/> instance that can be used to change themes.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no SukiTheme has been defined in App.axaml.</exception>
        public static SukiTheme GetInstance()
        {
            return GetInstance(Application.Current!);
        }

        /// <summary>
        /// Changes the UI compactness.
        /// </summary>
        /// <remarks>When called off the UI thread, the change is posted to the UI thread.</remarks>
        public void ChangeCompactness(SukiCompactness compactness)
        {
            if (PostToUIThreadIfNeeded(() => ChangeCompactness(compactness))) return;
            Compactness = compactness;
        }

        private void ApplyCompactness(SukiCompactness compactness) =>
            SetResources(Resources, SukiCompactnessResources.Build(compactness));

        private void ApplyLocale(string? locale)
        {
            var resource = TryGetLocaleResource(locale, out var localeResource) && localeResource is not null
                ? localeResource
                : DefaultResource;

            SetResources(Resources, resource);
        }

        private static bool TryGetLocaleResource(string? locale, out ResourceDictionary? resourceDictionary)
        {
            if (LocaleToResource.TryGetValue(locale ?? string.Empty, out var resource))
            {
                resourceDictionary = resource;
                return true;
            }

            resourceDictionary = DefaultResource;
            return false;
        }

        public static void OverrideLocaleResources(Application application, CultureInfo? culture)
        {
            if (culture is null) return;
            if (!LocaleToResource.TryGetValue(culture.Name, out var resources)) return;
            SetResources(application.Resources, resources);
        }

        public static void OverrideLocaleResources(StyledElement element, CultureInfo? culture)
        {
            if (culture is null) return;
            if (!LocaleToResource.TryGetValue(culture.Name, out var resources)) return;
            SetResources(element.Resources, resources);
        }
    }
}
