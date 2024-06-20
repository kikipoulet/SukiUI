using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SkiaSharp;

namespace SukiUI.Utilities.Background
{
    /// <summary>
    /// Represents a shader used to render the background of a window.
    /// The uniforms available will be injected automatically into every shader and these are the only ones that will be available.
    /// float iTime | vec3 iResolution | vec3 iPrimary | vec3 iAccent | vec3 iBase
    /// </summary>
    public class SukiBackgroundEffect : IDisposable
    {
        private const string Uniforms = "uniform float iTime;        // Scaled shader playback time (s)\n" +
                                        "uniform float iDark      ;   // DarkTheme\n" +
                                        "uniform vec3 iResolution;   // Viewport resolution (pixels)\n" +
                                        "uniform vec3 iPrimary;      // Currently active primary color\n" +
                                        "uniform vec3 iAccent;       // Currently active accent color\n" +
                                        "uniform vec3 iBase;         // Currently active base color\n";

        private readonly string _effectString;
        internal SKRuntimeEffect Effect => SKRuntimeEffect.Create(_effectString, out _);

        private SukiBackgroundEffect(string content)
        {
            _effectString = content;
        }

        /// <summary>
        /// Attempts to load a ".sksl" shader file from the assembly.
        /// You don't need to provide the extension.
        /// REMEMBER: For files to be discoverable in the assembly they should be marked as an embedded resource.
        /// </summary>
        /// <param name="shaderName">Name of the shader to load.</param>
        /// <returns>An instance of a SukiBackgroundShader with the loaded shader.</returns>
        public static SukiBackgroundEffect FromEmbeddedResource(string shaderName)
        {
            shaderName = shaderName.ToLowerInvariant();
            if (!shaderName.EndsWith(".sksl"))
                shaderName += ".sksl";
            var assembly = Assembly.GetEntryAssembly();
            var resName = assembly!.GetManifestResourceNames()
                .FirstOrDefault(x => x.ToLowerInvariant().Contains(shaderName));
            if (resName is null)
            {
                assembly = Assembly.GetExecutingAssembly();
                resName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(x => x.ToLowerInvariant().Contains(shaderName));
            }
            if (resName is null)
                throw new FileNotFoundException(
                    $"Unable to find a file with the name \"{shaderName}\" anywhere in the assembly.");
            using var tr = new StreamReader(assembly.GetManifestResourceStream(resName)!);
            return FromString(tr.ReadToEnd());
        }
        
        
        public static SukiBackgroundEffect FromString(string shaderString)
        {
            var withUniforms = Uniforms + shaderString;
            if (SKRuntimeEffect.Create(withUniforms, out var errors) is null) // used to check for compiler errors.
                throw new SKShaderCompileException(errors);
            return new SukiBackgroundEffect(withUniforms);
        }

        public void Dispose()
        {
            Effect.Dispose();
        }

        public class SKShaderCompileException : Exception
        {
            public SKShaderCompileException(string message) : base(message)
            {
            }
        }
    }
}