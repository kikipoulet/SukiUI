using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using SukiUI.Enums;

namespace SukiUI.CompactnessTheme
{
    /// <summary>
    /// Builds the resource layer of one <see cref="SukiCompactness"/> level from the per-control
    /// <c>*.Compactness.axaml</c> files. Each file is a dictionary of keyed dictionaries, one per level:
    /// <c>Normal</c> carries every key; the other levels only list what differs from it. The layer is
    /// always the Normal values with the requested level written over them, so a key a level leaves
    /// out falls back to Normal instead of going missing.
    /// </summary>
    internal static class SukiCompactnessResources
    {
        private const string BaseUri = "avares://SukiUI/";

        /// <summary>Every control that takes part in the system: one line per control.</summary>
        private static readonly string[] Sources =
        [
            "Controls/Standard/Buttons/Button/Button.Compactness.axaml",
        ];

        private static List<ResourceDictionary>? _loaded;

        internal static IReadOnlyList<string> SourcePaths => Sources;

        /// <summary>The loaded per-control files (a dictionary of level dictionaries each).</summary>
        internal static IReadOnlyList<ResourceDictionary> Files => _loaded ??= Load();

        internal static List<KeyValuePair<object, object?>> Build(SukiCompactness level)
        {
            var values = new Dictionary<object, object?>();
            foreach (var file in Files)
            {
                Overlay(values, file, SukiCompactness.Normal);
                if (level != SukiCompactness.Normal)
                    Overlay(values, file, level);
            }
            return [.. values];
        }

        internal static ResourceDictionary? Level(ResourceDictionary file, SukiCompactness level) =>
            file.TryGetResource(level.ToString(), null, out var value) ? value as ResourceDictionary : null;

        private static void Overlay(Dictionary<object, object?> values, ResourceDictionary file, SukiCompactness level)
        {
            if (Level(file, level) is not { } dictionary) return;
            foreach (var pair in dictionary)
                values[pair.Key] = pair.Value;
        }

        private static List<ResourceDictionary> Load()
        {
            var files = new List<ResourceDictionary>(Sources.Length);
            foreach (var source in Sources)
            {
                var include = new ResourceInclude(new Uri(BaseUri)) { Source = new Uri(BaseUri + source) };
                files.Add((ResourceDictionary)include.Loaded);
            }
            return files;
        }
    }
}
