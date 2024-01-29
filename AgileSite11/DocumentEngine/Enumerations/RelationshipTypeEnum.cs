namespace CMS.DocumentEngine
{
    /// <summary>
    /// Relationship side enumeration.
    /// </summary>
    public enum RelationshipSideEnum
    {
        /// <summary>
        /// Document participates on both sides.
        /// </summary>
        Both = 0,

        /// <summary>
        /// Document participates on left side only.
        /// </summary>
        Left = 1,

        /// <summary>
        /// Document participates on right side only.
        /// </summary>
        Right = 2,
    }
}