namespace CMS.Helpers
{
    /// <summary>
    /// Document mark type.
    /// </summary>
    public enum DocumentMarkEnum
    {
        /// <summary>
        /// Document is link mark.
        /// </summary> 
        Link = 1,

        /// <summary>
        /// Document uses redirect mark.
        /// </summary>
        Redirect = 2,

        /// <summary>
        /// Document is not translated mark.
        /// </summary>
        NoTranslation = 3,

        /// <summary>
        /// Document is archived mark.
        /// </summary>
        Archived = 4,

        /// <summary>
        /// Document is checked out mark.
        /// </summary>
        CheckedOut = 5,

        /// <summary>
        /// Document is published mark.
        /// </summary>
        Published = 6,

        /// <summary>
        /// Document is unpublished mark.
        /// </summary>
        Unpublished = 7,

        /// <summary>
        /// Document version is not published.
        /// </summary>
        VersionNotPublished = 8,

        /// <summary>
        /// Document is scheduled to be published.
        /// </summary>
        ScheduledToBePublished = 9,

        /// <summary>
        /// Document is waiting for translation (from external translation service).
        /// </summary>
        DocumentWaitingForTranslation = 10,
    }
}