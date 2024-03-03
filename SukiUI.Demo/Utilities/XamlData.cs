using System.Collections.Frozen;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SukiUI.Demo.Utilities
{
    internal static class XamlData
    {
        internal static readonly FrozenDictionary<string, string> Buttons =
            new Dictionary<string, string>
            {
                {
                    "ButtonFlat", """<Button Classes="Flat" Content="Flat" />"""
                },
                {
                    "ButtonFlatRounded", """<Button Classes="Flat Rounded" Content="Rounded" />"""
                },
                {
                    "ButtonFlatLarge", """<Button Classes="Flat Large" Content="Large" />"""
                },
                {
                    "ButtonBasic", """<Button Classes="Basic" Content="Basic" />"""
                },
                {
                    "ButtonBasicAccent", """<Button Classes="Basic Accent" Content="Basic Accent" />"""
                },
                {
                    "ButtonNeutral", """<Button Content="Neutral" />"""
                },
                {
                    "ButtonOutlined", """<Button Classes="Outlined" Content="Outlined" />"""
                }
            }.ToFrozenDictionary();

        internal static readonly FrozenDictionary<string, string> Inputs =
            new Dictionary<string, string>
            {
                {
                    "TextBox", """<TextBox Text="Text box" />"""
                },
                {
                    "ToggleSwitch", "<ToggleSwitch />"
                },
                {
                    "ToggleButton", """<ToggleButton Content="Toggle Me" />"""
                },
                {
                    "Slider", """
                              <Slider MinWidth="120" IsSnapToTickEnabled="True"
                               Maximum="100" Minimum="0" TickFrequency="1" Value="50">
                              </Slider>
                              """
                },
                {
                    "ComboBox", """
                                 <ComboBox PlaceholderText="Select a Color">
                                    <ComboBoxItem>
                                       <TextBlock>Red</TextBlock>
                                    </ComboBoxItem>
                                 </ComboBox>
                                """
                },
                {
                    "CheckBox", """<CheckBox IsChecked="True" />"""
                },
                {
                    "RadioButton", "<RadioButton>Item 1</RadioButton>"
                },
                {
                    "NumericUpDown", "<NumericUpDown></NumericUpDown>"
                },
                {
                    "DatePicker", "<DatePicker />"
                }
            }.ToFrozenDictionary();

        internal static readonly FrozenDictionary<string, string> Progress =
            new Dictionary<string, string>
            {
                {
                    "WaveProgress", """
                                    <suki:WaveProgress Value="40" IsTextVisible="True" />
                                    """
                },
                {
                    "Stepper", """
                               <suki:Stepper Margin="30,15" Index="1">
                                 <suki:Stepper.Steps>
                                   <objectModel:ObservableCollection x:TypeArguments="system:String">
                                     <system:String>Step One</system:String>
                                     <system:String>Step Two</system:String>
                                     </objectModel:ObservableCollection>
                                 </suki:Stepper.Steps>
                               </suki:Stepper>
                               """
                },
                {
                    "CircleProgressBar", """
                                         <suki:CircleProgressBar Height="130" StrokeWidth="11" Value="60" Width="130">
                                            <TextBlock Margin="0,1,0,0" Classes="h3">60%</TextBlock>
                                         </suki:CircleProgressBar>
                                         """
                },
                {
                    "StepperAlternativeStyle", """
                                               <suki:Stepper AlternativeStyle="True" Margin="30,15" Index="1">
                                                 <suki:Stepper.Steps>
                                                   <objectModel:ObservableCollection x:TypeArguments="system:String">
                                                     <system:String>Step One</system:String>
                                                     <system:String>Step Two</system:String>
                                                     </objectModel:ObservableCollection>
                                                 </suki:Stepper.Steps>
                                               </suki:Stepper>
                                               """
                },
                {
                    "CircleProgressBarIndeterminate", """<suki:CircleProgressBar IsIndeterminate="True" />"""
                },
                {
                    "Loading", "<suki:Loading />"
                },
                {
                    "ProgressBar60", """<ProgressBar  Value="60" />"""
                },
                {
                    "ProgressBar50WithProgressText", """<ProgressBar  Value="50" ShowProgressText="True" />"""
                },
                {
                    "ProgressBarIndeterminate", """<ProgressBar IsIndeterminate="True" />"""
                }
            }.ToFrozenDictionary();

        internal static readonly FrozenDictionary<string, string> Lists =
            new Dictionary<string, string>
            {
                {
                    "ListBox", """
                               <ListBox>
                                  <TextBlock>item 1</TextBlock>
                                  <TextBlock>item 2</TextBlock>
                                  <TextBlock>item 3</TextBlock>
                               </ListBox>
                               """
                },
                {
                    "TreeView", """
                                <TreeView >
                                  <TreeViewItem Header="Test 1">
                                    <TreeViewItem Header="Test 2" />
                                    <TreeViewItem Header="Test 3" />
                                  </TreeViewItem>
                                  <TreeViewItem Header="Test 4" />
                                </TreeView>
                                """
                }
            }.ToFrozenDictionary();

        internal static readonly FrozenDictionary<string, string> Layout =
            new Dictionary<string, string>
            {
                {
                    "GlassCard", "<suki:GlassCard />"
                },
                {
                    "GroupBox", """
                                <suki:GroupBox Header="Header">
                                  <Grid Height="100" Width="300">
                                    <TextBlock VerticalAlignment="Center" HorizontalAlignment="Center">Test Content</TextBlock>
                                  </Grid>
                                </suki:GroupBox>
                                """
                },
                {
                    "Expander", """
                                <Expander Header="Click To Expand">
                                   <TextBlock>Expanded</TextBlock>
                                </Expander>
                                """
                },
                {
                    "TabControl", """
                                  <TabControl>
                                    <TabItem Header="Red Tab">
                                      <Grid Background="#44ff0000" />
                                    </TabItem>
                                   </TabControl>
                                  """
                }
            }.ToFrozenDictionary();

        internal const string PlaygroundStartingCode =
            """
              <suki:GlassCard Width="300" Height="320" Margin="15" VerticalAlignment="Top">
                  <Grid>
                      <TextBlock FontSize="16" FontWeight="DemiBold" Text="Humidity" />
                      <Viewbox Width="175" Height="175" Margin="0,0,0,5" HorizontalAlignment="Center" VerticalAlignment="Center">
                          <suki:WaveProgress Value="{Binding Value, ElementName=SliderT}" />
                      </Viewbox>
                      <DockPanel VerticalAlignment="Bottom">
                          <icons:MaterialIcon Width="20" Height="20" DockPanel.Dock="Left" Foreground="#666666" Kind="TemperatureLow" />
                          <icons:MaterialIcon Width="20" Height="20" DockPanel.Dock="Right" Foreground="#666666" Kind="TemperatureHigh" />
                          <Slider Name="SliderT" Margin="8,0" Maximum="100" Minimum="0" Value="23" />
                      </DockPanel>
                  </Grid>
              </suki:GlassCard>
            """;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string InsertIntoGridControl(string xamlCode)
            => """
               <Grid HorizontalAlignment="Center" VerticalAlignment="Center"
                   	xmlns:system="clr-namespace:System;assembly=netstandard"
                   	xmlns:objectModel="clr-namespace:System.Collections.ObjectModel;assembly=System.ObjectModel"
                   	xmlns:icons="clr-namespace:Material.Icons.Avalonia;assembly=Material.Icons.Avalonia"
                   	xmlns:suki="clr-namespace:SukiUI.Controls;assembly=SukiUI"
                   	xmlns='https://github.com/avaloniaui'
                   	xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>

               """ + xamlCode + "</Grid>";
    }
}