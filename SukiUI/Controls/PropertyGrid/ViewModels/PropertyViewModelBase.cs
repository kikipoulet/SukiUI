using SukiUI.Helpers;
using System.ComponentModel;
using System.Reflection;

namespace SukiUI.Controls
{
    public abstract class PropertyViewModelBase<T> : SukiObservableObject, IPropertyViewModel<T?>
    {
        private readonly string _propertyName;

        private T? _value;

        public T? Value
        {
            get => _value;
            set
            {
                if (SetAndRaise(ref _value, value))
                {
                    ViewModelSetter(value);
                }
            }
        }

        object? IPropertyViewModel.Value
        {
            get => Value;
            set
            {
                switch (value)
                {
                    case null:
                        Value = default;
                        break;
                    case T typedValue:
                        Value = typedValue;
                        break;
                    default:
                    {
                        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                        if (!targetType.IsPrimitive)
                        {
                            throw new InvalidOperationException(
                                $"Unsupported conversion from {value.GetType()} to {targetType}");
                        }

                        Value = (T?)Convert.ChangeType(value, targetType);
                        break;
                    }
                }
            }
        }

        public string DisplayName { get; }
        public bool IsReadOnly { get; init; }
        protected PropertyInfo PropertyInfo { get; }
        protected INotifyPropertyChanged Viewmodel { get; }

        public PropertyViewModelBase(INotifyPropertyChanged viewmodel, string displayName, PropertyInfo propertyInfo)
        {
            Viewmodel = viewmodel;
            DisplayName = displayName;
            PropertyInfo = propertyInfo;
            IsReadOnly = !propertyInfo.CanWrite;
            _propertyName = propertyInfo.Name;
            _value = ViewModelGetter();
            Viewmodel.PropertyChanged += ViewModelPropertyChanged;
        }

        private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || _propertyName == e.PropertyName)
            {
                Value = ViewModelGetter();
            }

            OnViewModelPropertyChanged(e.PropertyName);
        }

        /// <summary>
        /// Called for every <see cref="INotifyPropertyChanged.PropertyChanged"/> notification raised by the ViewModel,
        /// so derived editors can react to additional properties without adding their own event handler.
        /// </summary>
        protected virtual void OnViewModelPropertyChanged(string? propertyName)
        {
        }

        protected T? ViewModelGetter()
        {
            return (T?)PropertyInfo.GetValue(Viewmodel);
        }

        protected void ViewModelSetter(T? newValue)
        {
            if (PropertyInfo.CanWrite)
            {
                PropertyInfo.SetValue(Viewmodel, newValue);
            }
        }

        public virtual void Dispose()
        {
            Viewmodel.PropertyChanged -= ViewModelPropertyChanged;
        }
    }
}
