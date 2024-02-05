using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Globalization;

namespace DataProtection
{
    /// <summary>
    /// Class responsible for retrieving customer's personal data. 
    /// </summary>
    internal class SampleCustomerDataCollectorCore
    {
        // Lists store Tuples of database column names and their corresponding display names.
        private readonly List<CollectedColumn> customerInfoColumns = new List<CollectedColumn> {
            new CollectedColumn("CustomerFirstName", "First name"),
            new CollectedColumn("CustomerLastName", "Last name"),
            new CollectedColumn("CustomerEmail", "Email"),
            new CollectedColumn("CustomerPhone", "Phone"),
            new CollectedColumn("CustomerCompany", "Company"),
            new CollectedColumn("CustomerLastModified", "Last modified"),
            new CollectedColumn("CustomerCreated", "Created"),
            new CollectedColumn("CustomerID", "ID"),
        };

        private readonly List<CollectedColumn> orderInfoColumns = new List<CollectedColumn> {
            new CollectedColumn("OrderGUID", "GUID"),
            new CollectedColumn("OrderID", ""),
            new CollectedColumn("OrderTotalPrice", "Total price"),
            new CollectedColumn("OrderTotalShipping", "Total shipping price"),
            new CollectedColumn("OrderTotalTax", "Total tax"),
            new CollectedColumn("OrderTotalPriceInMainCurrency", "Total price in main currency"),
            new CollectedColumn("OrderGrandTotal", "Grand total"),
            new CollectedColumn("OrderGrandTotalInMainCurrency", "Grand total in main currency"),
            new CollectedColumn("OrderIsPaid", "Is paid"),
            new CollectedColumn("OrderDate", "Date" ),
            new CollectedColumn("OrderTrackingNumber", "Tracking number"),
            new CollectedColumn("OrderLastModified", "Last modified"),
            new CollectedColumn("OrderCulture", "Culture"),
            new CollectedColumn("OrderShippingAddressID", ""),
            new CollectedColumn("OrderBillingAddressID", "")
        };

        private readonly List<CollectedColumn> skuInfoColumns = new List<CollectedColumn> {
            new CollectedColumn("SKUNumber", "Product number"),
            new CollectedColumn("SKUName", "Product name")
        };

        private readonly List<CollectedColumn> orderItemInfoColumns = new List<CollectedColumn> {
            new CollectedColumn("OrderItemUnitCount", "Unit count"),
            new CollectedColumn("OrderItemLastModified", "Last modified"),
            new CollectedColumn("OrderItemTotalPrice", "Total price"),
            new CollectedColumn("OrderItemTotalPriceInMainCurrency", "Total price in main currency")
        };

        private readonly List<CollectedColumn> orderAddressInfoColumns = new List<CollectedColumn> {
            new CollectedColumn("AddressID", ""),
            new CollectedColumn("AddressLine1", "Address line"),
            new CollectedColumn("AddressLine2", "Address line 2"),
            new CollectedColumn("AddressCity", "City"),
            new CollectedColumn("AddressZip", "Zip"),
            new CollectedColumn("AddressPhone","Phone"),
            new CollectedColumn("AddressGUID", "GUID" ),
            new CollectedColumn("AddressLastModified", "Last modified")
        };

        private readonly List<CollectedColumn> shoppingCartInfoColumns = new List<CollectedColumn> {
            new CollectedColumn("ShoppingCartID", ""),
            new CollectedColumn("ShoppingCartGUID", "GUID"),
            new CollectedColumn("ShoppingCartLastUpdate", "Last update"),
            new CollectedColumn("ShoppingCartBillingAddressID", "" ),
            new CollectedColumn("ShoppingCartShippingAddressID", "" ),
            new CollectedColumn("ShoppingCartCompanyAddressID", "")
        };

        private readonly List<CollectedColumn> shoppingCartItemInfoColumns = new List<CollectedColumn> {
            new CollectedColumn("SKUUnits", "Unit count"),
            new CollectedColumn("CartItemGUID", "GUID"),
            new CollectedColumn("SKUNumber", "Product number"),
            new CollectedColumn("SKUName", "Product name")
        };

