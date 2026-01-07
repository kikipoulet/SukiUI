using System.ComponentModel;

namespace SukiUI.MessageBox;

/// <summary>
/// Specifies the buttons that are displayed on a message box.
/// </summary>
public enum SukiMessageBoxButtons
{
    [Description("[OK]")] OK,
    [Description("[OK] [Abort]")] OKAbort,
    [Description("[OK] [Cancel]")] OKCancel,
    [Description("[Yes] [No]")] YesNo,
    [Description("[Yes] [No] [Abort]")] YesNoAbort,
    [Description("[Yes] [No] [Cancel]")] YesNoCancel,
    [Description("[Yes] [Ignore]")] YesIgnore,
    [Description("[Apply] [Cancel]")] ApplyCancel,
    [Description("[Retry] [Cancel]")] RetryCancel,
    [Description("[Retry] [Ignore] [Abort]")] RetryIgnoreAbort,
    [Description("[Retry] [Continue] [Cancel]")] RetryContinueCancel,
    [Description("[Close]")] Close,
}