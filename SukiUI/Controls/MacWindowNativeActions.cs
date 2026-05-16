using Avalonia.Controls;
using System.Runtime.InteropServices;

namespace SukiUI.Controls;

internal static class MacWindowNativeActions
{
    private const string NSWindowHandleDescriptor = "NSWindow";

    public static bool TryToggleZoom(Window window, bool isMaximized)
    {
        var nsWindow = TryGetNSWindow(window);
        if (nsWindow == IntPtr.Zero)
        {
            return false;
        }

        if (IsZoomed(nsWindow))
        {
            SendVoid(nsWindow, "performZoom:", nsWindow);
            return true;
        }

        if (isMaximized)
        {
            return false;
        }

        SendVoid(nsWindow, "performZoom:", nsWindow);
        return true;
    }

    public static bool TryToggleFullScreen(Window window)
    {
        var nsWindow = TryGetNSWindow(window);
        if (nsWindow == IntPtr.Zero)
        {
            return false;
        }

        SendVoid(nsWindow, "toggleFullScreen:", IntPtr.Zero);
        return true;
    }

    private static IntPtr TryGetNSWindow(Window window)
    {
        if (window is null)
        {
            return IntPtr.Zero;
        }

        if (!OperatingSystem.IsMacOS())
        {
            return IntPtr.Zero;
        }

        var platformHandle = window.TryGetPlatformHandle();
        if (platformHandle?.Handle == IntPtr.Zero ||
            platformHandle?.HandleDescriptor != NSWindowHandleDescriptor)
        {
            return IntPtr.Zero;
        }

        return platformHandle.Handle;
    }

    private static IntPtr GetSelector(string name) => sel_registerName(name);

    private static bool IsZoomed(IntPtr nsWindow) =>
        objc_msgSend_bool(nsWindow, GetSelector("isZoomed"));

    private static void SendVoid(IntPtr receiver, string selector, IntPtr value) =>
        objc_msgSend_void_IntPtr(receiver, GetSelector(selector), value);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    [return: MarshalAs(UnmanagedType.I1)]
    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void_IntPtr(IntPtr receiver, IntPtr selector, IntPtr value);
}
