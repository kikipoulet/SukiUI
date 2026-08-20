using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Styling;
using SkiaSharp;
using SukiUI.Models;

namespace SukiUI.Utilities.Effects
{
    public abstract class EffectDrawBase : CompositionCustomVisualHandler
    {
        public static readonly object StartAnimations = new(), StopAnimations = new(), 
            EnableForceSoftwareRendering = new(), DisableForceSoftwareRendering = new(),
            DisposeHandler = new();

        public readonly record struct BaseThemeChangedMessage(ThemeVariant Variant);

        public readonly record struct ColorThemeChangedMessage(SukiColorTheme Theme);
        
        private SukiEffect? _effect;
        private readonly Dictionary<ShaderCacheKey, SKShader> _shaderCache = new();

        public SukiEffect? Effect
        {
            get => _effect;
            set
            {
                var old = _effect;
                if (Equals(old, value)) return;
                _effect = value;
                InvalidateShaderCache();
                EffectChanged(old, _effect);
            }
        }

        private bool _animationEnabled;
        public bool AnimationEnabled
        {
            get => _animationEnabled;
            set
            {
                if (_animationEnabled == value) return;
                if (value) _animationTick.Start();
                else _animationTick.Stop();
                _animationEnabled = value;
                InvalidateShaderCache();
            }
        }
        
        public bool ForceSoftwareRendering { get; set; }

        protected float AnimationSpeedScale { get; set; } = 0.1f;
        
        protected ThemeVariant ActiveVariant { get; private set; }
        
        protected SukiColorTheme ActiveTheme { get; private set; }
        
        protected float AnimationSeconds => (float)_animationTick.Elapsed.TotalSeconds;
        
        private readonly Stopwatch _animationTick = new();
        private readonly bool _invalidateRect;
        private bool _isDisposed;

        protected EffectDrawBase(bool invalidateRect = true)
        {
            _invalidateRect = invalidateRect;
            var theme = SukiTheme.GetInstance();
            ActiveVariant = theme.ActiveBaseTheme;
            ActiveTheme = theme.ActiveColorTheme!;
        }

        public override void OnRender(ImmediateDrawingContext context)
        {
            // DisposeHandler frees the derived SKPaint and every cached shader, but it arrives as a
            // separate compositor commit from the visual removal, so a render can still be scheduled
            // after it.
            if (_isDisposed) return;
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature is null) throw new InvalidOperationException("Unable to lease Skia API");
            using var lease = leaseFeature.Lease();
            var rect = SKRect.Create((float)EffectiveSize.X, (float)EffectiveSize.Y);
            if(lease.GrContext is null || ForceSoftwareRendering) // GrContext is null whenever there is no hardware acceleration available
                RenderSoftware(lease.SkCanvas, rect);
            else
                Render(lease.SkCanvas, rect);
        }
        
        public override void OnMessage(object message)
        {
            if (message == StartAnimations)
            {
                AnimationEnabled = true;
                RegisterForNextAnimationFrameUpdate();
            }
            else if (message == StopAnimations)
            {
                AnimationEnabled = false;
            }
            else if (message == EnableForceSoftwareRendering)
            {
                ForceSoftwareRendering = true;
                Invalidate();
            }
            else if (message == DisableForceSoftwareRendering)
            {
                ForceSoftwareRendering = false;
                Invalidate();
            }
            else if (message == DisposeHandler)
            {
                Dispose();
            }
            else if (message is BaseThemeChangedMessage baseThemeChanged)
            {
                ActiveVariant = baseThemeChanged.Variant;
                InvalidateShaderCache();
                InvalidateTheme();
            }
            else if (message is ColorThemeChangedMessage colorThemeChanged)
            {
                ActiveTheme = colorThemeChanged.Theme;
                InvalidateShaderCache();
                InvalidateTheme();
            }
            else if (message is SukiEffect effect)
            {
                Effect = effect;
            }
        }

        public override void OnAnimationFrameUpdate()
        {
            if (!AnimationEnabled) return;
            if(_invalidateRect)
                Invalidate(GetRenderBounds());
            else
                Invalidate();
            RegisterForNextAnimationFrameUpdate();
        }

        //protected abstract void InvalidateInternal();

        /// <summary>
        /// Called every frame to render content.
        /// </summary>
        protected abstract void Render(SKCanvas canvas, SKRect rect);
        
        /// <summary>
        /// Called every frame whenever the app falls back to software rendering (or <see cref="ForceSoftwareRendering"/> is enabled)
        /// </summary>
        protected abstract void RenderSoftware(SKCanvas canvas, SKRect rect);

        protected SKShader? EffectWithUniforms(float alpha = 1f) => 
            EffectWithUniforms(Effect, alpha);

        protected SKShader? EffectWithUniforms(SukiEffect? effect, float alpha = 1f)
        {
            if (effect is null) return null;
            var bounds = GetRenderBounds();
            return EffectWithUniforms(effect, bounds, alpha);
        }

