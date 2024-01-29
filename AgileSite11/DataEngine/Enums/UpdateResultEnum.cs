namespace CMS.DataEngine
{
    /// <summary>
    /// Result of the update operation.
    /// </summary>
    public enum UpdateResultEnum
    {
        /// <summary>
        /// Not processed.
        /// </summary>
        NotProcessed,

        /// <summary>
        /// Imported correctly.
        /// </summary>
        OK,

        /// <summary>
        /// Imported with errors.
        /// </summary>
        Error,

        /// <summary>
        /// The object was skipped.
        /// </summary>
        Skipped,

        /// <summary>
        /// Imported, but post processing required.
        /// </summary>
        PostProcess
    }
}