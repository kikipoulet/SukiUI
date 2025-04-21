
﻿using System;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Dialogs;
using SukiUI.MessageBox;
using SukiUI.Toasts;
using System.Text;
using SukiUI.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs;

public partial class DialogsViewModel(ISukiDialogManager dialogManager, ISukiToastManager toastManager) : DemoPageBase("Dialogs", MaterialIconKind.Forum)
{
    private readonly ISukiDialogManager _dialogManager = dialogManager;

    public NotificationType[] NotificationTypes { get; } = Enum.GetValues<NotificationType>();

    [ObservableProperty] private NotificationType _selectedType;

    public SukiMessageBoxIcons[] MessageBoxIcons { get; } = Enum.GetValues<SukiMessageBoxIcons>();

    [ObservableProperty] private SukiMessageBoxIcons _selectedMessageBoxIcon = SukiMessageBoxIcons.Information;

    public SukiMessageBoxButtons[] MessageBoxButtons { get; } = Enum.GetValues<SukiMessageBoxButtons>();

    [ObservableProperty] private SukiMessageBoxButtons _selectedMessageBoxButtons = SukiMessageBoxButtons.YesNoCancel;
    [ObservableProperty] private bool _useAlternativeHeaderStyle;
    [ObservableProperty] private bool _showHeaderContentSeparator = true;
    [ObservableProperty] private bool _useNativeWindow;

    private void ShowOptionToast(int option)
    {
        toastManager.CreateToast()
            .WithTitle("Dialog Option Clicked")
            .WithContent($"You clicked option #{option}")
            .Dismiss().ByClicking()
            .Dismiss().After(TimeSpan.FromSeconds(3))
            .Queue();
    }

    [RelayCommand]
    private void OpenDialogWindowDemo()
        => new DialogWindowDemo(_dialogManager).Show();

    [RelayCommand]
    private void OpenDialogNativeWindowDemo()
    {
        var dialogHost = new SukiDialogHost
        {
            Manager = new SukiDialogManager()
        };
        var toastHost = new SukiToastHost
        {
            Manager = new SukiToastManager()
        };

        var window = new Window
        {
            Title = "SukiUI in a stock Avalonia Window",
            Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://SukiUI.Demo/Assets/OIG.N5o-removebg-preview.png"))),
            Content = new SukiMainHost
            {
                Hosts = [dialogHost, toastHost],
                Content = new DialogsViewModel(dialogHost.Manager, toastHost.Manager)
            }
        };

        window.Show();
    }

    [RelayCommand]
    private void OpenToolWindowDemo()
    {
        var sb = new StringBuilder();

        for (int i = 0; i < 100; i++)
        {
            sb.AppendLine(
                "Lorem Ipsum is simply dummy text of the printing and typesetting industry. " +
                "Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. " +
                "It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. " +
                "It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, " +
                "and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.");
            sb.AppendLine();
        }

        var model = new ToolWindowModel
        {
            Header = "Testing a tool window dialog with auto size constrained to a max-size related to screen",
            Message = sb.ToString(),
            MaxHeightScreenRatio = 0.6,
            MaxWidthScreenRatio = 0.6
        };

        var window = new ToolWindow
        {
            Title = "Tool Window Demo",
            DataContext = model,
        };

        window.Show();
    }
}