using System.Collections.Generic;
using System.Linq;

using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Ecommerce;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Helper methods for fetching various data needed mainly by Strands Recommendation web part. In general, data are fetched from the contexts (i.e. EcommerceContext) and database.
    /// </summary>
    public static class StrandsProductsProvider
    {
        #region "Public methods"

        /// <summary>
        /// Gets all products from current user shopping cart and finds corresponding nodes.
        /// </summary>
        /// <returns>Collection containing all node IDs representing current user shopping cart items</returns>
        public static IEnumerable<string> GetItemIDsFromCurrentShoppingCart()
        {
            var currentUserShoppingCart = ECommerceContext.CurrentShoppingCart;
            if (currentUserShoppingCart != null)
            {
                var skuIDs = currentUserShoppingCart
                    .CartProducts
                    .Where(c => c.SKUID != 0)
                    .Select(c => GetSKURootID(c.SKU));  
                return GetNodeIDsForSKUs(skuIDs);
            }
            return Enumerable.Empty<string>();
        }


        /// <summary>
        /// Gets all products from current customer/user most recent order and finds corresponding nodes.
        /// </summary>
        /// <returns>Collection containing all node IDs representing current customer/user most recent order</returns>
        public static IEnumerable<string> GetItemsIDsFromRecentOrder()
        {
            // If EcommerceContext does not contain CurrentCustomer what will happen when current user is Anonymous, try to get Customer related to current Contact
            var currentCustomer = ECommerceContext.CurrentCustomer ?? GetCustomerRelatedToCurrentContact();

            if (currentCustomer == null)
            {
                return Enumerable.Empty<string>();
            }
            
            var lastOrder = OrderInfoProvider.GetOrders()
                .OnSite(SiteContext.CurrentSiteID)
                .WhereEquals("OrderCustomerID", currentCustomer.CustomerID)
                .OrderByDescending("OrderDate")
                .FirstObject;
            
            if (lastOrder != null)
            {
                var skuIDs = OrderItemInfoProvider.GetOrderItems(lastOrder.OrderID)
                    .Select(c => GetSKURootID(c.OrderItemSKU));

                return GetNodeIDsForSKUs(skuIDs);
            }

            return Enumerable.Empty<string>();
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets Customer ID related to current contact.
        /// </summary>
        /// <returns>ID of related customer</returns>
        private static CustomerInfo GetCustomerRelatedToCurrentContact()
        {
            var customerIDsRelatedToCurrentContact = ContactMembershipInfoProvider.GetRelationships()
                .WhereEquals("ContactID", ContactManagementContext.CurrentContactID)
                .WhereEquals("MemberType", MemberTypeEnum.EcommerceCustomer)
                .Column("RelatedID");

            // Get Customer related to current contact which was created most recently
            return CustomerInfoProvider.GetCustomers()
                .WhereIn("CustomerID", customerIDsRelatedToCurrentContact)
                .OrderByDescending("CustomerCreated")
                .FirstObject;
        }


        /// <summary>
        /// Gets tree node for every SKU item given and returns its nodeID.
        /// </summary>
        /// <param name="skuIDs">Collection of SKU items</param>
        /// <returns>Collection containing nodeIDs of all tree nodes corresponding to given SKU items</returns>
        private static IEnumerable<string> GetNodeIDsForSKUs(IEnumerable<int> skuIDs)
        {
            skuIDs = skuIDs.ToList();
            if (!skuIDs.Any())
            {
                return Enumerable.Empty<string>();
            }
            
            return DocumentHelper.GetDocuments(
                     SiteContext.CurrentSiteName,
                     TreeProvider.ALL_DOCUMENTS,
                     TreeProvider.ALL_CULTURES,
                     false,
                     TreeProvider.ALL_CLASSNAMES,
                     string.Format("[SKUID] IN ({0})", string.Join(",", skuIDs)),
                     null,
                     TreeProvider.ALL_LEVELS,
                     true,
                     new TreeProvider(AuthenticationHelper.GlobalPublicUser)
                 ).Cast<SKUTreeNode>()
                 .GroupBy(c => c.SKU.SKUID)  // Since one SKU can be displayed within several documents, IDs are grouped to remove duplicates
                 .Select(c => StrandsCatalogPropertiesMapper.GetItemID(c.First()));     // Items in Strands are defined by node ID
        }


        /// <summary>
        /// Get SKUID of SKU parent, if exists. Otherwise gets ID of SKU itself.
        /// </summary>
        /// <param name="sku">SKU should be ID taken from</param>
        /// <remarks>If SKU is of the type variant, it does not have connection with tree node, whose ID is used for item identification. Thus its SKU parent tree node ID has to be used instead</remarks>
        /// <returns>SKUParentID, if not null, SKUID otherwise</returns>
        private static int GetSKURootID(SKUInfo sku)
        {
            return sku.SKUParentSKUID != 0 ? sku.SKUParentSKUID : sku.SKUID;
        }

        #endregion
    }
}
