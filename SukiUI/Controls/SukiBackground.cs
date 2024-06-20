using System;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using SukiUI.Enums;
using SukiUI.Utilities.Background;

namespace SukiUI.Controls
{
    public class SukiBackground : Control
    {

        public static readonly StyledProperty<SukiBackgroundStyle> StyleProperty =
            AvaloniaProperty.Register<SukiWindow, SukiBackgroundStyle>(nameof(Style),
                defaultValue: SukiBackgroundStyle.Gradient);
        
        /// <summary>
        /// Which of the default background styles to use.
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

        public bool AnimationEnabled
        {
            get => GetValue(AnimationEnabledProperty);
            set => SetValue(AnimationEnabledProperty, value);
        }
        
        private readonly ShaderBackgroundDraw _draw;
        private SukiBackgroundEffect _effect;
        private readonly IDisposable _observables;
        
        public SukiBackground()
        {
            IsHitTestVisible = false;
            _draw = new ShaderBackgroundDraw(new Rect(0, 0, Bounds.Width, Bounds.Height));
            var bgStyleObs = this.GetObservable(StyleProperty)
                .Select(_ => Unit.Default);
            var bgShaderFileObs = this.GetObservable(ShaderFileProperty)
                .Select(_ => Unit.Default)
                .Merge(bgStyleObs);
            var bgShaderCodeObs = this.GetObservable(ShaderCodeProperty)
                .Select(_ => Unit.Default)
                .Merge(bgShaderFileObs)
                .Do(_ => HandleBackgroundStyleChanges())
                .ObserveOn(new AvaloniaSynchronizationContext());
            _observables = bgShaderCodeObs.Subscribe();
        }
        
        public override void Render(DrawingContext context)
        {
            _draw.Bounds = Bounds;
            _draw.Effect = _effect;
            _draw.AnimEnabled = AnimationEnabled;
            context.Custom(_draw);
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }
        
        private void HandleBackgroundStyleChanges()
        {
            if (ShaderFile is not null)
                _effect = SukiBackgroundEffect.FromEmbeddedResource(ShaderFile);
            else if (ShaderCode is not null) 
                _effect = SukiBackgroundEffect.FromString(ShaderCode);
            else
                _effect = SukiBackgroundEffect.FromEmbeddedResource(Style.ToString());
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            _observables.Dispose();
        }
    }
}