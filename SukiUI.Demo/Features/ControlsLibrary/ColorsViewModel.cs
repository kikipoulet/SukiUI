using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using SukiUI.Enums;
using SukiUI.Models;

namespace SukiUI.Demo.Features.ControlsLibrary
{
    public partial class ColorsViewModel : DemoPageBase
    {
        private readonly string[] _keys =
        [
            "SukiText", "SukiLowText", "SukiCardBackground", "SukiLightBorderBrush", "SukiControlBorderBrush",
            "GlassBorderBrush", "SukiDialogBackground", "SukiMediumBorderBrush", "SukiMediumBorderBrush",
            "SukiControlTouchBackground", "SukiBorderBrush", "SukiBorderBrush", "SukiStrongBackground"
        ];

        public AvaloniaList<ColorViewModel> Colors { get; } = new();

        private readonly Dictionary<ThemeVariant, Dictionary<string, IBrush>> _baseThemeCache = new();
        private readonly Dictionary<SukiColorTheme, Dictionary<string, IBrush>> _colorThemeCache = new();

        private readonly SukiTheme _theme;

        public ColorsViewModel() : base("Colors", MaterialIconKind.Paintbrush)
        {
            _theme = SukiTheme.GetInstance();


            Colors.AddRange(BuildOrGetColorTheme(_theme.ActiveColorTheme)
                .Select(x => new ColorViewModel(x.Key, x.Value)));
            Colors.AddRange(BuildOrGetBaseTheme(_theme.ActiveBaseTheme)
                .OrderBy(x => x.Key)
                .Select(x => new ColorViewModel(x.Key, x.Value)));

            _theme.OnBaseThemeChanged += variant => ReApply(_theme.ActiveColorTheme, variant);
            _theme.OnColorThemeChanged += theme => ReApply(theme, _theme.ActiveBaseTheme);
        }

        private void ReApply(SukiColorTheme theme, ThemeVariant variant)
        {
            var themeBrushes = BuildOrGetColorTheme(theme);
            var baseBrushes = BuildOrGetBaseTheme(variant);
            foreach (var vm in Colors)
            {
                if (themeBrushes.TryGetValue(vm.Name, out var themeBrush))
                    vm.Brush = themeBrush;
                else if (baseBrushes.TryGetValue(vm.Name, out var baseBrush))
                    vm.Brush = baseBrush;
            }
        }

        private Dictionary<string, IBrush> BuildOrGetColorTheme(SukiColorTheme theme)
        {
            if (_colorThemeCache.TryGetValue(theme, out var res))
                return res;
            var colors = Application.Current.Resources
                .ToDictionary(x => (string)x.Key, y => (IBrush)new SolidColorBrush((Color)y.Value));
            _colorThemeCache[theme] = colors;
            return _colorThemeCache[theme];
        }

        private Dictionary<string, IBrush> BuildOrGetBaseTheme(ThemeVariant variant)
        {
            if (_baseThemeCache.TryGetValue(variant, out var res))
                return res;
            var newRes = new Dictionary<string, IBrush>();
            foreach (var key in _keys)
            {
                if (Application.Current.TryGetResource(key, variant, out var obj) && obj is Color col)
                    newRes.TryAdd(key, new SolidColorBrush(col));
            }

            _baseThemeCache[variant] = newRes;
            return _baseThemeCache[variant];
        }
    }

    public partial class ColorViewModel : ObservableObject
    {
        [ObservableProperty] private string _name;
        [ObservableProperty] private IBrush _brush;

        public ColorViewModel(string name, IBrush brush)
        {
            _brush = brush;
            _name = name;
        }
    }
}