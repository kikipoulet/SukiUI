using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Indentation.CSharp;
using SukiUI.Controls;
using TextMateSharp.Grammars;
using SukiUI.Demo.Utilities;

namespace SukiUI.Demo.Features.Playground
{
    public partial class PlaygroundView : UserControl
    {
        private CompletionWindow? _completionWindow;

        private TextEditor _textEditor;

        private GlassCard _glassPlayground;

        public PlaygroundView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _textEditor = this.FindControl<TextEditor>("Editor");
            _glassPlayground = this.FindControl<GlassCard>("GlassExample");
            _textEditor.TextArea.TextEntered += TextEditor_TextArea_TextEntered;
            _textEditor.TextArea.TextEntering += TextEditor_TextArea_TextEntering;
            _textEditor.Text = XamlData.PlaygroundStartingCode;
            _textEditor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(_textEditor.Options);
            _textEditor.TextArea.RightClickMovesCaret = true;

            OnBaseThemeChanged(Application.Current.ActualThemeVariant);
            SukiTheme.GetInstance().OnBaseThemeChanged += OnBaseThemeChanged;
        }

        private void OnBaseThemeChanged(ThemeVariant currentTheme)
        {
            var registryOptions = new RegistryOptions(
                currentTheme == ThemeVariant.Dark ? ThemeName.DarkPlus : ThemeName.LightPlus);

            var textMateInstallation = _textEditor.InstallTextMate(registryOptions);
            textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions
                .GetLanguageByExtension(".xaml").Id));
        }

        private void TextEditor_TextArea_TextEntering(object sender, TextInputEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]) && _completionWindow.CompletionList.SelectedItem != null)
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);

                    // if space (char 32 ' ') is used for confirmation
                    if (char.IsWhiteSpace(e.Text[0]))
                        e.Handled = true;
                }
            }

            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        private void TextEditor_TextArea_TextEntered(object sender, TextInputEventArgs e)
        {
            if (e.Text is not ("<" or "/")) return;

            _completionWindow = new CompletionWindow(_textEditor.TextArea);
            _completionWindow.Closed += (_, _) => _completionWindow = null;

            var data = _completionWindow.CompletionList.CompletionData;
            data.Add(new MyCompletionData("suki:GlassCard"));
            data.Add(new MyCompletionData("Grid"));
            data.Add(new MyCompletionData("Button"));
            data.Add(new MyCompletionData("TextBlock"));

            _completionWindow.Show();
        }

        private void Editor_OnTextChanged(object? sender, EventArgs e)
        {
            string previewCode = XamlData.InsertIntoGridControl(_textEditor.Text);

            try
            {
                Control demoContent = AvaloniaRuntimeXamlLoader.Parse<Grid>(previewCode);
                _glassPlayground.Content = demoContent;
            }
            catch
            {
                // here were are ignoring XmlException, InvalidCastException, XamlLoadException
            }
        }

        private void OpenPane(object? sender, RoutedEventArgs e)
        {
            this.Get<SplitView>("Playground").IsPaneOpen = true;

            ((Button)sender).IsHitTestVisible = false;
            ((Button)sender).Animate<double>(OpacityProperty,1,0);

            this.Get<DockPanel>("TabControls").Animate<double>(OpacityProperty,0,1);
            this.Get<DockPanel>("TabControls").IsHitTestVisible = true;
        }

        private void ClosePane(object? sender, RoutedEventArgs e)
        {
            this.Get<SplitView>("Playground").IsPaneOpen = false;

            this.Get<Button>("OpenPaneButton").IsHitTestVisible = true;
            this.Get<Button>("OpenPaneButton").Animate<double>(OpacityProperty,0,1);

            this.Get<DockPanel>("TabControls").Animate<double>(OpacityProperty,1,0);
            this.Get<DockPanel>("TabControls").IsHitTestVisible = false;
        }

        private void AddNewControls(object? sender, PointerPressedEventArgs e)
        {
            _textEditor.Text = _textEditor.Text.Insert(_textEditor.CaretOffset, ((GlassCard)sender).Tag.ToString());
        }
    }

    internal sealed class MyCompletionData(string text) : ICompletionData
    {
        public IImage? Image => null;

        public string Text { get; } = text;

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content => Text;

        public object Description => "Avalonia Control";

        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
    }
}