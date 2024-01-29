namespace CMS.TranslationServices
{
    /// <summary>
    /// Translation events
    /// </summary>
    public static class TranslationEvents
    {
        /// <summary>
        /// Fires when the system and service culture codes are mapped
        /// </summary>
        public static TranslationCultureMappingHandler MapCultureCode = new TranslationCultureMappingHandler
        {
            Name = "TranslationEvents.MapCultureCode",
        };


        /// <summary>
        /// Fires when the translation is being processed
        /// </summary>
        public static ProcessTranslationHandler ProcessTranslation = new ProcessTranslationHandler
        {
            Name = "TranslationEvents.ProcessTranslation",
        };


        /// <summary>
        /// Fires when the submission item is being created
        /// </summary>
        public static CreateSubmissionItemHandler CreateSubmissionItem = new CreateSubmissionItemHandler
        {
            Name = "TranslationEvents.CreateSubmissionItem",
        };
    }
}