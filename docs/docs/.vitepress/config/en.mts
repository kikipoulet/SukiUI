import { defineConfig } from 'vitepress'

export const en = defineConfig({
  description: "A Desktop UI Library for Avalonia",
  themeConfig: {
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Documentation', link: '/documentation' }
    ],

    sidebar: [
      {
        text: 'Get Started',
        items: [
          { text: 'Introduction', link: '/documentation/getting-started/introduction' },
          { text: 'Installation', link: '/documentation/getting-started/installation' },
          { text: 'Launch', link: '/documentation/getting-started/launch' },
        ]
      },
      {
        text: 'Theming',
        items: [
          { text: 'Basic', link: '/documentation/theming/basic' },
          { text: 'Light & Dark', link: '/documentation/theming/theme' },
          { text: 'Color', link: '/documentation/theming/theme-color' },
        ]
      },
      {
        text: 'Controls',
        items: [
          {
            text: 'Navigation',
            items: [
              { text: 'SideMenu', link: '/documentation/controls/navigation/sidemenu' },
              { text: 'StackPage', link: '/documentation/controls/navigation/stackpage' },
              { text: 'TabControl', link: '/documentation/controls/navigation/tabcontrol' },
            ]
          },
          {
            text: 'Layout',
            items: [
              { text: 'GlassCard', link: '/documentation/controls/layout/glasscard' },
              { text: 'SettingsLayout', link: '/documentation/controls/layout/settingslayout' },
              { text: 'SukiWindow', link: '/documentation/controls/layout/sukiwindow' },
              { text: 'Dock', link: '/documentation/controls/layout/dock' },
            ]
          },
          {
            text: 'Inputs',
            items: [
              { text: 'Button', link: '/documentation/controls/inputs/button' },
              { text: 'DropDownButton', link: '/documentation/controls/inputs/dropdownbutton' },
              { text: 'ToggleSwitch', link: '/documentation/controls/inputs/toggleswitch' },
              { text: 'ToggleButton', link: '/documentation/controls/inputs/togglebutton' },
              { text: 'Slider', link: '/documentation/controls/inputs/slider' },
              { text: 'ComboBox', link: '/documentation/controls/inputs/combobox' },
              { text: 'NumericUpDown', link: '/documentation/controls/inputs/numericupdown' },
              { text: 'TextBox', link: '/documentation/controls/inputs/textbox' },
              { text: 'CheckBox', link: '/documentation/controls/inputs/checkbox' },
              { text: 'RadioButton', link: '/documentation/controls/inputs/radiobutton' },
              { text: 'ContextMenu', link: '/documentation/controls/inputs/contextmenu' },
              { text: 'AutoCompleteBox', link: '/documentation/controls/inputs/autocompletebox' },
            ]
          },
          {
            text: 'Date & Time',
            items: [
              { text: 'Calendar', link: '/documentation/controls/datetime/calendar' },
              { text: 'DatePicker', link: '/documentation/controls/datetime/datepicker' },
              { text: 'CalendarDatePicker', link: '/documentation/controls/datetime/calendardatepicker' },
              { text: 'TimePicker', link: '/documentation/controls/datetime/timepicker' },
            ]
          },
          {
            text: 'Progress',
            items: [
              { text: 'WaveProgress', link: '/documentation/controls/progress/waveprogress' },
              { text: 'Stepper', link: '/documentation/controls/progress/stepper' },
              { text: 'CircleProgressBar', link: '/documentation/controls/progress/circleprogressbar' },
              { text: 'Loading', link: '/documentation/controls/progress/loading' },
              { text: 'ProgressBar', link: '/documentation/controls/progress/progressbar' },
            ]
          },
          {
            text: 'Data Presentation',
            items: [
              { text: 'DataGrid', link: '/documentation/controls/data/datagrid' },
              { text: 'ListBox', link: '/documentation/controls/data/listbox' },
              { text: 'TreeView', link: '/documentation/controls/data/treeview' },
              { text: 'GroupBox', link: '/documentation/controls/data/groupbox' },
              { text: 'BusyArea', link: '/documentation/controls/data/busyarea' },
              { text: 'Expander', link: '/documentation/controls/data/expander' },
            ]
          },
          {
            text: 'Notification',
            items: [
              { text: 'InfoBar', link: '/documentation/controls/notification/infobar' },
              { text: 'InfoBadge', link: '/documentation/controls/notification/infobadge' },
            ]
          },
          {
            text: 'System',
            items: [
              { text: 'FilePicker', link: '/documentation/controls/system/filepicker' },
            ]
          }
        ]
      },
      {
        text: 'Hosts',
        items: [
          { text: 'About Hosts', link: '/documentation/hosts/hosts' },
          { text: 'Dialog', link: '/documentation/hosts/dialog' },
          { text: 'Toast', link: '/documentation/hosts/toast' },
        ]
      },
      {
        text: 'Style',
        items: [
          { text: 'Text', link: '/documentation/style/text' },
          { text: 'Color', link: '/documentation/style/color' },
          { text: 'Icon', link: '/documentation/style/icon' },
        ]
      }
    ]
  }
})
