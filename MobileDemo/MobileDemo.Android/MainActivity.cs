using Android.App;
using Android.Content.PM;
using Avalonia.Android;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;
using SukiUI.Controls;
using System.Linq;

namespace MobileDemo.Android
{
    [Activity(Label = "MobileDemo.Android", Theme = "@style/MyTheme.NoActionBar", Icon = "@drawable/icon", LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : AvaloniaMainActivity
    {
        public override void OnBackPressed()
        {

            MobileStack navControl = ((ISingleViewApplicationLifetime)MobileDemo.App.Current.ApplicationLifetime).MainView.GetVisualDescendants().OfType<MobileStack>().First();

            navControl.PopPage();

        }
    }

   
   
}