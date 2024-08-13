using System.Collections.Concurrent;
using System.Collections.Generic;
using SukiUI.Controls;
using SukiUI.Dialogs;

namespace SukiUI.Helpers
{
    internal static class DialogPool
    {
        private static readonly ConcurrentBag<ISukiDialog> Pool = new();
        
        internal static ISukiDialog Get()
        {
            var dialog = Pool.TryTake(out var item) ? item : new SukiDialog();
            return dialog;//.ResetToDefault();
        }

        internal static void Return(ISukiDialog toast) => Pool.Add(toast);

        internal static void Return(IEnumerable<ISukiDialog> dialogs)
        {
            foreach (var dialog in dialogs)
                Pool.Add(dialog);
        }
    }
}