using System;

namespace SukiUI.Models;

public readonly record struct SukiToastModel(string Title, object Content, TimeSpan Lifetime)
{
    public string Title { get; } = Title;
    public object Content { get; } = Content;
    public TimeSpan Lifetime { get; } = Lifetime;
}