using System.Collections;
using System.Collections.Generic;
using SukiUI.Controls;

namespace SukiUI.Helpers;

internal static class SukiToastPool
{
    private static readonly Stack<SukiToast> Pool = new ();

    internal static SukiToast Get()
    {
        return Pool.Count >= 1 ? Pool.Pop() : new SukiToast();
    }

    internal static void Return(SukiToast toast) => Pool.Push(toast);
}