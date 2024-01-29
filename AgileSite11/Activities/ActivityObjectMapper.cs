using CMS.DataEngine;

namespace CMS.Activities
{
    /// <summary>
    /// Object used to map activity to its related objects, for example Forum post activity to related forum.
    /// </summary>
    public static class ActivityObjectMapper
    {
        #region "Methods"

        /// <summary>
        /// Gets function to resolve object based on activity type and given object type.
        /// </summary>
        public static BaseInfo GetLinkedObject(ActivityInfo info, string predefinedObjectType)
        {
            switch (info.ActivityType)
            {
                case PredefinedActivityType.POLL_VOTING:
                    if (predefinedObjectType == PredefinedObjectType.POLL)
                    {
                        return ProviderHelper.GetInfoById(PredefinedObjectType.POLL, info.ActivityItemID);
                    }
                    break;
                case PredefinedActivityType.BIZFORM_SUBMIT:
                    if (predefinedObjectType == PredefinedObjectType.BIZFORM)
                    {
                        return ProviderHelper.GetInfoById(PredefinedObjectType.BIZFORM, info.ActivityItemID);
                    }
                    break;
                case PredefinedActivityType.FORUM_POST:
                    if (predefinedObjectType == PredefinedObjectType.FORUM)
                    {
                        return ProviderHelper.GetInfoById(PredefinedObjectType.FORUM, info.ActivityItemID);
                    }
                    break;
                case PredefinedActivityType.MESSAGE_BOARD_COMMENT:
                    if (predefinedObjectType == PredefinedObjectType.BOARD)
                    {
                        return ProviderHelper.GetInfoById(PredefinedObjectType.BOARD, info.ActivityItemID);
                    }
                    break;
                case PredefinedActivityType.PRODUCT_ADDED_TO_SHOPPINGCART:
                case PredefinedActivityType.PRODUCT_ADDED_TO_WISHLIST:
                case PredefinedActivityType.PRODUCT_REMOVED_FROM_SHOPPINGCART:
                case PredefinedActivityType.PURCHASE:
                case PredefinedActivityType.PURCHASEDPRODUCT:
                    if (predefinedObjectType == PredefinedObjectType.SKU)
                    {
                        return ProviderHelper.GetInfoById(PredefinedObjectType.SKU, info.ActivityItemID);
                    }
                    break;
                case PredefinedActivityType.NEWSLETTER_CLICKTHROUGH:
                case PredefinedActivityType.NEWSLETTER_OPEN:
                case PredefinedActivityType.NEWSLETTER_SUBSCRIBING:
                case PredefinedActivityType.NEWSLETTER_UNSUBSCRIBING:
                    if (predefinedObjectType == PredefinedObjectType.NEWSLETTERISSUE)
                    {
                        return ProviderHelper.GetInfoById(PredefinedObjectType.NEWSLETTERISSUE, info.ActivityItemID);
                    }
                    if (predefinedObjectType == PredefinedObjectType.NEWSLETTER)
                    {
                        return ProviderHelper.GetInfoById(PredefinedObjectType.NEWSLETTER, info.ActivityItemDetailID);
                    }
                    break;
            }
            return null;
        }

        #endregion
    }
}