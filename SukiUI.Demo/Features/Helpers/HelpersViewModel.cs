using System.Diagnostics.Metrics;
using Avalonia.Collections;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Enums;

namespace SukiUI.Demo.Features.Helpers
{
    public partial class HelpersViewModel : DemoPageBase
    {
        
        [ObservableProperty] private bool myBool;
        [ObservableProperty] private int counter;
        
        
        public void IncreaseCounter()
        {
            Counter++;
        }
    
        public void DecreaseCounter()
        {
            Counter--;
        }
        
        public void InvertBool()
        {
            MyBool = !MyBool;
        }

        
        public HelpersViewModel() : base("Helpers", MaterialIconKind.PaletteOutline, -2)
        {
            
        }
    }
}