        private readonly List<CollectedColumn> countryInfoColumns = new List<CollectedColumn> {
            new CollectedColumn("CountryDisplayName", "Country")
        };

        private readonly List<CollectedColumn> stateInfoColumns = new List<CollectedColumn> {
            new CollectedColumn("StateDisplayName", "State")
        };

        private readonly IPersonalDataWriter writer;


        /// <summary>
        /// Class represents the full address of the customer.
        /// </summary>
        private class FullAddressInfo
        {
            public IAddress Address { get; set; }


            public CountryInfo Country { get; set; }


            public StateInfo State { get; set; }
        }


        /// <summary>
        /// Constructs a new instance of the <see cref="SampleCustomerDataCollectorCore"/>.
        /// </summary>
        /// <param name="writer">Writer to format output data.</param>
        public SampleCustomerDataCollectorCore(IPersonalDataWriter writer)
        {
            this.writer = writer;
        }


        /// <summary>
        /// Collect and format all e-commerce personal data about given <paramref name="identities"/>.
        /// Returns null if no data was found.
        /// </summary>
        /// <param name="identities">Identities to collect data about.</param>
        /// <returns>Formatted personal data.</returns>
        public string CollectData(IEnumerable<BaseInfo> identities)
        {
            var customerInfos = identities.OfType<CustomerInfo>().ToList();
            if (!customerInfos.Any())
            {
                return null;
            }

            writer.WriteStartSection("EcommerceData", "E-commerce data");

            foreach (var customerInfo in customerInfos)
            {
                var customerID = customerInfo.CustomerID;
                var userID = customerInfo.CustomerUserID;

                writer.WriteStartSection(CustomerInfo.OBJECT_TYPE, "Customer");
                writer.WriteBaseInfo(customerInfo, customerInfoColumns);
                writer.WriteEndSection();

                WriteOrders(customerID);

                WriteShoppingCarts(userID);

                WriteWishList(userID);
            }

            writer.WriteEndSection();

            return writer.GetResult();
        }


        /// <summary>
        /// Writes all orders for the given customer to the current writer.
        /// </summary>
        private void WriteOrders(int customerID)
        {
            var orders = OrderInfoProvider.GetOrders()
                .Columns(orderInfoColumns.Select(c => c.Name))
                .WhereEquals("OrderCustomerID", customerID)
                .AsEnumerable();

            // Union all address IDs to prevent multiple database accesses
            var addressIDs = orders
                .Select(o => o.OrderBillingAddressID)
                .Union(orders.Select(o => o.OrderShippingAddressID));

            var orderAddresses = OrderAddressInfoProvider.GetAddresses()
                .Columns(orderAddressInfoColumns.Union(countryInfoColumns).Union(stateInfoColumns).Select(c => c.Name))
                .WhereIn("AddressID", addressIDs.ToList())
                .Source(s => s.LeftJoin<CountryInfo>("COM_OrderAddress.AddressCountryID", "CountryID"))
                .Source(s => s.LeftJoin<StateInfo>("COM_OrderAddress.AddressStateID", "StateID"));

            Dictionary<int, FullAddressInfo> orderAddressesDict = new Dictionary<int, FullAddressInfo>();
            orderAddresses.ForEachRow(row =>
            {
                var address = new AddressInfo(row);
                var country = new CountryInfo(row);
                var state = new StateInfo(row);

                var result = new FullAddressInfo
                {
                    Address = address,
                    Country = country,
                    State = state
                };
                orderAddressesDict.Add(address.AddressID, result);
            });

            foreach (var orderInfo in orders)
            {
                WriteOrder(orderInfo, orderAddressesDict);
            }
        }


