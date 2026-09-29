using System.Collections;

namespace SukiUI.Controls
{
    /// <summary>
    /// Implemented by a ViewModel that wants to offer a fixed or observable set of values for one or
    /// more of its properties, turning those properties into combo boxes.
    /// <para>
    /// Because the values are handed over through this method instead of being exposed as public
    /// properties, they are never mistaken for properties to edit and need no attribute to hide them.
    /// </para>
    /// </summary>
    public interface IPropertyGridOptionSource
    {
        /// <summary>
        /// Returns the values to offer for <paramref name="propertyName"/>, or <see langword="null"/>
        /// when that property should be edited normally.
        /// </summary>
        /// <param name="propertyName">The name of the property being edited, usually via <c>nameof</c>.</param>
        /// <remarks>
        /// Return the same collection instance for a given property. Edits made to an observable
        /// collection are picked up automatically; replacing the instance is picked up through
        /// <see cref="System.ComponentModel.INotifyPropertyChanged"/>.
        /// </remarks>
        IEnumerable? GetOptions(string propertyName);
    }
}
