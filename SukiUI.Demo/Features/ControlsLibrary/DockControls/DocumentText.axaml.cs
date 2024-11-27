using Avalonia.Controls;
using SukiUI.Demo.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary.DockControls
{
    public partial class DocumentText : UserControl
    {
        private const string DefaultCode = """
            <UserControl xmlns="https://github.com/avaloniaui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
                         xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                         mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
                         x:Class="SukiUI.Demo.Features.ControlsLibrary.DockView">
                Welcome to Avalonia!
            </UserControl>

            """;

        public DocumentText()
        {
            InitializeComponent();

            var _textEditor = this.FindControl<CodeEditor>("Editor");

            if (_textEditor != null)
            {
                _textEditor.Text = DefaultCode;
            }
        }
    }
}