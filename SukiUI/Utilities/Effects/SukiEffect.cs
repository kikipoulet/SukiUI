using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Styling;
using SkiaSharp;
using SukiUI.Extensions;

namespace SukiUI.Utilities.Effects
{
    /// <summary>
    /// Represents an SKSL shader that SukiUI can handle and pass relevant uniforms into.
    /// Use the static methods <see cref="SukiEffect.FromEmbeddedResource"/> and <see cref="SukiEffect.FromString"/> for creation.
    /// </summary>
    public class SukiEffect
    {
        // Basic uniforms passed into the shader from the CPU.
        private static readonly string[] Uniforms =
        {
            "uniform float iTime;",
            "uniform float iDark;",
            "uniform float iAlpha;",
            "uniform vec3 iResolution;",
            "uniform vec3 iPrimary;",
            "uniform vec3 iAccent;",
            "uniform vec3 iBase;"
        };

        private static readonly List<SukiEffect> LoadedEffects = new();
        private static readonly Dictionary<string, SukiEffect> EffectCache = new(StringComparer.Ordinal);
        private static readonly object LifecycleLock = new();
        private static IControlledApplicationLifetime? _applicationLifetime;

        private readonly string _rawShaderString;
        private readonly string _shaderString;
        private readonly int _hashCode;

        /// <summary>
        /// The compiled <see cref="SKRuntimeEffect"/> that will actually be used in draw calls. 
        /// </summary>
        public SKRuntimeEffect Effect { get; }

        private SukiEffect(string shaderString, string rawShaderString)
        {
            _shaderString = shaderString;
            _rawShaderString = rawShaderString;
            _hashCode = StringComparer.Ordinal.GetHashCode(_shaderString);
            var compiledEffect = SKRuntimeEffect.CreateShader(_shaderString, out var errors);
            Effect = compiledEffect ?? throw new ShaderCompilationException(errors);
            lock (LifecycleLock)
                LoadedEffects.Add(this);
        }

        static SukiEffect()
        {
            AppDomain.CurrentDomain.ProcessExit += (_, _) => EnsureDisposed();
            EnsureExitSubscription();
        }

        /// <summary>
        /// Attempts to load and compile a ".sksl" shader file from the assembly.
        /// You don't need to provide the extension.
        /// The shader will be pre-compiled
        /// REMEMBER: For files to be discoverable in the assembly they should be marked as an embedded resource.
        /// </summary>
        /// <param name="shaderName">Name of the shader to load, with or without extension. - MUST BE .sksl</param>
        /// <returns>An instance of a SukiBackgroundShader with the loaded shader.</returns>
        public static SukiEffect FromEmbeddedResource(string shaderName)
        {
            shaderName = shaderName.ToLowerInvariant();
            if (!shaderName.EndsWith(".sksl"))
                shaderName += ".sksl";



            var assembly = Assembly.GetEntryAssembly();
            var resName = assembly?.GetManifestResourceNames()
                .FirstOrDefault(x => x.ToLowerInvariant().Contains(shaderName));
            
            if (resName is null)
            {
                assembly = Assembly.GetExecutingAssembly();
                resName = assembly?.GetManifestResourceNames()
                    .FirstOrDefault(x => x.ToLowerInvariant().Contains(shaderName));
            }

            if (resName is null)
            {
                assembly = typeof(SukiEffect).Assembly;
                resName = assembly?.GetManifestResourceNames()
                    .FirstOrDefault(x => x.ToLowerInvariant().Contains(shaderName));
            }
           
            if (resName is null)
                throw new FileNotFoundException(
                    $"Unable to find a file with the name \"{shaderName}\" anywhere in the assembly.");

            var resourceAssembly = assembly ?? typeof(SukiEffect).Assembly;
            using var stream = resourceAssembly.GetManifestResourceStream(resName)
                               ?? throw new FileNotFoundException(
                                   $"Unable to open the embedded shader resource \"{resName}\".");
            using var tr = new StreamReader(stream);
            return FromString(tr.ReadToEnd());
        }

