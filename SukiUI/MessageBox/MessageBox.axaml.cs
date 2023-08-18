using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Material.Icons.Avalonia;

namespace SukiUI.MessageBox
{
    public partial class MessageBox : Window
    {
        public MessageBox()
        {
            InitializeComponent();
        }

        public MessageBox(string Title, string Message)
        {
            InitializeComponent();
            this.FindControl<TextBlock>("Title").Text = Title;
            this.FindControl<TextBlock>("Message").Text = Message;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.CanResize = false;
        }

        public static void Info(Window owner, string Title, string Message, WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen)
        {
            var mbox = new MessageBox(Title, Message);
            mbox.FindControl<MaterialIcon>("Icone").Kind = Material.Icons.MaterialIconKind.InformationCircle;
            mbox.FindControl<MaterialIcon>("Icone").Foreground = new SolidColorBrush(Color.FromRgb(47,84,235));
            mbox.WindowStartupLocation = startupLocation;
            mbox.ShowDialog(owner);
        }

        public static void Success(Window owner, string Title, string Message, WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen)
        {
            var mbox = new MessageBox(Title, Message);
            mbox.FindControl<MaterialIcon>("Icone").Kind = Material.Icons.MaterialIconKind.CheckboxMarkedCircleOutline;
            mbox.FindControl<MaterialIcon>("Icone").Foreground = Brushes.DarkGreen;
            mbox.WindowStartupLocation = startupLocation;
            mbox.ShowDialog(owner);
        }

        public static void Error(Window owner, string Title, string Message, WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen)
        {
            var mbox = new MessageBox(Title, Message);
            mbox.FindControl<MaterialIcon>("Icone").Kind = Material.Icons.MaterialIconKind.CloseCircleOutline;
            mbox.FindControl<MaterialIcon>("Icone").Foreground = Brushes.DarkRed;
            mbox.WindowStartupLocation = startupLocation;
            mbox.ShowDialog(owner);
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
