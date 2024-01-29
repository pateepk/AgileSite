using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Data definition item change handler
    /// </summary>
    public class DataDefinitionItemChangeHandler : AdvancedHandler<DataDefinitionItemChangeHandler, DataDefinitionItemChangeEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="classInfo">Data class info</param>
        /// <param name="originalItem">Original data item</param>
        /// <param name="item">Current data item</param>
        public DataDefinitionItemChangeHandler StartEvent(DataClassInfo classInfo, IDataDefinitionItem originalItem, IDataDefinitionItem item)
        {
            var e = new DataDefinitionItemChangeEventArgs
            {
                ClassInfo = classInfo,
                OriginalItem = originalItem,
                Item = item
            };

            return StartEvent(e);
        }
    }
}