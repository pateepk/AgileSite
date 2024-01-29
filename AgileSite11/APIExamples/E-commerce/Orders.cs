using System;
using System.Linq;

using CMS.Base;
using CMS.Ecommerce;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DataEngine;

namespace APIExamples
{
    /// <summary>
    /// Holds order API examples.
    /// </summary>
    /// <pageTitle>Orders</pageTitle>
    internal class OrdersMain
    {
        /// <summary>
        /// Holds order API examples.
        /// </summary>
        /// <groupHeading>Orders</groupHeading>
        private class Orders
        {
            /// <heading>Creating a new order</heading>
            private void CreateOrder()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                // Prepares the order addresses
                OrderAddressInfo orderBillingAddress = null;
                OrderAddressInfo orderShippingAddress = null;

                // Gets the customer's address
                AddressInfo customerAddress = AddressInfoProvider.GetAddresses()
                                                                    .WhereEquals("AddressCustomerID", customer.CustomerID)
                                                                    .FirstObject;

                if (customerAddress != null)
                {
                    // Gets the data from the customer's address
                    orderBillingAddress = OrderAddressInfoProvider.CreateOrderAddressInfo(customerAddress);
                    orderShippingAddress = OrderAddressInfoProvider.CreateOrderAddressInfo(customerAddress);

                    // Sets the order addresses
                    OrderAddressInfoProvider.SetAddressInfo(orderBillingAddress);
                    OrderAddressInfoProvider.SetAddressInfo(orderShippingAddress);
                }

                // Gets a status for the order
                OrderStatusInfo orderStatus = OrderStatusInfoProvider.GetOrderStatusInfo("NewStatus", SiteContext.CurrentSiteName);

                // Gets a currency for the order
                CurrencyInfo currency = CurrencyInfoProvider.GetCurrencyInfo("NewCurrency", SiteContext.CurrentSiteName);

                if ((customer != null) && (orderStatus != null) && (currency != null) && (orderBillingAddress != null))
                {
                    // Creates a new order object and sets its properties
                    OrderInfo newOrder = new OrderInfo
                    {
                        OrderInvoiceNumber = "1",
                        OrderBillingAddress = orderBillingAddress,
                        OrderShippingAddress = orderShippingAddress,
                        OrderTotalPrice = 200,
                        OrderGrandTotal = 200,
                        OrderTotalTax = 30,
                        OrderDate = DateTime.Now,
                        OrderStatusID = orderStatus.StatusID,
                        OrderCustomerID = customer.CustomerID,
                        OrderSiteID = SiteContext.CurrentSiteID,
                        OrderCurrencyID = currency.CurrencyID
                    };

                    // Saves the order to the database
                    OrderInfoProvider.SetOrderInfo(newOrder);
                }
            }


            /// <heading>Updating an order</heading>
            private void GetAndUpdateOrder()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                if (customer != null)
                {
                    // Gets the customer's first order
                    OrderInfo order = OrderInfoProvider.GetOrders()
                                                    .WhereEquals("OrderCustomerID", customer.CustomerID)
                                                    .FirstObject;
                    if (order != null)
                    {
                        // Updates the price of the order
                        order.OrderTotalPrice = order.OrderTotalPrice + 200;
                        order.OrderGrandTotal = order.OrderGrandTotal + 200;

                        // Saves the changes to the database
                        OrderInfoProvider.SetOrderInfo(order);
                    }
                }
            }


