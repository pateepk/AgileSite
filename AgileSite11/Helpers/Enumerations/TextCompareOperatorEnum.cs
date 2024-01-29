namespace CMS.Helpers
{
    /// <summary>
    /// Specifies possible operators for comparing text (used mostly in CMSFormControls/Filters/TextFilter control).
    /// </summary>
    public enum TextCompareOperatorEnum
    {
        /// <summary>
        /// Like/Contains
        /// </summary>
        Like,

        /// <summary>
        /// Not like/Doesn't contain
        /// </summary>
        NotLike,

        /// <summary>
        /// Equals/=
        /// </summary>
        Equals,

        /// <summary>
        /// Does not equal/!=
        /// </summary>
        NotEquals,

        /// <summary>
        /// Starts with
        /// </summary>
        StartsWith,

        /// <summary>
        /// Doesn't start with
        /// </summary>
        NotStartsWith,

        /// <summary>
        /// Ends with
        /// </summary>
        EndsWith,

        /// <summary>
        /// Does not end with
        /// </summary>
        NotEndsWith,

        /// <summary>
        /// Is empty
        /// </summary>
        Empty,

        /// <summary>
        /// Is not empty
        /// </summary>
        NotEmpty
    }
}