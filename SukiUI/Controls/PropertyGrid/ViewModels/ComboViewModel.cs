using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace SukiUI.Controls
{
    public sealed class ComboViewModel : PropertyViewModelBase<object?>
    {
        private readonly IPropertyGridOptionSource _optionSource;

        private IEnumerable? _items;
        private bool _itemsResolved;

        /// <summary>
        /// The values offered by the editor. Resolved once and reused until the ViewModel reports a
        /// change, so binding does not query the option source on every read.
        /// </summary>
        public IEnumerable? Items
        {
            get
            {
                if (!_itemsResolved)
                {
                    _items = _optionSource.GetOptions(PropertyInfo.Name);
                    _itemsResolved = true;
                }

                return _items;
            }
        }

        public ComboViewModel(
            INotifyPropertyChanged viewmodel,
            string displayName,
            PropertyInfo propertyInfo,
            IPropertyGridOptionSource optionSource)
            : base(viewmodel, displayName, propertyInfo)
        {
            _optionSource = optionSource;
        }

        protected override void OnViewModelPropertyChanged(string? propertyName)
        {
            var items = _optionSource.GetOptions(PropertyInfo.Name);
            var wasResolved = _itemsResolved;
            _itemsResolved = true;

            // Only notify when the source actually swapped the collection. In-place edits to an
            // observable collection already reach the editor on their own.
            if (wasResolved && ReferenceEquals(items, _items))
            {
                return;
            }

            _items = items;
            OnPropertyChanged(nameof(Items));
        }
    }
}
