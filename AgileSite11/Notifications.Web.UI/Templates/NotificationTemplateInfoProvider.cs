using System;

using CMS.DataEngine;


namespace CMS.Notifications.Web.UI
{
    using TypedDataSet = InfoDataSet<NotificationTemplateInfo>;

    /// <summary>
    /// Class providing notification template info management.
    /// </summary>
    public class NotificationTemplateInfoProvider : AbstractInfoProvider<NotificationTemplateInfo, NotificationTemplateInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public NotificationTemplateInfoProvider()
            : base(NotificationTemplateInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true,
					Load = LoadHashtableEnum.All
				})
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns the query for all notification templates.
        /// </summary>   
        public static ObjectQuery<NotificationTemplateInfo> GetNotificationTemplates()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the global notification template.
        /// </summary>
        /// <param name="notificationTemplateName">Notification template name</param>        
        public static NotificationTemplateInfo GetNotificationTemplateInfo(string notificationTemplateName)
        {
            return GetNotificationTemplateInfo(notificationTemplateName, 0);
        }


        /// <summary>
        /// Returns the site related notification template.
        /// </summary>
        /// <param name="notificationTemplateName">Notification template name</param>
        /// <param name="siteId">Template site ID. User zero to get global template</param>
        public static NotificationTemplateInfo GetNotificationTemplateInfo(string notificationTemplateName, int siteId)
        {
            return ProviderObject.GetInfoByCodeName(notificationTemplateName, siteId);
        }


        /// <summary>
        /// Returns the notification template info structure for the specified notification template.
        /// </summary>
        /// <param name="notificationTemplateId">Notification template ID</param>
        public static NotificationTemplateInfo GetNotificationTemplateInfo(int notificationTemplateId)
        {
            return ProviderObject.GetInfoById(notificationTemplateId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified notification template.
        /// </summary>
        /// <param name="notificationTemplate">Notification template to set</param>
        public static void SetNotificationTemplateInfo(NotificationTemplateInfo notificationTemplate)
        {
            ProviderObject.SetInfo(notificationTemplate);
        }


        /// <summary>
        /// Deletes specified notification template.
        /// </summary>
        /// <param name="infoObj">Notification template object</param>
        public static void DeleteNotificationTemplateInfo(NotificationTemplateInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified notification template.
        /// </summary>
        /// <param name="notificationTemplateId">Notification template ID</param>
        public static void DeleteNotificationTemplateInfo(int notificationTemplateId)
        {
            NotificationTemplateInfo infoObj = GetNotificationTemplateInfo(notificationTemplateId);
            DeleteNotificationTemplateInfo(infoObj);
        }

        #endregion
    }
}