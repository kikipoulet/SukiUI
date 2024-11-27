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

        ActualThemeVariantChanged += (_, _) => UpdateEditorTheme();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == LanguageProperty)
        {
            UpdateEditorTheme();
        }

        base.OnPropertyChanged(change);
    }

    private void UpdateEditorTheme()
    {
        var languageId = Language;

        if (string.IsNullOrEmpty(languageId))
        {
            return;
        }

        var theme = ActualThemeVariant == ThemeVariant.Light
                    ? ThemeName.LightPlus
                    : ThemeName.DarkPlus;

        var options = new RegistryOptions(theme);

        var installation = this.InstallTextMate(options);

        installation.SetGrammar(options.GetScopeByLanguageId(languageId));
    }
}
