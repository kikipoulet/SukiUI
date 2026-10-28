using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using SukiUI.CompactnessTheme;
using SukiUI.Enums;

namespace SukiUI.Theme
{
    /// <summary>
    /// Sets the <see cref="SukiCompactness"/> level of one element and of everything inside it (its template, its
    /// content, its popups) over the global <see cref="SukiTheme.Compactness"/>; unset follows the level above.
    /// Works from a style too: <c>&lt;Setter Property="suki:Compactness.Level" Value="SuperCompact" /&gt;</c>.
    /// </summary>
    public static class Compactness
    {
        public static readonly AttachedProperty<SukiCompactness?> LevelProperty =
            AvaloniaProperty.RegisterAttached<StyledElement, SukiCompactness?>("Level", typeof(Compactness));

        static Compactness() => LevelProperty.Changed.AddClassHandler<StyledElement>(OnLevelChanged);

        public static SukiCompactness? GetLevel(StyledElement element) => element.GetValue(LevelProperty);

        public static void SetLevel(StyledElement element, SukiCompactness? value) => element.SetValue(LevelProperty, value);

        // Changed only fires on a real change: going back to null means a scope was installed, so Resources
        // already exists and reading it allocates nothing.
        private static void OnLevelChanged(StyledElement element, AvaloniaPropertyChangedEventArgs e)
        {
            var merged = element.Resources.MergedDictionaries;
            Scope? scope = null;
            foreach (var provider in merged)
                if (provider is Scope found) { scope = found; break; }

            if ((SukiCompactness?)e.NewValue is not { } level)
            {
                if (scope is not null) merged.Remove(scope);
            }
            else if (scope is null)
                merged.Add(new Scope(SukiCompactnessResources.Table(level)));
            else
                scope.Swap(SukiCompactnessResources.Table(level));
        }

        /// <summary>
        /// Answers the keys of one level from its shared table: one small object per scoped element, no copy of
        /// the values. A key the application overrides in Application.Resources is left to it, as it is over the
        /// global layer.
        /// </summary>
        private sealed class Scope(IReadOnlyDictionary<object, object?> values) : ResourceProvider
        {
            private IReadOnlyDictionary<object, object?> _values = values;

            public override bool HasResources => true;

            public override bool TryGetResource(object key, ThemeVariant? theme, out object? value)
            {
                if (_values.TryGetValue(key, out value) &&
                    Application.Current?.Resources.TryGetResource(key, theme, out _) != true)
                    return true;
                value = null;
                return false;
            }

            // a new level: one notification, the subtree's DynamicResources and motions read their sizes again
            internal void Swap(IReadOnlyDictionary<object, object?> values)
            {
                _values = values;
                RaiseResourcesChanged();
            }
        }
    }
}
