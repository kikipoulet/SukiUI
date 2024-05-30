using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace SukiUI.Demo.Features.ControlsLibrary.DockControls
{
    public partial class DocumentText : UserControl
    {
        public DocumentText()
        {
            InitializeComponent();
            
              
            var _textEditor = this.FindControl<TextEditor>("Editor");
            _textEditor.Text = @"<UserControl xmlns=""https://github.com/avaloniaui""
             xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
             xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
             xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
             mc:Ignorable=""d"" d:DesignWidth=""800"" d:DesignHeight=""450""
             x:Class=""SukiUI.Demo.Features.ControlsLibrary.DockView"">
    Welcome to Avalonia!
</UserControl>
";

//Here we initialize RegistryOptions with the theme we want to use.
            var  _registryOptions = new RegistryOptions(ThemeName.DarkPlus);

//Initial setup of TextMate.
            var _textMateInstallation = _textEditor.InstallTextMate(_registryOptions);

//Here we are getting the language by the extension and right after that we are initializing grammar with this language.
//And that's all 😀, you are ready to use AvaloniaEdit with syntax highlighting!
            _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".cs").Id));
        }
    }
}