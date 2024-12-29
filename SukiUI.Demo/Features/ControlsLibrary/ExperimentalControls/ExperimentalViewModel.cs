using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Material.Icons.Avalonia;
using SukiUI.Controls.Experimental;

namespace SukiUI.Demo.Features.ControlsLibrary
{
    public partial class ExperimentalViewModel() : DemoPageBase("Experimental", MaterialIconKind.Microscope)
    {
        public new ObservableCollection<ChatMessage> Messages { get; set; } = new ObservableCollection<ChatMessage>()
        {
            new ChatMessage()
            {
                Content = "Hello Bob !", IsUser = false, IsWriting = false
            },
            new ChatMessage()
            {
                Content = "Hello, nice to meet you !", IsUser = true, IsWriting = false
            },
        };

        [RelayCommand]
        private void AddFriendMessage()
        {
            Messages.Add(new ChatMessage()
            {
                IsUser = false, IsWriting = false, Content = "Hello again !"
            });
        }
        
        [RelayCommand]
        private void AddFriendFile()
        {
            StackPanel stack = new StackPanel(){ Margin = new Thickness(0,10,0,0)};
            stack.Children.Add(new MaterialIcon()
            {
                Foreground = Brushes.Gray ,Kind = MaterialIconKind.FileDownloadOutline, Height = 55, Width = 55, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
            });
            stack.Children.Add(new TextBlock()
            {
                Classes = { "SukiLowText" }, FontWeight =  FontWeight.DemiBold,Text = "App.axaml", Margin = new Thickness(2), HorizontalAlignment = HorizontalAlignment.Center
            });
            stack.Children.Add(new Button()
            {
                Classes = { "Flat" }, Content = "Download", Padding = new Thickness(35,8), Margin = new Thickness(8,15,8,8)
            });
            Messages.Add(new ChatMessage()
            {
                IsUser = false, IsWriting = false, Content = stack
            });
        }
    }
}