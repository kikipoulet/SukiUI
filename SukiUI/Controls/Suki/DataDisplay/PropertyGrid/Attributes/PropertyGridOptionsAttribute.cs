namespace SukiUI.Controls
{
    /// <summary>
    /// Marks a property as using a combo box editor populated by
    /// <see cref="IPropertyGridOptionSource"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PropertyGridOptionsAttribute : Attribute
    {
        /// <summary>
        /// The name passed to <see cref="IPropertyGridOptionSource.GetOptions"/>.
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
