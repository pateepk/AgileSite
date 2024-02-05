using System;
using System.Linq;

using CMS.DataEngine;
using CMS.Base;

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// Class providing notification template text info management.
    /// </summary>
    public class NotificationTemplateTextInfoProvider : AbstractInfoProvider<NotificationTemplateTextInfo, NotificationTemplateTextInfoProvider>
    {
        #region "Variables"

        // Hashtable indexed by key "[gatewayID]_[templateID]".
        private static CMSStatic<ProviderInfoDictionary<string>> mTemplateTextsByName;

        private static readonly object lockObject = new object();

        #endregion


        #region "Properties"


        /// <summary>
        /// Hashtable indexed by key "[gatewayID]_[templateID]".
        /// </summary>
        private static ProviderInfoDictionary<string> TemplateTextsByName
        {
            get
            {
                return LockHelper.Ensure(ref mTemplateTextsByName, CreateTextsDictionary, lockObject);
            }
            set
            {
                mTemplateTextsByName.Value = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates the texts dictionary
        /// </summary>
        private static CMSStatic<ProviderInfoDictionary<string>> CreateTextsDictionary()
        {
            return new CMSStatic<ProviderInfoDictionary<string>>(() => new ProviderInfoDictionary<string>(NotificationTemplateTextInfo.OBJECT_TYPE, "GatewayID;TemplateID"));
        }


        /// <summary>
        /// Returns the query for all notification templates.
        /// </summary>   
        public static ObjectQuery<NotificationTemplateTextInfo> GetNotificationTemplateTexts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the notification template text info structure for the specified notification template text.
        /// </summary>
        /// <param name="templateTextId">Template text ID</param>        
        public static NotificationTemplateTextInfo GetNotificationTemplateTextInfo(int templateTextId)
        {
            return ProviderObject.GetInfoById(templateTextId);
        }


        /// <summary>
        /// Returns the notification template text info structure for the specified notification template text.
        /// </summary>
        /// <param name="gatewayId">Gateway ID</param>
        /// <param name="templateId">Template ID</param>
        public static NotificationTemplateTextInfo GetNotificationTemplateTextInfo(int gatewayId, int templateId)
        {
            // Look for info in cache first
            return (TemplateTextsByName[GetHashKey(gatewayId, templateId)] as NotificationTemplateTextInfo)
                ?? GetNotificationTemplateTextInfoFromDB(gatewayId, templateId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified notification template text.
        /// </summary>
        /// <param name="notificationTemplateText">Notification template text to set</param>
        public static void SetNotificationTemplateTextInfo(NotificationTemplateTextInfo notificationTemplateText)
        {
            ProviderObject.SetInfo(notificationTemplateText);
        }


        /// <summary>
        /// Deletes specified notification template text.
        /// </summary>
        /// <param name="infoObj">Notification template text object</param>
        public static void DeleteNotificationTemplateTextInfo(NotificationTemplateTextInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified notification template text.
        /// </summary>
        /// <param name="gatewayId">Gateway ID</param>
        /// <param name="templateId">Template ID</param>
        public static void DeleteNotificationTemplateTextInfo(int gatewayId, int templateId)
        {
            NotificationTemplateTextInfo infoObj = GetNotificationTemplateTextInfo(gatewayId, templateId);
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified notification template text.
        /// </summary>
        /// <param name="templateTextId">Template text ID</param>
        public static void DeleteNotificationTemplateTextInfo(int templateTextId)
        {
            NotificationTemplateTextInfo infoObj = GetNotificationTemplateTextInfo(templateTextId);
            ProviderObject.DeleteInfo(infoObj);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            if (TemplateTextsByName != null)
            {
                TemplateTextsByName.Invalidate(logTasks);
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(NotificationTemplateTextInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);
                TemplateTextsByName.Delete(GetHashKey(info.GatewayID, info.TemplateID));
            }
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(NotificationTemplateTextInfo info)
        {
            base.SetInfo(info);
            TemplateTextsByName.Update(GetHashKey(info.GatewayID, info.TemplateID), info);
        }


        /// <summary>
        /// Returns key for the hashtable of the templatetexts according to the parameters.
        /// </summary>
        /// <param name="gatewayId">ID of the gateway</param>
        /// <param name="templateId">ID of the template</param>
        private static string GetHashKey(int gatewayId, int templateId)
        {
            return gatewayId + "_" + templateId;
        }


        /// <summary>
        /// Returns the notification template text info structure for the specified notification template text.
        /// </summary>
        /// <param name="gatewayId">Gateway ID</param>
        /// <param name="templateId">Template ID</param>
        private static NotificationTemplateTextInfo GetNotificationTemplateTextInfoFromDB(int gatewayId, int templateId)
        {
            NotificationTemplateTextInfo result = GetNotificationTemplateTexts().TopN(1)
                .WhereEquals("GatewayID", gatewayId)
                .WhereEquals("TemplateID", templateId).FirstOrDefault();

            if (result != null)
            {
                TemplateTextsByName.Update(GetHashKey(gatewayId, templateId), result);
            }

            return result;
        }

        #endregion
    }
}