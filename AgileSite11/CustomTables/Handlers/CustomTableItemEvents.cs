namespace CMS.CustomTables
{
    /// <summary>
    /// Custom table item events
    /// </summary>
    public static class CustomTableItemEvents
    {
        /// <summary>
        /// Fires when custom table item is updated
        /// </summary>
        public static CustomTableItemHandler Update = new CustomTableItemHandler { Name = "CustomTableItemEvents.Update" };


        /// <summary>
        /// Fires when custom table item is inserted
        /// </summary>
        public static CustomTableItemHandler Insert = new CustomTableItemHandler { Name = "CustomTableItemEvents.Insert" };


        /// <summary>
        /// Fires when custom table item is deleted
        /// </summary>
        public static CustomTableItemHandler Delete = new CustomTableItemHandler { Name = "CustomTableItemEvents.Delete" };
    }
}