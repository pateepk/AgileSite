namespace CMS.Helpers
{
    /// <summary>
    /// Type of rendered diff text.
    /// </summary>
    public enum TextComparisonTypeEnum : int
    {
        /// <summary>
        /// Default setting, render source diff text.
        /// </summary>
        SourceText = 0,

        /// <summary>
        /// Render destination diff text.
        /// </summary>
        DestinationText = 1
    }
}