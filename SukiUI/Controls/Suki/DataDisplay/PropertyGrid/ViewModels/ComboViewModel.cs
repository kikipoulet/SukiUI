using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace SukiUI.Controls
{
    public sealed class ComboViewModel : PropertyViewModelBase<object?>
    {
        private readonly IPropertyGridOptionSource _optionSource;
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
            IPropertyGridOptionSource optionSource)
            : base(viewmodel, displayName, propertyInfo)
        {
            _optionSource = optionSource;
            _optionsPropertyName = optionsPropertyName;
            _items = optionSource.GetOptions(_optionsPropertyName);
        }

        protected override void OnViewModelPropertyChanged(string? propertyName)
        {
            if (propertyName is not (null or "") && propertyName != _optionsPropertyName)
            {
                return;
            }

            var items = _optionSource.GetOptions(_optionsPropertyName);

            // Only notify when the source actually swapped the collection. In-place edits to an
            // observable collection already reach the editor on their own.
            if (ReferenceEquals(items, _items))
            {
                return;
            }

            _items = items;
            OnPropertyChanged(nameof(Items));
        }
    }
}
