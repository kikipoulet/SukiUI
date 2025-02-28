using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Linq.Dynamic.Core;

namespace SukiUI.Helpers.ConditionalXAML
{
     public class B_If : MarkupExtension
    {
        public string Expression { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(Expression))
                return AvaloniaProperty.UnsetValue;

            var provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (!(provideValueTarget?.TargetObject is AvaloniaObject targetObject) ||
                !(provideValueTarget.TargetProperty is AvaloniaProperty targetProperty))
            {
                return AvaloniaProperty.UnsetValue;
            }

            var updater = new ReactiveUpdater(targetObject, targetProperty, Expression);
            updater.Attach();
            
            return updater.CurrentValue;
        }

        private class ReactiveUpdater
        {
            private readonly AvaloniaObject _targetObject;
            private readonly AvaloniaProperty _targetProperty;
            private readonly string _expression;
            private INotifyPropertyChanged _dataContextNotifier;

            public object CurrentValue { get; private set; } = AvaloniaProperty.UnsetValue;

            public ReactiveUpdater(AvaloniaObject targetObject, AvaloniaProperty targetProperty, string expression)
            {
                _targetObject = targetObject;
                _targetProperty = targetProperty;
                _expression = expression;
            }

            public void Attach()
            {
                if (_targetObject is Control control)
                {
                    control.DataContextChanged += OnDataContextChanged;
                    SubscribeToDataContext(control.DataContext);
                    EvaluateAndSet(control.DataContext);
                }
            }

            private void OnDataContextChanged(object sender, EventArgs e)
            {
               if (sender is Control control)
                {
                    UnsubscribeFromDataContext();
                    SubscribeToDataContext(control.DataContext);
                    EvaluateAndSet(control.DataContext);
                }
            }

            private void SubscribeToDataContext(object dataContext)
            {
                if (dataContext is INotifyPropertyChanged notifier)
                {
                    _dataContextNotifier = notifier;
                    _dataContextNotifier.PropertyChanged += DataContext_PropertyChanged;
                }
            }

            private void UnsubscribeFromDataContext()
            {
                if (_dataContextNotifier != null)
                {
                    _dataContextNotifier.PropertyChanged -= DataContext_PropertyChanged;
                    _dataContextNotifier = null;
                }
            }

            private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                EvaluateAndSet(sender);
            }

            private void EvaluateAndSet(object dataContext)
            {
                if (dataContext == null)
                {
                    _targetObject.SetValue(_targetProperty, AvaloniaProperty.UnsetValue);
                    CurrentValue = AvaloniaProperty.UnsetValue;
                    return;
                }

                try
                {
                    var config = new ParsingConfig();
                    var parameter = System.Linq.Expressions.Expression.Parameter(dataContext.GetType(), "x");
                    var lambda = DynamicExpressionParser.ParseLambda(
                        config,
                        new[] { parameter },
                        typeof(object),
                        _expression
                    );
                    var result = lambda.Compile().DynamicInvoke(dataContext);
                    CurrentValue = result;
                    _targetObject.SetValue(_targetProperty, result);
                }
                catch
                {
                    _targetObject.SetValue(_targetProperty, AvaloniaProperty.UnsetValue);
                    CurrentValue = AvaloniaProperty.UnsetValue;
                }
            }
        }
    }
}