using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SukiUI.Enums;
using SukiUI.Motion;

namespace SukiUI.CompactnessTheme
{
    /// <summary>
    /// How code, not a template, reads the sizes of the current <see cref="SukiCompactness"/> level (a motion
    /// that aims at a size no visual carries yet) and reacts when the level changes.
    /// </summary>
    internal static class SukiCompactnessExtensions
    {
        /// <summary>
        /// The value of a compactness key for this element's level. A level that does not list the key
        /// falls back on Normal, exactly as the resource layer does; a key that no file declares is a
        /// mistake and throws.
        /// </summary>
        internal static T GetCompactness<T>(this StyledElement owner, string key)
        {
            if (owner.TryFindResource(key, owner.ActualThemeVariant, out var value))
                return Cast<T>(key, value);

            // the element is outside the theme (or the layer is not applied yet): the Normal level of the files
            foreach (var file in SukiCompactnessResources.Files)
                if (SukiCompactnessResources.Level(file, SukiCompactness.Normal) is { } normal &&
                    normal.TryGetResource(key, null, out value))
                    return Cast<T>(key, value);

            throw new KeyNotFoundException($"No SukiCompactness resource is declared under '{key}'.");
        }

        private static T Cast<T>(string key, object? value) =>
            value is T typed
                ? typed
                : throw new InvalidCastException($"The SukiCompactness resource '{key}' is a {value?.GetType().Name ?? "null"}, not a {typeof(T).Name}.");

        // ResourcesChanged is not an EventArgs event, which the motion layer wants: each handler gets a wrapper.
        private static readonly ConditionalWeakTable<EventHandler, EventHandler<ResourcesChangedEventArgs>> Wrappers = new();

        /// <summary>
        /// When the compactness level may have changed: a new level replaces the resources of the theme, which
        /// is when the sizes a motion aims at have to be read again. (Other resource changes, such as the accent
        /// colour, raise it too: play something idempotent.)
        /// </summary>
        internal static TriggerBuilder<TControl, TProfile, EventArgs> OnCompactnessChanged<TControl, TProfile>(
            this MotionBuilder<TControl, TProfile> motion)
            where TControl : Interactive =>
            motion.OnEvent(
                (control, handler) =>
                {
                    var wrapper = new EventHandler<ResourcesChangedEventArgs>((sender, _) => handler(sender, EventArgs.Empty));
                    Wrappers.AddOrUpdate(handler, wrapper);
                    control.ResourcesChanged += wrapper;
                },
                (control, handler) =>
                {
                    if (Wrappers.TryGetValue(handler, out var wrapper))
                        control.ResourcesChanged -= wrapper;
                });
    }
}
