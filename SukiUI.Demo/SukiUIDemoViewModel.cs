using Avalonia;
using Avalonia.Collections;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.ControlsAnimation;
using SukiUI.Demo.Features;
using SukiUI.Demo.Features.CustomTheme;
using SukiUI.Demo.Features.Theming;
using SukiUI.Demo.Services;
using SukiUI.Demo.Utilities;
using SukiUI.Dialogs;
using SukiUI.Enums;
using SukiUI.Models;
using SukiUI.Theme.Shadcn;
using SukiUI.Toasts;

namespace SukiUI.Demo
{
    public partial class SukiUIDemoViewModel : ObservableObject
    {
        private readonly SukiTheme _theme;
        private readonly ThemingViewModel _theming;
        [ObservableProperty] private DemoPageBase? _activePage;
        [ObservableProperty] private bool _animationsEnabled;
        [ObservableProperty] private SukiBackgroundStyle _backgroundStyle = SukiBackgroundStyle.GradientSoft;

        [ObservableProperty] private ThemeVariant _baseTheme;
        [ObservableProperty] private string? _customShaderFile;
        [ObservableProperty] private double _renderScale = 1.0;
        [ObservableProperty] private bool _showBottomBar = true;

        [ObservableProperty] private bool _showTitleBar = true;
        [ObservableProperty] private bool _titleBarVisible = true;
        [ObservableProperty] private bool _transitionsEnabled;
        [ObservableProperty] private double _transitionTime;
        [ObservableProperty] private double _volume = 50;
        [ObservableProperty] private bool _windowLocked;

