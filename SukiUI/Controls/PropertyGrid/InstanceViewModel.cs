using Avalonia.Collections;
using SukiUI.Helpers;
using System.ComponentModel;
using System.Reflection;

namespace SukiUI.Controls
{
    public class InstanceViewModel : SukiObservableObject, IDisposable
    {

        public INotifyPropertyChanged ViewModel { get; }
        
        public IAvaloniaReadOnlyList<CategoryViewModel> Categories { get; }

        public InstanceViewModel(INotifyPropertyChanged viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            
            ViewModel = viewModel;
            Categories = GenerateCategories(viewModel);
        }

        /// <summary>
        /// Factory creating all the categories for a given instance of a ViewModel implementing <see cref="INotifyPropertyChanged"/>.
        /// <para>
        /// Property reflection and attribute lookups come from <see cref="PropertyGridMetadata"/>,
        /// which resolves metadata per view model <see cref="Type"/>. That lookup is a cheap
        /// dictionary hit when <see cref="PropertyGridMetadata.IsCachingEnabled"/> is enabled, and a
        /// full rebuild otherwise — the default. Either way properties marked
        /// <see cref="PropertyGridIgnoreAttribute"/> are filtered out at the metadata level and never
        /// reach this method.
        /// </para>
        /// </summary>
        /// <param name="viewModel">the ViewModel instance, to generate/show controls/categories for</param>
        /// <returns><see cref="IAvaloniaReadOnlyList{CategoryViewModel}"/> holding representations for each public non-static property</returns>
        public virtual IAvaloniaReadOnlyList<CategoryViewModel> GenerateCategories(INotifyPropertyChanged viewModel)
        {
            var typeMetadata = PropertyGridMetadata.Get(viewModel.GetType());

            var categories = typeMetadata.Properties.GroupBy(p => p.Category);

            var categoryViewModels = new AvaloniaList<CategoryViewModel>();
            
            foreach (var grouping in categories)
            {
                var propertyViewModels = new AvaloniaList<IPropertyViewModel>();
                
                foreach (var metadata in grouping)
                {
                    var propertyViewModel = default(IPropertyViewModel?);
                    var property = metadata.Property;
                    var displayName = metadata.DisplayName ?? property.Name;

                    if (viewModel is IPropertyGridOptionSource optionSource 
                        && optionSource.GetOptions(property.Name) is not null)
                    {
                        propertyViewModel = new ComboViewModel(viewModel, displayName, property, optionSource);
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

                // Properties with no matching editor (commands, unsupported types) would otherwise
                // leave a category behind with nothing in it.
                if (propertyViewModels.Count == 0)
                {
                    continue;
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
