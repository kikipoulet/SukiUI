using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using SukiUI.Content;
using SukiUI.Extensions;

namespace SukiUI.Controls;

public partial class MessageBox : Window
{
    private static readonly SolidColorBrush InfoBrush = new(Color.FromRgb(47, 84, 235));

    public MessageBox()
    {
        InitializeComponent();
    }

    public MessageBox(string title, string message)
    {
        InitializeComponent();
        this.FindRequiredControl<TextBlock>("Title").Text = title;
        this.FindRequiredControl<TextBlock>("Message").Text = message;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        CanResize = false;
    }

    public static void Info(Window owner, string title, string message, WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen)
    {
        var mbox = new MessageBox(title, message);
        if (mbox.FindControl<PathIcon>("InfoIcon") is not { } icon) return;
        icon.Data = Icons.CircleInformation;
        icon.Foreground = InfoBrush;
        mbox.WindowStartupLocation = startupLocation;
        mbox.ShowDialog(owner);
    }

    public static void Success(Window owner, string title, string message, WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen)
    {
        var mbox = new MessageBox(title, message);
        if (mbox.FindControl<PathIcon>("InfoIcon") is not { } icon) return;
        icon.Data = Icons.CircleCheck;
        icon.Foreground = Brushes.DarkGreen;
        mbox.WindowStartupLocation = startupLocation;
        mbox.ShowDialog(owner);
    }

    public static void Error(Window owner, string title, string message, WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen)
    {
        var mbox = new MessageBox(title, message);
        if (mbox.FindControl<PathIcon>("InfoIcon") is not { } icon) return;
        icon.Data = Icons.CircleClose;
        icon.Foreground = Brushes.DarkRed;
        mbox.WindowStartupLocation = startupLocation;
        mbox.ShowDialog(owner);
    }

    private void Close(object sender, RoutedEventArgs e)
    {
        Close();
    }
}