namespace CMS.Helpers
{
    /// <summary>
    /// Type of TextPart difference against other string enumeration.
    /// </summary>
    public enum DiffStatus : int
    {
        /// <summary>
        /// TextPart wasn't processed yet.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// TextPart wasn't included in particular string.
        /// </summary>
        NotIncluded = 1,

        /// <summary>
        /// TextPart is missing in destination string.
        /// </summary>
        RemovedFromSource = 2,

        /// <summary>
        /// TextPart was found both in source and destination string.
        /// </summary>
        Matched = 3,

        /// <summary>
        /// TextPart is missing in source string.
        /// </summary>
        AddedToDestination = 4,

        /// <summary>
        /// TextPart containing HTML markup.
        /// </summary>
        HTMLPart = 5,

        /// <summary>
        /// HTML part which wasn't included in particular string.
        /// </summary>
        HTMLNotIncluded = 6
    }
}