        protected SKShader? EffectWithUniforms(SukiEffect? effect, Rect bounds, float alpha = 1f)
        {
            if (effect is null) return null;
            if (AnimationEnabled)
                return effect.ToShaderWithUniforms(AnimationSeconds, ActiveVariant, bounds, AnimationSpeedScale, alpha);

            var key = CreateShaderCacheKey(effect, bounds, alpha);
            if (TryGetCachedShader(key, out var cached))
                return cached;
            PruneShaderCache((float)bounds.Width, (float)bounds.Height);
            return AddCachedShader(key,
                effect.ToShaderWithUniforms(AnimationSeconds, ActiveVariant, bounds, AnimationSpeedScale, alpha));
        }

        protected SKShader? EffectWithCustomUniforms(Func<SKRuntimeEffect,SKRuntimeEffectUniforms> uniformFactory, float alpha = 1f) =>
            EffectWithCustomUniforms(Effect, uniformFactory, alpha);
        
        protected SKShader? EffectWithCustomUniforms(SukiEffect? effect,
            Func<SKRuntimeEffect, SKRuntimeEffectUniforms> uniformFactory, float alpha = 1f)
        {
            if (effect is null) return null;
            var bounds = GetRenderBounds();
            return EffectWithCustomUniforms(effect, uniformFactory, bounds, alpha);
        }

        protected SKShader? EffectWithCustomUniforms(SukiEffect? effect,
            Func<SKRuntimeEffect, SKRuntimeEffectUniforms> uniformFactory, Rect bounds, float alpha = 1f)
        {
            return effect?.ToShaderWithCustomUniforms(uniformFactory, AnimationSeconds, bounds, AnimationSpeedScale, alpha);
        }

        /// <summary>
        /// Invalidates all cached shaders. Custom-uniform renderers should call this after changing
        /// values captured by their uniform factory.
        /// </summary>
        protected void InvalidateShaderCache()
        {
            foreach (var shader in _shaderCache.Values)
                shader.Dispose();
            _shaderCache.Clear();
        }

        protected void InvalidateShaderCache(SukiEffect effect)
        {
            // SukiEffect equality is by shader source and FromString hands out shared instances, so the
            // outgoing effect can be the very same object as the live one. Dropping its entries would
            // discard shaders the current effect is still hitting.
            if (Equals(_effect, effect)) return;

            var matchingKeys = new List<ShaderCacheKey>();
            foreach (var pair in _shaderCache)
            {
                if (Equals(pair.Key.Effect, effect))
                    matchingKeys.Add(pair.Key);
            }

            foreach (var key in matchingKeys)
            {
                _shaderCache[key].Dispose();
                _shaderCache.Remove(key);
            }
        }

        protected virtual void EffectChanged(SukiEffect? oldValue, SukiEffect? newValue)
        {
            // no-op
        }
        
        public virtual void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            AnimationEnabled = false;
            _animationTick.Stop();
            InvalidateShaderCache();
        }

        private ShaderCacheKey CreateShaderCacheKey(SukiEffect effect, Rect bounds, float alpha) =>
            new(effect, (float)bounds.Width, (float)bounds.Height, alpha);

        private bool TryGetCachedShader(ShaderCacheKey key, out SKShader shader) =>
            _shaderCache.TryGetValue(key, out shader!);

        private SKShader? AddCachedShader(ShaderCacheKey key, SKShader? shader)
        {
            // ToShader returns null when the uniforms do not satisfy the compiled effect. Caching that
            // would read back as a hit forever, and would throw on Dispose during invalidation.
            if (shader is null) return null;
            _shaderCache.Add(key, shader);
            return shader;
        }

        private void PruneShaderCache(float width, float height)
        {
            if (_shaderCache.Count == 0) return;

            var staleKeys = new List<ShaderCacheKey>();
            foreach (var pair in _shaderCache)
            {
                var key = pair.Key;
                // Entries for a size we no longer render can never be hit again. Without this, a handler
                // with AnimationEnabled == false (the SukiBackground default) retains one compiled shader
                // for every intermediate size a window passes through while being resized.
                var staleSize = key.Width != width || key.Height != height;
                if (staleSize)
                    staleKeys.Add(key);
            }

            foreach (var key in staleKeys)
            {
                _shaderCache[key].Dispose();
                _shaderCache.Remove(key);
            }
        }

        private readonly record struct ShaderCacheKey(SukiEffect Effect, float Width, float Height, float Alpha);

        private void InvalidateTheme()
        {
            if (_invalidateRect)
                Invalidate(GetRenderBounds());
            else
                Invalidate();
        }
        
        public virtual bool Equals(ICustomDrawOperation other) => false;
        
        public virtual bool HitTest(Point p) => false;
    }
}
