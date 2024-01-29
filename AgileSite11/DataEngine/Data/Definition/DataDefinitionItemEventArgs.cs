using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Data definition item arguments
    /// </summary>
    public class DataDefinitionItemEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Data item
        /// </summary>
        public IDataDefinitionItem Item
        {
            get;
            internal set;
        }


        /// <summary>
        /// Class being edited
        /// </summary>
        public DataClassInfo ClassInfo
        {
            get;
            internal set;
        }
    }
}