using System;
using Avalonia.Controls;
using SukiUI.Helpers;

namespace SukiUI.Dialogs
{
    public class SukiDialogBuilder
    {
        public ISukiDialogManager Manager { get; }
        public ISukiDialog Dialog { get; }

        public SukiDialogBuilder(ISukiDialogManager manager)
        {
            Manager = manager;
            Dialog = DialogPool.Get();
        }
        
        public bool TryShow() => Manager.TryShowDialog(Dialog);

        public void SetTitle(string title) => Dialog.Title = title;

        public void SetContent(object content) => Dialog.Content = content;

        public void SetViewModel(object viewModel) => Dialog.ViewModel = viewModel;

        public void SetCanDismissWithBackgroundClick(bool canDismissWithBackgroundClick) =>
            Dialog.CanDismissWithBackgroundClick = canDismissWithBackgroundClick;
        
        public void SetOnDismissed(Action<ISukiDialog> onDismissed) => Dialog.OnDismissed = onDismissed;
        
        public void AddActionButton(object? buttonContent, Action<ISukiDialog> onClicked, bool dismissOnClick)
        {
            var btn = new Button()
            {
                Content = buttonContent
            };
            btn.Click += (_,_) =>
            {
                onClicked(Dialog);
                if (!dismissOnClick) return;
                Manager.TryDismissDialog(Dialog);
            };
            Dialog.ActionButtons.Add(btn);
        }

        public class DismissDialog
        {
            public SukiDialogBuilder Builder { get; }

            public DismissDialog(SukiDialogBuilder builder)
            {
                Builder = builder;
            }
        }
    }
}