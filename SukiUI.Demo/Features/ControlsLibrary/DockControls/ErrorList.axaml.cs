using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SukiUI.Demo.Features.ControlsLibrary.DockControls
{
    public partial class ErrorList : UserControl
    {
        public ErrorList()
        {
            InitializeComponent();
            this.Get<DataGrid>("DG").ItemsSource = new ObservableCollection<ErrorD>()
            {
                new ErrorD(),
                new ErrorD()
                {
                    Description = "Unused local variable."
                },
                new ErrorD()
                {
                    Description = "Type 'Person' not defined."
                },
            };
        }
    }
}

public class ErrorD
    {
        public string Code { get; set; } = "BC30230";
        public string Description { get; set; } = "'Inherits' not valid.";
        public string Project { get; set; } = "Avalonia.DockDemo";
        public string File { get; set; } = "Program.cs";
        public string Line { get; set; } = "2";
    }
