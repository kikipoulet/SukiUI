using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using SukiUI.Utilities.Effects;
using TextMateSharp.Grammars;

namespace SukiUI.Demo.Features.Effects
{
    public partial class EffectsView : UserControl
    {
        private TextEditor _textEditor;
        private ShaderToyRenderer _toyRenderer;
        private TextBox _errorText;

        public EffectsView()
        {
            InitializeComponent();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            
            _textEditor = this.FindControl<TextEditor>("Editor")!;
            
            _toyRenderer = this.FindControl<ShaderToyRenderer>("ShaderToyRenderer")!;
            
            _errorText = this.FindControl<TextBox>("ErrorText")!;
            
            var effect = SukiEffect.FromEmbeddedResource("shaderart");
            _textEditor.Text = effect.ToString();
            _toyRenderer.SetEffect(effect);
            
            OnBaseThemeChanged(Application.Current!.ActualThemeVariant);
            SukiTheme.GetInstance().OnBaseThemeChanged += OnBaseThemeChanged;
        }
        
        private void OnBaseThemeChanged(ThemeVariant currentTheme)
        {
            var registryOptions = new RegistryOptions(
                currentTheme == ThemeVariant.Dark ? ThemeName.DarkPlus : ThemeName.LightPlus);

            var textMateInstallation = _textEditor.InstallTextMate(registryOptions);
            textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions
                .GetLanguageByExtension(".hlsl").Id));
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            try
            {
                _errorText.Text = string.Empty;
                var effect = SukiEffect.FromString(_textEditor.Text);
                _toyRenderer.SetEffect(effect);
            }
            catch(Exception ex)
            {
                _errorText.Text = ex.Message;
            }
        }
    }
}