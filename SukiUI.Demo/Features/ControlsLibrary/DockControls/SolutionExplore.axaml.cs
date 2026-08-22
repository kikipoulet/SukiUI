using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SukiUI.Demo.Features.ControlsLibrary.DockControls;

public partial class SolutionExplore : UserControl
{
    public SolutionExplore()
    {
        InitializeComponent();
        FolderContents = CreateDemoTree();
        this.FindControl<TreeView>("TV").ItemsSource = FolderContents;
    }

    public ObservableCollection<FolderItem> FolderContents { get; }

    private static ObservableCollection<FolderItem> CreateDemoTree() =>
    [
        Directory("SukiUI", File("App.axaml"), File("App.axaml.cs"), Directory("Controls", File("SukiWindow.cs"))),
        Directory("SukiUI.Demo", File("Program.cs"), File("SukiUIDemoView.axaml"), Directory("Features", File("DockView.axaml"))),
        Directory("tests", File("SukiUI.Tests.csproj")),
        File("SukiUI.sln"),
        File("Directory.Packages.props")
    ];

    private static FolderItem File(string name) => new(name, false);

    private static FolderItem Directory(string name, params FolderItem[] children) => new(name, true)
    {
        Children = new ObservableCollection<FolderItem>(children)
    };
}

public class FolderItem
{
    public FolderItem(string name, bool isDirectory)
    {
        Name = name;
        IsDirectory = isDirectory;
        Children = new ObservableCollection<FolderItem>();
    }

    public string Name { get; }
    public bool IsDirectory { get; }
    public ObservableCollection<FolderItem> Children { get; init; }
}



