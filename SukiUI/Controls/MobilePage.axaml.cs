using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;

namespace SukiUI.Controls
{
    public class SukiMobilePage
    {
        public string Header { get; set; } = "Page";
        public Control Content { get; set; } = new Grid();
    }
    public partial class MobilePage : UserControl
    {
        public MobilePage()
        {
            InitializeComponent();

            DataContext = ViewModel;
        }

        private MobilePageViewModel ViewModel = new MobilePageViewModel();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
          
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            if (Pages.Count == 0)
                return;

            var page = Pages.Pop();
            Header = page.Header;
            Content = page.Content;
            CurrentPage = page;

            if(Pages.Count == 0)
                BackVisibility = false;
        }
        public SukiMobilePage CurrentPage;
        public void NewPage(string header, UserControl content,bool DisableComeBack = false)
        {
            

            if(CurrentPage == null)
                CurrentPage = new SukiMobilePage() { Header = Header, Content = (Control)Content  };

            Pages.Push(CurrentPage);

            BackVisibility = true;

            if (DisableComeBack)
            {
                Pages.Clear();
                BackVisibility = false;
            }

            var m = new SukiMobilePage() { Header = header, Content = content };
            CurrentPage = m;
            Header = m.Header;
            Content = m.Content;

            
        }

        Stack<SukiMobilePage> Pages = new Stack<SukiMobilePage>();

        public static readonly StyledProperty<string> HeaderProperty =
       AvaloniaProperty.Register<MobilePage, string>(nameof(Header), defaultValue: "Header");

        public string Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        private static readonly StyledProperty<bool> BackVisibilityProperty =
       AvaloniaProperty.Register<MobilePage, bool>(nameof(BackVisibility), defaultValue: false);

        public bool BackVisibility
        {
            get { return GetValue(BackVisibilityProperty); }
            set { SetValue(BackVisibilityProperty, value); }
        }

        public void ShowDialog(Control Content)
        {
            ViewModel.CurrentDialog = Content;
            ViewModel.DialogOpen = true;
        }

   
    }
}
