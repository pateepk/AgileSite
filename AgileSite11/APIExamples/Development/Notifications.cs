using System;

using CMS.Notifications.Web.UI;
using CMS.Base;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.Membership;

namespace APIExamples
{
    /// <summary>
    /// Holds notification API examples.
    /// </summary>
    /// <pageTitle>Notifications</pageTitle>
    internal class Notifications
    {
        /// <summary>
        /// Holds notification gateway API examples.
        /// </summary>
        /// <groupHeading>Notification gateways</groupHeading>
        private class NotificationGateways
        {
            /// <heading>Creating a notification gateway</heading>
            private void CreateNotificationGateway()
            {
                // Creates a new notification gateway object
                NotificationGatewayInfo newGateway = new NotificationGatewayInfo();

                // Sets the gateway properties
                newGateway.GatewayDisplayName = "New gateway";
                newGateway.GatewayName = "NewGateway";
                newGateway.GatewayAssemblyName = "CustomNotificationGateways";
                newGateway.GatewayClassName = "NewGateway";
                newGateway.GatewayDescription = "This is a notification gateway created through the API.";
                newGateway.GatewaySupportsEmail = true;
                newGateway.GatewaySupportsPlainText = true;
                newGateway.GatewaySupportsHTMLText = true;
                newGateway.GatewayEnabled = true;

                // Saves the new notification gateway to the database
                NotificationGatewayInfoProvider.SetNotificationGatewayInfo(newGateway);
            }


            /// <heading>Updating a notification gateway</heading>
            private void GetAndUpdateNotificationGateway()
            {
                // Gets the notification gateway
                NotificationGatewayInfo updateGateway = NotificationGatewayInfoProvider.GetNotificationGatewayInfo("NewGateway");
                if (updateGateway != null)
                {
                    // Updates the gateway properties
                    updateGateway.GatewayDisplayName = updateGateway.GatewayDisplayName.ToLowerCSafe();

                    // Saves the updated gateway to the database
                    NotificationGatewayInfoProvider.SetNotificationGatewayInfo(updateGateway);
                }
            }


            /// <heading>Updating multiple notification gateways</heading>
            private void GetAndBulkUpdateNotificationGateways()
            {
                // Gets all notification gateways whose code name starts with 'New'
                ObjectQuery<NotificationGatewayInfo> gateways = NotificationGatewayInfoProvider.GetNotificationGateways().WhereStartsWith("GatewayName", "New");
                
                // Loops through individual gateways
                foreach (NotificationGatewayInfo gateway in gateways)
                {
                    // Updates the gateway properties
                    gateway.GatewayDisplayName = gateway.GatewayDisplayName.ToUpper();

                    // Saves the modified notification gateway to the database
                    NotificationGatewayInfoProvider.SetNotificationGatewayInfo(gateway);
                }
            }


            /// <heading>Deleting a notification gateway</heading>
            private void DeleteNotificationGateway()
            {
                // Gets the notification gateway
                NotificationGatewayInfo deleteGateway = NotificationGatewayInfoProvider.GetNotificationGatewayInfo("NewGateway");

                if (deleteGateway != null)
                {
                    // Deletes the notification gateway
                    NotificationGatewayInfoProvider.DeleteNotificationGatewayInfo(deleteGateway);
                }
            }
        }


        /// <summary>
        /// Holds notification template API examples.
        /// </summary>
        /// <groupHeading>Notification templates</groupHeading>
        private class NotificationTemplates
        {
            /// <heading>Creating a notification template</heading>
            private void CreateNotificationTemplate()
            {
                // Creates a new notification template object
                NotificationTemplateInfo newTemplate = new NotificationTemplateInfo();

                // Sets the notification template properties
                newTemplate.TemplateDisplayName = "New template";
                newTemplate.TemplateName = "NewTemplate";
                newTemplate.TemplateSiteID = SiteContext.CurrentSiteID;

                // Saves the notification template to the database
                NotificationTemplateInfoProvider.SetNotificationTemplateInfo(newTemplate);

                // Gets a notification gateway
                NotificationGatewayInfo gateway = NotificationGatewayInfoProvider.GetNotificationGatewayInfo("NewGateway");

                if (gateway != null)
                {
                    // Creates a new object that holds the text of the notification template
                    NotificationTemplateTextInfo newText = new NotificationTemplateTextInfo();

                    // Sets the text values for the template
                    newText.TemplateSubject = "Template subject";                    
                    newText.TemplateHTMLText = "Template text. <br />";
                    newText.TemplatePlainText = "Template text.";

                    // Assigns the text values to the template for the specified notification gateway
                    newText.TemplateID = newTemplate.TemplateID;
                    newText.GatewayID = gateway.GatewayID;

                    // Saves the notification template text to the database
                    NotificationTemplateTextInfoProvider.SetNotificationTemplateTextInfo(newText);
                }
            }


