using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SukiUI.Controls
{
    /// <summary>
    /// <para>
    /// Optional per-type cache for the reflection work behind <see cref="PropertyGrid"/>. Caching is
    /// <b>off by default</b> and must be opted into via <see cref="IsCachingEnabled"/>.
    /// </para>
    /// <para>
    /// Building the metadata for one view model type costs roughly 32&#160;µs on a 20-property type,
    /// which is ~80% of the cost of creating an <see cref="InstanceViewModel"/>. Caching it makes
    /// every subsequent instance of that type essentially free — about 5x faster end to end — at the
    /// price of holding the <see cref="Type"/> and its <see cref="PropertyInfo"/>s for as long as the
    /// entry lives.
    /// </para>
    /// <para>
    /// That retention is roughly 1.6&#160;KB per property, so ~32&#160;KB for a 20-property type, and
    /// it scales with the number of properties on the type. It is also unbounded: entries are never
    /// released unless <see cref="InvalidateCache"/> or <see cref="ClearCache"/> is called. Caching
    /// is therefore worth turning on when grids are rebuilt repeatedly from the same small set of
    /// view model types, for example on navigation; it is not worth it for a fixed screen built once,
    /// or when the set of view model types is large or unbounded.
    /// </para>
    /// </summary>
    public static class PropertyGridMetadata
    {
        private static readonly ConcurrentDictionary<Type, Lazy<TypeMetadata>> Entries = new();

        private static volatile bool _isCachingEnabled;

        /// <summary>
        /// Whether metadata is cached per view model type. Disabled by default; set it to
        /// <see langword="true"/> to opt in.
        /// <para>
        /// Enabling it makes every <see cref="PropertyGrid"/> instance after the first for a given
        /// type roughly 5x cheaper to build, and keeps one entry per type (~1.6&#160;KB per property,
        /// so ~32&#160;KB for a 20-property type) alive. Entries are never released on their own, so
        /// drop them with <see cref="InvalidateCache"/> or <see cref="ClearCache"/> when a type is no
        /// longer in use.
        /// </para>
        /// <para>
        /// Setting this to <see langword="false"/> also calls <see cref="ClearCache"/>, because the
        /// point of disabling it is to stop holding on to the reflected types. A disabled cache
        /// retains nothing and costs only a volatile read per <see cref="PropertyGrid"/> instance.
        /// </para>
        /// <para>
        /// This is a configuration switch, so set it up front. A build already in flight on another
        /// thread can land one entry just after the cache is cleared; call <see cref="ClearCache"/>
        /// again if you need a hard guarantee.
        /// </para>
        /// </summary>
        public static bool IsCachingEnabled
        {
            get => _isCachingEnabled;
            set
            {
                _isCachingEnabled = value;
                if (!value)
                {
                    ClearCache();
                }
            }
        }

        /// <summary>
        /// The number of view model types currently cached. Useful for checking that
        /// <see cref="ClearCache"/> had the effect you expected.
        /// </summary>
        public static int CachedTypeCount => Entries.Count;

        /// <summary>
        /// Drops every cached entry, releasing the reflected types. The next
        /// <see cref="PropertyGrid"/> for a given type rebuilds its metadata from scratch.
        /// </summary>
        public static void ClearCache() => Entries.Clear();

        /// <summary>
        /// Drops the cached entry for a single view model type, so the next
        /// <see cref="PropertyGrid"/> bound to it rebuilds its metadata.
        /// </summary>
        /// <param name="viewModelType">The type passed to <see cref="InstanceViewModel"/>.</param>
        /// <returns><see langword="true"/> if an entry was removed.</returns>
        public static bool InvalidateCache(Type viewModelType)
        {
            ArgumentNullException.ThrowIfNull(viewModelType);

            return Entries.TryRemove(viewModelType, out _);
        }

        /// <summary>
        /// Gets the metadata for a given type.
        /// If <see cref="IsCachingEnabled"/> is <see langword="true"/>, the metadata will be cached.
        /// </summary>
        /// <param name="viewModelType">
        /// The <see cref="Type"/> for which to get metadata.
        /// </param>
        /// <returns>
        /// The <see cref="TypeMetadata"/> for the specified type.
        /// </returns>
        internal static TypeMetadata Get(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
            Type viewModelType)
        {
            if (!_isCachingEnabled)
            {
                return new TypeMetadata(viewModelType);
            }

            // Wrapping in Lazy is what makes this run exactly once. ConcurrentDictionary.GetOrAdd on
            // its own does not: when several threads miss on the same key they each invoke the
            // factory, and every result but one is thrown away. Here the racing factory calls only
            // allocate a cheap Lazy; the metadata is built by the single instance that gets published.
            return Entries.GetOrAdd(
                viewModelType,
                static ([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] type) => new Lazy<TypeMetadata>(
                    () => new TypeMetadata(type), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        /// <summary>
        /// Everything the PropertyGrid needs to know about one view model type, so that property
        /// reflection and attribute lookups do not have to happen per instance.
        /// </summary>
        internal sealed class TypeMetadata
        {
            internal sealed class PropertyMetadata(PropertyInfo property)
            {
                public PropertyInfo Property { get; } = property;
                public string? Category { get; } = property.GetCustomAttribute<CategoryAttribute>()?.Category ?? "Properties";
                public string? DisplayName { get; } = property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
            }

            public TypeMetadata([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type)
            {
                Properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(property => property.CanRead && property.GetCustomAttribute<PropertyGridIgnoreAttribute>() is null)
                    .Select(property => new PropertyMetadata(property))
                    .ToArray();
            }

            public PropertyMetadata[] Properties { get; }
        }
    }
}
