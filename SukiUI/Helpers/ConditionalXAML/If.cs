using Avalonia.Controls;
using System;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Avalonia;
using System.ComponentModel;
using Avalonia.LogicalTree;

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

        private INotifyPropertyChanged? _dataContextNotifier;

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            SubscribeToDataContext();
            UpdateVisibility();
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            UnsubscribeFromDataContext();
            base.OnDetachedFromLogicalTree(e);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            UnsubscribeFromDataContext();

            base.OnDataContextChanged(e);

            SubscribeToDataContext();

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

        private void SubscribeToDataContext()
        {
            if (_dataContextNotifier is not null || DataContext is not INotifyPropertyChanged notifier)
                return;
            _dataContextNotifier = notifier;
            notifier.PropertyChanged += DataContext_PropertyChanged;
        }

        private void UnsubscribeFromDataContext()
        {
            if (_dataContextNotifier is null)
                return;
            _dataContextNotifier.PropertyChanged -= DataContext_PropertyChanged;
            _dataContextNotifier = null;
        }

        private void DataContext_PropertyChanged(object? sender, PropertyChangedEventArgs e)
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
