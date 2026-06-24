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
        var theme = ActualThemeVariant == ThemeVariant.Light
            ? ThemeName.LightPlus
            : ThemeName.DarkPlus;

        _options = new RegistryOptions(theme);

        _installation ??= this.InstallTextMate(_options);
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