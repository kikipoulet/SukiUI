using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using SukiUI.Controls;
using SukiUI.Models;

namespace SukiUI.Theme.Shadcn
{
    public static class Shadcn
    {
        public static void Configure(Application application, ThemeVariant startupTheme)
        {
            Application.Current.Resources.MergedDictionaries.Add(new ResourceInclude(new Uri("avares://SukiUI/Theme/Shadcn/BlackWhiteTheme.axaml")
            )
            {
                Source = new Uri("avares://SukiUI/Theme/Shadcn/BlackWhiteTheme.axaml")
            });
        
            var whiteTheme = new SukiColorTheme("White", new Color(255, 255, 255, 255), new Color(255, 255, 255, 255));
            var blackTheme = new SukiColorTheme("Black", new Color(255, 0, 0, 0), new Color(255, 0, 0, 0));

            SukiTheme.GetInstance().AddColorThemes(new []{whiteTheme, blackTheme});

            var BlackStyles = new StyleInclude(new Uri("avares://SukiUI/Theme/Shadcn/ShadDarkStyles.axaml"))
            {
                Source = new Uri("avares://SukiUI/Theme/Shadcn/ShadDarkStyles.axaml")
            };

        
            SukiTheme.GetInstance().OnBaseThemeChanged += variant =>
            {
                if (variant == ThemeVariant.Dark)
                {
                    application.Styles.Add(BlackStyles);
                    SukiTheme.GetInstance().ChangeColorTheme(whiteTheme);
                
                }
                else
                {
                    SukiTheme.GetInstance().ChangeColorTheme(blackTheme);
                    application.Styles.Remove(BlackStyles);
                }
            };

            try
            {
                ((SukiWindow)((ClassicDesktopStyleApplicationLifetime)application.ApplicationLifetime).MainWindow)
                    .BackgroundShaderFile = "backgroundshadcn";
            }catch{}
            
            SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Light);
            SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Dark);
            SukiTheme.GetInstance().ChangeBaseTheme(startupTheme);

        }
    }
}