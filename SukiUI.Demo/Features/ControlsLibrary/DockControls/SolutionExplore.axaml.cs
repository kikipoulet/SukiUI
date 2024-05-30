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
            FolderContents = new ObservableCollection<FolderItem>();
            LoadFolderContents("../");
            this.FindControl<TreeView>("TV").ItemsSource = FolderContents;
        }
    
        public ObservableCollection<FolderItem> FolderContents { get; set; }

    

        private void LoadFolderContents(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            var files = dirInfo.GetFiles();
            var directories = dirInfo.GetDirectories();

            foreach (var file in files)
            {
                FolderContents.Add(new FolderItem(file.Name, false));
            }

            foreach (var directory in directories)
            {
                var folderItem = new FolderItem(directory.Name, true);
                LoadFolderContents(directory.FullName);
                folderItem.Children = new ObservableCollection<FolderItem>(GetFolderItems(directory.FullName));
                FolderContents.Add(folderItem);
            }
        
            FolderContents = new ObservableCollection<FolderItem>(FolderContents.OrderBy(item => !item.IsDirectory).ThenBy(item => item.Name));
        }

        private IEnumerable<FolderItem> GetFolderItems(string path)
        {
            var result = new List<FolderItem>();
            var dirInfo = new DirectoryInfo(path);
            var files = dirInfo.GetFiles();
            var directories = dirInfo.GetDirectories();

            foreach (var file in files)
            {
                result.Add(new FolderItem(file.Name, false));
            }

            foreach (var directory in directories)
            {
                result.Add(new FolderItem(directory.Name, true));
            }

            return result;
        }
    }


    public class FolderItem
    {
        public string Name { get; set; }
        public bool IsDirectory { get; set; }
        public ObservableCollection<FolderItem> Children { get; set; }

        public FolderItem(string name, bool isDirectory)
        {
            Name = name;
            IsDirectory = isDirectory;
            Children = new ObservableCollection<FolderItem>();
        }
    }




