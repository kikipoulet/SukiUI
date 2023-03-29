using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;
using Avalonia.VisualTree;

namespace SukiUI.Controls
{
    
    
    public partial class MobileStack : UserControl
    {
        public MobileStack()
        {
            InitializeComponent();
        }

     
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void PopPage()
        {    
            if (Pages.Count == 0)
                return;

            var page = Pages.Pop();

            Content = page;
            CurrentPage = page;
        }

        public static void Pop()
        {
            var instance = GetMobileStackInstance();
            
            if (instance.Pages.Count == 0)
                return;

            var page = instance.Pages.Pop();
           
            instance.Content = page;
            instance.CurrentPage = page;

        }
        public Control CurrentPage;

        public static void Push(UserControl content,bool DisableComeBack = false)
        {
            var instance = GetMobileStackInstance();

            if(instance.CurrentPage == null)
                instance.CurrentPage =(Control)instance.Content  ;

            instance.Pages.Push(instance.CurrentPage);

            if (DisableComeBack)
                instance.Pages.Clear();

            var m =  content ;
            instance.CurrentPage = m;
            
            instance.Content = m;

            
        }

        Stack<Control> Pages = new Stack<Control>();
        
        private static MobileStack GetMobileStackInstance()
        {
            MobileStack container = null;
            try
            {
                container = ((ISingleViewApplicationLifetime)Application.Current.ApplicationLifetime).MainView.GetVisualDescendants().OfType<MobileStack>().First();
                
            }
            catch (Exception exc)
            {
            
                try
                {
                    container = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow.GetVisualDescendants().OfType<MobileStack>().First();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "You are trying to use a InteractiveContainer functionality without declaring one !");
                }
                
            }

            return container;
        }

    }
}
