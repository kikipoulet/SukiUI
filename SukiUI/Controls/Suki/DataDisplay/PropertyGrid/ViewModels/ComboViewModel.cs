using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace SukiUI.Controls
{
    public sealed class ComboViewModel : PropertyViewModelBase<object?>
    {
        private readonly PropertyInfo _optionsProperty;
        private readonly string _optionsPropertyName;

        private IEnumerable _items;

        /// <summary>
        /// The values offered by the editor. Observable collections update the editor in place;
        /// replacement collections are picked up when the ViewModel reports a change.
        /// </summary>
        public IEnumerable Items => _items;
        public ComboViewModel(
            INotifyPropertyChanged viewmodel,
            string displayName,
            PropertyInfo propertyInfo,
            string optionsPropertyName,
            PropertyInfo optionsProperty)
            : base(viewmodel, displayName, propertyInfo)
        {
            if (propertyInfo.PropertyType != typeof(string) &&
                typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
            {
                throw new InvalidOperationException(
                    $"Property '{propertyInfo.Name}' is marked with {nameof(PropertyGridOptionsAttribute)} " +
                    "and must be a scalar property for single selection.");
            }

            _optionsPropertyName = optionsPropertyName;
            _optionsProperty = optionsProperty;

            if (!typeof(IEnumerable).IsAssignableFrom(_optionsProperty.PropertyType))
            {
                throw new InvalidOperationException(
                    $"Options source '{optionsPropertyName}' for property '{propertyInfo.Name}' " +
                    $"must be an enumerable property, but is '{_optionsProperty.PropertyType.Name}'.");
            }

            _items = GetOptions();
        }

        protected override void OnViewModelPropertyChanged(string? propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName) && propertyName != _optionsPropertyName)
            {
                return;
            }

            var items = GetOptions();

            // Only notify when the source actually swapped the collection. In-place edits to an
            // observable collection already reach the editor on their own.
            if (ReferenceEquals(items, _items))
            {
                return;
            }

            _items = items;
            OnPropertyChanged(nameof(Items));
        }

        private IEnumerable GetOptions()
        {
            return _optionsProperty.GetValue(Viewmodel) as IEnumerable
                ?? throw new InvalidOperationException(
                    $"Options source '{_optionsPropertyName}' for property '{PropertyInfo.Name}' " +
                    "is null; it must contain an enumerable collection.");
        }
    }
}
