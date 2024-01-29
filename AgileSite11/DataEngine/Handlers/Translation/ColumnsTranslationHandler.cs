using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Columns translation event handler.
    /// </summary>
    public class ColumnsTranslationHandler : SimpleHandler<ColumnsTranslationHandler, ColumnsTranslationEventArgs>
    {
        /// <summary>
        /// Initiates the data registration event handling.
        /// </summary>
        /// <param name="th">Translation helper</param>
        /// <param name="objectType">Object type</param>
        /// <param name="data">Data container</param>
        public ColumnsTranslationEventArgs StartEvent(TranslationHelper th, string objectType, IDataContainer data)
        {
            var e = new ColumnsTranslationEventArgs
            {
                TranslationHelper = th,
                ObjectType = objectType,
                Data = data
            };

            return StartEvent(e);
        }
    }
}