using System.Collections.Concurrent;
using System.Collections.Generic;
using SukiUI.Controls;
using SukiUI.Dialogs;

namespace SukiUI.Helpers
{
    internal static class DialogPool
    {
        private static readonly ConcurrentStack<ISukiDialog> Pool = new();

        static DialogPool()
        {
            Pool.Push(new SukiDialog());
            Pool.Push(new SukiDialog());
        }
        
        internal static ISukiDialog Get()
        {
            var dialog = Pool.TryPop(out var item) ? item : new SukiDialog();
            dialog.ResetToDefault();
            return dialog;
        }

        internal static void Return(ISukiDialog dialog) => Pool.Push(dialog);

        internal static void Return(IEnumerable<ISukiDialog> dialogs)
        {
            foreach (var dialog in dialogs)
            {
                Pool.Push(dialog);
            }
        }
    }
}