
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace SukiUI;

[TemplatePart("PART_Root", typeof(Border), IsRequired = true)]
[TemplatePart("PART_ContentPresenter", typeof(ContentPresenter), IsRequired = true)]
[TemplatePart("PART_Button", typeof(Button), IsRequired = true)]
public class TextResetButton : ContentControl
{
    private Button? _button;

    /// <summary>
    /// Identifies the IsDefaultText direct property for the TextResetButton control.
    /// </summary>
    /// <remarks>This field is used to reference the IsDefaultText property in property system operations,
    /// such as data binding or property change notifications. It is typically used by framework infrastructure and
    /// advanced scenarios.</remarks>
    public static readonly DirectProperty<TextResetButton, bool> IsDefaultTextProperty =
        AvaloniaProperty.RegisterDirect<TextResetButton, bool>(nameof(IsDefaultText), o => o.IsDefaultText);

    private bool _isDefaultText;

    public bool IsDefaultText
    {
        get => _isDefaultText;
        private set => SetAndRaise(IsDefaultTextProperty, ref _isDefaultText, value);
    }

    /// <summary>
    /// Identifies the DefaultText styled property.
    /// </summary>
    /// <remarks>This field is used to register and reference the DefaultText property on the TextResetButton
    /// control. It is typically used in property system operations such as data binding, styling, and
    /// animations.</remarks>
    public static readonly StyledProperty<string?> DefaultTextProperty =
        AvaloniaProperty.Register<TextResetButton, string?>(nameof(DefaultText), string.Empty);

    public string? DefaultText
    {
        get => GetValue(DefaultTextProperty);
        set => SetValue(DefaultTextProperty, value);
    }

    /// <summary>
    /// Identifies the Text dependency property for the TextResetButton control.
    /// </summary>
    /// <remarks>This field enables styling, data binding, animation, and default value support for the Text
    /// property on TextResetButton instances. It is typically used when interacting with the Avalonia property
    /// system.</remarks>
    public static readonly StyledProperty<string?> TextProperty = TextBox.TextProperty.AddOwner<TextResetButton>();


    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_button is not null)
        {
            _button.Click -= Button_OnClick;
        }

        _button = e.NameScope.Find<Button>("PART_Button");

        if (_button is not null)
        {
            _button.Click += Button_OnClick;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsDefaultTextProperty)
        {
            IsEnabled = !IsDefaultText;
        }
        else if (e.Property == DefaultTextProperty
            || e.Property == TextProperty)
        {
            IsDefaultText = Text == DefaultText;
        }
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(TextProperty, DefaultText);
    }
}