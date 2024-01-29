using System;

using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class providing <see cref="EmailWidgetInfo"/> management.
    /// </summary>
    public class EmailWidgetInfoProvider : AbstractInfoProvider<EmailWidgetInfo, EmailWidgetInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Creates a new instances of the <see cref="EmailWidgetInfoProvider"/> class.
        /// </summary>
        public EmailWidgetInfoProvider()
            : base(EmailWidgetInfo.TYPEINFO, new HashtableSettings
            {
                GUID = true
            })
        {
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns a query for all the <see cref="EmailWidgetInfo"/> objects.
        /// </summary>
        public static ObjectQuery<EmailWidgetInfo> GetEmailWidgets()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="EmailWidgetInfo"/> with specified ID.
        /// </summary>
        /// <param name="id">Email widget ID.</param>
        public static EmailWidgetInfo GetEmailWidgetInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns <see cref="EmailWidgetInfo"/> with specified name.
        /// </summary>
        /// <param name="name">Email widget name.</param>
        /// <param name="siteName">Site name.</param>
        public static EmailWidgetInfo GetEmailWidgetInfo(string name, string siteName)
        {
            return ProviderObject.GetInfoByCodeName(name, SiteInfoProvider.GetSiteID(siteName));
        }


        /// <summary>
        /// Returns <see cref="EmailWidgetInfo"/> with specified GUID.
        /// </summary>
        /// <param name="guid">Email widget GUID.</param>                
        /// <param name="siteName">Site name.</param>         
        public static EmailWidgetInfo GetEmailWidgetInfo(Guid guid, string siteName)
        {
            return ProviderObject.GetInfoByGuid(guid, SiteInfoProvider.GetSiteID(siteName));
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="EmailWidgetInfo"/>.
        /// </summary>
        /// <param name="infoObj">Email widget to be set.</param>
        public static void SetEmailWidgetInfo(EmailWidgetInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="EmailWidgetInfo"/>.
        /// </summary>
        /// <param name="infoObj">EmailWidgetInfo to be deleted.</param>
        public static void DeleteEmailWidgetInfo(EmailWidgetInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="EmailWidgetInfo"/> with specified ID.
        /// </summary>
        /// <param name="id">Email widget ID.</param>
        public static void DeleteEmailWidgetInfo(int id)
        {
            EmailWidgetInfo infoObj = GetEmailWidgetInfo(id);
            DeleteEmailWidgetInfo(infoObj);
        }

        #endregion
    }
}