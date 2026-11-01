using Avalonia.Controls;
using SukiUI.Extensions;

namespace SukiUI.MessageBox;

/// <summary>
/// Represents the metadata and actions associated with a button in a Suki message box, including its result, owner
/// window, and an optional link to open.
/// </summary>
/// <remarks>Use this class to encapsulate information about a message box button, such as the result it
/// represents and an optional link that can be opened when the button is activated. The class provides methods to open
/// the associated link, which may refer to a file, directory, or URI, using the context of the specified owner
/// window.</remarks>
public class SukiMessageBoxButtonTag
{
    /// <summary>
    /// Gets or sets the window that owns this window.
    /// </summary>
    /// <remarks>Setting the owner establishes an ownership relationship between windows, which can affect
    /// window modality, activation, and positioning. The owner must be a window that is not closed. If set to null, the
    /// window will have no owner.</remarks>
    public Window? Owner { get; set; }

    /// <summary>
    /// Gets or sets the result selected by the user in the message box, or null if no selection has been made.
    /// </summary>
    public SukiMessageBoxResult? Result { get; set; }

    /// <summary>
    /// Gets or sets the link to open associated with the item.
    /// </summary>
    public string? Link { get; set; }

    /// <summary>
    /// Gets or sets an optional tag associated with the object.
    /// </summary>
    public object? Tag { get; set; }

    public SukiMessageBoxButtonTag()
    {
    }

    public SukiMessageBoxButtonTag(SukiMessageBoxResult result)
    {
        Result = result;
    }

    /// <summary>
    /// Attempts to open the link specified by the current instance as a file, directory, or URI using the associated
    /// launcher.
    /// </summary>
    /// <remarks>The method checks whether the link refers to an existing file, directory, or a valid URI, and
    /// attempts to open it accordingly. If the link is not valid or the owner is not set, the method returns <see
    /// langword="false"/>.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the link was
    /// successfully opened; otherwise, <see langword="false"/>.</returns>
    public Task<bool> OpenLink()
    {
        return Owner is null
            ? Task.FromResult(false)
            : Owner.LaunchLinkAsync(Link);
    }
}