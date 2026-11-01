using Avalonia.Controls;
using System;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Avalonia;
using System.ComponentModel;
using Avalonia.Logging;
using Avalonia.LogicalTree;
using Avalonia.Threading;

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
        private bool _isAttachedToLogicalTree;

        // The evaluator is cached per control rather than in a shared static cache:
        // a process-wide cache keyed by type and condition text would grow without
        // bound and keep DataContext types alive.
        // A null evaluator with a non-null key means compiling that key failed.
        private Func<object, bool>? _conditionEvaluator;
        private Type? _compiledDataContextType;
        private string? _compiledCondition;

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            _isAttachedToLogicalTree = true;
            SubscribeToDataContext();
            UpdateVisibility();
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _isAttachedToLogicalTree = false;
            UnsubscribeFromDataContext();
            base.OnDetachedFromLogicalTree(e);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            UnsubscribeFromDataContext();

            base.OnDataContextChanged(e);

            if (_isAttachedToLogicalTree)
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
            if (Dispatcher.UIThread.CheckAccess())
                UpdateVisibility();
            else
                Dispatcher.UIThread.Post(UpdateVisibility);
        }

        private void ClearConditionEvaluator()
        {
            _conditionEvaluator = null;
            _compiledDataContextType = null;
            _compiledCondition = null;
        }


        private void UpdateVisibility()
        {

            try
            {
                var dataContext = DataContext;
                var condition = Condition;
                if (dataContext == null || string.IsNullOrWhiteSpace(condition))
                {
                    ClearConditionEvaluator();
                    IsVisible = false;
                    return;
                }

                var dataContextType = dataContext.GetType();
                if (_compiledCondition != condition || _compiledDataContextType != dataContextType)
                {
                    // Record the key before compiling so a parse failure is cached too:
                    // later notifications skip reparsing until the condition or type changes.
                    _conditionEvaluator = null;
                    _compiledCondition = condition;
                    _compiledDataContextType = dataContextType;

                    var parameter = Expression.Parameter(dataContextType, "x");
                    var lambda = DynamicExpressionParser.ParseLambda(
                        new ParsingConfig(),
                        new[] { parameter },
                        typeof(bool),
                        condition
                    );

                    // Pass the current context as an argument: the cached delegate must
                    // not retain a previous DataContext instance. A typed adapter also
                    // avoids DynamicInvoke's argument array and boxed return value.
                    var context = Expression.Parameter(typeof(object), "context");
                    _conditionEvaluator = Expression.Lambda<Func<object, bool>>(
                        Expression.Invoke(lambda, Expression.Convert(context, dataContextType)),
                        context
                    ).Compile();
                }

                IsVisible = _conditionEvaluator is not null && _conditionEvaluator(dataContext);
            }
            catch (Exception ex)
            {
                Logger.TryGet(LogEventLevel.Warning, LogArea.Control)?.Log(
                    this, "Failed to evaluate If condition '{Condition}': {Exception}", Condition, ex);
                IsVisible = false;
            }
        }
    }
}