        /// <summary>
        ///  Writes the given order with all the addresses to the current writer.
        /// </summary>
        private void WriteOrder(OrderInfo orderInfo, Dictionary<int, FullAddressInfo> orderAddresses)
        {
            FullAddressInfo billingAddressInfo, shippingAddressInfo, companyAddressInfo;
            orderAddresses.TryGetValue(orderInfo.OrderBillingAddressID, out billingAddressInfo);
            orderAddresses.TryGetValue(orderInfo.OrderShippingAddressID, out shippingAddressInfo);
            orderAddresses.TryGetValue(orderInfo.OrderCompanyAddressID, out companyAddressInfo);

            writer.WriteStartSection(OrderInfo.OBJECT_TYPE, "Order");

            writer.WriteBaseInfo(orderInfo, orderInfoColumns);
            
            WriteAddress(billingAddressInfo, "BillingAddress", "Billing address");
            WriteAddress(shippingAddressInfo, "ShippingAddress", "Shipping address");
            WriteAddress(companyAddressInfo, "CompanyAddress", "Company address");

            WriteOrderItems(orderInfo.OrderID);

            writer.WriteEndSection();
        }


        /// <summary>
        ///  Writes the order items for given order to the current writer.
        /// </summary>
        private void WriteOrderItems(int orderID)
        {
            var orderItems = OrderItemInfoProvider.GetOrderItems()
                .Columns(orderItemInfoColumns.Union(skuInfoColumns).Select(c => c.Name))
                .WhereEquals("OrderItemOrderID", orderID)
                .Source(s => s.InnerJoin<SKUInfo>("COM_OrderItem.OrderItemSKUID", "SKUID"));

            orderItems.ForEachRow(row =>
            {
                var orderItemInfo = new OrderItemInfo(row);
                var skuInfo = new SKUInfo(row);

                writer.WriteStartSection(OrderItemInfo.OBJECT_TYPE, "Order item");
                writer.WriteBaseInfo(skuInfo, skuInfoColumns);
                writer.WriteBaseInfo(orderItemInfo, orderItemInfoColumns);
                writer.WriteEndSection();
            });
        }


        /// <summary>
        ///  Writes the given address to the current writer.
        /// </summary>
        private void WriteAddress(FullAddressInfo addressInfo, string addressTypeName, string addressTypeDisplayName)
        {
            if (addressInfo == null)
            {
                return;
            }
            writer.WriteStartSection(addressTypeName, addressTypeDisplayName);

            var address = new AddressInfo();
            address.AddressLine1 = addressInfo.Address.AddressLine1;
            address.AddressLine2 = addressInfo.Address.AddressLine2;
            address.AddressCity = addressInfo.Address.AddressCity;
            address.AddressZip = addressInfo.Address.AddressZip;
            address.AddressPhone = addressInfo.Address.AddressPhone;
            address.AddressPersonalName = addressInfo.Address.AddressPersonalName;
            address.AddressGUID = addressInfo.Address.AddressGUID;
            address.AddressLastModified = addressInfo.Address.AddressLastModified;

            writer.WriteBaseInfo(address, orderAddressInfoColumns);
            writer.WriteBaseInfo(addressInfo.State, stateInfoColumns);
            writer.WriteBaseInfo(addressInfo.Country, countryInfoColumns);

            writer.WriteEndSection();
        }


