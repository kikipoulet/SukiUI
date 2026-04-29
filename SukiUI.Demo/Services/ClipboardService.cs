using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;

namespace SukiUI.Demo.Services
{
    public class ClipboardService(IClassicDesktopStyleApplicationLifetime liftime)
    {
        public void CopyToClipboard(string text) => liftime.MainWindow?.Clipboard?.SetTextAsync(text);
    }
}
