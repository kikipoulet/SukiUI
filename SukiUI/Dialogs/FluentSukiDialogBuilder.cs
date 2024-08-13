using System;
using Avalonia.Controls.Notifications;

namespace SukiUI.Dialogs
{
    public static class FluentSukiDialogBuilder
    {
        #region Content

        public static SukiDialogBuilder CreateDialog(this ISukiDialogManager mgr) => new(mgr);

        public static SukiDialogBuilder OfType(this SukiDialogBuilder builder, NotificationType type)
        {
            builder.SetType(type);
            return builder;
        }
        
        public static SukiDialogBuilder WithTitle(this SukiDialogBuilder builder, string title)
        {
            builder.SetTitle(title);
            return builder;
        }

        public static SukiDialogBuilder WithContent(this SukiDialogBuilder builder, object content)
        {
            builder.SetContent(content);
            return builder;
        }

        public static SukiDialogBuilder WithViewModel(this SukiDialogBuilder builder, Func<ISukiDialog,object> viewModel)
        {
            builder.SetViewModel(viewModel);
            return builder;
        }

        #endregion

        #region Dismissing

        public static SukiDialogBuilder.DismissDialog Dismiss(this SukiDialogBuilder builder) => new(builder);

        public static SukiDialogBuilder ByClickingBackground(this SukiDialogBuilder.DismissDialog dismissBuilder)
        {
            dismissBuilder.Builder.SetCanDismissWithBackgroundClick(true);
            return dismissBuilder.Builder;
        }

        #endregion

        #region Actions

        public static SukiDialogBuilder WithActionButton(this SukiDialogBuilder builder, object? content, Action<ISukiDialog> onClicked,
            bool dismissOnClick = false)
        {
            builder.AddActionButton(content, onClicked, dismissOnClick);
            return builder;
        }

        public static SukiDialogBuilder OnDismissed(this SukiDialogBuilder builder, Action<ISukiDialog> onDismissed)
        {
            builder.SetOnDismissed(onDismissed);
            return builder;
        }

        #endregion
        
    }
}