            /// <heading>Updating a notification template</heading>
            private void GetAndUpdateNotificationTemplate()
            {
                // Gets the notification template
                NotificationTemplateInfo updateTemplate = NotificationTemplateInfoProvider.GetNotificationTemplateInfo("NewTemplate", SiteContext.CurrentSiteID);
                if (updateTemplate != null)
                {
                    // Updates the basic notification template properties
                    updateTemplate.TemplateDisplayName = updateTemplate.TemplateDisplayName.ToLowerCSafe();

                    // Saves the modified notification template to the database
                    NotificationTemplateInfoProvider.SetNotificationTemplateInfo(updateTemplate);

                    // Gets a notification gateway
                    NotificationGatewayInfo gateway = NotificationGatewayInfoProvider.GetNotificationGatewayInfo("NewGateway");

                    if (gateway != null)
                    {
                        // Gets the object holding the template's text for the specified notification gateway
                        NotificationTemplateTextInfo updateText = NotificationTemplateTextInfoProvider
                                                                    .GetNotificationTemplateTextInfo(gateway.GatewayID, updateTemplate.TemplateID);

                        if (updateText != null)
                        {
                            // Updates the template's subject text for the specified gateway
                            updateText.TemplateSubject = updateText.TemplateSubject.ToLowerCSafe();

                            // Saves the modified notification template text to the database
                            NotificationTemplateTextInfoProvider.SetNotificationTemplateTextInfo(updateText);                            
                        }
                    }
                }
            }


            /// <heading>Updating multiple notification templates</heading>
            private void GetAndBulkUpdateNotificationTemplates()
            {
                // Gets all notification templates assigned to the current site whose code name starts with 'New'
                var templates = NotificationTemplateInfoProvider.GetNotificationTemplates()
                                                                .WhereStartsWith("TemplateName", "New")
                                                                .WhereEquals("TemplateSiteID", SiteContext.CurrentSiteID);
                
                // Loops through individual notification templates
                foreach (NotificationTemplateInfo template in templates)
                {
                    // Updates the basic notification template properties
                    template.TemplateDisplayName = template.TemplateDisplayName.ToUpper();

                    // Saves the modified notification template to the database
                    NotificationTemplateInfoProvider.SetNotificationTemplateInfo(template);

                    // Gets all template text objects that belong to the given template (for all notification gateways)
                    var templateTexts = NotificationTemplateTextInfoProvider.GetNotificationTemplateTexts().WhereEquals("TemplateID", template.TemplateID);

                    // Loops through the template's text objects for all gateways
                    foreach (NotificationTemplateTextInfo templateText in templateTexts)
                    {
                        // Updates the template's subject text
                        templateText.TemplateSubject = templateText.TemplateSubject.ToUpper();

                        // Saves the modified notification template text to the database
                        NotificationTemplateTextInfoProvider.SetNotificationTemplateTextInfo(templateText);
                    }
                }
            }


            /// <heading>Deleting a notification template</heading>
            private void DeleteNotificationTemplate()
            {
                // Gets the notification template
                NotificationTemplateInfo deleteTemplate = NotificationTemplateInfoProvider.GetNotificationTemplateInfo("NewTemplate", SiteContext.CurrentSiteID);

                if (deleteTemplate != null)
                {
                    // Gets all template text objects that belong to the given template (for all notification gateways)
                    var templateTexts = NotificationTemplateTextInfoProvider.GetNotificationTemplateTexts().WhereEquals("TemplateID", deleteTemplate.TemplateID);

                    // Loops through the template's text objects for all gateways
                    foreach (NotificationTemplateTextInfo deleteText in templateTexts)
                    {
                        // Deletes the notification template's text
                        NotificationTemplateTextInfoProvider.DeleteNotificationTemplateTextInfo(deleteText);
                    }

                    // Deletes the notification template
                    NotificationTemplateInfoProvider.DeleteNotificationTemplateInfo(deleteTemplate);
                }
            }
        }


