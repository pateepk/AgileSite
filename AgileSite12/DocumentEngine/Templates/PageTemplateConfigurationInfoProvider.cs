using System;
using CMS.DataEngine;


namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class providing <see cref="PageTemplateConfigurationInfo"/> management.
    /// </summary>
    public class PageTemplateConfigurationInfoProvider : AbstractInfoProvider<PageTemplateConfigurationInfo, PageTemplateConfigurationInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="PageTemplateConfigurationInfoProvider"/>.
        /// </summary>
        public PageTemplateConfigurationInfoProvider()
            : base(PageTemplateConfigurationInfo.TYPEINFO, new HashtableSettings {
                GUID = true
            })
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="PageTemplateConfigurationInfo"/> objects.
        /// </summary>
        public static ObjectQuery<PageTemplateConfigurationInfo> GetPageTemplateConfigurations()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="PageTemplateConfigurationInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="PageTemplateConfigurationInfo"/> ID.</param>
        public static PageTemplateConfigurationInfo GetPageTemplateConfigurationInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns <see cref="PageTemplateConfigurationInfo"/> with specified GUID.
        /// </summary>
        /// <param name="guid"><see cref="PageTemplateConfigurationInfo"/> GUID.</param>
        /// <param name="siteId">Site ID.</param>
        public static PageTemplateConfigurationInfo GetPageTemplateConfigurationInfoByGUID(Guid guid, int siteId)
        {
            return ProviderObject.GetInfoByGuid(guid, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="PageTemplateConfigurationInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="PageTemplateConfigurationInfo"/> to be set.</param>
        public static void SetPageTemplateConfigurationInfo(PageTemplateConfigurationInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="PageTemplateConfigurationInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="PageTemplateConfigurationInfo"/> to be deleted.</param>
        public static void DeletePageTemplateConfigurationInfo(PageTemplateConfigurationInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="PageTemplateConfigurationInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="PageTemplateConfigurationInfo"/> ID.</param>
        public static void DeletePageTemplateConfigurationInfo(int id)
        {
            PageTemplateConfigurationInfo infoObj = GetPageTemplateConfigurationInfo(id);
            DeletePageTemplateConfigurationInfo(infoObj);
        }
    }
}