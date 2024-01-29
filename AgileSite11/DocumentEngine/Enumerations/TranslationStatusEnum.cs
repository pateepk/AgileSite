namespace CMS.DocumentEngine
{
    /// <summary>
    /// Translation status enumeration.
    /// </summary>
    public enum TranslationStatusEnum
    {
        /// <summary>
        /// Translation of document is up to date.
        /// </summary>
        Translated = 0,

        /// <summary>
        /// Translation is out of date.
        /// </summary>
        Outdated = 1,

        /// <summary>
        /// Translation is not avialable.
        /// </summary>
        NotAvailable = 2,


        /// <summary>
        /// Waiting for translation - submitted to translation service
        /// </summary>
        WaitingForTranslation = 3
    }
}