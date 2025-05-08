using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using SukiUI.MessageBox;

namespace SukiUI.Controls;

[TemplatePart("PART_AlternativeHeaderGrid", typeof(Grid))]
[TemplatePart("PART_AlternativeIcon", typeof(ContentPresenter))]
[TemplatePart("PART_AlternativeHeader", typeof(ContentPresenter))]
[TemplatePart("PART_HeaderGrid", typeof(Grid))]
[TemplatePart("PART_Icon", typeof(ContentPresenter))]
[TemplatePart("PART_Header", typeof(ContentPresenter))]
[TemplatePart("PART_Content", typeof(ScrollViewer))]
[TemplatePart("PART_FooterGrid", typeof(Grid))]
[TemplatePart("PART_LeftContentItems", typeof(ItemsControl))]
[TemplatePart("PART_ActionButtons", typeof(ItemsControl))]
public class SukiMessageBoxHost : HeaderedContentControl
{
    private const int DefaultItemsSpacing = 10;
    private const double DefaultIconPresetSize = 24;


    /// <summary>
    /// Defines the <see cref="UseAlternativeHeaderStyle"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> UseAlternativeHeaderStyleProperty = AvaloniaProperty.Register<SukiMessageBoxHost, bool>(nameof(UseAlternativeHeaderStyle));

    /// <summary>
    /// Gets or sets a value indicating whether to use the alternative header style.
    /// </summary>
    public bool UseAlternativeHeaderStyle
    {
        get => GetValue(UseAlternativeHeaderStyleProperty);
        set => SetValue(UseAlternativeHeaderStyleProperty, value);
    }

    public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<SukiMessageBoxHost, object?>(nameof(Icon));

    /// <summary>
    /// Gets or sets the icon content to display on the header.
    /// </summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="IconPreset"/> property.
    /// </summary>
    public static readonly StyledProperty<SukiMessageBoxIcons?> IconPresetProperty =
        AvaloniaProperty.Register<SukiMessageBoxHost, SukiMessageBoxIcons?>(nameof(IconPreset));

