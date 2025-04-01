using System.ComponentModel;

namespace SukiUI.MessageBox;

/// <summary>
/// Specifies the buttons that are displayed on a message box.
/// </summary>
public enum SukiMessageBoxButtons
{
    [Description("[OK]")] OK,
    [Description("[OK] [Cancel]")] OKCancel,
    [Description("[Yes] [No]")] YesNo,
    [Description("[Yes] [No] [Cancel]")] YesNoCancel,
    [Description("[Yes] [Ignore]")] YesIgnore,
    [Description("[Apply] [Cancel]")] ApplyCancel,
    [Description("[Retry] [Cancel]")] RetryCancel,
    [Description("[Retry] [Cancel] [Abort]")] RetryIgnoreAbort,
    [Description("[Retry] [Continue] [Abort]")] RetryContinueCancel,
    [Description("[Close]")] Close
}