using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using Avalonia.Styling;

namespace SukiUI.HyperGraphics;

/// <summary>Le catalogue de materiaux (voir <c>SkeumoMaterials.axaml</c>) : ressources sombre/clair que tout SVG nomme en <c>var(--SkeumoXxx)</c>.</summary>
public sealed partial class SkeumoMaterials : ResourceDictionary
{
    private static SkeumoMaterials? _catalog;

    /// <summary>Initialise une nouvelle instance du catalogue de matériaux Skeumo-Suki.</summary>
    public SkeumoMaterials() => AvaloniaXamlLoader.Load(this);

    /// <summary>Un materiau du catalogue pour le theme courant. Appeler depuis le thread UI.</summary>
    public static bool TryFind(string key, ThemeVariant theme, out object? value)
    {
        _catalog ??= new SkeumoMaterials();
        return _catalog.TryGetResource(key, theme, out value);
    }
}
