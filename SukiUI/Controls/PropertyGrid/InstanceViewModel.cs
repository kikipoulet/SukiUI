using Avalonia.Collections;
using SukiUI.Helpers;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SukiUI.Controls
{
    public class InstanceViewModel : SukiObservableObject, IDisposable
    {
        private static readonly ConcurrentDictionary<Type, PropertyMetadata[]> MetadataCache = new();

        public INotifyPropertyChanged ViewModel { get; }
        
        public IAvaloniaReadOnlyList<CategoryViewModel> Categories { get; }

        public InstanceViewModel(INotifyPropertyChanged viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            
            ViewModel = viewModel;
            Categories = GenerateCategories(viewModel);
        }

        private sealed class PropertyMetadata(PropertyInfo property)
        {
            public PropertyInfo Property { get; } = property;
            public string? Category { get; } = property.GetCustomAttribute<CategoryAttribute>()?.Category ?? "Properties";
            public string? DisplayName { get; } = property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
            public PropertyGridComboAttribute? ComboValues { get; } = property.GetCustomAttribute<PropertyGridComboAttribute>();
        }

        private static PropertyMetadata[] GetMetadata(Type viewModelType)
        {
            return MetadataCache.GetOrAdd(viewModelType, 
                ([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] type) =>
                    type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(property => property.CanRead && property.GetCustomAttribute<PropertyGridIgnoreAttribute>() is null)
                        .Select(property => new PropertyMetadata(property))
                        .ToArray());
        }

        /// <summary>
        /// Factory creating all the categories for a given instance of a ViewModel implementing <see cref="INotifyPropertyChanged"/>.
        /// </summary>
        /// <param name="viewModel">the ViewModel instance, to generate/show controls/categories for</param>
        /// <returns>CategoryViewModels holding representations for each public non-static property</returns>
        public virtual IAvaloniaReadOnlyList<CategoryViewModel> GenerateCategories(INotifyPropertyChanged viewModel)
        {
            var properties = GetMetadata(viewModel.GetType());
            
            var ignoredNames = properties
                .Select(p => p.ComboValues)
                .OfType<PropertyGridComboAttribute>()
                .Select(a => a.ItemsSourcePropertyName)
                .ToHashSet();

            var categories = properties
                .Where(p => !ignoredNames.Contains(p.Property.Name))
                .Distinct()
                .GroupBy(p => p.Category);

            var categoryViewModels = new AvaloniaList<CategoryViewModel>();
            foreach (var grouping in categories)
            {
                var propertyViewModels = new AvaloniaList<IPropertyViewModel>();
                foreach (var metadata in grouping)
                {
                    var propertyViewModel = default(IPropertyViewModel?);
                    var property = metadata.Property;
                    var displayName = metadata.DisplayName ?? property.Name;
                    
                    if (metadata.ComboValues is { } comboAttribute)
                    {
                        propertyViewModel = new ComboViewModel(viewModel, displayName, property, comboAttribute);
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        propertyViewModel = new StringViewModel(viewModel, displayName, property);
                    }
                    else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                    {
                        propertyViewModel = new IntegerViewModel(viewModel, displayName, property);
                    }
                    else if (property.PropertyType == typeof(long) || property.PropertyType == typeof(long?))
                    {
                        propertyViewModel = new LongViewModel(viewModel, displayName, property);
                    }
                    else if (property.PropertyType == typeof(double) || property.PropertyType == typeof(double?))
                    {
                        propertyViewModel = new DoubleViewModel(viewModel, displayName, property);
                    }
                    else if (property.PropertyType == typeof(float) || property.PropertyType == typeof(float?))
                    {
                        propertyViewModel = new FloatViewModel(viewModel, displayName, property);
                    }
                    else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
                    {
                        propertyViewModel = new DecimalViewModel(viewModel, displayName, property);
                    }
                    else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                    {
                        propertyViewModel = new BoolViewModel(viewModel, displayName, property);
                    }
                    else if (property.PropertyType.IsEnum ||
                             Nullable.GetUnderlyingType(property.PropertyType)?.IsEnum == true)
                    {
                        propertyViewModel = new EnumViewModel(viewModel, displayName, property);
                    }
                    else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                    {
                        propertyViewModel = new DateTimeViewModel(viewModel, displayName, property);
                    }
                    else if (property.PropertyType == typeof(DateTimeOffset) || property.PropertyType == typeof(DateTimeOffset?))
                    {
                        propertyViewModel = new DateTimeOffsetViewModel(viewModel, displayName, property);
                    }
                    else
                    {
                        if (property.GetValue(viewModel) is INotifyPropertyChanged)
                        {
                            propertyViewModel = new ComplexTypeViewModel(viewModel, displayName, property);
                        }
                    }

                    if (propertyViewModel is not null)
                    {
                        propertyViewModels.Add(propertyViewModel);
                    }
                }

                var categoryViewModel = new CategoryViewModel(grouping.Key!, propertyViewModels);
                categoryViewModels.Add(categoryViewModel);
            }

            return categoryViewModels;
        }

        public void Dispose()
        {
            foreach (var category in Categories)
            {
                foreach (var property in category.Properties)
                {
                    property.Dispose();
                }
            }
        }
    }
}
