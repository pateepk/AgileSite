namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Class represents an item in <see cref="PopUpWindow"/>'s list.
    /// </summary>
    internal class ABTestVariantListItem
    {
        /// <summary>
        /// Display name of the item.
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Unique key of the item.
        /// </summary>
        public string Key { get; set; }


        /// <summary>
        /// Indicates the state of the edit action for this item.
        /// </summary>
        /// <remarks>
        /// In order for edit to work, Javascript function has to be set in <see cref="ABTestVariantListing.EditItemCallback"/> property.
        /// </remarks>
        public ABTestVariantListItemActionState EditActionState { get; set; }

        
        /// <summary>
        /// Indicates the state of the remove action for this item.
        /// </summary>
        /// <remarks>
        /// In order for remove to work, Javascript function has to be set in <see cref="ABTestVariantListing.RemoveItemCallback"/> property.
        /// </remarks>
        public ABTestVariantListItemActionState RemoveActionState { get; set; }
    }
}
