using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using System;
using TextMateSharp.Grammars;

namespace SukiUI.Demo.Controls;

public class CodeEditor : TextEditor
{
    private static readonly Lazy<RegistryOptions> s_lightOptions =
        new(() => new RegistryOptions(ThemeName.LightPlus));

    private static readonly Lazy<RegistryOptions> s_darkOptions =
        new(() => new RegistryOptions(ThemeName.DarkPlus));

    public static readonly StyledProperty<string?> LanguageProperty =
        AvaloniaProperty.Register<CodeEditor, string?>(nameof(Language));

    protected override Type StyleKeyOverride => typeof(TextEditor);

    private RegistryOptions? _options;
    private TextMate.Installation? _installation;

    public string? Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
    }

    public CodeEditor()
    {
        ShowLineNumbers = true;
        FontFamily = FontFamily.Parse("Consolas");
        FlowDirection = FlowDirection.LeftToRight;

        InitializeTextMate();

        ActualThemeVariantChanged += (_, _) =>
        {
            InitializeTextMate();
            UpdateGrammar();
        };
    }

    /// <summary>
    /// Preloads the immutable TextMate grammar registries without blocking the UI thread.
    /// </summary>
    internal static void WarmupRegistryOptions()
    {
        _ = s_lightOptions.Value;
        _ = s_darkOptions.Value;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == LanguageProperty)
        {
            UpdateGrammar();
        }
    }

    private void InitializeTextMate()
    {
        var options = ActualThemeVariant == ThemeVariant.Light
            ? s_lightOptions.Value
            : s_darkOptions.Value;

        if (ReferenceEquals(_options, options))
            return;

        _options = options;
        if (_installation is null)
        {
            _installation = this.InstallTextMate(options);
            return;
        }

        _installation.SetTheme(options.GetDefaultTheme());
    }

    private void UpdateGrammar()
    {
        if (_installation == null || _options == null)
            return;

        if (string.IsNullOrWhiteSpace(Language))
            return;

        _installation.SetGrammar(_options.GetScopeByLanguageId(Language));
    }
}
