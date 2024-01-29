namespace CMS.DataEngine
{
    /// <summary>
    /// Events for data definition item handling
    /// </summary>
    public class DataDefinitionItemEvents
    {
        /// <summary>
        /// Fires after a definition item was added
        /// </summary>
        public static DataDefinitionItemHandler AddItem = new DataDefinitionItemHandler
        {
            Name = "DataDefinitionItemEvents.AddItem"
        };


        /// <summary>
        /// Fires before a definition item was removed
        /// </summary>
        public static DataDefinitionItemHandler RemoveItem = new DataDefinitionItemHandler
        {
            Name = "DataDefinitionItemEvents.RemoveItem"
        };

    
        /// <summary>
        /// Fires after a definition item was changed
        /// </summary>
        public static DataDefinitionItemChangeHandler ChangeItem = new DataDefinitionItemChangeHandler
        {
            Name = "DataDefinitionItemEvents.ChangeItem"
        };
    }
}