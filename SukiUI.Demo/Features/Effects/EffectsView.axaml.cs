using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using SukiUI.Controls;
using SukiUI.Demo.Controls;
using SukiUI.Utilities.Effects;
using System;

namespace SukiUI.Demo.Features.Effects
{
    public partial class EffectsView : UserControl
    {
        private CodeEditor? _textEditor;
        private ShaderToyRenderer? _toyRenderer;
        private InfoBar? _errorText;

        public EffectsView()
        {
            InitializeComponent();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _textEditor = this.FindControl<CodeEditor>("Editor")!;

            _toyRenderer = this.FindControl<ShaderToyRenderer>("ShaderToyRenderer")!;

            _errorText = this.FindControl<InfoBar>("ErrorText")!;

            var effect = SukiEffect.FromEmbeddedResource("shaderart");

            _textEditor.Text = effect.ToString();
            _toyRenderer.SetEffect(effect);
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            if (_errorText == null || _textEditor == null || _toyRenderer == null)
            {
                return;
            }

            try
            {
                _errorText.Message = string.Empty;
                var effect = SukiEffect.FromString(_textEditor.Text);
                _toyRenderer.SetEffect(effect);
                _errorText.IsVisible = false;
            }
            catch (Exception ex)
            {
                _errorText.Message = ex.Message;
                _errorText.IsVisible = true;
            }
        }
    }
}