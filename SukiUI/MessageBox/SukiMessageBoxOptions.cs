using Avalonia.Controls;
using Avalonia.Media;
using SukiUI.Controls;
using SukiUI.Enums;

namespace SukiUI.MessageBox;

/// <summary>
/// Represents the options for a message box window.
/// </summary>
public record SukiMessageBoxOptions
{
    /// <summary>
    /// Gets if the window should use the native <see cref="Window"/>, otherwise it will use the <see cref="SukiWindow"/>.
    /// </summary>
    public bool UseNativeWindow { get; init; }

    /// <summary>
    /// Gets if the window can resize.
    /// </summary>
    public bool CanResize { get; init; }

    /// <summary>
    /// Gets if the window can minimize.
    /// </summary>
    public bool CanMinimize { get; init; } = true;

    /// <summary>
    /// Gets if the window can maximize.
    /// </summary>
    public bool CanMaximize { get; init; }

    /// <summary>
    /// Gets the size to content behavior of the window.
    /// </summary>
    public SizeToContent SizeToContent { get; init; } = SizeToContent.WidthAndHeight;

    /// <summary>
    /// Gets the window startup location.
    /// </summary>
    public WindowStartupLocation WindowStartupLocation { get; init; } = WindowStartupLocation.CenterOwner;

    /// <summary>
    /// Gets if the window can show in the taskbar.
    /// </summary>
    public bool ShowInTaskbar { get; init; } = false;

    /// <summary>
    /// Gets the flow direction of the window.
    /// </summary>
    public FlowDirection FlowDirection { get; init; }

    /// <summary>
    /// Gets the minimum width of the window.
    /// </summary>
    public double MinWidth { get; init; } = 400;

    /// <summary>
    /// Gets the minimum height of the window.
    /// </summary>
    public double MinHeight { get; init; } = 150;

    /// <summary>
    /// Gets the width of the window.
    /// </summary>
    public double Width { get; init; } = double.NaN;

    /// <summary>
    /// Gets the height of the window.
    /// </summary>
    public double Height { get; init; } = double.NaN;

    /// <summary>
    /// Gets the maximum width of the window as a ratio of the screen width.
    /// </summary>
    public double MaxWidthScreenRatio { get; init; } = 0.75;

    /// <summary>
    /// Gets the maximum height of the window as a ratio of the screen height.
    /// </summary>
    public double MaxHeightScreenRatio { get; init; } = 0.75;

    /// <summary>
    /// Gets if the title bar is visible.
    /// </summary>
    public bool IsTitleBarVisible { get; init; } = true;

    /// <summary>
    /// Gets the logo to display in the window title bar.
    /// </summary>
    public Control? LogoContent { get; init; }

    /// <summary>
    /// Gets the window title to display in the title har.
    /// </summary>
    public string Title { get; init; } = string.Empty;


    /// <inheritdoc cref="SukiBackground.AnimationEnabled"/>
    public bool? BackgroundAnimationEnabled { get; init; }


    /// <inheritdoc cref="SukiBackground.Style"/>
    public SukiBackgroundStyle? BackgroundStyle { get; init; }


    /// <inheritdoc cref="SukiBackground.ShaderFile"/>
    public string? BackgroundShaderFile { get; init; }


    /// <inheritdoc cref="SukiBackground.ShaderCode"/>
    public string? BackgroundShaderCode { get; init; }


    /// <inheritdoc cref="SukiBackground.TransitionsEnabled"/>
    public bool? BackgroundTransitionsEnabled { get; init; }

    /// <inheritdoc cref="SukiBackground.TransitionsEnabled"/>
    public double? BackgroundTransitionTime { get; init; }

    /// <inheritdoc cref="SukiBackground.ForceSoftwareRendering"/>
    public bool? BackgroundForceSoftwareRendering { get; init; }
}