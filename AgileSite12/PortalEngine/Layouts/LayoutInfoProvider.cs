using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Provides access to information about layout.
    /// </summary>
    public class LayoutInfoProvider : AbstractInfoProvider<LayoutInfo, LayoutInfoProvider>
    {
        #region "Variables"

        /// <summary>
        /// List of default layout namespaces
        /// </summary>
        private static List<string> mDefaultNamespaces = new List<string>() { "CMS.DocumentEngine.Web.UI", "CMS.PortalEngine.Web.UI", "CMS.DocumentEngine" };


        /// <summary>
        /// Layouts directory.
        /// </summary>
        private const string LAYOUTS_DIRECTORY = "~/CMSVirtualFiles/Layouts";


        private static string[] mDefaultDirectives;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the value that indicates whether shared layouts should be stored externally
        /// </summary>
        public static bool StoreLayoutsInExternalStorage
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSStoreLayoutsInFS");
            }
            set
            {
                SettingsKeyInfoProvider.SetGlobalValue("CMSStoreLayoutsInFS", value);
            }
        }


        /// <summary>
        /// Gets layouts directory.
        /// </summary>
        public static string LayoutsDirectory
        {
            get
            {
                return LAYOUTS_DIRECTORY;
            }
        }


        /// <summary>
        /// List of default transformation namespaces
        /// </summary>
        public static List<string> DefaultNamespaces
        {
            get
            {
                return mDefaultNamespaces;
            }
        }


        /// <summary>
        /// Returns list of default layout directives (ToLower).
        /// </summary>
        public static string[] DefaultDirectives
        {
            get
            {
                if (mDefaultDirectives == null)
                {
                    mDefaultDirectives = GetLayoutDirectives().ToLowerCSafe().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                }
                return mDefaultDirectives;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public LayoutInfoProvider()
            : base(LayoutInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns layout info for specified path
        /// </summary>
        /// <param name="path">Path</param>
        public static LayoutInfo GetVirtualObject(string path)
        {
            List<string> prefixes = new List<string>();
            // Get layout code name and web part code name
            string layoutName = VirtualPathHelper.GetVirtualObjectName(path, LayoutsDirectory, ref prefixes);
            return GetLayoutInfo(layoutName);
        }


        /// <summary>
        /// Sets the specified layout data.
        /// </summary>
        /// <param name="layout">Layout object</param>
        public static void SetLayoutInfo(LayoutInfo layout)
        {
            ProviderObject.SetInfo(layout);
        }


        /// <summary>
        /// Returns the LayoutInfo structure for the specified layout.
        /// </summary>
        /// <param name="layoutCodeName">Layout code name</param>
        public static LayoutInfo GetLayoutInfo(string layoutCodeName)
        {
            return ProviderObject.GetInfoByCodeName(layoutCodeName);
        }


        /// <summary>
        /// Returns the LayoutInfo structure for the specified layout.
        /// </summary>
        /// <param name="layoutId">Layout ID</param>
        public static LayoutInfo GetLayoutInfo(int layoutId)
        {
            return ProviderObject.GetInfoById(layoutId);
        }


        /// <summary>
        /// Deletes specified layout.
        /// </summary>
        /// <param name="layout">Layout object</param>
        public static void DeleteLayoutInfo(LayoutInfo layout)
        {
            ProviderObject.DeleteInfo(layout);

            // Clear PageTemplateInfo hash tables
            ProviderHelper.ClearHashtables(PageTemplateInfo.OBJECT_TYPE, true);

            if (layout != null)
            {
                DeviceProfileLayoutInfoProvider.ClearTargetLayoutIdentifierHashtable(true);
            }
        }


        /// <summary>
        /// Deletes specified layout.
        /// </summary>
        /// <param name="layoutId">Layout ID</param>
        public static void DeleteLayoutInfo(int layoutId)
        {
            // Get layout info
            LayoutInfo li = GetLayoutInfo(layoutId);
            DeleteLayoutInfo(li);
        }


        /// <summary>
        /// Returns all layouts.
        /// </summary>
        public static ObjectQuery<LayoutInfo> GetLayouts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns full layout name from the given virtual layout path.
        /// </summary>
        /// <param name="url">Virtual layout path</param>
        [Obsolete("Method is not intended to be used in custom code.")]
        public static string GetLayoutName(string url)
        {
            if (CMSHttpContext.Current != null)
            {
                string physicalPath = CMSHttpContext.Current.Request.MapPath(url);
                string physicalPathVirtDir = CMSHttpContext.Current.Request.MapPath(LayoutsDirectory);

                // gets the path behind layout directory
                string newPath = physicalPath.Remove(0, physicalPathVirtDir.Length + 1);
                newPath = newPath.Replace("\\", ".");
                newPath = URLHelper.RemoveFirstPart(newPath, ".");

                // gets file name from the specified path without extension
                newPath = Path.GetFileNameWithoutExtension(newPath);

                return newPath;
            }
            return "";
        }


        /// <summary>
        /// Layout directives are set before layout code.
        /// </summary>
        public static string GetLayoutDirectives()
        {
            // Get language code from web.config.
            string lang = DataHelper.GetNotEmpty(SettingsHelper.AppSettings["CMSProgrammingLanguage"], "C#");

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<%@ Control Language=\"{0}\" Inherits=\"CMS.PortalEngine.Web.UI.CMSAbstractLayout\" %> \n", lang);

            // Add namespaces
            foreach (string ns in mDefaultNamespaces)
            {
                string reg = TransformationInfoProvider.GetNamespaceRegistration("cc1", ns, null);
                sb.AppendLine(reg);
            }

            return sb.ToString();
        }


        /// <summary>
        /// Adds the layout directives to the transformation.
        /// </summary>
        /// <param name="code">Code of the layout</param>
        /// <param name="type">Type of the layout</param>
        public static string AddLayoutDirectives(string code, LayoutTypeEnum type)
        {
            if (type == LayoutTypeEnum.Ascx)
            {
                return AddLayoutDirectives(code);
            }

            // Do not add directives to non-ascx code.
            return code;
        }


        /// <summary>
        /// Adds the layout directives to the transformation.
        /// </summary>
        /// <param name="code">Code of the layout</param>
        public static string AddLayoutDirectives(string code)
        {
            if ((code != null) && !code.StartsWithCSafe("<%@ Control ", true))
            {
                // Add the directives
                return GetLayoutDirectives() + code;
            }
            else
            {
                // Already contains directives
                return code;
            }
        }


        /// <summary>
        /// Gets the layout type enumeration for the given string value.
        /// </summary>
        /// <param name="type">String type</param>
        public static LayoutTypeEnum GetLayoutTypeEnum(string type)
        {
            return LayoutHelper.GetLayoutTypeEnum(type, LayoutTypeEnum.Ascx);
        }


        /// <summary>
        /// Gets the layout type code for the given enum value.
        /// </summary>
        /// <param name="type">String type</param>
        public static string GetLayoutTypeCode(LayoutTypeEnum type)
        {
            return LayoutHelper.GetLayoutTypeCode(type);
        }


        /// <summary>
        /// Counts webpart zones in layout code.
        /// </summary>
        /// <param name="layoutCode">Layout code</param>
        /// <param name="layoutType">Layout code type enum</param>
        /// <returns>Number of webpart zones in layout code.</returns>
        public static int CountWebpartZones(string layoutCode, LayoutTypeEnum layoutType)
        {
            return ProviderObject.CountWebpartZonesInternal(layoutCode, layoutType);
        }


        /// <summary>
        /// Counts webpart zones in layout code.
        /// </summary>
        /// <param name="layoutCode">Layout code</param>
        /// <param name="layoutTypeName">Layout code type name</param>
        /// <returns>Number of webpart zones in layout code.</returns>
        public static int CountWebpartZones(string layoutCode, string layoutTypeName)
        {
            return CountWebpartZones(layoutCode, GetLayoutTypeEnum(layoutTypeName));
        }


        #endregion


        #region "Internal methods"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(LayoutInfo info)
        {
            if (info != null)
            {
                // Change version GUID for VPP only if the code changed
                bool layoutChanged = info.ItemChanged("LayoutCode") || info.ItemChanged("LayoutCSS");
                if (layoutChanged || String.IsNullOrEmpty(info.LayoutVersionGUID))
                {
                    info.LayoutVersionGUID = Guid.NewGuid().ToString();
                }

                if ((info.LayoutZoneCount < 0) || info.ItemChanged("LayoutCode") || info.ItemChanged("LayoutType"))
                {
                    LayoutInfo oldInfo = GetLayoutInfo(info.LayoutId);
                    if ((info.LayoutZoneCount < 0) || ((oldInfo != null) && (oldInfo.LayoutZoneCountAutomatic == oldInfo.LayoutZoneCount) && (oldInfo.LayoutZoneCount == info.LayoutZoneCount)))
                    {
                        info.LayoutZoneCount = info.LayoutZoneCountAutomatic;
                    }
                }
                base.SetInfo(info);
            }
        }


        /// <summary>
        /// Counts webpart zones in layout code.
        /// </summary>
        /// <param name="layoutCode">Layout code</param>
        /// <param name="layoutType">Layout code</param>
        /// <returns>Number of webpart zones in layout code.</returns>
        protected virtual int CountWebpartZonesInternal(string layoutCode, LayoutTypeEnum layoutType)
        {
            int count = 0;
            if (!String.IsNullOrEmpty(layoutCode))
            {
                switch (layoutType)
                {
                    // ASCX type
                    case LayoutTypeEnum.Ascx:
                        try
                        {
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml("<?xml version='1.0'?><root xmlns:cms=\"kentico.cms\" xmlns:cc1=\"kentico.cc1\">" + layoutCode + "</root>");
                            XmlNodeList zonesCms = xmlDoc.GetElementsByTagName("cms:CMSWebPartZone");
                            XmlNodeList zonesCc1 = xmlDoc.GetElementsByTagName("cc1:CMSWebPartZone");
                            count = zonesCms.Count + zonesCc1.Count;
                        }
                        catch
                        {
                            count = layoutCode.Split(new[] { ":CMSWebPartZone" }, StringSplitOptions.None).Length - 1;
                        }
                        break;

                    // HTML type
                    case LayoutTypeEnum.Html:
                        count = layoutCode.Split(new[] { "{^WebPartZone|" }, StringSplitOptions.None).Length - 1;
                        break;
                }
            }
            return count;
        }

        #endregion
    }
}