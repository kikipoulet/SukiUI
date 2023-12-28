using System;
using Avalonia.Controls.Primitives;

namespace SukiUI.Controls;

/// <summary>
/// Hosts both Dialogs and Notifications
/// </summary>
public class SukiHost : TemplatedControl
{
    protected override Type StyleKeyOverride => typeof(SukiHost);
}