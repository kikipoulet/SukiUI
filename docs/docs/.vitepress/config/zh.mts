import { defineConfig } from 'vitepress'

export const zh = defineConfig({
  description: "一个 Avalonia 控件库",
  themeConfig: {
    nav: [
      { text: '主页', link: '/zh' },
      { text: '文档', link: '/zh/documentation' }
    ],

    sidebar: [
      {
        text: '开始',
        items: [
          { text: '介绍', link: '/zh/documentation/getting-started/introduction' },
          { text: '安装', link: '/zh/documentation/getting-started/installation' },
          { text: '启动应用', link: '/zh/documentation/getting-started/launch' },
        ]
      },
      {
        text: '主题',
        items: [
          { text: '主题实例', link: '/zh/documentation/theming/basic' },
          { text: '明暗主题切换', link: '/zh/documentation/theming/theme' },
          { text: '主题色', link: '/zh/documentation/theming/theme-color' },
        ]
      },
      {
        text: '控件',
        items: [
          {
            text: 'Navigation',
            items: [
              { text: 'SideMenu', link: '/zh/documentation/controls/navigation/sidemenu' },
            ]
          },
          {
            text: 'Layout',
            items: [
              { text: 'GlassCard', link: '/zh/documentation/controls/layout/glasscard' },
              { text: 'TabControl', link: '/zh/documentation/controls/layout/tabcontrol' },
              { text: 'StackPage', link: '/zh/documentation/controls/layout/stackpage' },
              { text: 'SettingsLayout', link: '/zh/documentation/controls/layout/settingslayout' },
              { text: 'SukiWindow', link: '/zh/documentation/controls/layout/sukiwindow' },
              { text: 'Dock', link: '/zh/documentation/controls/layout/dock' },
            ]
          },
          {
            text: 'Inputs',
            items: [
              { text: 'Button', link: '/zh/documentation/controls/inputs/button' },
              { text: 'DropDownButton', link: '/zh/documentation/controls/inputs/dropdownbutton' },
              { text: 'ToggleSwitch', link: '/zh/documentation/controls/inputs/toggleswitch' },
              { text: 'ToggleButton', link: '/zh/documentation/controls/inputs/togglebutton' },
              { text: 'Slider', link: '/zh/documentation/controls/inputs/slider' },
              { text: 'ComboBox', link: '/zh/documentation/controls/inputs/combobox' },
              { text: 'NumericUpDown', link: '/zh/documentation/controls/inputs/numericupdown' },
              { text: 'TextBox', link: '/zh/documentation/controls/inputs/textbox' },
              { text: 'CheckBox', link: '/zh/documentation/controls/inputs/checkbox' },
              { text: 'RadioButton', link: '/zh/documentation/controls/inputs/radiobutton' },
              { text: 'ContextMenu', link: '/zh/documentation/controls/inputs/contextmenu' },
            ]
          },
          {
            text: 'Text',
            items: [
              { text: 'AutoCompleteBox', link: '/zh/documentation/controls/text/autocompletebox' },
              { text: 'TextBox', link: '/zh/documentation/controls/text/textbox' },
            ]
          },
          {
            text: 'Date & Time',
            items: [
              { text: 'Calendar', link: '/zh/documentation/controls/datetime/calendar' },
              { text: 'DatePicker', link: '/zh/documentation/controls/datetime/datepicker' },
            ]
          },
          {
            text: 'Progress',
            items: [
              { text: 'WaveProgress', link: '/zh/documentation/controls/progress/waveprogress' },
              { text: 'Stepper', link: '/zh/documentation/controls/progress/stepper' },
              { text: 'CircleProgressBar', link: '/zh/documentation/controls/progress/circleprogressbar' },
              { text: 'Loading', link: '/zh/documentation/controls/progress/loading' },
              { text: 'ProgressBar', link: '/zh/documentation/controls/progress/progressbar' },
            ]
          },
          {
            text: 'Data Presentation',
            items: [
              { text: 'DataGrid', link: '/zh/documentation/controls/data/datagrid' },
              { text: 'ListBox', link: '/zh/documentation/controls/data/listbox' },
              { text: 'TreeView', link: '/zh/documentation/controls/data/treeview' },
              { text: 'GroupBox', link: '/zh/documentation/controls/data/groupbox' },
              { text: 'BusyArea', link: '/zh/documentation/controls/data/busyarea' },
              { text: 'Expander', link: '/zh/documentation/controls/data/expander' },
            ]
          },
          {
            text: 'System',
            items: [
              { text: 'FilePicker', link: '/zh/documentation/controls/system/filepicker' },
            ]
          }
        ]
      },
      {
        text: '通知',
        items: [
          { text: 'Dialog', link: '/zh/documentation/notification/dialog' },
          { text: 'Toast', link: '/zh/documentation/notification/toast' },
          { text: 'MessageBox', link: '/zh/documentation/notification/messagebox' },
          { text: 'InfoBar', link: '/zh/documentation/notification/infobar' },
          { text: 'InfoBadge', link: '/zh/documentation/notification/infobadge' },
        ]
      },
      {
        text: '样式',
        items: [
          { text: 'Text', link: '/zh/documentation/style/text' },
          { text: 'Color', link: '/zh/documentation/style/color' },
          { text: 'Icon', link: '/zh/documentation/style/icon' },
        ]
      }
    ]
  }
})
