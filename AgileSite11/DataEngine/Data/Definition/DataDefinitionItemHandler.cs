using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Data definition item handler
    /// </summary>
    public class DataDefinitionItemHandler : AdvancedHandler<DataDefinitionItemHandler, DataDefinitionItemEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="classInfo">Data class info</param>
        /// <param name="item">Data item</param>
        public DataDefinitionItemHandler StartEvent(DataClassInfo classInfo, IDataDefinitionItem item)
        {
            var e = new DataDefinitionItemEventArgs
            {
                ClassInfo = classInfo,
                Item = item
            };

            return StartEvent(e);
        }
    }
}