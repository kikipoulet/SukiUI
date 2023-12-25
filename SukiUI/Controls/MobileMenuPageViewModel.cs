using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;

//using DialogHostAvalonia.Positioners;

namespace SukiUI.Controls
{
    public class MobileMenuPageViewModel : INotifyPropertyChanged
    {
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        private string headertext = "Home";

        public string HeaderText
        {
            get => headertext;
            set 
            {
                headertext = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HeaderText)));
            }
        }

        private bool isdialogopen = false;

        public bool IsDialogOpen
        {
            get => isdialogopen;
            set 
            {
                isdialogopen = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDialogOpen)));
            }
        }


        private double toastOpacity = 0;

        public double ToastOpacity
        {
            get => toastOpacity;
            set 
            {
                toastOpacity = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToastOpacity)));
            }
        }

        private Control toastMessage = new Grid();

        public Control ContentToast
        {
            get => toastMessage;
            set 
            {
                toastMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContentToast)));
            }
        }
        
        private Thickness toastMargin = new Thickness(0,125,0,0);

        public Thickness ToastMargin
        {
            get => toastMargin;
            set 
            {
                toastMargin = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToastMargin)));
            }
        }

        
        
       /* private AlignmentDialogPopupPositioner dialogPosition = new AlignmentDialogPopupPositioner();

        public AlignmentDialogPopupPositioner DialogPosition
        {
            get => dialogPosition;
            set => this.RaiseAndSetIfChanged(ref dialogPosition, value);
        } */



        private Control dialogchild = new Grid();

        public Control DialogChild
        {
            get => dialogchild;
            set 
            {
                dialogchild = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DialogChild)));
            }
        }


        public void ShowMenu()
        {
            MenuVisibility = false;
            MenuVisibility = true;
        }

        public void HideMenu()
        {
            MenuVisibility = false;
        }

        private bool menuvisibility = false;

        public bool MenuVisibility
        {
            get => menuvisibility;
            set 
            {
                menuvisibility = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MenuVisibility)));
            }
        }

        public void ChangeMenuVisibility()
        {
            MenuVisibility = !MenuVisibility;

        }

       

        private object headerContent = new Grid();

        public object HeaderContent
        {
            get => headerContent;
            set 
            {
                headerContent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HeaderContent)));
            }
        }

        private List<SideMenuItem> menuItems = new List<SideMenuItem>();

        public List<SideMenuItem> MenuItems
        {
            get => menuItems;
            set 
            {
                menuItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MenuItems)));
            }
        }
        
        private object currentPage = new Grid();

        public object CurrentPage
        {
            get => currentPage;
            set 
            {
                currentPage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentPage)));
            }
        }

        public MobileMenuPageViewModel()
        {
            DoTheThing = ReactiveCommand.Create<SideMenuItem>(ChangePage);
        }

        public ReactiveCommand<SideMenuItem, Unit> DoTheThing { get; set; }
        public void ChangePage(SideMenuItem o)
        {
            
                Console.WriteLine(o);
                HeaderText = o.Header;
                CurrentPage = o.Content;

                Task.Run(() =>
                {
                    Thread.Sleep(50);
                    MenuVisibility = false;
                });
                
            
        }
    }
}
