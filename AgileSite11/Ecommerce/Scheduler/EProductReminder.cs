using System;
using System.Collections.Generic;
using System.Data;

using CMS.EmailEngine;
using CMS.Helpers;
using CMS.Scheduler;
using CMS.MacroEngine;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.DataEngine.Query;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides an ITask interface to send notifications to users about their expiring e-product downloads.
    /// If started as global scheduled task, it sends notifications about expiring e-products on all sites.
    /// </summary>
    public class EProductReminder : ITask
    {
        private int taskSiteId = 0;
        private EmailTemplateInfo template = null;

        // Settings
        private int reminderDays = 0;
        private string emailFrom = null;

     
        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                taskSiteId = task.TaskSiteID;

                // Get email template information
                template = EmailTemplateProvider.GetEmailTemplate("Ecommerce.EproductExpirationNotification", taskSiteId);

                // If email template was not found
                if (template == null)
                {
                    throw new Exception("Unable to retrieve 'Ecommerce.EproductExpirationNotification' email template.");
                }

                // Get settings
                reminderDays = ECommerceSettings.EProductsReminder(taskSiteId);
                emailFrom = ECommerceSettings.SendEmailsFrom(taskSiteId);

                // If sender email address not defined
                if (String.IsNullOrEmpty(emailFrom))
                {
                    throw new Exception("Unable to send emails. E-commerce setting 'Send e-commerce e-mails from' not defined.");
                }

                // Get only for customers with email address specified
                var where = new WhereCondition()
                                .WhereEquals("COM_SKU.SKUProductType", SKUProductTypeEnum.EProduct.ToStringRepresentation())
                                .WhereNotEquals("COM_Customer.CustomerEmail".AsColumn().IsNull(String.Empty), String.Empty)
                                .ToString(true);

                // Get expiring e-products
                DataSet dsEproducts = OrderItemInfoProvider.GetExpiringOrderItems(reminderDays, taskSiteId, where, true);
                if (!DataHelper.DataSourceIsEmpty(dsEproducts))
                {
                    // Notifications data
                    var notifications = new List<NotificationData>();
                    NotificationData currentNotification = null;

                    // Order expiring e-products
                    var drOrderedEproducts = dsEproducts.Tables[0].Select(null, "CustomerID, OrderItemSKUName, OrderItemValidTo ASC");

                    foreach (DataRow dr in drOrderedEproducts)
                    {
                        var customerId = ValidationHelper.GetInteger(dr["CustomerID"], 0);

                        // If it is the first notification or another for different customer
                        if ((currentNotification == null) || (currentNotification.CustomerID != customerId))
                        {
                            currentNotification = new NotificationData();

                            // Set general info
                            currentNotification.CustomerID = customerId;
                            currentNotification.CustomerEmail = ValidationHelper.GetString(dr["CustomerEmail"], null);

                            // Set table schema
                            currentNotification.EproductsTable = dsEproducts.Tables[0].Clone();

                            notifications.Add(currentNotification);
                        }

                        // Add expiring e-product to the table
                        currentNotification.EproductsTable.ImportRow(dr);
                    }

                    // Set up notifications
                    var notificationEmails = new List<EmailMessage>();

                    foreach (var notificationData in notifications)
                    {
                        notificationEmails.Add(GetNotificationEmail(notificationData));
                    }

                    // Send notifications
                    foreach (var email in notificationEmails)
                    {
                        EmailSender.SendEmail(taskSiteId > 0 ? SiteInfoProvider.GetSiteName(taskSiteId) : String.Empty, email);
                    }

                    // Clear 'Send notification' flag
                    foreach (DataRow item in dsEproducts.Tables[0].Rows)
                    {
                        var oii = new OrderItemInfo(item);

                        // Clear flag
                        oii.SetValue("OrderItemSendNotification", null);

                        // Save changes
                        OrderItemInfoProvider.SetOrderItemInfo(oii);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        /// <summary>
        /// Returns notification email message according to given notification data.
        /// </summary>
        /// <param name="notificationData">Notification data</param>
        private EmailMessage GetNotificationEmail(NotificationData notificationData)
        {
            // Get resolver
            var resolver = GetNotificationEmailMacroResolver(notificationData);

            // Set up notification email
            var email = new EmailMessage
            {
                From = EmailHelper.GetSender(template, emailFrom),
                Recipients = notificationData.CustomerEmail,
                CcRecipients = template.TemplateCc,
                BccRecipients = template.TemplateBcc,
                Subject = resolver.ResolveMacros(EmailHelper.GetSubject(template, ResHelper.GetString("com.eproductreminder.emailsubject"))),
                Body = resolver.ResolveMacros(template.TemplateText),
                PlainTextBody = resolver.ResolveMacros(template.TemplatePlainText)
            };

            return email;
        }


        /// <summary>
        /// Get macro resolver for expiring e-products notification email.
        /// </summary>
        /// <param name="notificationData">Notification data</param>
        private MacroResolver GetNotificationEmailMacroResolver(NotificationData notificationData)
        {
            var resolver = MacroResolver.GetInstance();
            resolver.Settings.EncodeResolvedValues = false;

            // Set table with expiring e-products
            resolver.SetNamedSourceData("EproductsTable", notificationData.EproductsTable.Rows);

            return resolver;
        }


        /// <summary>
        /// Represents e-product expiration notification data.
        /// </summary>
        private class NotificationData
        {
            // ID of the customer whom the notification is sent
            public int CustomerID
            {
                get;
                set;
            }


            /// <summary>
            /// Customer e-mail where the notification is sent
            /// </summary>
            public string CustomerEmail
            {
                get;
                set;
            }


            /// <summary>
            /// Table with customer expiring e-products
            /// </summary>
            public DataTable EproductsTable
            {
                get;
                set;
            }
        }
    }
}