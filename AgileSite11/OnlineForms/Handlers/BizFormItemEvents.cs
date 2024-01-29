namespace CMS.OnlineForms
{
    /// <summary>
    /// BizForm item events
    /// </summary>
    public static class BizFormItemEvents
    {
        /// <summary>
        /// Fires when BizForm item is updated
        /// </summary>
        public static BizFormItemHandler Update = new BizFormItemHandler { Name = "BizFormItemEvents.Update" };


        /// <summary>
        /// Fires when BizForm item is inserted
        /// </summary>
        public static BizFormItemHandler Insert = new BizFormItemHandler { Name = "BizFormItemEvents.Insert" };


        /// <summary>
        /// Fires when BizForm item is deleted
        /// </summary>
        public static BizFormItemHandler Delete = new BizFormItemHandler { Name = "BizFormItemEvents.Delete" };
    }
}