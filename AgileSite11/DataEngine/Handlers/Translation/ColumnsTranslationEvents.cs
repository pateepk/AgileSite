namespace CMS.DataEngine
{
    /// <summary>
    /// Columns translation events handlers.
    /// </summary>
    public static class ColumnsTranslationEvents
    {
        /// <summary>
        /// Fires when an objects data is registered for translation.
        /// </summary>
        public static ColumnsTranslationHandler RegisterRecords = new ColumnsTranslationHandler { Name = "ColumnsTranslationEvents.RegisterRecords" };


        /// <summary>
        /// Fires when an object ID columns are translated.
        /// </summary>
        public static ColumnsTranslationHandler TranslateColumns = new ColumnsTranslationHandler { Name = "ColumnsTranslationEvents.TranslateColumns" };
    }
}