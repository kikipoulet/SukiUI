using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
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
    }
}