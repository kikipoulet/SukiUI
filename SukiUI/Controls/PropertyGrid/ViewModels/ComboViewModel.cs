using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace SukiUI.Controls
{
    public sealed class ComboViewModel : PropertyViewModelBase<object?>
    {
        private readonly PropertyInfo _itemsSourceProperty;

        public IEnumerable? Items => _itemsSourceProperty.GetValue(Viewmodel) as IEnumerable;

        public ComboViewModel(
            INotifyPropertyChanged viewmodel,
            string displayName,
            PropertyInfo propertyInfo,
            PropertyGridComboAttribute attribute)
            : base(viewmodel, displayName, propertyInfo)
        {
            _itemsSourceProperty = GetItemsSourceProperty(attribute);
            Viewmodel.PropertyChanged += OnItemsSourcePropertyChanged;
        }

        private PropertyInfo GetItemsSourceProperty(PropertyGridComboAttribute attribute)
        {
            var property = Viewmodel.GetType()
                .GetProperty(attribute.ItemsSourcePropertyName, BindingFlags.Public | BindingFlags.Instance);

            if (property is null)
            {
                throw new InvalidOperationException(
                    $"Property '{attribute.ItemsSourcePropertyName}' was not found on {Viewmodel.GetType().Name}.");
            }

            if (!typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                throw new InvalidOperationException(
                    $"Property '{property.Name}' must implement {nameof(IEnumerable)}.");
            }

            return property;
        }

        private void OnItemsSourcePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _itemsSourceProperty.Name)
            {
                OnPropertyChanged(nameof(Items));
            }
        }

        public override void Dispose()
        {
            Viewmodel.PropertyChanged -= OnItemsSourcePropertyChanged;
            base.Dispose();
        }
    }
}
