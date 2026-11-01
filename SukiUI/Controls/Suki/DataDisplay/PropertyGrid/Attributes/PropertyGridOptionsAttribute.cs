namespace SukiUI.Controls
{
    /// <summary>
    /// Marks a property as using a combo box editor populated by a collection property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PropertyGridOptionsAttribute : Attribute
    {
        /// <summary>
        /// The name of the collection property that supplies the options.
        /// </summary>
        public string SourcePropertyName { get; }

        /// <summary>
        /// Marks a property as a combo box and identifies the options source to use.
        /// </summary>
        /// <param name="sourcePropertyName">The options source name for the property.</param>
        public PropertyGridOptionsAttribute(string sourcePropertyName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sourcePropertyName);

            SourcePropertyName = sourcePropertyName;
        }
    }
}
