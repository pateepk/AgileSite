namespace CMS.DocumentEngine
{
    /// <summary>
    /// Path type enumeration.
    /// </summary>
    public enum PathTypeEnum
    {
        /// <summary>
        /// Path used as it is (no additional modification).
        /// </summary>
        Explicit = 0,

        /// <summary>
        /// Path representing single document.
        /// </summary>
        Single = 1,

        /// <summary>
        /// Path representing all child documents.
        /// </summary>
        Children = 2,

        /// <summary>
        /// Path representing document and its child documents.
        /// </summary>
        Section = 3,
    }
}