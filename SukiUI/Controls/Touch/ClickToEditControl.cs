using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using SukiUI.Dialogs;
using SukiUI.MessageBox;

namespace SukiUI.Controls.Touch;

public class ClickToEditTextControl : UserControl
    {
        public static readonly StyledProperty<ISukiDialogManager> DialogManagerProperty =
            AvaloniaProperty.Register<ClickToEditTextControl, ISukiDialogManager>(
                nameof(DialogManager));
        
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<ClickToEditTextControl, string>(
                nameof(Text), "Click to edit …");

        public static readonly StyledProperty<string> SubtitleProperty =
            AvaloniaProperty.Register<ClickToEditTextControl, string>(
                nameof(Subtitle), "Subtitle");

        public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
            AvaloniaProperty.Register<ClickToEditTextControl, CornerRadius>(
                nameof(CornerRadius), new CornerRadius(15));

        public ISukiDialogManager DialogManager
        {
            get => GetValue(DialogManagerProperty);
            set => SetValue(DialogManagerProperty, value);
        }
        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Subtitle
        {
            get => GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => GetValue(CornerRadiusProperty);
            set
            {
                SetValue(CornerRadiusProperty, value);
                rootBorder.CornerRadius = value;
            }
        }

        private GlassCard rootBorder;
        public ClickToEditTextControl()
        {
  
            rootBorder = new GlassCard()
            {
                BorderThickness = new Thickness(0)
            };

           // rootBorder.Bind(Border.CornerRadiusProperty, this.GetObservable(CornerRadiusProperty));

            var dockPanel = new DockPanel
            {
                Margin = new Thickness(15, 9)
            };

            var iconBorder = new Border
            {
                Margin = new Thickness(0, 0, 55, 2),
                Height = 50,
                Width = 50,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };


            iconBorder.Child = new PathIcon()
            {
                Data = SukiUI.Content.Icons.Pencil, Height = 30, Width = 30, 
                HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(15,0,-15,0),
                VerticalAlignment = VerticalAlignment.Center, 
                [!BorderBrushProperty] = new DynamicResourceExtension("SukiLowText"),
                Opacity = 0.8
            };

      
            var textStack = new StackPanel
            {
                Spacing = 12
            };

            var mainTextBlock = new TextBlock
            {
                FontSize = 22, FontWeight = FontWeight.DemiBold
            };
            mainTextBlock.Bind(TextBlock.TextProperty, this.GetObservable(TextProperty));

            var subtitleTextBlock = new TextBlock
            {
                FontWeight = FontWeight.Light,
                FontSize = 18,
                [!ForegroundProperty] = new DynamicResourceExtension("SukiLowText"),
            };
            subtitleTextBlock.Bind(TextBlock.TextProperty, this.GetObservable(SubtitleProperty));

            textStack.Children.Add(mainTextBlock);
            textStack.Children.Add(subtitleTextBlock);

            DockPanel.SetDock(iconBorder, Dock.Left);
            dockPanel.Children.Add(iconBorder);
            dockPanel.Children.Add(textStack);
            rootBorder.Content = dockPanel;
            Content = rootBorder;

            Cursor = new Cursor(StandardCursorType.Hand);

            rootBorder.PointerPressed += (_, __) => OpenKeyboard();
        }

        private void OpenKeyboard()
        {
            var keyboard = new OnScreenKeyboard(Text, onDone: newText =>
            {

                SetCurrentValue(TextProperty, newText);
            }, DialogManager);


            DialogManager.CreateDialog().WithContent(keyboard).Dismiss().ByClickingBackground().TryShow(); 
        
        }
    }


    public class OnScreenKeyboard : UserControl
    {
        private readonly TextBox _input;
        private readonly Action<string> _onDone;
        private ISukiDialogManager _dialogManager;

        public OnScreenKeyboard(string initialText, Action<string> onDone, ISukiDialogManager dialogManager)
        {
            _dialogManager = dialogManager;
            _onDone = onDone ?? (_ => { });

            _input = new TextBox
            {
                Watermark = "Enter Your Text", VerticalAlignment = VerticalAlignment.Center,
                Text = initialText ?? string.Empty, FontSize = 40, Height = 75,
                Margin = new Thickness(12),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var root = new Border
            {
               
                Padding = new Thickness(15),
                Child = BuildLayout()
            };

            Content = root;
        }

        private Control BuildLayout()
        {
            var stack = new StackPanel
            {
                Spacing = 8
            };

            var dock = new DockPanel(){Margin = new Thickness(0,0,0,30)};


            var sp = new StackPanel(){Spacing = 55, Margin = new Thickness(0,0), Orientation = Orientation.Horizontal};
            sp.Children.Add(new TextBlock(){Foreground = Brushes.White, FontSize = 20, Text = "Done"});

            
            var okbutton = new Button
            {
                Content = sp, Margin = new Thickness(45,10,0,10), VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 126, Height = 75, Classes = { "Flat" }, CornerRadius = new CornerRadius(12)
            }.Also(b => b.Click += (_, __) =>
            {
                _onDone(_input.Text ?? string.Empty);
                _dialogManager.DismissDialog();
            });
            
            DockPanel.SetDock(okbutton, Dock.Right);
            
            dock.Children.Add(okbutton);
            dock.Children.Add(_input);
            
            stack.Children.Add(dock);
            
            _input.CaretIndex = _input.Text.Length;


            var rows = new[]
            {
                "QWERTYUIOP",
                 "ASDFGHJKL",
                  "ZXCVBNM"
            };

            foreach (var row in rows)
                stack.Children.Add(MakeKeyRow(row));
            
            var actions = new UniformGrid
            {
                Rows = 1,
                Columns = 4,
                Margin = new Thickness(0, 50, 0, 0)
            };
      

  

            return stack;
        }

        private Panel MakeKeyRow(string keys)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 0),
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            foreach (var ch in keys)
            {
                panel.Children.Add(MakeKeyButton(ch.ToString()));
            }
            return panel;
        }

        private Button MakeKeyButton(string label)
        {
            var btn = new Button
            {
                Content = new TextBlock(){Text = label, FontWeight = FontWeight.DemiBold, FontSize = 27}, Background = new SolidColorBrush(Color.FromArgb(20,50,50,50)),
                Margin = new Thickness(5),
                MinWidth = 120, CornerRadius = new CornerRadius(12),
                MinHeight = 120
            };

            btn.Click += (_, __) => InsertText(label);
            return btn;
        }

        private Button MakeActionButton(string label, Action action)
        {
            var btn = new Button
            {
                Content = label,
                Margin = new Thickness(2),
                MinHeight = 36
            };
            btn.Click += (_, __) => action();
            return btn;
        }

        private void InsertText(string s)
        {
            var text = _input.Text ?? string.Empty;
            var selStart = _input.CaretIndex;
            var selLength = _input.SelectionStart != _input.SelectionEnd
                ? Math.Abs(_input.SelectionEnd - _input.SelectionStart)
                : 0;

            if (selLength > 0)
            {
                var start = Math.Min(_input.SelectionStart, _input.SelectionEnd);
                text = text.Remove(start, selLength).Insert(start, s);
                _input.Text = text;
                _input.CaretIndex = start + s.Length;
            }
            else
            {
                text = text.Insert(selStart, s);
                _input.Text = text;
                _input.CaretIndex = selStart + s.Length;
            }
        }

        private void Backspace()
        {
            var text = _input.Text ?? string.Empty;
            if (_input.SelectionStart != _input.SelectionEnd)
            {
                var start = Math.Min(_input.SelectionStart, _input.SelectionEnd);
                var len = Math.Abs(_input.SelectionEnd - _input.SelectionStart);
                _input.Text = text.Remove(start, len);
                _input.CaretIndex = start;
            }
            else if (_input.CaretIndex > 0 && text.Length > 0)
            {
                var idx = _input.CaretIndex - 1;
                _input.Text = text.Remove(idx, 1);
                _input.CaretIndex = idx;
            }
        }
    }

    internal static class ObjectExtensions
    {
        public static T Also<T>(this T obj, Action<T> act)
        {
            act?.Invoke(obj);
            return obj;
        }
    }
