using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using SukiUI.Enums;
using SukiUI.Utilities.Effects;

namespace SukiUI.Controls
{
    public class SukiBackground : Control
    {
        public static readonly StyledProperty<SukiBackgroundStyle> StyleProperty =
            AvaloniaProperty.Register<SukiWindow, SukiBackgroundStyle>(nameof(Style),
                defaultValue: SukiBackgroundStyle.Bubble);

        /// <summary>
        /// Which of the default background styles to use - DEFAULT: Gradient
        /// </summary>
        public SukiBackgroundStyle Style
        {
            get => GetValue(StyleProperty);
            set => SetValue(StyleProperty, value);
        }

        public static readonly StyledProperty<string?> ShaderFileProperty =
            AvaloniaProperty.Register<SukiWindow, string?>(nameof(ShaderFile));

        /// <summary>
        /// Specify a filename of an EMBEDDED RESOURCE file of type `.SkSL` with or without extension and it will be loaded and displayed.
        /// This takes priority over the <see cref="ShaderCode"/> property, which in turns takes priority over <see cref="Style"/>.
        /// </summary>
        public string? ShaderFile
        {
            get => GetValue(ShaderFileProperty);
            set => SetValue(ShaderFileProperty, value);
        }

        public static readonly StyledProperty<string?> ShaderCodeProperty =
            AvaloniaProperty.Register<SukiWindow, string?>(nameof(ShaderCode));

        /// <summary>
        /// Specify the shader code to use directly, simpler if you don't want to create an .SkSL file or want to generate the shader effect at runtime in some way.
        /// This takes priority over the <see cref="Style"/> property, but is second in priority to <see cref="ShaderFile"/> if it is set.
        /// </summary>
        public string? ShaderCode
        {
            get => GetValue(ShaderCodeProperty);
            set => SetValue(ShaderCodeProperty, value);
        }

        public static readonly StyledProperty<bool> AnimationEnabledProperty =
            AvaloniaProperty.Register<SukiWindow, bool>(nameof(AnimationEnabled), defaultValue: false);
        
        /// <summary>
        /// Enables/disables animations - DEFAULT: False
        /// </summary>
        public bool AnimationEnabled
        {
            get => GetValue(AnimationEnabledProperty);
            set => SetValue(AnimationEnabledProperty, value);
        }

        public static readonly StyledProperty<bool> TransitionsEnabledProperty =
            AvaloniaProperty.Register<SukiBackground, bool>(nameof(TransitionsEnabled), defaultValue: false);
        
        /// <summary>
        /// Enables/disables transition animations when switching backgrounds - DEFAULT: False
        /// </summary>
        public bool TransitionsEnabled
        {
            get => GetValue(TransitionsEnabledProperty);
            set => SetValue(TransitionsEnabledProperty, value);
        }

        public static readonly StyledProperty<double> TransitionTimeProperty =
            AvaloniaProperty.Register<SukiBackground, double>(nameof(TransitionTime), defaultValue: 1.0);
        
        /// <summary>
        /// The amount of time in seconds the background transition will take - DEFAULT: 1.0
        /// </summary>
        public double TransitionTime
        {
            get => GetValue(TransitionTimeProperty);
            set => SetValue(TransitionTimeProperty, value);
        }

        public static readonly StyledProperty<bool> ForceSoftwareRenderingProperty = AvaloniaProperty.Register<SukiBackground, bool>(nameof(ForceSoftwareRendering));

        public bool ForceSoftwareRendering
        {
            get => GetValue(ForceSoftwareRenderingProperty);
            set => SetValue(ForceSoftwareRenderingProperty, value);
        }
        
        private readonly EffectBackgroundDraw _draw;

        public SukiBackground()
        {
            IsHitTestVisible = false;
            _draw = new EffectBackgroundDraw(new Rect(0, 0, Bounds.Width, Bounds.Height));
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            _draw.ForceSoftwareRendering = ForceSoftwareRendering;
            _draw.TransitionsEnabled = TransitionsEnabled;
            _draw.TransitionTime = TransitionTime;
            _draw.AnimationEnabled = AnimationEnabled;
            HandleBackgroundStyleChanges();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ForceSoftwareRenderingProperty && change.NewValue is bool forceSoftwareRendering)
                _draw.ForceSoftwareRendering = forceSoftwareRendering;
            else if(change.Property == TransitionsEnabledProperty && change.NewValue is bool transitionEnabled)
                _draw.TransitionsEnabled = transitionEnabled;
            else if(change.Property == TransitionTimeProperty && change.NewValue is double transitionTime)
                _draw.TransitionTime = transitionTime;
            else if(change.Property == AnimationEnabledProperty && change.NewValue is bool animationEnabled)
                _draw.AnimationEnabled = animationEnabled;
            else if(change.Property == StyleProperty || change.Property == ShaderFileProperty || change.Property == ShaderCodeProperty)
                HandleBackgroundStyleChanges();
        }

        public override void Render(DrawingContext context)
        {
            _draw.Bounds = Bounds;
            context.Custom(_draw);
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }

        private void HandleBackgroundStyleChanges()
        {
            if (ShaderFile is not null)
                _draw.Effect = SukiEffect.FromEmbeddedResource(ShaderFile);
            else if (ShaderCode is not null)
                _draw.Effect = SukiEffect.FromString(ShaderCode);
            else
                _draw.Effect = SukiEffect.FromEmbeddedResource(Style.ToString());
        }
    }
}