using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.DataProtection;
using CMS.Ecommerce;
using CMS.Helpers;

namespace DataProtection
{
    /// <summary>
    /// Sample implementation of <see cref="IPersonalDataEraser"/> interface for erasing customer's personal data.
    /// </summary>
    internal class SampleCustomerPersonalDataEraser : IPersonalDataEraser
    {
        /// <summary>
        /// Erases personal data based on given <paramref name="identities"/> and <paramref name="configuration"/>.
        /// </summary>
        /// <param name="identities">Collection of identities representing a data subject.</param>
        /// <param name="configuration">Configures which personal data should be erased.</param>
        /// <remarks>
        /// The erasure process can be configured via the following <paramref name="configuration"/> parameters:
        /// <list type="bullet">
        /// <item>
        /// <term>deleteCustomers</term>
        /// <description>Flag indicating whether customer(s) contained in <paramref name="identities"/> are to be deleted.</description>
        /// </item>
        /// <item>
        /// <term>deleteOrdersOlderThanYears</term>
        /// <description>Number indicating how old orders are to be deleted. Omit the parameter to keep all orders.</description>
        /// </item>
        /// <item>
        /// <term>deleteShoppingCarts</term>
        /// <description>Flag indicating whether shopping carts associated with customer(s), user(s) or contact(s) are to be deleted.</description>
        /// </item>
        /// <item>
        /// <term>deleteWishlists</term>
        /// <description>Flag indicating whether wishlists associated with customer(s) are to be deleted.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public void Erase(IEnumerable<BaseInfo> identities, IDictionary<string, object> configuration)
        {
            var customers = identities.OfType<CustomerInfo>().ToList();
            var customerIds = customers.Select(c => c.CustomerID).ToList();

            // Get contact ids
            var contactIds = identities.OfType<ContactInfo>().Select(c => c.ContactID).ToList();

            using (new CMSActionContext { CreateVersion = false })
            {
                DeleteOrders(customerIds, configuration);

                DeleteShoppingCarts(customerIds, contactIds, configuration);

                DeleteWishlists(customers, configuration);

                DeleteCustomers(customers, configuration);
            }
        }


        /// <summary>
        /// Deletes orders based on <paramref name="configuration"/>'s <c>deleteOrdersOlderThanYears</c> number for given <paramref name="customerIds"/>.
        /// </summary>
        private void DeleteOrders(List<int> customerIds, IDictionary<string, object> configuration)
        {
            object ordersOlderThanYears;
            if (configuration.TryGetValue("deleteOrdersOlderThanYears", out ordersOlderThanYears))
            {
                int years = ValidationHelper.GetInteger(ordersOlderThanYears, 0);
                DeleteOrdersCore(customerIds, DateTime.Now.AddYears(-years));
            }
        }


        /// <summary>
        /// Deletes orders <paramref name="ordersOlderThan"/> for given <paramref name="customerIds"/>.
        /// </summary>
        private void DeleteOrdersCore(List<int> customerIds, DateTime ordersOlderThan)
        {
            var orders = OrderInfoProvider.GetOrders()
                .WhereLessThan("OrderDate", ordersOlderThan)
                .WhereIn("OrderCustomerID", customerIds);

            foreach (var order in orders)
            {
                OrderInfoProvider.DeleteOrderInfo(order);
            }
        }


        /// <summary>
        /// Deletes shopping carts for given <paramref name="customerIds"/> and <paramref name="contactIds"/>
        /// based on <paramref name="configuration"/>'s <c>deleteShoppingCarts</c> flag.
        /// </summary>
        private void DeleteShoppingCarts(List<int> customerIds, List<int> contactIds, IDictionary<string, object> configuration)
        {
            object deleteShoppingCarts;
            if (configuration.TryGetValue("deleteShoppingCarts", out deleteShoppingCarts) && ValidationHelper.GetBoolean(deleteShoppingCarts, false))
            {
                DeleteShoppingCartsCore(customerIds, contactIds);
            }
        }


        /// <summary>
        /// Deletes shopping carts for given <paramref name="customerIds"/> and <paramref name="contactIds"/>
        /// </summary>
        private void DeleteShoppingCartsCore(List<int> customerIds, List<int> contactIds)
        {
            var carts = ShoppingCartInfoProvider.GetShoppingCarts();

            if (customerIds.Count > 0)
            {
                carts.Or().WhereIn("ShoppingCartCustomerID", customerIds);
            }

            if (contactIds.Count > 0)
            {
                carts.Or().WhereIn("ShoppingCartContactID", contactIds);
            }

            foreach (var cart in carts.ToList())
            {
                ShoppingCartInfoProvider.DeleteShoppingCartInfo(cart);
            }
        }


        /// <summary>
        /// Deletes wishlists for given <paramref name="customers"/> based on <paramref name="configuration"/>'s <c>deleteWishlists</c> flag.
        /// </summary>
        private void DeleteWishlists(List<CustomerInfo> customers, IDictionary<string, object> configuration)
        {
            object deleteWishlists;
            if (configuration.TryGetValue("deleteWishlists", out deleteWishlists) && ValidationHelper.GetBoolean(deleteWishlists, false))
            {
                var wishlists = WishlistItemInfoProvider.GetWishlistItems()
                    .WhereIn("UserID", customers.Select(c => c.CustomerUserID).ToList());

                foreach (var wishlist in wishlists)
                {
                    wishlist.Delete();
                }
            }
        }


        /// <summary>
        /// Deletes <paramref name="customers"/> based on <paramref name="configuration"/>'s <c>deleteCustomers</c> flag.
        /// </summary>
        private static void DeleteCustomers(IEnumerable<CustomerInfo> customers, IDictionary<string, object> configuration)
        {
            object deleteCustomers;
            if (configuration.TryGetValue("deleteCustomers", out deleteCustomers) && ValidationHelper.GetBoolean(deleteCustomers, false))
            {
                foreach (var customer in customers)
                {
                    CustomerInfoProvider.DeleteCustomerInfo(customer);
                }
            }
        }
    }
}
