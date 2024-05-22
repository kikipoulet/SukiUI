using System;
using SukiUI.Enums;

namespace SukiUI.Models;

public readonly record struct ToastModel(string Title, object Content, ToastType Type, TimeSpan Lifetime, Action? OnClicked)
{
    public string Title { get; } = Title;
    public object Content { get; } = Content;
    public ToastType Type { get; } = Type;
    public TimeSpan Lifetime { get; } = Lifetime;
    public Action? OnClicked { get; } = OnClicked;
}