    /// <summary>
    /// Gets or sets the preset icon to display on the header.
    /// </summary>
    public SukiMessageBoxIcons? IconPreset
    {
        get => GetValue(IconPresetProperty);
        set => SetValue(IconPresetProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="IconPresetSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> IconPresetSizeProperty =
        AvaloniaProperty.Register<SukiMessageBoxHost, double>(nameof(IconPresetSize), DefaultIconPresetSize);

    /// <summary>
    /// Gets or sets the size of the preset icon.
    /// </summary>
    public double IconPresetSize
    {
        get => GetValue(IconPresetSizeProperty);
        set => SetValue(IconPresetSizeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ShowHeaderContentSeparator"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowHeaderContentSeparatorProperty =
        AvaloniaProperty.Register<SukiMessageBoxHost, bool>(nameof(ShowHeaderContentSeparator));

    /// <summary>
    /// Gets or sets a value indicating whether to show the header/content separator.
    /// </summary>
    /// <remarks>Only visible if <see cref="UseAlternativeHeaderStyle"/> is <c>false</c>.</remarks>
    public bool ShowHeaderContentSeparator
    {
        get => GetValue(ShowHeaderContentSeparatorProperty);
        set => SetValue(ShowHeaderContentSeparatorProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="FooterLeftItemsSource"/> property.
    /// </summary>
    public static readonly StyledProperty<Avalonia.Controls.Controls?> FooterLeftItemsSourceProperty =
        AvaloniaProperty.Register<SukiMessageBoxHost, Avalonia.Controls.Controls?>(nameof(FooterLeftItemsSource));

    /// <summary>
    /// Gets or sets the items source to display in the footer left of the message box
    /// </summary>
    public Avalonia.Controls.Controls? FooterLeftItemsSource
    {
        get => GetValue(FooterLeftItemsSourceProperty);
        set => SetValue(FooterLeftItemsSourceProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ActionButtonsSource"/> property.
    /// </summary>
    public static readonly StyledProperty<AvaloniaList<Button>?> ActionButtonsSourceProperty =
        AvaloniaProperty.Register<SukiMessageBoxHost, AvaloniaList<Button>?>(nameof(ActionButtonsSource));

    /// <summary>
    /// Gets or sets the action buttons to display in bottom right of the message box.
    /// </summary>
    public AvaloniaList<Button>? ActionButtonsSource
    {
        get => GetValue(ActionButtonsSourceProperty);
        set => SetValue(ActionButtonsSourceProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ActionButtonsPreset"/> property.
    /// </summary>
    public static readonly StyledProperty<SukiMessageBoxButtons?> ActionButtonsPresetProperty =
        AvaloniaProperty.Register<SukiMessageBoxHost, SukiMessageBoxButtons?>(nameof(ActionButtonsPreset));

    /// <summary>
    /// Gets or sets the action buttons to display in bottom right of the message box.
    /// </summary>
    public SukiMessageBoxButtons? ActionButtonsPreset
    {
        get => GetValue(ActionButtonsPresetProperty);
        set => SetValue(ActionButtonsPresetProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ItemsSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<int> ItemsSpacingProperty =
        AvaloniaProperty.Register<SukiMessageBoxHost, int>(nameof(ItemsSpacing), DefaultItemsSpacing);

    /// <summary>
    /// Gets or sets the spacing between the items (<see cref="FooterLeftItemsSource"/> and <see cref="ActionButtonsSource"/>) in the message box.
    /// </summary>
    public int ItemsSpacing
    {
        get => GetValue(ItemsSpacingProperty);
        set => SetValue(ItemsSpacingProperty, value);
    }

    public SukiMessageBoxHost()
    {
        FooterLeftItemsSource = [];
        ActionButtonsSource = [];
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (ReferenceEquals(e.Property, IconPresetProperty) ||
            ReferenceEquals(e.Property, IconPresetSizeProperty))
        {
            var preset = IconPreset;
            if (preset is null) return;

            Icon = SukiMessageBoxIconsFactory.CreateIcon(preset.Value, IconPresetSize);
        }
        else if (ReferenceEquals(e.Property, ActionButtonsPresetProperty))
        {
            var preset = ActionButtonsPreset;
            if (preset is null) return;

            Button[] buttons = preset switch
            {
                SukiMessageBoxButtons.OK =>
                [
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.OK)
                ],
                SukiMessageBoxButtons.OKCancel =>
                [
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.OK),
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Cancel)
                ],
                SukiMessageBoxButtons.YesNo =>
                [
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Yes),
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.No)
                ],
                SukiMessageBoxButtons.YesNoCancel =>
                [
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Yes),
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.No),
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Cancel)
                ],
                SukiMessageBoxButtons.YesIgnore =>
                [
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Yes),
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Ignore)
                ],
                SukiMessageBoxButtons.ApplyCancel =>
                [
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Apply),
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Cancel)
                ],
                SukiMessageBoxButtons.RetryCancel =>
                [
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Retry),
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Cancel)
                ],
                SukiMessageBoxButtons.RetryIgnoreAbort =>
                [
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Retry),
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Ignore),
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Abort)
                ],
                SukiMessageBoxButtons.RetryContinueCancel =>
                [
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Retry),
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Continue),
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Cancel)
                ],
                SukiMessageBoxButtons.Close =>
                [
                    SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Close)
                ],
                _ => throw new ArgumentOutOfRangeException(nameof(preset), preset, null)
            };

            if (ActionButtonsSource is null)
            {
                ActionButtonsSource = new AvaloniaList<Button>(buttons);
            }
            else
            {
                ActionButtonsSource.Clear();
                ActionButtonsSource.AddRange(buttons);
            }
        }
    }

    public void ResetToDefaults()
    {
        UseAlternativeHeaderStyle = false;
        ShowHeaderContentSeparator = false;

        Icon = null;
        IconPreset = null;
        IconPresetSize = DefaultIconPresetSize;
        Content = null;

        FooterLeftItemsSource?.Clear();
        ActionButtonsSource?.Clear();
        ActionButtonsPreset = null;

        ItemsSpacing = DefaultItemsSpacing;
    }
}