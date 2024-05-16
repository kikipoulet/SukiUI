using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SukiUI.Demo.Utilities;

public static class UrlUtilities
{
    /// <summary>
    /// Open the URL in the default browser.
    /// </summary>
    /// <param name="url"></param>
    public static void OpenUrl(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start(new ProcessStartInfo(url.Replace("&", "^&")) { UseShellExecute = true });
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            Process.Start("xdg-open", url);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            Process.Start("open", url);
    }
}