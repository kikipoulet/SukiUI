using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
    }
}
