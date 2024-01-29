namespace CMS.Helpers
{
    /// <summary>
    /// Discussion action enumeration.
    /// </summary>
    public enum DiscussionActionEnum : int
    {
        /// <summary>
        /// Insert URL (using simple dialog).
        /// </summary>
        InsertURL = 0,

        /// <summary>
        /// Insert image (using simple dialog).
        /// </summary>
        InsertImage = 1,

        /// <summary>
        /// Insert quote.
        /// </summary>
        InsertQuote = 2,

        /// <summary>
        /// Insert code snippet.
        /// </summary>
        InsertCode = 3,

        /// <summary>
        /// Change font to bold format.
        /// </summary>
        FontBold = 4,

        /// <summary>
        /// Change font to italics format.
        /// </summary>
        FontItalics = 5,

        /// <summary>
        /// Change font to underline format.
        /// </summary>
        FontUnderline = 6,

        /// <summary>
        /// Change font to strike format.
        /// </summary>
        FontStrike = 7,

        /// <summary>
        /// Change font color.
        /// </summary>
        FontColor = 8,

        /// <summary>
        /// Insert URL (using advanced dialog).
        /// </summary>
        InsertAdvancedURL = 9,

        /// <summary>
        /// Insert simple Image (using advanced dialog).
        /// </summary>
        InsertAdvancedImage = 10,
    }
}