        /// <summary>
        /// Writes all shopping carts for given user to the current writer.
        /// </summary>
        private void WriteShoppingCarts(int userID)
        {
            var shoppingCarts = ShoppingCartInfoProvider.GetShoppingCarts()
                .Columns(shoppingCartInfoColumns.Select(c => c.Name))
                .WhereEquals("ShoppingCartUserID", userID).ToList();

            var shoppingCartsAddressIDs = shoppingCarts
                .Select(o => o.ShoppingCartBillingAddressID)
                .Union(shoppingCarts.Select(o => o.ShoppingCartShippingAddressID))
                .Union(shoppingCarts.Select(o => o.ShoppingCartCompanyAddressID));

            var shoppingCartsAddresses = AddressInfoProvider.GetAddresses()
                .Columns(orderAddressInfoColumns.Union(countryInfoColumns).Union(stateInfoColumns).Select(c => c.Name))
                .WhereIn("AddressID", shoppingCartsAddressIDs.ToList())
                .Source(s => s.LeftJoin<CountryInfo>("COM_Address.AddressCountryID", "CountryID"))
                .Source(s => s.LeftJoin<StateInfo>("COM_Address.AddressStateID", "StateID"));

            Dictionary<int, FullAddressInfo> orderAddressesDict = new Dictionary<int, FullAddressInfo>();
            shoppingCartsAddresses.ForEachRow(r =>
            {
                var address = new AddressInfo(r);
                var country = new CountryInfo(r);
                var state = new StateInfo(r);

                var result = new FullAddressInfo
                {
                    Address = address,
                    Country = country,
                    State = state
                };
                orderAddressesDict.Add(address.AddressID, result);
            });

            foreach (var shoppingCart in shoppingCarts)
            {
                WriteShoppingCart(shoppingCart, orderAddressesDict);
            }
        }


        /// <summary>
        ///  Writes the given shopping cart with all the order addresses to the current writer.
        /// </summary>
        private void WriteShoppingCart(ShoppingCartInfo shoppingCart, Dictionary<int, FullAddressInfo> orderAddresses)
        {
            FullAddressInfo billingAddressInfo, shippingAddressInfo, companyAddressInfo;
            orderAddresses.TryGetValue(shoppingCart.ShoppingCartBillingAddressID, out billingAddressInfo);
            orderAddresses.TryGetValue(shoppingCart.ShoppingCartShippingAddressID, out shippingAddressInfo);
            orderAddresses.TryGetValue(shoppingCart.ShoppingCartCompanyAddressID, out companyAddressInfo);

            writer.WriteStartSection(ShoppingCartInfo.OBJECT_TYPE, "Shopping cart");
            writer.WriteBaseInfo(shoppingCart, shoppingCartInfoColumns);

            WriteAddress(billingAddressInfo, "BillingAddress", "Billing address");
            WriteAddress(shippingAddressInfo, "ShippingAddress", "Shipping address");
            WriteAddress(companyAddressInfo, "CompanyAddress", "Company address");

            WriteShoppingCartItems(shoppingCart.ShoppingCartID);

            writer.WriteEndSection();
        }


        /// <summary>
        ///  Writes the given shopping cart to the current writer.
        /// </summary>
        private void WriteShoppingCartItems(int shoppingCartID)
        {
            var shopingCartItems = ShoppingCartItemInfoProvider.GetShoppingCartItems()
                .Columns(shoppingCartItemInfoColumns.Select(c => c.Name))
                .WhereEquals("ShoppingCartID", shoppingCartID)
                .Source(s => s.InnerJoin<SKUInfo>("COM_ShoppingCartSKU.SKUID", "SKUID"));

            shopingCartItems.ForEachRow(row =>
            {
                var cartItem = new ShoppingCartItemInfo(row);
                var skuInfo = new SKUInfo(row);

                writer.WriteStartSection(ShoppingCartItemInfo.OBJECT_TYPE, "Shopping cart item");
                writer.WriteBaseInfo(skuInfo, skuInfoColumns);
                writer.WriteBaseInfo(cartItem, shoppingCartItemInfoColumns);
                writer.WriteEndSection();
            });
        }


        /// <summary>
        ///  Writes the given wish list for given user.
        /// </summary>
        private void WriteWishList(int userID)
        {
            var wishlistItems = WishlistItemInfoProvider.GetWishlistItems()
                .WhereEquals("UserID", userID)
                .Source(s => s.InnerJoin<SKUInfo>("COM_Wishlist.SKUID", "SKUID"));

            wishlistItems.ForEachRow(row =>
            {
                var skuItemInfo = new SKUInfo(row);

                writer.WriteStartSection(WishlistItemInfo.OBJECT_TYPE, "Wishlist");
                writer.WriteBaseInfo(skuItemInfo, skuInfoColumns);
                writer.WriteEndSection();
            });
        }
    }
}
