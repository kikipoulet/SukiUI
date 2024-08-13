using System.Collections.Concurrent;
using SukiUI.Controls;
using System.Collections.Generic;
using SukiUI.Toasts;

namespace SukiUI.Helpers;

internal static class ToastPool
{
    private static readonly ConcurrentBag<ISukiToast> Pool = new();

    internal static ISukiToast Get()
    {
        var toast = Pool.TryTake(out var item) ? item : new SukiToast();
        return toast.ResetToDefault();
    }

    internal static void Return(ISukiToast toast) => Pool.Add(toast);

    internal static void Return(IEnumerable<ISukiToast> toasts)
    {
        foreach (var toast in toasts)
            Pool.Add(toast);
    }
}