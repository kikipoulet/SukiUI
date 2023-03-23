using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI;
using SukiUI.Controls;

namespace AndroidTest.Views
{
    public partial class SettingsPage : UserControl
    {
        private static readonly Lazy<SettingsPage> lazy =
            new Lazy<SettingsPage>(() => new SettingsPage());

        public static SettingsPage Instance { get { return lazy.Value; } }
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ComeBack(object? sender, RoutedEventArgs e)
        {
            MobileStack.Pop();
        }

        private void DarkThemeSwitch_OnClick(object? sender, RoutedEventArgs e)
        {
            if((bool)this.FindControl<ToggleButton>("DarkThemeSwitch").IsChecked)
                ColorTheme.LoadDarkTheme(Application.Current);
            else
                ColorTheme.LoadLightTheme(Application.Current);
                
            
        }

        private void GoToLogin(object? sender, RoutedEventArgs e)
        {
            MobileStack.Push(new MainView());
        }
    }
}
