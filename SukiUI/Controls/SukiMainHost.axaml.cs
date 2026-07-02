using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using SukiUI.Enums;

namespace SukiUI.Controls
{
    [TemplatePart("PART_Root", typeof(Panel), IsRequired = true)]
    [TemplatePart("PART_VisualLayerManager", typeof(VisualLayerManager), IsRequired = true)]
    [TemplatePart("PART_Background", typeof(SukiBackground), IsRequired = true)]
    public class SukiMainHost : ContentControl
    {
        // Background properties
        public static readonly StyledProperty<bool> BackgroundAnimationEnabledProperty =
            AvaloniaProperty.Register<SukiMainHost, bool>(nameof(BackgroundAnimationEnabled), false);

        public static readonly StyledProperty<SukiBackgroundStyle> BackgroundStyleProperty =
            AvaloniaProperty.Register<SukiMainHost, SukiBackgroundStyle>(nameof(BackgroundStyle),
                SukiBackgroundStyle.GradientSoft);

        public static readonly StyledProperty<string?> BackgroundShaderFileProperty =
            AvaloniaProperty.Register<SukiMainHost, string?>(nameof(BackgroundShaderFile));

        public static readonly StyledProperty<string?> BackgroundShaderCodeProperty =
            AvaloniaProperty.Register<SukiMainHost, string?>(nameof(BackgroundShaderCode));

        public static readonly StyledProperty<bool> BackgroundTransitionsEnabledProperty =
            AvaloniaProperty.Register<SukiMainHost, bool>(nameof(BackgroundTransitionsEnabled), false);

        public static readonly StyledProperty<double> BackgroundTransitionTimeProperty =
            AvaloniaProperty.Register<SukiMainHost, double>(nameof(BackgroundTransitionTime), 1.0);

        public static readonly StyledProperty<bool> BackgroundForceSoftwareRenderingProperty =
            AvaloniaProperty.Register<SukiMainHost, bool>(nameof(BackgroundForceSoftwareRendering));

        public static readonly StyledProperty<double> RenderScaleXProperty =
            AvaloniaProperty.Register<SukiMainHost, double>(nameof(RenderScaleX), 1.0, coerce: CoerceRenderScale);

        public static readonly StyledProperty<double> RenderScaleYProperty =
            AvaloniaProperty.Register<SukiMainHost, double>(nameof(RenderScaleY), 1.0, coerce: CoerceRenderScale);

        public static readonly StyledProperty<Avalonia.Controls.Controls> HostsProperty =
            AvaloniaProperty.Register<SukiMainHost, Avalonia.Controls.Controls>(nameof(Hosts));

        public SukiMainHost()
        {
            Hosts = [];
        }

        /// <inheritdoc cref="SukiBackground.AnimationEnabled"/>
        public bool BackgroundAnimationEnabled
        {
            get => GetValue(BackgroundAnimationEnabledProperty);
            set => SetValue(BackgroundAnimationEnabledProperty, value);
        }

        /// <inheritdoc cref="SukiBackground.Style"/>
        public SukiBackgroundStyle BackgroundStyle
        {
            get => GetValue(BackgroundStyleProperty);
            set => SetValue(BackgroundStyleProperty, value);
        }

        /// <inheritdoc cref="SukiBackground.ShaderFile"/>
        public string? BackgroundShaderFile
        {
            get => GetValue(BackgroundShaderFileProperty);
            set => SetValue(BackgroundShaderFileProperty, value);
        }

        /// <inheritdoc cref="SukiBackground.ShaderCode"/>
        public string? BackgroundShaderCode
        {
            get => GetValue(BackgroundShaderCodeProperty);
            set => SetValue(BackgroundShaderCodeProperty, value);
        }

        /// <inheritdoc cref="SukiBackground.TransitionsEnabled"/>
        public bool BackgroundTransitionsEnabled
        {
            get => GetValue(BackgroundTransitionsEnabledProperty);
            set => SetValue(BackgroundTransitionsEnabledProperty, value);
        }

        /// <inheritdoc cref="SukiBackground.TransitionTime"/>
        public double BackgroundTransitionTime
        {
            get => GetValue(BackgroundTransitionTimeProperty);
            set => SetValue(BackgroundTransitionTimeProperty, value);
        }

        /// <summary>
        /// Forces the background of the window to utilise software rendering.
        /// This prevents the use of any advanced effects or animations and provides only a flat background colour that changes with the theme.
        /// </summary>
        public bool BackgroundForceSoftwareRendering
        {
            get => GetValue(BackgroundForceSoftwareRenderingProperty);
            set => SetValue(BackgroundForceSoftwareRenderingProperty, value);
        }

        /// <summary>
        /// Gets or sets the horizontal scale factor for rendering.
        /// </summary>
        public double RenderScaleX
        {
            get => GetValue(RenderScaleXProperty);
            set => SetValue(RenderScaleXProperty, value);
        }

        /// <summary>
        /// Gets or sets the vertical scale factor for rendering.
        /// </summary>
        public double RenderScaleY
        {
            get => GetValue(RenderScaleYProperty);
            set => SetValue(RenderScaleYProperty, value);
        }

        /// <summary>
        /// These controls are displayed above all others and fill the entire window.
        /// You can include <see cref="SukiDialogHost"/> and <see cref="SukiToastHost"/> or create your own custom implementations.
        /// </summary>
        public Avalonia.Controls.Controls Hosts
        {
            get => GetValue(HostsProperty);
            set => SetValue(HostsProperty, value);
        }

        private static double CoerceRenderScale(AvaloniaObject property, double scaling)
        {
            return scaling switch
            {
                < 0.1 => 0.1,
                > 5.0 => 5.0,
                _ => scaling
            };
        }
    }
}