        /// <summary>
        /// Attempts to compile an sksl shader from a string.
        /// The shader will be pre-compiled and any errors will be thrown as an exception.
        /// REMEMBER: For files to be discoverable in the assembly they should be marked as an embedded resource.
        /// </summary>
        /// <param name="shaderString">The shader code to be compiled.</param>
        /// <returns>An instance of a SukiBackgroundShader with the loaded shader</returns>
        public static SukiEffect FromString(string shaderString)
        {
            EnsureExitSubscription();

            lock (LifecycleLock)
            {
                if (EffectCache.TryGetValue(shaderString, out var cached))
                    return cached;

                var sb = new StringBuilder();
                foreach (var uniform in Uniforms)
                    sb.AppendLine(uniform);
                sb.Append(shaderString);
                var effect = new SukiEffect(sb.ToString(), shaderString);
                EffectCache.Add(shaderString, effect);
                return effect;
            }
        }

        private static void EnsureExitSubscription()
        {
            if (Application.Current?.ApplicationLifetime is not IControlledApplicationLifetime controlled)
                return;

            lock (LifecycleLock)
            {
                if (ReferenceEquals(_applicationLifetime, controlled))
                    return;

                _applicationLifetime = controlled;
                controlled.Exit += (_, _) => EnsureDisposed();
            }
        }


        private static bool _disposed;

        /// <summary>
        /// Necessary to make sure all the unmanaged effects are disposed.
        /// </summary>
        internal static void EnsureDisposed()
        {
            lock (LifecycleLock)
            {
                if (_disposed)
                    return;
                _disposed = true;
                foreach (var loaded in LoadedEffects)
                    loaded.Effect.Dispose();
                LoadedEffects.Clear();
                EffectCache.Clear();
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is not SukiEffect effect) return false;
            return effect._shaderString == _shaderString;
        }

        public override int GetHashCode() => _hashCode;

        private static readonly float[] White = { 0.95f, 0.95f, 0.95f };
        private readonly float[] _backgroundAlloc = new float[3];
        private readonly float[] _backgroundAccentAlloc = new float[3];
        private readonly float[] _backgroundPrimaryAlloc = new float[3];
        private readonly float[] _boundsAlloc = new float[3];

        internal SKShader ToShaderWithUniforms(float timeSeconds, ThemeVariant activeVariant, Rect bounds,
            float animationScale, float alpha = 1f)
        {
            var suki = SukiTheme.GetInstance();
            if(suki is null) throw new InvalidOperationException("No Suki Theme Instance is available.");
            if (suki.ActiveColorTheme is null) throw new InvalidOperationException("No ActiveColorTheme is available.");
            
            // Update allocated color arrays.
            suki.ActiveColorTheme.Background.ToFloatArrayNonAlloc(_backgroundAlloc);
            suki.ActiveColorTheme.BackgroundAccent.ToFloatArrayNonAlloc(_backgroundAccentAlloc);
            suki.ActiveColorTheme.BackgroundPrimary.ToFloatArrayNonAlloc(_backgroundPrimaryAlloc);
            _boundsAlloc[0] = (float)bounds.Width;
            _boundsAlloc[1] = (float)bounds.Height;
            
            using var inputs = new SKRuntimeEffectUniforms(Effect)
            {
                { "iResolution", _boundsAlloc },
                { "iTime", timeSeconds * animationScale },
                {
                    "iBase",
                    activeVariant == ThemeVariant.Dark
                        ? _backgroundAlloc
                        : White
                },
                { "iAccent", _backgroundAccentAlloc },
                { "iPrimary", _backgroundPrimaryAlloc },
                { "iDark", activeVariant == ThemeVariant.Dark ? 1f : 0f },
                { "iAlpha", alpha }
            };
           
            return Effect.ToShader(inputs);
        }

        internal SKShader ToShaderWithCustomUniforms(Func<SKRuntimeEffect,SKRuntimeEffectUniforms> uniformFactory, float timeSeconds, Rect bounds,
            float animationScale, float alpha = 1f)
        {
            using var uniforms = uniformFactory(Effect);
            uniforms.Add("iResolution", new SKPoint3((float)bounds.Width, (float)bounds.Height, 0f));
            uniforms.Add("iTime", timeSeconds * animationScale);
            uniforms.Add("iAlpha", alpha);
            return Effect.ToShader(uniforms);
        }
        
        /// <summary>
        /// Returns the pure shader string without uniforms.
        /// </summary>
        public override string ToString()
        {
            return _rawShaderString;
        }

        private class ShaderCompilationException : Exception
        {
            public ShaderCompilationException(string message) : base(message)
            {
            }
        }
    }
}