        public SukiUIDemoViewModel(IEnumerable<DemoPageBase> demoPages, PageNavigationService pageNavigationService,
            ISukiToastManager toastManager, ISukiDialogManager dialogManager)
        {
            ToastManager = toastManager;
            DialogManager = dialogManager;
            DemoPages = new AvaloniaList<DemoPageBase>(demoPages.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
            PageNavigationService = pageNavigationService;
            _theming = (ThemingViewModel)DemoPages.First(x => x is ThemingViewModel);
            _theming.BackgroundStyleChanged += style => BackgroundStyle = style;
            _theming.BackgroundAnimationsChanged += enabled => AnimationsEnabled = enabled;
            _theming.CustomBackgroundStyleChanged += shader => CustomShaderFile = shader;
            _theming.BackgroundTransitionsChanged += enabled => TransitionsEnabled = enabled;

            BackgroundStyles = new AvaloniaList<SukiBackgroundStyle>(Enum.GetValues<SukiBackgroundStyle>());
            AnimationProfiles = new AvaloniaList<AnimationProfileChoice>
            {
                new("Normal", SukiAnimationProfile.Normal) { IsSelected = true },
                new("Lite", SukiAnimationProfile.Lite),
            };
            CompactnessLevels = new AvaloniaList<CompactnessChoice>(
                Enum.GetValues<SukiCompactness>().Select(level => new CompactnessChoice(level) { IsSelected = level == SukiCompactness.Normal }));
            _theme = SukiTheme.GetInstance();

            // Subscribe to the navigation service (when a page navigation is requested)
            pageNavigationService.NavigationRequested += pageType =>
            {
                var page = DemoPages.FirstOrDefault(x => x.GetType() == pageType);
                if (page is null || ActivePage?.GetType() == pageType) return;
                ActivePage = page;
            };

            Themes = _theme.ColorThemes;
            BaseTheme = _theme.ActiveBaseTheme;

            // Subscribe to the base theme changed events
            _theme.OnBaseThemeChanged += variant =>
            {
                BaseTheme = variant;
                ToastManager.CreateSimpleInfoToast()
                    .WithTitle("Theme Changed")
                    .WithContent($"Theme has changed to {variant}.")
                    .Queue();
            };

            // Subscribe to the color theme changed events
            _theme.OnColorThemeChanged += theme => ToastManager.CreateSimpleInfoToast()
                .WithTitle("Color Changed")
                .WithContent($"Color has changed to {theme.DisplayName}.")
                .Queue();
        }

        public IAvaloniaReadOnlyList<DemoPageBase> DemoPages { get; }
        public PageNavigationService PageNavigationService { get; }

        public IAvaloniaReadOnlyList<SukiColorTheme> Themes { get; }

        public IAvaloniaReadOnlyList<SukiBackgroundStyle> BackgroundStyles { get; }

        public IAvaloniaReadOnlyList<AnimationProfileChoice> AnimationProfiles { get; }

        public IAvaloniaReadOnlyList<CompactnessChoice> CompactnessLevels { get; }

        public ISukiToastManager ToastManager { get; }
        public ISukiDialogManager DialogManager { get; }

        [RelayCommand]
        private void ToggleAnimations()
        {
            AnimationsEnabled = !AnimationsEnabled;
            ToastManager.CreateSimpleInfoToast()
                .WithTitle(AnimationsEnabled ? "Animation Enabled" : "Animation Disabled")
                .WithContent(AnimationsEnabled
                    ? "Background animations are now enabled."
                    : "Background animations are now disabled.")
                .Queue();
        }

        [RelayCommand]
        private void ToggleTransitions()
        {
            TransitionsEnabled = !TransitionsEnabled;
            ToastManager.CreateSimpleInfoToast()
                .WithTitle(TransitionsEnabled ? "Transitions Enabled" : "Transitions Disabled")
                .WithContent(TransitionsEnabled
                    ? "Background transitions are now enabled."
                    : "Background transitions are now disabled.")
                .Queue();
        }

        [RelayCommand]
        private void ToggleBaseTheme()
        {
            _theme.SwitchBaseTheme();
        }

        public void ChangeTheme(SukiColorTheme theme)
        {
            _theme.ChangeColorTheme(theme);
        }

        public void UseCompactness(CompactnessChoice choice)
        {
            _theme.ChangeCompactness(choice.Level);
            foreach (var level in CompactnessLevels)
                level.IsSelected = ReferenceEquals(level, choice);

            ToastManager.CreateSimpleInfoToast()
                .WithTitle("Compactness")
                .WithContent($"Compactness set to {choice.DisplayName}.")
                .Queue();
        }

        public void UseAnimationProfile(AnimationProfileChoice choice)
        {
            SukiAnimationTheme.Use(choice.Profile);
            foreach (var profile in AnimationProfiles)
                profile.IsSelected = ReferenceEquals(profile, choice);

            ToastManager.CreateSimpleInfoToast()
                .WithTitle("Animation Mode")
                .WithContent($"Animation profile set to {choice.DisplayName}.")
                .Queue();
        }

        [RelayCommand]
        private void ShadCnMode()
        {
            Shadcn.Configure(Application.Current, Application.Current.ActualThemeVariant);
        }

        [RelayCommand]
        private void CreateCustomTheme()
        {
            DialogManager.CreateDialog()
                .WithViewModel(dialog => new CustomThemeDialogViewModel(_theme, dialog))
                .TryShow();
        }

        [RelayCommand]
        private void ToggleWindowLock()
        {
            WindowLocked = !WindowLocked;
            ToastManager.CreateSimpleInfoToast()
                .WithTitle($"Window {(WindowLocked ? "Locked" : "Unlocked")}")
                .WithContent($"Window has been {(WindowLocked ? "locked" : "unlocked")}.")
                .Queue();
        }

        [RelayCommand]
        private void ToggleTitleBackground()
        {
            ShowTitleBar = !ShowTitleBar;
            ShowBottomBar = !ShowBottomBar;
        }

        [RelayCommand]
        private void ToggleTitleBar()
        {
            TitleBarVisible = !TitleBarVisible;
            ToastManager.CreateSimpleInfoToast()
                .WithTitle($"Title Bar {(TitleBarVisible ? "Visible" : "Hidden")}")
                .WithContent($"Window title bar has been {(TitleBarVisible ? "shown" : "hidden")}.")
                .Queue();
        }

        [RelayCommand]
        private void ToggleRightToLeft()
        {
            _theme.IsRightToLeft = !_theme.IsRightToLeft;
        }

        [RelayCommand]
        private void DecreaseRenderScale()
        {
            RenderScale -= 0.05;
        }

        [RelayCommand]
        private void IncreaseRenderScale()
        {
            RenderScale += 0.05;
        }

        [RelayCommand]
        private void DecreaseVolume()
        {
            Volume -= 5;
        }

        [RelayCommand]
        private void IncreaseVolume()
        {
            Volume += 5;
        }

        [RelayCommand]
        private static void OpenUrl(string url)
        {
            UrlUtilities.OpenUrl(url);
        }

        partial void OnBackgroundStyleChanged(SukiBackgroundStyle value)
        {
            _theming.BackgroundStyle = value;
        }

        partial void OnAnimationsEnabledChanged(bool value)
        {
            _theming.BackgroundAnimations = value;
        }

        partial void OnTransitionsEnabledChanged(bool value)
        {
            _theming.BackgroundTransitions = value;
        }
    }

    /// <summary>A selectable <see cref="SukiCompactness"/> level for the demo's "Compactness" menu.</summary>
    public sealed partial class CompactnessChoice(SukiCompactness level) : ObservableObject
    {
        public string DisplayName { get; } = level.ToString();
        public SukiCompactness Level { get; } = level;

        [ObservableProperty] private bool _isSelected;
    }

    /// <summary>A selectable global animation profile for the demo's "Animation Mode" menu:
    /// a display name over the <see cref="SukiAnimationProfile"/> to hand to
    /// <see cref="SukiAnimationTheme.Use"/>.</summary>
    public sealed partial class AnimationProfileChoice(string displayName, SukiAnimationProfile profile) : ObservableObject
    {
        public string DisplayName { get; } = displayName;
        public SukiAnimationProfile Profile { get; } = profile;

        [ObservableProperty] private bool _isSelected;
    }
}