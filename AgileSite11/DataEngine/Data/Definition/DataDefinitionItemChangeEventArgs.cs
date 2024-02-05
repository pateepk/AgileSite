using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Data definition item change arguments
    /// </summary>
    public class DataDefinitionItemChangeEventArgs : DataDefinitionItemEventArgs
    {
        /// <summary>
        /// Original data item
        /// </summary>
        public IDataDefinitionItem OriginalItem
        {
            get;
            internal set;
        }
    }
}