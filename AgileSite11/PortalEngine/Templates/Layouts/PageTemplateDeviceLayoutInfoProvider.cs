using System;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Provides access to information about page template device layouts.
    /// </summary>
    public class PageTemplateDeviceLayoutInfoProvider : AbstractInfoProvider<PageTemplateDeviceLayoutInfo, PageTemplateDeviceLayoutInfoProvider>, IFullNameInfoProvider
    {
        #region "Variables"

        /// <summary>
        /// DeviceProfile layout directory directory.
        /// </summary>
        private const string mDeviceLayoutsDirectory = "~/CMSVirtualFiles/Templates/Device/Shared";


        /// <summary>
        /// AdHoc DeviceProfile layout directory directory.
        /// </summary>
        private const string mAdHocDeviceLayoutsDirectory = "~/CMSVirtualFiles/Templates/Device/AdHoc";

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets device profile layouts directory.
        /// </summary>
        public static string DeviceLayoutsDirectory
        {
            get
            {
                return mDeviceLayoutsDirectory;
            }
        }


        /// <summary>
        /// Gets device profile AdHoc layouts directory.
        /// </summary>
        public static string AdHocDeviceLayoutsDirectory
        {
            get
            {
                return mAdHocDeviceLayoutsDirectory;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public PageTemplateDeviceLayoutInfoProvider()
            : base(PageTemplateDeviceLayoutInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    FullName = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns TemplateDeviceInfo for specified template and device profile.
        /// </summary>
        /// <param name="templateId">Template id</param>
        /// <param name="deviceProfileId">Device profile id</param>
        public static PageTemplateDeviceLayoutInfo GetTemplateDeviceLayoutInfo(int templateId, int deviceProfileId)
        {
            return ProviderObject.GetTemplateDeviceLayoutInfoInternal(templateId, deviceProfileId);
        }


        /// <summary>
        /// Returns all template device layouts.
        /// </summary>
        public static ObjectQuery<PageTemplateDeviceLayoutInfo> GetTemplateDeviceLayouts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns template device layout with specified ID.
        /// </summary>
        /// <param name="deviceLayoutId">Template device layout ID.</param>        
        public static PageTemplateDeviceLayoutInfo GetTemplateDeviceLayoutInfo(int deviceLayoutId)
        {
            return ProviderObject.GetInfoById(deviceLayoutId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified template device layout.
        /// </summary>
        /// <param name="deviceLayoutObj">Template device layout to be set.</param>
        public static void SetTemplateDeviceLayoutInfo(PageTemplateDeviceLayoutInfo deviceLayoutObj)
        {
            ProviderObject.SetInfo(deviceLayoutObj);
        }


        /// <summary>
        /// Deletes specified template device layout.
        /// </summary>
        /// <param name="deviceLayoutObj">Template device layout to be deleted.</param>
        public static void DeleteTemplateDeviceLayoutInfo(PageTemplateDeviceLayoutInfo deviceLayoutObj)
        {
            ProviderObject.DeleteInfo(deviceLayoutObj);
        }


        /// <summary>
        /// Deletes template device layout with specified ID.
        /// </summary>
        /// <param name="deviceLayoutId">Template device layout ID.</param>
        public static void DeleteTemplateDeviceLayoutInfo(int deviceLayoutId)
        {
            PageTemplateDeviceLayoutInfo subscriptionObj = GetTemplateDeviceLayoutInfo(deviceLayoutId);
            DeleteTemplateDeviceLayoutInfo(subscriptionObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Creates new dictionary for caching the objects by full name
        /// </summary>
        public ProviderInfoDictionary<string> GetFullNameDictionary()
        {
            return new ProviderInfoDictionary<string>(PageTemplateDeviceLayoutInfo.OBJECT_TYPE, "PageTemplateID;ProfileID");
        }


        /// <summary>
        /// Gets the where condition that searches the object based on the given full name
        /// </summary>
        /// <param name="fullName">Object full name</param>
        public string GetFullNameWhereCondition(string fullName)
        {
            string deviceid;
            string templateid;

            // Parse the full name
            if (ObjectHelper.ParseFullName(fullName, out templateid, out deviceid))
            {
                // Build the where condition
                return new WhereCondition()
                    .WhereEquals("ProfileID", ValidationHelper.GetInteger(deviceid, 0))
                    .WhereEquals("PageTemplateID", ValidationHelper.GetInteger(templateid, 0))
                    .ToString(true);
            }

            return null;
        }


        /// <summary>
        /// Returns TemplateDeviceInfo for specified template and device profile.
        /// </summary>
        /// <param name="templateId">Template id</param>
        /// <param name="deviceProfileId">Device profile id</param>
        protected virtual PageTemplateDeviceLayoutInfo GetTemplateDeviceLayoutInfoInternal(int templateId, int deviceProfileId)
        {
            return GetInfoByFullName(templateId + "." + deviceProfileId);
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(PageTemplateDeviceLayoutInfo info)
        {
            // Change version GUID for VPP only if the code changed
            bool layoutChanged = info.ItemChanged("LayoutCode") || info.ItemChanged("LayoutCSS");

            if (layoutChanged || String.IsNullOrEmpty(info.LayoutVersionGUID))
            {
                info.LayoutVersionGUID = Guid.NewGuid().ToString();
            }

            base.SetInfo(info);
        }

        #endregion


        #region "Public methods - Extended"

        /// <summary>
        /// Returns the layout object and type for specific page template
        /// </summary>
        /// <param name="ti">Template info</param>
        /// <param name="dpi">Current device profile info</param>
        public static object GetLayoutObject(PageTemplateInfo ti, DeviceProfileInfo dpi)
        {
            PageTemplateLayoutTypeEnum type;
            return GetLayoutObject(ti, dpi, out type);
        }


        /// <summary>
        /// Returns the layout object and type for specific page template
        /// </summary>
        /// <param name="ti">Template info</param>
        /// <param name="dpi">Current device profile info</param>
        /// <param name="type">Template layout type</param>
        public static object GetLayoutObject(PageTemplateInfo ti, DeviceProfileInfo dpi, out PageTemplateLayoutTypeEnum type)
        {
            // Try get current device profile
            if (dpi != null)
            {
                // Try custom device mapping
                PageTemplateDeviceLayoutInfo dli = GetTemplateDeviceLayoutInfo(ti.PageTemplateId, dpi.ProfileID);
                if (dli != null)
                {
                    // Shared layout
                    if (dli.LayoutID > 0)
                    {
                        type = PageTemplateLayoutTypeEnum.DeviceSharedLayout;
                        return LayoutInfoProvider.GetLayoutInfo(dli.LayoutID);
                    }
                    // Custom device layout
                    else
                    {
                        type = PageTemplateLayoutTypeEnum.DeviceLayout;
                        return dli;
                    }
                }

                // Try auto mapping
                if (ti.LayoutID > 0)
                {
                    LayoutInfo mappedInfo = DeviceProfileLayoutInfoProvider.GetTargetLayoutInfo(dpi.ProfileID, ti.LayoutID);
                    if (mappedInfo != null)
                    {
                        type = PageTemplateLayoutTypeEnum.SharedLayoutMapped;
                        return mappedInfo;
                    }
                }
            }

            // Shared layout
            if (ti.LayoutID > 0)
            {
                type = PageTemplateLayoutTypeEnum.SharedLayout;
                return LayoutInfoProvider.GetLayoutInfo(ti.LayoutID);
            }

            // Custom template layout
            type = PageTemplateLayoutTypeEnum.PageTemplateLayout;
            return ti;
        }


        /// <summary>
        /// Clone the info object and combine it with the layout content (layout code, type, css...).
        /// </summary>
        /// <param name="infoObj">The source info object</param>
        /// <param name="layoutId">The id of the source layout</param>
        public static BaseInfo CloneInfoObject(BaseInfo infoObj, int layoutId)
        {
            BaseInfo infoObjCloned = null;

            LayoutInfo li = LayoutInfoProvider.GetLayoutInfo(layoutId);

            if (infoObj is PageTemplateInfo)
            {
                // We have to work with the clone, because we need to change the data of the object 
                // (copy from shared layout)
                PageTemplateInfo ptiClone = ((PageTemplateInfo)infoObj).Clone();

                if (li != null)
                {
                    // Delete the shared layout
                    ptiClone.LayoutID = 0;

                    // Copy values
                    ptiClone.PageTemplateLayout = li.LayoutCode;
                    ptiClone.PageTemplateCSS = li.LayoutCSS;
                    ptiClone.PageTemplateLayoutType = li.LayoutType;
                }

                infoObjCloned = ptiClone;
            }
            else if (infoObj is PageTemplateDeviceLayoutInfo)
            {
                PageTemplateDeviceLayoutInfo deviceLayoutClone = ((PageTemplateDeviceLayoutInfo)infoObj).Clone();

                // Get the old layout
                li = LayoutInfoProvider.GetLayoutInfo(layoutId);
                if (li != null)
                {
                    deviceLayoutClone.LayoutID = 0;

                    // Device layout - copy values
                    deviceLayoutClone.LayoutCode = li.LayoutCode;
                    deviceLayoutClone.LayoutCSS = li.LayoutCSS;
                    deviceLayoutClone.LayoutType = li.LayoutType;
                }

                infoObjCloned = deviceLayoutClone;
            }

            return infoObjCloned;
        }


        /// <summary>
        /// Returns web part layout info for specified path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="isAdHoc">Indicates whether current object has binding on ad-hoc template</param>
        public static PageTemplateDeviceLayoutInfo GetVirtualObject(string path, bool isAdHoc)
        {
            List<string> prefixes = new List<string>();
            // Get layout code name and web part code name
            string templateName = VirtualPathHelper.GetVirtualObjectName(path, isAdHoc ? AdHocDeviceLayoutsDirectory : DeviceLayoutsDirectory, ref prefixes);

            string deviceName = isAdHoc ? prefixes[1] : prefixes[0];
            string siteName = isAdHoc ? prefixes[0] : null;

            PageTemplateInfo pti = PageTemplateInfoProvider.GetPageTemplateInfo(templateName, SiteInfoProvider.GetSiteID(siteName));
            if (pti != null)
            {
                DeviceProfileInfo dpi = DeviceProfileInfoProvider.GetDeviceProfileInfo(deviceName);
                if (dpi != null)
                {
                    return GetTemplateDeviceLayoutInfo(pti.PageTemplateId, dpi.ProfileID);
                }
            }
            return null;
        }


        /// <summary>
        /// Returns device layout name and prefixes if available
        /// </summary>
        /// <param name="path">Current path</param>
        /// <param name="isAdHoc">Indicates whether layout is on AdHoc page template</param>
        /// <param name="prefixes">Path prefixes</param>
        public static string GetDeviceLayoutName(string path, bool isAdHoc, ref List<string> prefixes)
        {
            string directory = DeviceLayoutsDirectory;

            if (isAdHoc)
            {
                directory = AdHocDeviceLayoutsDirectory;
            }
            return VirtualPathHelper.GetVirtualObjectName(path, directory, ref prefixes);
        }

        #endregion
    }
}