        /// <summary>
        /// Holds notification subscription API examples.
        /// </summary>
        /// <groupHeading>Notification subscriptions</groupHeading>
        private class NotificationSubscriptions
        {
            /// <heading>Creating a notification subscription</heading>
            private void CreateNotificationSubscription()
            {
                // Gets the notification gateway
                NotificationGatewayInfo gateway = NotificationGatewayInfoProvider.GetNotificationGatewayInfo("NewGateway");

                // Get the notification template
                NotificationTemplateInfo template = NotificationTemplateInfoProvider.GetNotificationTemplateInfo("NewTemplate", SiteContext.CurrentSiteID);

                if ((gateway != null) && (template != null))
                {
                    // Creates a new notification subscription object
                    NotificationSubscriptionInfo newSubscription = new NotificationSubscriptionInfo();

                    // Sets the subscription properties (subscribes the current user)
                    newSubscription.SubscriptionEventDisplayName = "Custom event";
                    newSubscription.SubscriptionGatewayID = gateway.GatewayID;
                    newSubscription.SubscriptionTemplateID = template.TemplateID;
                    newSubscription.SubscriptionTime = DateTime.Now;
                    newSubscription.SubscriptionUserID = MembershipContext.AuthenticatedUser.UserID;
                    newSubscription.SubscriptionTarget = MembershipContext.AuthenticatedUser.Email;
                    newSubscription.SubscriptionSiteID = SiteContext.CurrentSiteID;

                    // Saves the notification subscription to the database
                    NotificationSubscriptionInfoProvider.SetNotificationSubscriptionInfo(newSubscription);
                }
            }


            /// <heading>Updating notification subscriptions</heading>
            private void GetAndUpdateNotificationSubscription()
            {
                // Gets the notification gateway
                NotificationGatewayInfo gateway = NotificationGatewayInfoProvider.GetNotificationGatewayInfo("NewGateway");

                // Gets the notification template
                NotificationTemplateInfo template = NotificationTemplateInfoProvider.GetNotificationTemplateInfo("NewTemplate", SiteContext.CurrentSiteID);

                if ((gateway != null) && (template != null))
                {
                    // Gets all subscriptions that use the specified notification gateway and template
                    var subscriptions = NotificationSubscriptionInfoProvider.GetNotificationSubscriptions()
                                                                            .WhereEquals("SubscriptionGatewayID", gateway.GatewayID)
                                                                            .WhereEquals("SubscriptionTemplateID", template.TemplateID);

                    // Loops throug individual subscriptions
                    foreach (NotificationSubscriptionInfo subscription in subscriptions)                    
                    {
                        // Updates the subscription properties
                        subscription.SubscriptionEventDisplayName = subscription.SubscriptionEventDisplayName.ToLowerCSafe();

                        // Saves the updated notification subscription to the database
                        NotificationSubscriptionInfoProvider.SetNotificationSubscriptionInfo(subscription);
                    }
                }
            }


            /// <heading>Deleting notification subscriptions</heading>
            private void DeleteNotificationSubscription()
            {
                // Gets the notification gateway
                NotificationGatewayInfo gateway = NotificationGatewayInfoProvider.GetNotificationGatewayInfo("NewGateway");

                // Gets the notification template
                NotificationTemplateInfo template = NotificationTemplateInfoProvider.GetNotificationTemplateInfo("NewTemplate", SiteContext.CurrentSiteID);

                if ((gateway != null) && (template != null))
                {
                    // Gets all subscriptions that use the specified notification gateway and template
                    var subscriptions = NotificationSubscriptionInfoProvider.GetNotificationSubscriptions()
                                                                            .WhereEquals("SubscriptionGatewayID", gateway.GatewayID)
                                                                            .WhereEquals("SubscriptionTemplateID", template.TemplateID);

                    // Loops throug individual subscriptions
                    foreach (NotificationSubscriptionInfo deleteSubscription in subscriptions)
                    {
                        // Deletes the notification subscription
                        NotificationSubscriptionInfoProvider.DeleteNotificationSubscriptionInfo(deleteSubscription);
                    }
                }
            }
        }
    }
}
