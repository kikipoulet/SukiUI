using Avalonia.Controls;
using System;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Avalonia;
using System.ComponentModel;

namespace SukiUI.Helpers.ConditionalXAML
{
     public class If : ContentControl
    {
        public static readonly StyledProperty<string> ConditionProperty = AvaloniaProperty.Register<If, string>(nameof(Condition));
        public string Condition
        {
            get => GetValue(ConditionProperty);
            set => SetValue(ConditionProperty, value);
        }

        private INotifyPropertyChanged _dataContextNotifier;

        protected override void OnDataContextChanged(EventArgs e)
        {
            if (_dataContextNotifier != null)
            {
                _dataContextNotifier.PropertyChanged -= DataContext_PropertyChanged;
                _dataContextNotifier = null;
            }

            base.OnDataContextChanged(e);

            if (DataContext is INotifyPropertyChanged notifier)
            {
                _dataContextNotifier = notifier;
                _dataContextNotifier.PropertyChanged += DataContext_PropertyChanged;
            }

            UpdateVisibility();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ConditionProperty)
            {
                UpdateVisibility();
            }
        }

        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateVisibility();
        }


        private void UpdateVisibility()
        {

            try
            {
                if (DataContext == null || string.IsNullOrWhiteSpace(Condition))
                {
                    IsVisible = false;
                    return;
                }

                var config = new ParsingConfig();
                var parameter = Expression.Parameter(DataContext.GetType(), "x");
                var lambda = DynamicExpressionParser.ParseLambda(
                    config,
                    new[] { parameter },
                    typeof(bool),
                    Condition
                );

                var result = (bool)lambda.Compile().DynamicInvoke(DataContext);
                IsVisible = result;
            }
            catch
            {
                IsVisible = false;
            }
        }
    }
}