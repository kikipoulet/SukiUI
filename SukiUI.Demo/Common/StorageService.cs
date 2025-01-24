using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;

namespace SukiUI.Demo.Common
{
    internal static class StorageService
    {
        public static FilePickerFileType All { get; } = new("All")
        {
            Patterns = ["*.*"],
            MimeTypes = ["*/*"]
        };

        public static FilePickerFileType Json { get; } = new("Json")
        {
            Patterns = ["*.json"],
            AppleUniformTypeIdentifiers = ["public.json"],
            MimeTypes = ["application/json"]
        };

        public static IStorageProvider? GetStorageProvider()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
            {
                return window.StorageProvider;
            }

            if (Avalonia.Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime 
                {
                    MainView: { } mainView
                })
            {
                var visualRoot = mainView.GetVisualRoot();
                if (visualRoot is TopLevel topLevel)
                {
                    return topLevel.StorageProvider;
                }
            }

            return null;
        }
    }
}