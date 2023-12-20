namespace CMS.Helpers
{
    /// <summary>
    /// Type of TextPart sorting enumeration.
    /// </summary>
    public enum ComparisonTextPartSortBy : int
    {
        /// <summary>
        /// Default setting, sorting using text length property.
        /// </summary>
        TextLength = 0,

        /// <summary>
        /// Sorting using index to source string.
        /// </summary>
        SrcIndex = 1,

        /// <summary>
        /// Sorting using index to destination string.
        /// </summary>
        DestIndex = 2,

        /// <summary>
        /// Sorting using lower index value.
        /// </summary>
        BothIndexes = 3
    }
}