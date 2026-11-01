namespace SukiUI.Controls
{
    /// <summary>
    /// Marks a property as using a combo box editor populated by a collection property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PropertyGridOptionsAttribute : Attribute
    {
        /// <summary>
        /// The name of the options collection property.
        /// </summary>
        public string OptionsPropertyName { get; }

        /// <summary>
        /// Marks a property as a combo box and identifies the options source to use.
        /// </summary>
        /// <param name="optionsPropertyName">The name of the options collection property.</param>
        public PropertyGridOptionsAttribute(string optionsPropertyName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(optionsPropertyName);

            OptionsPropertyName = optionsPropertyName;
        }
    }
}
