using System;
using System.Linq;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing OrderInfo management.
    /// </summary>
    public class OrderInfoProvider : AbstractInfoProvider<OrderInfo, OrderInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public OrderInfoProvider()
            : base(OrderInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                UseWeakReferences = true
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all orders.
        /// </summary>
        public static ObjectQuery<OrderInfo> GetOrders()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns order with specified ID.
        /// </summary>
        /// <param name="orderId">Order ID</param>        
        public static OrderInfo GetOrderInfo(int orderId)
        {
            return ProviderObject.GetOrderInfoInternal(orderId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified order.
        /// </summary>
        /// <param name="orderObj">Order to be set</param>
        public static void SetOrderInfo(OrderInfo orderObj)
        {
            // Check the license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Ecommerce);
            }

            ProviderObject.SetOrderInfoInternal(orderObj);
        }


        /// <summary>
        /// Deletes specified order.
        /// </summary>
        /// <param name="orderObj">Order to be deleted</param>
        public static void DeleteOrderInfo(OrderInfo orderObj)
        {
            ProviderObject.DeleteOrderInfoInternal(orderObj);
        }


        /// <summary>
        /// Deletes order with specified ID.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        public static void DeleteOrderInfo(int orderId)
        {
            var orderObj = GetOrderInfo(orderId);
            DeleteOrderInfo(orderObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns a query for all orders for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>        
        public static ObjectQuery<OrderInfo> GetOrders(int siteId)
        {
            return ProviderObject.GetOrdersInternal(siteId);
        }


        /// <summary>
        /// Returns invoice number generated from given shopping cart data.
        /// </summary>
        /// <param name="cart">Shopping cart info object to generate invoice number for</param>
        public static string GenerateInvoiceNumber(ShoppingCartInfo cart)
        {
            return ProviderObject.GenerateInvoiceNumberInternal(cart);
        }

        #endregion


        #region "Public methods - Emails"

        /// <summary>
        /// Creates the macro resolver for the given shopping cart.
        /// </summary>
        /// <param name="cart">Shopping data</param>
        public static MacroResolver CreateEmailMacroResolver(ShoppingCartInfo cart)
        {
            return ProviderObject.CreateEmailMacroResolverInternal(cart);
        }


        /// <summary>
        /// Sends order notification to customer.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <param name="templateName">Name of the template which is used for e-mail. If null or empty, 'Ecommerce.OrderNotificationToCustomer' template is used</param>
        public static void SendOrderNotificationToCustomer(ShoppingCartInfo cart, string templateName = null)
        {
            ProviderObject.SendOrderNotificationToCustomerInternal(cart, templateName);
        }


        /// <summary>
        /// Sends order notification to administrator.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <param name="templateName">Name of the template which is used for e-mail. If null or empty, 'Ecommerce.OrderNotificationToAdmin' template is used</param>
        public static void SendOrderNotificationToAdministrator(ShoppingCartInfo cart, string templateName = null)
        {
            ProviderObject.SendOrderNotificationToAdministratorInternal(cart, templateName);
        }


        /// <summary>
        /// Sends order payment notification to customer.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <param name="templateName">Name of the template which is used for e-mail. If null or empty, 'Ecommerce.OrderPaymentNotificationToCustomer' template is used</param>
        public static void SendOrderPaymentNotificationToCustomer(ShoppingCartInfo cart, string templateName = null)
        {
            ProviderObject.SendOrderPaymentNotificationToCustomerInternal(cart, templateName);
        }


        /// <summary>
        /// Sends order payment notification to administrator.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <param name="templateName">Name of the template which is used for e-mail. If null or empty, 'Ecommerce.OrderPaymentNotificationToAdmin' template is used</param>
        public static void SendOrderPaymentNotificationToAdministrator(ShoppingCartInfo cart, string templateName = null)
        {
            ProviderObject.SendOrderPaymentNotificationToAdministratorInternal(cart, templateName);
        }


        /// <summary>
        /// Sends order status notification to customer.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <param name="templateName">Name of the template which is used for e-mail. If null or empty, 'Ecommerce.OrderStatusNotificationToCustomer' template is used</param>
        public static void SendOrderStatusNotificationToCustomer(ShoppingCartInfo cart, string templateName = null)
        {
            ProviderObject.SendOrderStatusNotificationToCustomerInternal(cart, templateName);
        }


        /// <summary>
        /// Sends order status notification to administrator.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <param name="templateName">Name of the template which is used for e-mail. If null or empty, 'Ecommerce.OrderStatusNotificationToAdmin' template is used</param>
        public static void SendOrderStatusNotificationToAdministrator(ShoppingCartInfo cart, string templateName = null)
        {
            ProviderObject.SendOrderStatusNotificationToAdministratorInternal(cart, templateName);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns order with specified ID.
        /// </summary>
        /// <param name="orderId">Order ID</param>        
        protected virtual OrderInfo GetOrderInfoInternal(int orderId)
        {
            return GetInfoById(orderId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified order.
        /// </summary>
        /// <param name="orderObj">Order to be set</param>        
        protected virtual void SetOrderInfoInternal(OrderInfo orderObj)
        {
            if (orderObj == null)
            {
                throw new ArgumentNullException(nameof(orderObj));
            }

            // Set order payment result
            if (orderObj.OrderPaymentResult != null)
            {
                orderObj.SetValue("OrderPaymentResult", orderObj.OrderPaymentResult.GetPaymentResultXml());
            }

            // Is it an new order?
            var newOrder = (orderObj.OrderID <= 0);
            var fireOrderPaidEvent = false;

            // Ensure addresses                
            orderObj.OrderBillingAddressID = EnsureAddress(orderObj.OrderBillingAddress);
            orderObj.OrderShippingAddressID = EnsureAddress(orderObj.OrderShippingAddress);
            orderObj.OrderCompanyAddressID = EnsureAddress(orderObj.OrderCompanyAddress);

            int originalStatusId = 0;
            bool originalOrderIsPaid = false;

            // Set order date, avoid setting when importing order
            if (newOrder)
            {
                if (orderObj.Generalized.UpdateTimeStamp && (orderObj.OrderDate == DateTimeHelper.ZERO_TIME))
                {
                    orderObj.OrderDate = DateTime.Now;
                }
            }
            else
            {
                // Get original order status id
                var originalOrder = GetInfoById(orderObj.OrderID, false);

                if (originalOrder != null)
                {
                    originalStatusId = originalOrder.OrderStatusID;
                    originalOrderIsPaid = originalOrder.OrderIsPaid;
                }
            }

            // Get status info
            var status = OrderStatusInfoProvider.GetOrderStatusInfo(orderObj.OrderStatusID);
            if (status != null)
            {
                // Set order as paid if required by new order status
                if (status.StatusOrderIsPaid && (orderObj.OrderStatusID != originalStatusId))
                {
                    orderObj.OrderIsPaid = true;
                }
            }

            using (var tr = BeginTransaction())
            {
                // Save order
                SetInfo(orderObj);

                // If order paid status changed
                if (orderObj.OrderIsPaid != originalOrderIsPaid)
                {
                    // Process order according to paid status change
                    ProcessOrderIsPaidChangeInternal(orderObj);
                }

                tr.Commit();
            }

            if (orderObj.OrderIsPaid != originalOrderIsPaid)
            {
                // If order paid status changed to not paid
                if (!orderObj.OrderIsPaid)
                {
                    // Log the change
                    EventLogProvider.LogEvent(EventType.WARNING, "Order", "ORDERNOTPAID", String.Format("Order with ID {0} changed paid status to not paid.", orderObj.OrderID), null, 0, null, 0, null, null, SiteContext.CurrentSiteID);
                }
                else
                {
                    // Order status changed to paid
                    fireOrderPaidEvent = true;
                }
            }

            // Update order status history and send notification emails
            UpdateOrderStatusHistoryInternal(orderObj, originalStatusId, newOrder);

            // Fire order is paid event for successfully paid order
            if (fireOrderPaidEvent && orderObj.OrderIsPaid)
            {
                EcommerceEvents.OrderPaid.StartEvent(orderObj);
            }

            // Invalidate customer
            CustomerInfoProvider.InvalidateCustomer(orderObj.OrderCustomerID);
        }


        /// <summary>
        /// Save address if necessary and return id. 
        /// </summary>
        /// <param name="address">The address.</param>        
        private static int EnsureAddress(IAddress address)
        {
            if (address == null)
            {
                return 0;
            }

            if (address.AddressID < 1)
            {
                address.SetAddress();
            }

            return address.AddressID;
        }


        /// <summary>
        /// Deletes specified order.
        /// </summary>
        /// <param name="orderObj">Order to be deleted</param>        
        protected virtual void DeleteOrderInfoInternal(OrderInfo orderObj)
        {
            if (orderObj == null)
            {
                throw new ArgumentNullException(nameof(orderObj));
            }

            // Invalidate customer
            CustomerInfoProvider.InvalidateCustomer(orderObj.OrderCustomerID);

            using (var tr = BeginTransaction())
            {
                // Delete order items
                OrderItemInfoProvider.DeleteOrderItems(orderObj.OrderID);

                // Delete order
                DeleteInfo(orderObj);

                OrderAddressInfoProvider.DeleteAddressInfo(orderObj.OrderBillingAddressID);
                OrderAddressInfoProvider.DeleteAddressInfo(orderObj.OrderShippingAddressID);
                OrderAddressInfoProvider.DeleteAddressInfo(orderObj.OrderCompanyAddressID);

                tr.Commit();
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns a query for all orders for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param> 
        protected virtual ObjectQuery<OrderInfo> GetOrdersInternal(int siteId)
        {
            return GetOrders().OnSite(siteId);
        }


        /// <summary>
        /// Returns invoice number generated from given shopping cart data.
        /// </summary>
        /// <param name="cart">Shopping cart info object</param>
        protected virtual string GenerateInvoiceNumberInternal(ShoppingCartInfo cart)
        {
            string invoiceNumberPattern = ECommerceSettings.InvoiceNumberPattern(cart.SiteName);

            // When no pattern found, use OrderID as previous CMS versions did
            if (string.IsNullOrEmpty(invoiceNumberPattern))
            {
                return cart.OrderId.ToString();
            }

            // Create resolver
            var resolver = ShoppingCartInfoProvider.GetShoppingCartResolver(cart);

            // Generate invoice number
            return resolver.ResolveMacros(invoiceNumberPattern);
        }


        /// <summary>
        /// Updates order status history and sends notification emails about order status change.
        /// </summary>
        /// <param name="orderObj">Order data</param>
        /// <param name="originalStatusId">ID of the status before change. Don't care if newOrder == true</param>
        /// <param name="newOrder">True - it is a new order, False - it is an existing order</param>
        protected virtual void UpdateOrderStatusHistoryInternal(OrderInfo orderObj, int originalStatusId, bool newOrder)
        {
            // Indicates if order status was changed
            var sendNotification = false;
            var statusChanged = (orderObj.OrderStatusID != 0) && newOrder;

            if (newOrder)
            {
                originalStatusId = 0;
            }

            // Get new order status information
            var newStatus = OrderStatusInfoProvider.GetOrderStatusInfo(orderObj.OrderStatusID);
            if (newStatus != null)
            {
                statusChanged = newStatus.StatusID != originalStatusId;
                sendNotification = newStatus.StatusSendNotification;
            }

            if (statusChanged)
            {
                // Send notification when order status has changed
                if (sendNotification && CMSActionContext.CurrentSendEmails)
                {
                    var cart = ShoppingCartInfoProvider.GetShoppingCartInfoFromOrder(orderObj.OrderID);
                    if (cart != null)
                    {
                        SendOrderStatusNotificationToAdministrator(cart);
                        SendOrderStatusNotificationToCustomer(cart);
                    }
                }

                // Prepare order status history record
                var historyItem = new OrderStatusUserInfo
                {
                    OrderID = orderObj.OrderID,
                    FromStatusID = originalStatusId,
                    ToStatusID = orderObj.OrderStatusID,
                    ChangedByUserID = MembershipContext.AuthenticatedUser.UserID,
                    Date = DateTime.Now,
                    Note = orderObj.OrderNote
                };

                // Update order status history
                OrderStatusUserInfoProvider.SetOrderStatusUserInfo(historyItem);
            }
        }


        /// <summary>
        /// Process order items of given order according to order is paid change.
        /// </summary>
        /// <param name="order">Order</param>
        protected virtual void ProcessOrderIsPaidChangeInternal(OrderInfo order)
        {
            if (order == null)
            {
                return;
            }

            // Store current date and time to use the same value for whole order
            var now = DateTime.Now;

            var orderItems = OrderItemInfoProvider.GetOrderItems(order.OrderID).ToList();

            ProcessMembershipsInternal(order, orderItems.Where(i => i.OrderItemSKU.SKUProductType == SKUProductTypeEnum.Membership), now);
            ProcessEProductsInternal(order, orderItems.Where(i => i.OrderItemSKU.SKUProductType == SKUProductTypeEnum.EProduct), now);

            if (order.OrderIsPaid)
            {
                // Send notification emails
                if (ECommerceSettings.SendPaymentNotification(order.OrderSiteID) && CMSActionContext.CurrentSendEmails)
                {
                    var cart = ShoppingCartInfoProvider.GetShoppingCartInfoFromOrder(order.OrderID);

                    SendOrderPaymentNotificationToAdministrator(cart);
                    SendOrderPaymentNotificationToCustomer(cart);
                }
            }
        }


        /// <summary>
        /// Process membership order items of given order.
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="membershipItems">Membership items of given order</param>
        /// <param name="now">Date and time to use as current date and time value</param>
        private void ProcessMembershipsInternal(OrderInfo order, IEnumerable<OrderItemInfo> membershipItems, DateTime now)
        {
            var orderItemInfos = membershipItems.ToList();
            if (orderItemInfos.Any())
            {
                var customer = CustomerInfoProvider.GetCustomerInfo(order.OrderCustomerID);
                var user = customer?.CustomerUser;

                foreach (var item in orderItemInfos)
                {
                    ProcessMembershipInternal(order, item, item.OrderItemSKU, user, now);
                }
            }
        }


        /// <summary>
        /// Process EProduct order items of given order.
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="eproductItems">EProduct items of given order</param>
        /// <param name="now">Date and time to use as current date and time value</param>
        private void ProcessEProductsInternal(OrderInfo order, IEnumerable<OrderItemInfo> eproductItems, DateTime now)
        {
            using (new CMSActionContext { TouchParent = false })
            {
                foreach (var item in eproductItems)
                {
                    ProcessEProductInternal(order, item, item.OrderItemSKU, now);
                }
            }
        }


        /// <summary>
        /// Process user's membership according to given parameters.
        /// </summary>
        /// <param name="oi">Order</param>
        /// <param name="oii">Order item</param>
        /// <param name="skui">SKU</param>
        /// <param name="ui">User</param>
        /// <param name="now">Date and time to use as current date and time value</param>
        protected virtual void ProcessMembershipInternal(OrderInfo oi, OrderItemInfo oii, SKUInfo skui, UserInfo ui, DateTime now)
        {
            if ((oi == null) || (oii == null) || (ui == null) || (skui == null) || (skui.SKUProductType != SKUProductTypeEnum.Membership))
            {
                return;
            }

            // Get membership information
            var membership = MembershipInfoProvider.GetMembershipInfo(skui.SKUMembershipGUID, skui.SKUSiteID);
            if (membership == null)
            {
                return;
            }

            // Get membership user information
            MembershipUserInfo mui = MembershipUserInfoProvider.GetMembershipUserInfo(membership.MembershipID, ui.UserID);

            // If order is paid, prolong user's membership
            if (oi.OrderIsPaid)
            {
                if (mui == null)
                {
                    // Create new membership for the user
                    mui = new MembershipUserInfo
                    {
                        UserID = ui.UserID,
                        MembershipID = membership.MembershipID,
                        ValidTo = now
                    };
                }

                // If user's membership validity is not unlimited
                if (mui.ValidTo.CompareTo(DateTimeHelper.ZERO_TIME) != 0)
                {
                    // If user's membership already expired
                    if (mui.ValidTo.CompareTo(now) < 0)
                    {
                        mui.ValidTo = now;
                    }

                    DateTime newValidTo;

                    // Prolong user's membership accordingly
                    if (skui.SKUValidity == ValidityEnum.Until)
                    {
                        newValidTo = skui.SKUValidUntil;
                    }
                    else
                    {
                        newValidTo = DateTimeHelper.GetValidTo(mui.ValidTo, skui.SKUValidity, skui.SKUValidFor * oii.OrderItemUnitCount);
                    }

                    // If new validity is unlimited or is later than original
                    if ((newValidTo.CompareTo(DateTimeHelper.ZERO_TIME) == 0) || (newValidTo.CompareTo(mui.ValidTo) > 0))
                    {
                        // Set valid to date
                        mui.ValidTo = newValidTo;

                        // Set send notification flag
                        mui.SendNotification = true;

                        // Save membership user information
                        MembershipUserInfoProvider.SetMembershipUserInfo(mui);
                    }
                }
            }
            // If order is not paid, rollback user's membership prolongation
            else
            {
                if (mui != null)
                {
                    // If SKU validity is of 'valid until' type or user's membership validity is unlimited
                    if ((skui.SKUValidity == ValidityEnum.Until) || (mui.ValidTo.CompareTo(DateTimeHelper.ZERO_TIME) == 0))
                    {
                        // Remove user's membership completely
                        MembershipUserInfoProvider.RemoveMembershipFromUser(mui.MembershipID, mui.UserID);
                    }
                    else
                    {
                        // Shorten user's membership accordingly
                        DateTime newValidTo = DateTimeHelper.GetValidTo(mui.ValidTo, skui.SKUValidity, -1 * skui.SKUValidFor * oii.OrderItemUnitCount);

                        // Set valid to date
                        mui.ValidTo = newValidTo;

                        // Set send notification flag
                        mui.SendNotification = true;

                        // Save membership user information
                        MembershipUserInfoProvider.SetMembershipUserInfo(mui);
                    }
                }
            }

            // Invalidate user to reflect membership changes immediately
            ui.Generalized.Invalidate(false);
        }


        /// <summary>
        /// Process e-product according to given parameters.
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="item">Order item</param>
        /// <param name="skui">SKU</param>
        /// <param name="now">Date and time to use as current date and time value</param>
        protected virtual void ProcessEProductInternal(OrderInfo order, OrderItemInfo item, SKUInfo skui, DateTime now)
        {
            if ((order != null) && (item != null) && (skui != null) && (skui.SKUProductType == SKUProductTypeEnum.EProduct))
            {
                // If order is paid, create downloads according to the order
                if (order.OrderIsPaid)
                {
                    // Get SKU files
                    var skuFiles = SKUFileInfoProvider.GetSKUFiles().WhereEquals("FileSKUID", skui.SKUID);

                    foreach (var skuFile in skuFiles)
                    {
                        // Create new order item SKU file
                        var itemSKUFile = new OrderItemSKUFileInfo
                        {
                            OrderItemID = item.OrderItemID,
                            FileID = skuFile.FileID
                        };

                        // Save order item SKU file information
                        OrderItemSKUFileInfoProvider.SetOrderItemSKUFileInfo(itemSKUFile);

                        // Set order item valid to date and time
                        if (skui.SKUValidity == ValidityEnum.Until)
                        {
                            // Set specific date and time or set as unlimited
                            item.OrderItemValidTo = skui.SKUValidUntil;
                        }
                        else
                        {
                            // Set calculated date and time
                            item.OrderItemValidTo = DateTimeHelper.GetValidTo(now, skui.SKUValidity, skui.SKUValidFor * item.OrderItemUnitCount);
                        }

                        // Set send notification flag
                        item.OrderItemSendNotification = true;

                        // Save order item
                        OrderItemInfoProvider.SetOrderItemInfo(item);
                    }
                }
                // If order is not paid, delete downloads related to the order
                else
                {
                    // Get order item SKU files
                    var orderItemSkuFiles = OrderItemSKUFileInfoProvider.GetOrderItemSKUFiles().WhereEquals("OrderItemID", item.OrderItemID);

                    foreach (var skuFile in orderItemSkuFiles)
                    {
                        // Delete order item SKU file
                        OrderItemSKUFileInfoProvider.DeleteOrderItemSKUFileInfo(skuFile);
                    }

                    // Clear valid to and send notification values
                    item.SetValue("OrderItemValidTo", null);
                    item.SetValue("OrderItemSendNotification", null);

                    // Save order item
                    OrderItemInfoProvider.SetOrderItemInfo(item);
                }
            }
        }

        #endregion


        #region "Internal methods - Emails"

        /// <summary>
        /// Creates the macro resolver for the given shopping cart.
        /// </summary>
        /// <param name="cart">Shopping data</param>
        protected virtual MacroResolver CreateEmailMacroResolverInternal(ShoppingCartInfo cart)
        {
            // Get default shopping cart resolver
            return ShoppingCartInfoProvider.GetShoppingCartResolver(cart);
        }


        /// <summary>
        /// Sends order notification to customer.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <param name="templateName">Name of the template which is used for e-mail. If null or empty, 'Ecommerce.OrderNotificationToCustomer' template is used</param>
        protected virtual void SendOrderNotificationToCustomerInternal(ShoppingCartInfo cart, string templateName)
        {
            SendEmailNotificationInternal(cart, templateName, "Ecommerce.OrderNotificationToCustomer",
                                  "ordernotification.customersubject", true, "Order notification to customer");
        }


        /// <summary>
        /// Sends order notification to administrator.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <param name="templateName">Name of the template which is used for e-mail. If null or empty, 'Ecommerce.OrderNotificationToAdmin' template is used</param>
        protected virtual void SendOrderNotificationToAdministratorInternal(ShoppingCartInfo cart, string templateName)
        {
            SendEmailNotificationInternal(cart, templateName, "Ecommerce.OrderNotificationToAdmin",
                                  "ordernotification.administratorsubject", false, "Order notification to administrator");
        }


        /// <summary>
        /// Sends order payment notification to customer.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <param name="templateName">Name of the template which is used for e-mail. If null or empty, 'Ecommerce.OrderPaymentNotificationToCustomer' template is used</param>
        protected virtual void SendOrderPaymentNotificationToCustomerInternal(ShoppingCartInfo cart, string templateName)
        {
            SendEmailNotificationInternal(cart, templateName, "Ecommerce.OrderPaymentNotificationToCustomer",
                                  "orderpaymentnotification.customersubject", true, "Order payment notification to customer");
        }


        /// <summary>
        /// Sends order payment notification to administrator.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <param name="templateName">Name of the template which is used for e-mail. If null or empty, 'Ecommerce.OrderPaymentNotificationToAdmin' template is used</param>
        protected virtual void SendOrderPaymentNotificationToAdministratorInternal(ShoppingCartInfo cart, string templateName)
        {
            SendEmailNotificationInternal(cart, templateName, "Ecommerce.OrderPaymentNotificationToAdmin",
                                  "orderpaymentnotification.administratorsubject", false, "Order payment notification to administrator");
        }


        /// <summary>
        /// Sends order status notification to customer.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <param name="templateName">Name of the template which is used for e-mail. If null or empty, 'Ecommerce.OrderStatusNotificationToCustomer' template is used</param>
        protected virtual void SendOrderStatusNotificationToCustomerInternal(ShoppingCartInfo cart, string templateName)
        {
            SendEmailNotificationInternal(cart, templateName, "Ecommerce.OrderStatusNotificationToCustomer",
                                  "orderstatusnotification.customersubject", true, "Order status notification to customer");
        }


        /// <summary>
        /// Sends order status notification to administrator.
        /// </summary>
        /// <param name="cart">Shopping cart data</param>
        /// <param name="templateName">Name of the template which is used for e-mail. If null or empty, 'Ecommerce.OrderStatusNotificationToAdmin' template is used</param>
        protected virtual void SendOrderStatusNotificationToAdministratorInternal(ShoppingCartInfo cart, string templateName)
        {
            SendEmailNotificationInternal(cart, templateName, "Ecommerce.OrderStatusNotificationToAdmin",
                                  "orderstatusnotification.administratorsubject", false, "Order status notification to administrator");
        }


        /// <summary>
        /// Sends order notification to customer.
        /// </summary>
        /// <param name="cart">Shopping cart info object</param>
        /// <param name="templateName">Name of the e-mail template which should be used for creating an e-mail</param>
        /// <param name="defaultTemplate">Name of the e-mail template which is used if primary template is not specified</param>
        /// <param name="defaultSubject">E-mail default subject. It is used if template subject is not specified</param>
        /// <param name="toCustomer">True - it is a notification to customer. False - it is a notification to administrator</param>
        /// <param name="eventSource">Source of the event which is logged when sending fails</param>
        protected virtual void SendEmailNotificationInternal(ShoppingCartInfo cart, string templateName, string defaultTemplate, string defaultSubject, bool toCustomer, string eventSource)
        {
            // Do not send if required data missing
            if ((cart == null) || (cart.Customer == null))
            {
                return;
            }

            // Ensure template name
            if (string.IsNullOrEmpty(templateName))
            {
                templateName = defaultTemplate;
            }

            // Get email template
            EmailTemplateInfo template = EmailTemplateProvider.GetEmailTemplate(templateName, cart.SiteName);

            // Check template presence
            if (template == null)
            {
                // Ensure event source
                if (string.IsNullOrEmpty(eventSource))
                {
                    eventSource = "E-mail notification";
                }

                // Log error
                EventLogProvider.LogException(eventSource, "ECOMMERCE", new Exception(string.Format("Email template '{0}' for site '{1}' not found.", templateName, cart.SiteName)));

                return;
            }

            // Get order data
            OrderInfo oi = GetOrderInfo(cart.OrderId);

            if (oi != null)
            {
                string sendFrom;
                string sendTo;

                // Notification to customer
                if (toCustomer)
                {
                    sendFrom = ECommerceSettings.SendEmailsFrom(cart.SiteName);
                    sendTo = cart.Customer.CustomerEmail;
                }
                // Notification to administrator
                else
                {
                    sendFrom = cart.Customer.CustomerEmail;
                    sendTo = ECommerceSettings.SendEmailsTo(cart.SiteName);
                }

                // Get sender address for given template
                sendFrom = EmailHelper.GetSender(template, sendFrom);

                if (!string.IsNullOrEmpty(sendFrom) && !string.IsNullOrEmpty(sendTo))
                {
                    string body = URLHelper.MakeLinksAbsolute(template.TemplateText);

                    // Store original shopping cart culture
                    string origCartCulture = cart.ShoppingCartCulture;

                    // Use default culture 
                    if (!toCustomer || !ECommerceSettings.UseCustomerCultureForEmails(cart.SiteName))
                    {
                        cart.ShoppingCartCulture = CultureHelper.GetDefaultCultureCode(cart.SiteName);
                    }

                    // Prepare the macro resolver
                    var resolver = CreateEmailMacroResolver(cart);

                    // Prepare the message
                    defaultSubject = ResHelper.GetString(defaultSubject, cart.ShoppingCartCulture);
                    var message = new EmailMessage
                    {
                        From = sendFrom,
                        Body = resolver.ResolveMacros(body),
                        Recipients = sendTo,
                        EmailFormat = EmailFormatEnum.Default,
                        PlainTextBody = resolver.ResolveMacros(template.TemplatePlainText),
                        Subject = resolver.ResolveMacros(EmailHelper.GetSubject(template, defaultSubject)),
                        CcRecipients = template.TemplateCc,
                        BccRecipients = template.TemplateBcc,
                        ReplyTo = template.TemplateReplyTo
                    };

                    try
                    {
                        // Send email
                        EmailHelper.ResolveMetaFileImages(message, template.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);
                        EmailSender.SendEmail(cart.SiteName, message);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            // Ensure event source
                            if (string.IsNullOrEmpty(eventSource))
                            {
                                eventSource = "E-mail notification";
                            }

                            EventLogProvider.LogException(eventSource, "ECOMMERCE", ex);
                        }
                        catch
                        {
                            // Unable to log the event
                        }
                    }
                    finally
                    {
                        // Restore shopping cart culture
                        cart.ShoppingCartCulture = origCartCulture;
                    }
                }
            }
        }

        #endregion
    }
}