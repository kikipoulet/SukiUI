using System;
using SukiUI.Enums;

namespace SukiUI.Models;

public readonly record struct ToastModel(string Title, object Content, ToastType Type = ToastType.Info, TimeSpan? Lifetime = null, Action? OnClicked = null, string? ActionButtonContent = null,Action? ActionButton= null)
{
    public string Title { get; } = Title;
    public object Content { get; } = Content;
    public ToastType Type { get; } = Type;
    public TimeSpan? Lifetime { get; } = Lifetime ;
    public Action? OnClicked { get; } = OnClicked;

    public string? ActionButtonContent { get; } = ActionButtonContent;
    public Action? OnActionButtonClicked { get; } = ActionButton;
}