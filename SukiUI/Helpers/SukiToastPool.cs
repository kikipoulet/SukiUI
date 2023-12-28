using System.Collections;
using System.Collections.Generic;
using SukiUI.Controls;

namespace SukiUI.Helpers;

public static class SukiToastPool
{
    private static readonly Stack<SukiToast> Pool = new ();

    public static SukiToast Get()
    {
        return Pool.Count >= 1 ? Pool.Pop() : new SukiToast();
    }

    public static void Return(SukiToast toast) => Pool.Push(toast);
}