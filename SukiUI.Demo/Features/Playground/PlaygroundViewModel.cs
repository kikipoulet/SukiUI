using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using SukiUI.Theme;

namespace SukiUI.Demo.Features.Playground
{
    public class PlaygroundViewModel : DemoPageBase
    {
        public PlaygroundViewModel() : base("Playground", MaterialIconKind.Pencil, -150)
        {
        }
        
        public ObservableCollection<string> ButtonsElements { get; set; } = new ObservableCollection<string>()
        {
            "<Button Classes=\"Flat\" Content=\"Flat\" />",
            "<Button Classes=\"Flat Rounded\" Content=\"Rounded\" />",
            "<Button Classes=\"Basic\" Content=\"Basic\" />",
            "<Button Content=\"Neutral\" />",
            "<Button Classes=\"Outlined\" Content=\"Outlined\" />",
            "<Button Classes=\"Flat Large\" Content=\"Button\" />"
        };
        
        public ObservableCollection<string> InputsElements { get; set; } = new ObservableCollection<string>()
        {
            "<TextBox Text=\"Textbox\" />",
            "<ToggleSwitch />",
            "<ToggleButton Content=\"Toggle Me\" />",
            "<Slider MinWidth=\"120\" IsSnapToTickEnabled=\"True\" Maximum=\"100\" Minimum=\"0\" TickFrequency=\"1\" Value=\"50\"></Slider>", 
            
            "<ComboBox PlaceholderText=\"Select a Color\">\n" +
            "   <ComboBoxItem>\n" +
            "      <TextBlock>Red</TextBlock>\n" +
            "   </ComboBoxItem>\n" +
            "</ComboBox>\n" ,
            
            "<CheckBox IsChecked=\"True\" />\n" ,
            
            
            "<RadioButton >Item 1</RadioButton>",
            
            "<NumericUpDown></NumericUpDown>",
            "<DatePicker />"
            
            
            
        };
        
        public ObservableCollection<string> ProgressElements { get; set; } = new ObservableCollection<string>()
        {
            "<suki:WaveProgress Value=\"40\" IsTextVisible=\"True\" />\n" ,
            
            "<suki:Stepper Margin=\"30,15\" Index=\"1\">\n" +
            "  <suki:Stepper.Steps>\n" +
            "    <objectModel:ObservableCollection x:TypeArguments=\"system:String\">\n" +
            "      <system:String>Step One</system:String>\n" +
            "      <system:String>Step Two</system:String>\n" +
            "      </objectModel:ObservableCollection>\n" +
            "  </suki:Stepper.Steps>\n" +
            "</suki:Stepper>\n" ,
            
            "<suki:Stepper AlternativeStyle=\"True\" Margin=\"30,15\" Index=\"1\">\n" +
            "  <suki:Stepper.Steps>\n" +
            "    <objectModel:ObservableCollection x:TypeArguments=\"system:String\">\n" +
            "      <system:String>Step One</system:String>\n" +
            "      <system:String>Step Two</system:String>\n" +
  
            "      </objectModel:ObservableCollection>\n" +
            "  </suki:Stepper.Steps>\n" +
            "</suki:Stepper>\n" ,
            
            "<suki:CircleProgressBar Height=\"130\" StrokeWidth=\"11\" Value=\"60\" Width=\"130\">\n" +
            "   <TextBlock Margin=\"0,1,0,0\" Classes=\"h3\">60%</TextBlock>\n" +
            "</suki:CircleProgressBar>\n" ,
            
            "<suki:CircleProgressBar IsIndeterminate=\"True\" />",
            
            "<suki:Loading />",
            
            "<ProgressBar  Value=\"60\" />" ,
            "<ProgressBar  Value=\"50\" ShowProgressText=\"True\" />",
            "<ProgressBar IsIndeterminate=\"True\" />",
        };

        public ObservableCollection<string> ListsElements { get; set; } = new ObservableCollection<string>()
        {
            "<ListBox>\n" +
            "   <TextBlock>item 1</TextBlock>\n" +
            "   <TextBlock>item 2</TextBlock>\n" +
            "   <TextBlock>item 3</TextBlock>\n" +
            "</ListBox>",
            
            "<TreeView >\n" +
            "  <TreeViewItem Header=\"Test 1\">\n" +
            "    <TreeViewItem Header=\"Test 2\" />\n" +
            "    <TreeViewItem Header=\"Test 3\" />\n" +
            "  </TreeViewItem>\n" +
            "  <TreeViewItem Header=\"Test 4\" />\n" +
            "</TreeView>"
        };
        
        public ObservableCollection<string> LayoutElements { get; set; } = new ObservableCollection<string>()
        {
            "<suki:GlassCard />",
            
            "<suki:GroupBox Header=\"Header\">\n" +
            "  <Grid Height=\"100\" Width=\"300\">\n" +
            "    <TextBlock VerticalAlignment=\"Center\" HorizontalAlignment=\"Center\">Test Content</TextBlock>\n" +
            "  </Grid>\n" +
            "</suki:GroupBox>" ,
            
            "<Expander Header=\"Click To Expand\">\n" +
            "   <TextBlock>Expanded</TextBlock>\n" +
            "</Expander>",
            
            "<TabControl>\n" +
            "  <TabItem Header=\"Red Tab\">\n" +
            "    <Grid Background=\"#44ff0000\" />\n" +
            "  </TabItem>\n" +
            "</TabControl>"
        };

    }
    
    public class StringToControlConverter : IValueConverter
    {
        public static readonly StringToControlConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string PreviewCode = "<Grid HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" xmlns:system=\"clr-namespace:System;assembly=netstandard\" xmlns:objectModel=\"clr-namespace:System.Collections.ObjectModel;assembly=System.ObjectModel\" xmlns:icons=\"clr-namespace:Material.Icons.Avalonia;assembly=Material.Icons.Avalonia\" xmlns:suki=\"clr-namespace:SukiUI.Controls;assembly=SukiUI\" xmlns='https://github.com/avaloniaui' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>" +
                                 value + "</Grid>";

            return AvaloniaRuntimeXamlLoader.Parse<Grid>(PreviewCode);
              
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}