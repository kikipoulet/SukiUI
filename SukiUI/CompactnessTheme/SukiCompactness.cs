namespace SukiUI.Enums
{
    /// <summary>
    /// How tight the UI is: the size of paddings, margins, fonts and fixed control dimensions.
    /// Each control declares its own values for every level in a <c>*.Compactness.axaml</c> file
    /// next to its template; <see cref="Normal"/> holds the historical values.
    /// </summary>
    public enum SukiCompactness
    {
        SuperCompact,
        Compact,
        Normal,
        Big
    }
}
