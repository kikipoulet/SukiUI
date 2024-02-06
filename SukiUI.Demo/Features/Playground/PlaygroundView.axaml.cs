using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
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

namespace SukiUI.Demo.Features.Playground
{
    public partial class PlaygroundView : UserControl
    {
        private CompletionWindow _completionWindow;
     
        private  TextEditor _textEditor;
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
            _textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
            _textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            _textEditor.Text = StartingString;
            _textEditor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(_textEditor.Options);

            OnBaseThemeChanged(Application.Current.ActualThemeVariant);
            SukiTheme.GetInstance().OnBaseThemeChanged += OnBaseThemeChanged;
            
        }

        private void OnBaseThemeChanged(ThemeVariant CurrentTheme)
        {
            var  _registryOptions = new RegistryOptions( CurrentTheme == ThemeVariant.Dark ? ThemeName.DarkPlus : ThemeName.LightPlus);

            var _textMateInstallation = _textEditor.InstallTextMate(_registryOptions);
            _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".xaml").Id));

        }


        private void textEditor_TextArea_TextEntering(object sender, TextInputEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }

         

            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }
        
        private void textEditor_TextArea_TextEntered(object sender, TextInputEventArgs e)
        {
            if (e.Text == "<" || e.Text == "/")
            {   

                _completionWindow = new CompletionWindow(_textEditor.TextArea);
                _completionWindow.Closed += (o, args) => _completionWindow = null;

                var data = _completionWindow.CompletionList.CompletionData;
                data.Add(new MyCompletionData("suki:GlassCard"));
                data.Add(new MyCompletionData("Grid"));
                data.Add(new MyCompletionData("Button"));
                data.Add(new MyCompletionData("TextBlock"));
             
                _completionWindow.Show();
            }
          
        }

        private void Editor_OnTextChanged(object? sender, EventArgs e)
        {
            string PreviewCode = "<Grid xmlns:icons=\"clr-namespace:Material.Icons.Avalonia;assembly=Material.Icons.Avalonia\" xmlns:suki=\"clr-namespace:SukiUI.Controls;assembly=SukiUI\" xmlns='https://github.com/avaloniaui' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>" +
                                 _textEditor.Text + "</Grid>";

            try
            {
                Control DemoContent = AvaloniaRuntimeXamlLoader.Parse<Grid>(PreviewCode);
                _glassPlayground.Content = DemoContent;
            }
            catch
            {
               
            }
        }

        private string StartingString =
            "<suki:GlassCard Width=\"300\" Height=\"320\" Margin=\"15\" VerticalAlignment=\"Top\">\n" +
            "    <Grid>\n" +
            "        <TextBlock FontSize=\"16\" FontWeight=\"DemiBold\" Text=\"Humidity\" />\n" +
            "        <Viewbox Width=\"175\" Height=\"175\" Margin=\"0,0,0,5\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\">\n" +
            "               <suki:WaveProgress Value=\"{Binding Value, ElementName=SliderT}\" />\n" +
            "        </Viewbox>\n" +
            "        <DockPanel VerticalAlignment=\"Bottom\">\n" +
            "            <icons:MaterialIcon Width=\"20\" Height=\"20\" DockPanel.Dock=\"Left\" Foreground=\"#666666\" Kind=\"TemperatureLow\" /> \n" +
            "            <icons:MaterialIcon Width=\"20\" Height=\"20\" DockPanel.Dock=\"Right\" Foreground=\"#666666\" Kind=\"TemperatureHigh\" />\n" +
            "            <Slider Name=\"SliderT\" Margin=\"8,0\" Maximum=\"100\" Minimum=\"0\" Value=\"23\" />\n" +
            "        </DockPanel>\n" +
            "     </Grid>\n" +
            "</suki:GlassCard>";

    }
    
  
    public class MyCompletionData : ICompletionData
    {
        public MyCompletionData(string text)
        {
            Text = text;
        }

        public IImage Image => null;

        public string Text { get; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content => Text;

        public object Description => "Avalonia Control";

        public double Priority { get; } = 0;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
    }
    
}