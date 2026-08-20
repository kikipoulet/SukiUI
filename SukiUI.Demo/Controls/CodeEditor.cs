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
    public static readonly StyledProperty<string?> LanguageProperty =
        AvaloniaProperty.Register<CodeEditor, string?>(nameof(Language));

    protected override Type StyleKeyOverride => typeof(TextEditor);

    private RegistryOptions? _options;
    private TextMate.Installation? _installation;
    private static readonly Lazy<RegistryOptions> DarkRegistryOptions = new(() => new RegistryOptions(ThemeName.DarkPlus));
    private static readonly Lazy<RegistryOptions> LightRegistryOptions = new(() => new RegistryOptions(ThemeName.LightPlus));

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

        ActualThemeVariantChanged += (_, _) => UpdateGrammar();
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
        if (_installation is not null)
            return;

        var theme = ActualThemeVariant == ThemeVariant.Light
            ? ThemeName.LightPlus
            : ThemeName.DarkPlus;
        _options = GetRegistryOptions(theme);

        _installation ??= this.InstallTextMate(_options);
    }

    private static RegistryOptions GetRegistryOptions(ThemeName theme) => theme switch
    {
        ThemeName.LightPlus => LightRegistryOptions.Value,
        _ => DarkRegistryOptions.Value
    };

    private void UpdateGrammar()
    {
        if (_installation == null || _options == null)
            return;

        if (string.IsNullOrWhiteSpace(Language))
            return;

        _installation.SetGrammar(_options.GetScopeByLanguageId(Language));
    }
}
