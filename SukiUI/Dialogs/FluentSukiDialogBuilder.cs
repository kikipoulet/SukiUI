using System;
using Avalonia.Controls.Notifications;

namespace SukiUI.Dialogs
{
    public static class FluentSukiDialogBuilder
    {
        #region Content
        /// <summary>
        /// Creates a <see cref="SukiDialogBuilder"/> instance that can be used to construct a <see cref="ISukiDialog"/>
        /// </summary>
        public static SukiDialogBuilder CreateDialog(this ISukiDialogManager mgr) => new(mgr);

        /// <summary>
        /// Gives the dialog a "type" which will display an icon representing it.
        /// This can be used for MessageBox style dialogs.
        /// </summary>
        public static SukiDialogBuilder OfType(this SukiDialogBuilder builder, NotificationType type)
        {
            builder.SetType(type);
            return builder;
        }
        
        /// <summary>
        /// Gives the dialog a title.
        /// </summary>
        public static SukiDialogBuilder WithTitle(this SukiDialogBuilder builder, string title)
        {
            builder.SetTitle(title);
            return builder;
        }

        /// <summary>
        /// Gives the dialog content. This content can be a ViewModel if desired.
        /// </summary>
        public static SukiDialogBuilder WithContent(this SukiDialogBuilder builder, object content)
        {
            builder.SetContent(content);
            return builder;
        }
        
        /// <summary>
        /// Hide Card background.
        /// </summary>
        public static SukiDialogBuilder ShowCardBackground(this SukiDialogBuilder builder, bool show)
        {
            builder.SetShowCardBackground(show);
            return builder;
        }

        /// <summary>
        /// Gives the dialog a ViewModel. If this is used, Title/Content are ignored and only the ViewModel is rendered - the View being located by the usual strategy.
        /// This is useful for custom dialogs.
        /// </summary>
        public static SukiDialogBuilder WithViewModel(this SukiDialogBuilder builder, Func<ISukiDialog,object> viewModel, bool isViewModelOnly = true)
        {
            builder.SetViewModel(viewModel, isViewModelOnly);
            return builder;
        }

        #endregion

        #region Dismissing

        /// <summary>
        /// Begins a dismiss chain.
        /// </summary>
        public static SukiDialogBuilder.DismissDialog Dismiss(this SukiDialogBuilder builder) => new(builder);

        /// <summary>
        /// Allows the dialog to be dismissed simply by clicking the background
        /// </summary>
        public static SukiDialogBuilder ByClickingBackground(this SukiDialogBuilder.DismissDialog dismissBuilder)
        {
            dismissBuilder.Builder.SetCanDismissWithBackgroundClick(true);
            return dismissBuilder.Builder;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Adds a button to the bottom of the dialog which will call the supplied callback when clicked.
        /// Any number of buttons can be added to the dialog.
        /// </summary>
        public static SukiDialogBuilder WithActionButton(this SukiDialogBuilder builder, object? content, Action<ISukiDialog> onClicked,
            bool dismissOnClick = false, params string[] classes)
        {
            builder.AddActionButton(content, onClicked, dismissOnClick, classes);
            return builder;
        }

        /// <summary>
        /// Provides a callback that will be called when this dialog is dismissed for any reason.
        /// </summary>
        public static SukiDialogBuilder OnDismissed(this SukiDialogBuilder builder, Action<ISukiDialog> onDismissed)
        {
            builder.SetOnDismissed(onDismissed);
            return builder;
        }

        #endregion
        
    }
}