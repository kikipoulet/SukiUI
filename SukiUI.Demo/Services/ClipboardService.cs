namespace SukiUI.Demo.Services
{
    public class ClipboardService(SukiUIDemoView mainWindow)
    {
        public void CopyToClipboard(string text) => mainWindow.Clipboard?.SetTextAsync(text);
    }
}