            /// <heading>Updating multiple orders</heading>
            private void GetAndBulkUpdateOrders()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                if (customer != null)
                {
                    // Gets the customer's orders
                    var orders = OrderInfoProvider.GetOrders().WhereEquals("OrderCustomerID", customer.CustomerID);
                    
                    // Loops through the customer's orders
                    foreach (OrderInfo order in orders)
                    {
                        // Updates the price of the order
                        order.OrderTotalPrice = order.OrderTotalPrice + 200;
                        order.OrderGrandTotal = order.OrderGrandTotal + 200;

                        // Saves the changes to the database
                        OrderInfoProvider.SetOrderInfo(order);
                    }
                }
            }


            /// <heading>Deleting an order</heading>
            private void DeleteOrder()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                if (customer != null)
                {
                    // Gets the customer's order
                    OrderInfo order = OrderInfoProvider.GetOrders()
                                                    .WhereEquals("OrderCustomerID", customer.CustomerID)
                                                    .FirstObject;
                    if (order != null)
                    {
                        // Deletes the order
                        OrderInfoProvider.DeleteOrderInfo(order);
                    }
                }
            }
        }

        /// <summary>
        /// Holds order item API examples.
        /// </summary>
        /// <groupHeading>Order items</groupHeading>
        private class OrderItems
        {
            /// <heading>Creating an order item</heading>
            private void CreateOrderItem()
            {
                // Gets the 'NewProduct' product
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                   .WhereEquals("SKUName", "NewProduct")
                                   .FirstObject;

                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;                

                if (customer != null)
                {
                    // Gets the customer's first order
                    OrderInfo order = OrderInfoProvider.GetOrders()
                                                            .WhereEquals("OrderCustomerID", customer.CustomerID)
                                                            .FirstObject;

                    if ((order != null) && (product != null))
                    {
                        // Creates a new order item object and sets its properties
                        OrderItemInfo newItem = new OrderItemInfo
                        {
                            OrderItemSKUName = "NewProduct",
                            OrderItemOrderID = order.OrderID,
                            OrderItemSKUID = product.SKUID,
                            OrderItemUnitPrice = 200,
                            OrderItemUnitCount = 1
                        };

                        // Saves the order item object to the database
                        OrderItemInfoProvider.SetOrderItemInfo(newItem);
                    }
                }
            }


            /// <heading>Updating an order item</heading>
            private void GetAndUpdateOrderItem()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                if (customer != null)
                {
                    // Gets the customer's first order
                    OrderInfo order = OrderInfoProvider.GetOrders()
                                                            .WhereEquals("OrderCustomerID", customer.CustomerID)
                                                            .FirstObject;

                    if (order != null)
                    {
                        // Gets the first item in the order
                        OrderItemInfo orderItem = OrderItemInfoProvider.GetOrderItems(order.OrderID).FirstOrDefault();
                        if (orderItem != null)
                        {
                            // Updates the order item's SKU name
                            orderItem.OrderItemSKUName = orderItem.OrderItemSKUName.ToLowerCSafe();

                            // Saves the changes to the database
                            OrderItemInfoProvider.SetOrderItemInfo(orderItem);
                        }
                    }
                }
            }


            /// <heading>Updating multiple order items</heading>
            private void GetAndBulkUpdateOrderItems()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                if (customer != null)
                {
                    // Gets the customer's first order
                    OrderInfo order = OrderInfoProvider.GetOrders()
                                                            .WhereEquals("OrderCustomerID", customer.CustomerID)
                                                            .FirstObject;

                    if (order != null)
                    {
                        // Gets all items in the given order
                        var orderItems = OrderItemInfoProvider.GetOrderItems(order.OrderID);

                        // Loops through the order items
                        foreach (OrderItemInfo orderItem in orderItems)
                        {
                            // Updates the order item's SKU name
                            orderItem.OrderItemSKUName = orderItem.OrderItemSKUName.ToUpperCSafe();

                            // Saves the order item to the database
                            OrderItemInfoProvider.SetOrderItemInfo(orderItem);
                        }
                    }
                }
            }


            /// <heading>Deleting an order item</heading>
            private void DeleteOrderItem()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                if (customer != null)
                {
                    // Gets the customer's first order
                    OrderInfo order = OrderInfoProvider.GetOrders()
                                                            .WhereEquals("OrderCustomerID", customer.CustomerID)
                                                            .FirstObject;

                    if (order != null)
                    {
                        // Gets the first item from the order
                        OrderItemInfo orderItem = OrderItemInfoProvider.GetOrderItems(order.OrderID).FirstOrDefault();
                        if (orderItem != null)
                        {
                            // Deletes the order item
                            OrderItemInfoProvider.DeleteOrderItemInfo(orderItem);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Holds order status API examples.
        /// </summary>
        /// <groupHeading>Order statuses</groupHeading>
        private class OrderStatuses
        {
            /// <heading>Creating an order status</heading>
            private void CreateOrderStatus()
            {
                // Creates a new order status object
                OrderStatusInfo newStatus = new OrderStatusInfo();

                // Sets the order status properties
                newStatus.StatusDisplayName = "New status";
                newStatus.StatusName = "NewStatus";
                newStatus.StatusEnabled = true;
                newStatus.StatusSiteID = SiteContext.CurrentSiteID;
                newStatus.StatusOrder = 1;

                // Saves the order status to the database
                OrderStatusInfoProvider.SetOrderStatusInfo(newStatus);
            }


            /// <heading>Updating an order status</heading>
            private void GetAndUpdateOrderStatus()
            {
                // Gets the order status
                OrderStatusInfo updateStatus = OrderStatusInfoProvider.GetOrderStatusInfo("NewStatus", SiteContext.CurrentSiteName);
                if (updateStatus != null)
                {
                    // Updates the display name of the order status
                    updateStatus.StatusDisplayName = updateStatus.StatusDisplayName.ToLowerCSafe();

                    // Saves the changes to the database
                    OrderStatusInfoProvider.SetOrderStatusInfo(updateStatus);
                }
            }


            /// <heading>Updating mupltiple order statuses</heading>
            private void GetAndBulkUpdateOrderStatuses()
            {
                // Gets all order statuses on the current site whose code name starts with 'NewStatus'
                var statuses = OrderStatusInfoProvider.GetOrderStatuses()
                                                        .OnSite(SiteContext.CurrentSiteID)
                                                        .WhereStartsWith("StatusName", "NewStatus");

                // Loops through the order statuses
                foreach (OrderStatusInfo modifyStatus in statuses)
                {
                    // Updates the display name of the order status
                    modifyStatus.StatusDisplayName = modifyStatus.StatusDisplayName.ToUpperCSafe();

                    // Saves the changes to the database
                    OrderStatusInfoProvider.SetOrderStatusInfo(modifyStatus);
                }
            }


            /// <heading>Deleting an order status</heading>
            private void DeleteOrderStatus()
            {
                // Gets the order status
                OrderStatusInfo deleteStatus = OrderStatusInfoProvider.GetOrderStatusInfo("NewStatus", SiteContext.CurrentSiteName);

                if (deleteStatus != null) 
                {
                    // Deletes the order status
                    OrderStatusInfoProvider.DeleteOrderStatusInfo(deleteStatus);
                }
            }


            /// <heading>Changing an order's status to the next enabled order status</heading>
            private void ChangeOrderStatus()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                if (customer != null)
                {
                    // Gets the customer's first order
                    OrderInfo order = OrderInfoProvider.GetOrders()
                                                            .WhereEquals("OrderCustomerID", customer.CustomerID)
                                                            .FirstObject;

                    if (order != null)
                    {
                        // Gets the next enabled status for the order
                        OrderStatusInfo nextOrderStatus = OrderStatusInfoProvider.GetNextEnabledStatus(order.OrderStatusID);

                        if (nextOrderStatus != null)
                        {
                            // Creates a new object representing an order status change (performed by a specific user)
                            OrderStatusUserInfo orderStatusChange = new OrderStatusUserInfo()
                            {
                                OrderID = order.OrderID,
                                ChangedByUserID = MembershipContext.AuthenticatedUser.UserID,
                                FromStatusID = order.OrderStatusID,
                                ToStatusID = nextOrderStatus.StatusID,
                                Date = DateTime.Now
                            };

                            // Saves the status change to the order's history
                            OrderStatusUserInfoProvider.SetOrderStatusUserInfo(orderStatusChange);

                            // Assigns the new order status to the order
                            order.OrderStatusID = nextOrderStatus.StatusID;

                            // Saves the updated order to the database
                            OrderInfoProvider.SetOrderInfo(order);
                        }
                    }
                }
            }


            /// <heading>Deleting the status history of an order</heading>
            private void DeleteHistory()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                if (customer != null)
                {
                    // Gets the customer's first order
                    OrderInfo order = OrderInfoProvider.GetOrders()
                                                            .WhereEquals("OrderCustomerID", customer.CustomerID)
                                                            .FirstObject;

                    if (order != null)
                    {
                        // Gets the order status changes in the order's history
                        var statuses = OrderStatusUserInfoProvider.GetOrderStatusHistory(order.OrderID);

                        // Loops through the order status changes
                        foreach (OrderStatusUserInfo status in statuses)
                        {
                            // Deletes the order status changes
                            OrderStatusUserInfoProvider.DeleteOrderStatusUserInfo(status);
                        }
                    }
                }
            }
        }
        

        /// <summary>
        /// Holds invoice API examples.
        /// </summary>
        /// <groupHeading>Invoices</groupHeading>
        private class Invoices
        {
            /// <heading>Updating an invoice</heading>
            private void GetAndUpdateInvoice()
            {
                // Gets the current site name
                string siteName = SiteContext.CurrentSiteName;

                // Gets the current site's invoice template
                string invoice = ECommerceSettings.InvoiceTemplate(siteName);

                if (!String.IsNullOrEmpty(invoice))
                {
                    // Prepares a new string with the invoice text
                    invoice = "<h1>Updated invoice</h1>" + invoice;

                    // Saves the invoice changes to the database
                    SettingsKeyInfoProvider.SetValue(ECommerceSettings.INVOICE_TEMPLATE, siteName, invoice);
                }
            }
        }
    }
}
