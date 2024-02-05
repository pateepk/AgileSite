using System;
using System.Web.UI;

using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Base.Web.UI;
using CMS.DataEngine;

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// Base class for e-mail notification gateways.
    /// </summary>
    public class CMSEmailNotificationGateway : CMSNotificationGateway
    {
        #region "Methods"

        /// <summary>
        /// Returns the e-mail gateway form.
        /// </summary>
        public override CMSNotificationGatewayForm GetNotificationGatewayForm()
        {
            try
            {
                if (NotificationSubscriptionControl != null)
                {
                    Control ctrl = NotificationSubscriptionControl.Page.LoadUserControl("~/CMSModules/Notifications/Controls/NotificationSubscription/EmailNotificationForm.ascx");
                    if (ctrl != null)
                    {
                        ctrl.ID = ValidationHelper.GetIdentifier("notif" + NotificationGatewayObj.GatewayName);

                        return (CMSNotificationGatewayForm)ctrl;
                    }
                    else
                    {
                        return null;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("CMSEmailNotificationGateway", "EXCEPTION", ex);
            }

            return null;
        }


        /// <summary>
        /// Sends the notification via e-mail.
        /// </summary>
        public override void SendNotification()
        {
            try
            {
                if (NotificationSubscriptionObj == null)
                {
                    return;
                }

                // Get template text
                NotificationTemplateTextInfo templateText = NotificationTemplateTextInfoProvider.GetNotificationTemplateTextInfo(NotificationGatewayObj.GatewayID, NotificationSubscriptionObj.SubscriptionTemplateID);
                if (templateText == null)
                {
                    return;
                }

                // Get the site name
                string siteName = null;
                SiteInfo si = SiteInfoProvider.GetSiteInfo(NotificationSubscriptionObj.SubscriptionSiteID);
                if (si != null)
                {
                    siteName = si.SiteName;
                }

                string from = EmailNotificationsFrom(siteName);

                if (from == string.Empty)
                {
                    return;
                }

                bool htmlSupported = (NotificationSubscriptionObj.SubscriptionUseHTML) &&
                                     (NotificationGatewayObj.GatewaySupportsHTMLText) &&
                                     (templateText.TemplateHTMLText != string.Empty);

                EmailMessage message = new EmailMessage
                                           {
                                               From = from,
                                               Recipients = NotificationSubscriptionObj.SubscriptionTarget,
                                               Subject = GetNotificationSubject(templateText),
                                               EmailFormat = htmlSupported ? EmailFormatEnum.Html : EmailFormatEnum.PlainText,
                                           };

                if (htmlSupported)
                {
                    message.Body = GetNotificationBody(templateText, true);
                }
                else
                {
                    message.PlainTextBody = GetNotificationBody(templateText, false);
                }

                EmailSender.SendEmail(siteName, message);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("CMSEmailNotificationGateway", "EXCEPTION", ex);
            }
        }


        /// <summary>
        /// Gets the notification subject.
        /// </summary>
        /// <param name="templateText">The template text</param>
        /// <returns>Notification e-mail subject</returns>
        private string GetNotificationSubject(NotificationTemplateTextInfo templateText)
        {
            return Resolver != null ? Resolver.ResolveMacros(templateText.TemplateSubject) : templateText.TemplateSubject;
        }


        /// <summary>
        /// Gets the notification body.
        /// </summary>
        /// <param name="templateText">The template text</param>
        /// <param name="htmlSupported">If <c>true</c> the HTML e-mails are supported</param>
        /// <returns>Notification e-mail body</returns>
        private string GetNotificationBody(NotificationTemplateTextInfo templateText, bool htmlSupported)
        {
            if (htmlSupported)
            {
                return Resolver != null ? Resolver.ResolveMacros(templateText.TemplateHTMLText) : templateText.TemplateHTMLText;
            }
            else
            {
                return Resolver != null ? Resolver.ResolveMacros(templateText.TemplatePlainText) : templateText.TemplatePlainText;
            }
        }


        /// <summary>
        /// Gets the e-mail addres that should be used as a sender of a notification.
        /// </summary>
        /// <param name="siteName">Name of the site</param>
        /// <returns>E-mail address of the notification sender</returns>
        private static string EmailNotificationsFrom(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSSendEmailNotificationsFrom");
        }

        #endregion
    }
}