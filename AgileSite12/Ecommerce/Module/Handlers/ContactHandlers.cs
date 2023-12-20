using CMS.Base;
using CMS.ContactManagement;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides handlers for Ecommerce and contact interaction.
    /// </summary>
    internal static class ContactHandlers
    {
        /// <summary>
        /// Initializes the contact handlers.
        /// </summary>
        public static void Init()
        {
            ContactManagementEvents.ContactMerge.Execute += MoveShoppingCarts;
        }


        /// <summary>
        /// Moves all shopping carts from the given source contact to the target contact.
        /// </summary>
        private static void MoveShoppingCarts(object sender, CMSEventArgs<ContactMergeModel> e)
        {
            var contactMergeModel = e.Parameter;

            ShoppingCartInfoProvider.GetShoppingCarts().WhereEquals("ShoppingCartContactID", contactMergeModel.SourceContact.ContactID).ForEachObject(shoppingCart =>
            {
                shoppingCart.SetValue("ShoppingCartContactID", contactMergeModel.TargetContact.ContactID);
                shoppingCart.Update();
            });
        }
    }
}