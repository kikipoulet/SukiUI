using System;

namespace SukiUI.Controls
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PropertyGridComboAttribute : Attribute
    {
        public string ItemsSourcePropertyName { get; }

        public PropertyGridComboAttribute(string itemsSourcePropertyName)
        {
            if (string.IsNullOrWhiteSpace(itemsSourcePropertyName))
            {
                throw new ArgumentException("A property name is required.", nameof(itemsSourcePropertyName));
            }

            ItemsSourcePropertyName = itemsSourcePropertyName;
        }
    }
}
