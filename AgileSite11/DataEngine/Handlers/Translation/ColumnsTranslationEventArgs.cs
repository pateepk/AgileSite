using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Columns translation event arguments
    /// </summary>
    public class ColumnsTranslationEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Translation helper which keeps translations of objects ID columns. Use this object to register translations or translate column value.
        /// </summary>
        public TranslationHelper TranslationHelper
        {
            get;
            set;
        }


        /// <summary>
        /// Object type which data is stored in the container.
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Data container with object data.
        /// </summary>
        public IDataContainer Data
        {
            get;
            set;
        }
    }
}