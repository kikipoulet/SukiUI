using SukiUI.Controls;
using System.Collections.Generic;

namespace SukiUI.Helpers;

internal static class ToastPool
{
    private static readonly Stack<SukiToast> Pool = new();

    internal static SukiToast Get()
    {
        var toast = Pool.Count > 0 ? Pool.Pop() : new SukiToast();
        return toast.ResetToDefault();
    }

    internal static void Return(SukiToast toast) => Pool.Push(toast);

    internal static void Return(IEnumerable<SukiToast> toasts)
    {
        foreach (var toast in toasts)
            Pool.Push(toast);
    }
}