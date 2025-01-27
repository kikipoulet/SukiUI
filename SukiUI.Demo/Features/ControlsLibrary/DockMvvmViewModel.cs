using System.Diagnostics;
using Avalonia.Collections;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Serializer;
using Material.Icons;
using SukiUI.Demo.Common;
using SukiUI.Demo.Features.ControlsLibrary.DockControls;

namespace SukiUI.Demo.Features.ControlsLibrary
{
    public partial class DockMvvmViewModel : DemoPageBase
    {
        private readonly IDockSerializer _serializer;
        private readonly IFactory _factory;
        [ObservableProperty] private IRootDock? _layout;

        public DockMvvmViewModel() : base("DockMvvm", MaterialIconKind.DockTop)
        {
            _serializer = new DockSerializer(typeof(AvaloniaList<>));
            _factory = new DockFactory(this);
            DebugFactoryEvents(_factory);

            Layout = _factory.CreateLayout();

            if (Layout is null)
            {
                return;
            }

            _factory.InitLayout(Layout);

            if (Layout is { } root)
            {
                root.Navigate.Execute("Home");
            }
        }
        
        [RelayCommand]
        private async Task SaveLayout()
        {
            var storageProvider = StorageService.GetStorageProvider();

            var file = await storageProvider!.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save layout",
                FileTypeChoices = GetOpenLayoutFileTypes(),
                SuggestedFileName = "layout",
                DefaultExtension = "json",
                ShowOverwritePrompt = true
            });

            if (file is not null)
            {
                try
                {
                    await using var stream = await file.OpenWriteAsync();

                    if (Layout is not null)
                    {
                        _serializer.Save(stream, Layout);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        [RelayCommand]
        private async Task OpenLayout()
        {
            var storageProvider = StorageService.GetStorageProvider();

            var result = await storageProvider!.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Open layout",
                    FileTypeFilter = GetOpenLayoutFileTypes(),
                    AllowMultiple = false
                });

            var file = result.FirstOrDefault();

            if (file is not null)
            {
                try
                {
                    await using var stream = await file.OpenReadAsync();
                    using var reader = new StreamReader(stream);

                    var layout = _serializer.Load<IRootDock?>(stream);

                    if (layout is not null)
                    {
                        _factory!.InitLayout(layout);
                        Layout = layout;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        
        private static List<FilePickerFileType> GetOpenLayoutFileTypes()
            =>
            [
                StorageService.Json,
                StorageService.All
            ];

        private static void DebugFactoryEvents(IFactory factory)
        {
            factory.ActiveDockableChanged += (_, args) =>
            {
                Debug.WriteLine($"[ActiveDockableChanged] Title='{args.Dockable?.Title}'");
            };

            factory.FocusedDockableChanged += (_, args) =>
            {
                Debug.WriteLine($"[FocusedDockableChanged] Title='{args.Dockable?.Title}'");
            };

            factory.DockableAdded += (_, args) =>
            {
                Debug.WriteLine($"[DockableAdded] Title='{args.Dockable?.Title}'");
            };

            factory.DockableRemoved += (_, args) =>
            {
                Debug.WriteLine($"[DockableRemoved] Title='{args.Dockable?.Title}'");
            };

            factory.DockableClosed += (_, args) =>
            {
                Debug.WriteLine($"[DockableClosed] Title='{args.Dockable?.Title}'");
            };

            factory.DockableMoved += (_, args) =>
            {
                Debug.WriteLine($"[DockableMoved] Title='{args.Dockable?.Title}'");
            };

            factory.DockableSwapped += (_, args) =>
            {
                Debug.WriteLine($"[DockableSwapped] Title='{args.Dockable?.Title}'");
            };

            factory.DockablePinned += (_, args) =>
            {
                Debug.WriteLine($"[DockablePinned] Title='{args.Dockable?.Title}'");
            };

            factory.DockableUnpinned += (_, args) =>
            {
                Debug.WriteLine($"[DockableUnpinned] Title='{args.Dockable?.Title}'");
            };

            factory.WindowOpened += (_, args) =>
            {
                Debug.WriteLine($"[WindowOpened] Title='{args.Window?.Title}'");
            };

            factory.WindowClosed += (_, args) =>
            {
                Debug.WriteLine($"[WindowClosed] Title='{args.Window?.Title}'");
            };

            factory.WindowClosing += (_, args) =>
            {
                // NOTE: Set to True to cancel window closing.
#if false
                args.Cancel = true;
#endif
                Debug.WriteLine(
                    $"[WindowClosing] Title='{args.Window?.Title}', Cancel={args.Cancel}");
            };

            factory.WindowAdded += (_, args) =>
            {
                Debug.WriteLine($"[WindowAdded] Title='{args.Window?.Title}'");
            };

            factory.WindowRemoved += (_, args) =>
            {
                Debug.WriteLine($"[WindowRemoved] Title='{args.Window?.Title}'");
            };

            factory.WindowMoveDragBegin += (_, args) =>
            {
                // NOTE: Set to True to cancel window dragging.
#if false
                args.Cancel = true;
#endif
                Debug.WriteLine(
                    $"[WindowMoveDragBegin] Title='{args.Window?.Title}', Cancel={args.Cancel}, X='{args.Window?.X}', Y='{args.Window?.Y}'");
            };

            factory.WindowMoveDrag += (_, args) =>
            {
                Debug.WriteLine(
                    $"[WindowMoveDrag] Title='{args.Window?.Title}', X='{args.Window?.X}', Y='{args.Window?.Y}");
            };

            factory.WindowMoveDragEnd += (_, args) =>
            {
                Debug.WriteLine(
                    $"[WindowMoveDragEnd] Title='{args.Window?.Title}', X='{args.Window?.X}', Y='{args.Window?.Y}");
            };
        }
    }
}