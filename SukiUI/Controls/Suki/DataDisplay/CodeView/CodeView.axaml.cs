using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using SukiUI.Content;
using SukiUI.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SukiUI.Controls;

public partial class CodeView : UserControl
{
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private string _text = "";
    private readonly TextBlock _textBlock;
    private readonly TextBlock _lineNumberTextBlock;
    private readonly Grid _codeGrid;

    public CodeView()
    {
        InitializeComponent();
        _codeGrid = new Grid();
        _codeGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _codeGrid.ColumnDefinitions.Add(new ColumnDefinition());

        _lineNumberTextBlock = new TextBlock()
        {
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Margin = new Thickness(10, 5, 5, 0),
            Foreground = Brushes.Gray
        };
        Grid.SetColumn(_lineNumberTextBlock, 0);

        _textBlock = new TextBlock()
        {
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Margin = new Thickness(10, 5, 5, 0),
            FontFamily = new FontFamily("Consolas")
        };
        Grid.SetColumn(_textBlock, 1);

        _codeGrid.Children.Add(_lineNumberTextBlock);
        _codeGrid.Children.Add(_textBlock);

        var gridcontent = new Grid();
        gridcontent.Children.Add(
            new PathIcon()
            {
                Data = Icons.ChevronRight,
                Classes = { "Flippable" },
                Foreground =
                    new Avalonia.Media.SolidColorBrush(
                        (Avalonia.Media.Color)Application.Current.FindRequiredResource("SukiText")),
                Height = 15,
                Width = 15,
            });

        gridcontent.Children.Add(new TextBlock() { IsVisible = false, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, FontSize = 12, Text = "Copied !" });

        var button = new Button()
        {
            Classes = { "Accent" },
            Content = gridcontent,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
        };

        button.Click += async (sender, args) =>
        {
            var topLevel = TopLevel.GetTopLevel(this);

            if (topLevel?.Clipboard is not { } clipboard)
            {
                return;
            }

            await clipboard.SetTextAsync(Text);

            Dispatcher.UIThread.Invoke(() =>
            {
                gridcontent.Children[0].IsVisible = false;
                gridcontent.Children[1].IsVisible = true;
            });

            await Task.Delay(3000);
            Dispatcher.UIThread.Invoke(() =>
            {
                gridcontent.Children[0].IsVisible = true;
                gridcontent.Children[1].IsVisible = false;
            });
        };

        Grid.SetColumn(button, 1);
        _codeGrid.Children.Add(button);

        Content = _codeGrid;
        UpdateText();
    }

    public string Text
    {
        get => _text;
        set
        {
            if (SetAndRaise(TextProperty, ref _text, value ?? string.Empty))
                UpdateText();
        }
    }

    public static readonly DirectProperty<CodeView, string> TextProperty =
        AvaloniaProperty.RegisterDirect<CodeView, string>(
            nameof(Text),
            o => o.Text,
            (o, v) => o.Text = v,
            defaultBindingMode: BindingMode.OneWay,
            enableDataValidation: true);

    private void UpdateText()
    {
        var lines = _text.Split('\n');
        var lineNumberText = string.Join('\n', Enumerable.Range(1, lines.Length)) + '\n';

        _codeGrid.RowDefinitions.Clear();
        for (var i = 0; i < lines.Length; i++)
        {
            _codeGrid.RowDefinitions.Add(new RowDefinition());
        }

        _codeGrid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Auto);
        _codeGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
        Grid.SetRowSpan(_lineNumberTextBlock, lines.Length);
        _lineNumberTextBlock.Text = lineNumberText;
        Grid.SetColumn(_textBlock, 1);
        Grid.SetRow(_textBlock, 0);
        Grid.SetRowSpan(_textBlock, lines.Length);
        _textBlock.Text = _text.TrimEnd();
    }
}
