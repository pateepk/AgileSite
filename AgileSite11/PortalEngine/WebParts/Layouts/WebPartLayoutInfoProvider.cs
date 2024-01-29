using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Class providing WebPartLayoutInfo management.
    /// </summary>
    public class WebPartLayoutInfoProvider : AbstractInfoProvider<WebPartLayoutInfo, WebPartLayoutInfoProvider>, IFullNameInfoProvider
    {
        #region "Variables"

        private const string mWebPartLayoutsDirectory = "~/CMSVirtualFiles/WebPartLayouts";

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the value that indicates whether web part layouts should be stored externally
        /// </summary>
        public static bool StoreWebPartLayoutsInExternalStorage
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSStoreWebPartLayoutsInFS");
            }
            set
            {
                SettingsKeyInfoProvider.SetGlobalValue("CMSStoreWebPartLayoutsInFS", value);
            }
        }


        /// <summary>
        /// Readonly property LayoutsDirectory.
        /// </summary>
        public static string WebPartLayoutsDirectory
        {
            get
            {
                return mWebPartLayoutsDirectory;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public WebPartLayoutInfoProvider()
            : base(WebPartLayoutInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                FullName = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static WebPartLayoutInfo GetWebPartLayoutInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the WebPartLayoutInfo structure for the specified webPartLayout.
        /// </summary>
        /// <param name="webPartLayoutId">WebPartLayout id</param>
        public static WebPartLayoutInfo GetWebPartLayoutInfo(int webPartLayoutId)
        {
            return ProviderObject.GetInfoById(webPartLayoutId);
        }


        /// <summary>
        /// Returns the WebPartLayoutInfo structure for the specified webPartLayout.
        /// </summary>
        /// <param name="webPartCodeName">Web part code name</param>
        /// <param name="webPartLayoutCodeName">WebPartLayout code name</param>
        public static WebPartLayoutInfo GetWebPartLayoutInfo(string webPartCodeName, string webPartLayoutCodeName)
        {
            return ProviderObject.GetWebPartLayoutInfoInternal(webPartCodeName, webPartLayoutCodeName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified webPartLayout.
        /// </summary>
        /// <param name="infoObj">Web part layout to set</param>
        public static void SetWebPartLayoutInfo(WebPartLayoutInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified webPartLayout.
        /// </summary>
        /// <param name="webPartLayoutObj">WebPartLayout object</param>
        public static void DeleteWebPartLayoutInfo(WebPartLayoutInfo webPartLayoutObj)
        {
            ProviderObject.DeleteInfo(webPartLayoutObj);
        }


        /// <summary>
        /// Deletes specified webPartLayout.
        /// </summary>
        /// <param name="webPartLayoutId">WebPartLayout id</param>
        public static void DeleteWebPartLayoutInfo(int webPartLayoutId)
        {
            WebPartLayoutInfo webPartLayoutObj = GetWebPartLayoutInfo(webPartLayoutId);
            DeleteWebPartLayoutInfo(webPartLayoutObj);
        }


        /// <summary>
        /// Deletes all web part layouts.
        /// </summary>
        /// <param name="webPartId">Web part ID</param>
        public static void DeleteWebPartLayouts(int webPartId)
        {
            // Delete all layouts
            DataSet layoutsDS = GetWebPartLayouts(webPartId);
            if (!DataHelper.DataSourceIsEmpty(layoutsDS))
            {
                foreach (DataRow dr in layoutsDS.Tables[0].Rows)
                {
                    int layoutId = ValidationHelper.GetInteger(dr["WebPartLayoutID"], 0);
                    DeleteWebPartLayoutInfo(layoutId);
                }
            }
        }


        /// <summary>
        /// Gets all web part layouts.
        /// </summary>
        public static ObjectQuery<WebPartLayoutInfo> GetWebPartLayouts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns all layouts associated to the web part specified by ID.
        /// </summary>
        /// <param name="webPartId">Web part ID</param>
        public static ObjectQuery<WebPartLayoutInfo> GetWebPartLayouts(int webPartId)
        {
            return GetWebPartLayouts().WhereEquals("WebPartLayoutWebPartID", webPartId);
        }


        /// <summary>
        /// Returns web part layout info for specified path
        /// </summary>
        /// <param name="path">Path</param>
        public static WebPartLayoutInfo GetVirtualObject(string path)
        {
            List<string> prefixes = new List<string>();

            // Get layout code name and web part code name
            string layoutCodeName = VirtualPathHelper.GetVirtualObjectName(path, WebPartLayoutsDirectory, ref prefixes);
            string webPartCodeName = prefixes[0];

            return GetWebPartLayoutInfo(webPartCodeName, layoutCodeName);
        }


        /// <summary>
        /// Resets the default layout indicator for the given web part.
        /// </summary>
        /// <param name="webPartId">The web part id</param>
        public static void ResetDefaultLayout(int webPartId)
        {
            ProviderObject.ResetDefaultLayoutInternal(webPartId);
        }


        /// <summary>
        /// Gets the default layout for the given inherited web part.
        /// </summary>
        /// <param name="webPartId">The web part id</param>
        public static WebPartLayoutInfo GetDefaultLayout(int webPartId)
        {
            return ProviderObject.GetDefaultLayoutInternal(webPartId);
        }


        /// <summary>
        /// Replaces codebehind for codefile in layout definition
        /// </summary>
        /// <param name="input">Layout definition</param>
        public static string ReplaceCodeFile(String input)
        {
            CMSRegex reg = new CMSRegex(@"(<%@.*)(CodeFile\s*=)(.[^%]*%>)", RegexOptions.Singleline);
            return reg.Replace(input, m => String.Format("{0}CodeBehind={1}", m.Groups[1], m.Groups[3]));
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the WebPartLayoutInfo structure for the specified webPartLayout.
        /// </summary>
        /// <param name="webPartCodeName">Web part code name</param>
        /// <param name="webPartLayoutCodeName">WebPartLayout code name</param>
        protected virtual WebPartLayoutInfo GetWebPartLayoutInfoInternal(string webPartCodeName, string webPartLayoutCodeName)
        {
            string fullCodeName = ObjectHelper.BuildFullName(webPartCodeName, webPartLayoutCodeName, "|");
            return ProviderObject.GetInfoByFullName(fullCodeName);
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(WebPartLayoutInfo info)
        {
            if (info != null)
            {
                // Change version GUID for VPP only if the layout or CSS changed
                bool layoutChanged = info.ItemChanged(WebPartLayoutInfo.EXTERNAL_COLUMN_CODE) || info.ItemChanged(WebPartLayoutInfo.EXTERNAL_COLUMN_CSS);
                if (layoutChanged || String.IsNullOrEmpty(info.WebPartLayoutVersionGUID))
                {
                    info.WebPartLayoutVersionGUID = Guid.NewGuid().ToString();
                }

                if (info.WebPartLayoutIsDefault)
                {
                    // Erase the IsDefault flag for all layouts of the parent web part (only for inherited web parts)
                    ResetDefaultLayout(info.WebPartLayoutWebPartID);
                }

                base.SetInfo(info);
            }
            else
            {
                throw new Exception("[WebPartLayoutInfoProvider.SetWebPartLayoutInfo]: No WebPartLayoutInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(WebPartLayoutInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);
            }
        }


        /// <summary>
        /// Resets the default layout indicator for all layouts of the given web part.
        /// </summary>
        /// <param name="webPartId">The web part id</param>
        protected virtual void ResetDefaultLayoutInternal(int webPartId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@WebPartId", webPartId);

            // Get the data
            ConnectionHelper.ExecuteQuery("cms.WebPartLayout.ResetDefaultLayout", parameters);

            ClearHashtables(true);
        }


        /// <summary>
        /// Gets the default layout for the given inherited web part.
        /// </summary>
        /// <param name="webPartId">The web part id</param>
        protected virtual WebPartLayoutInfo GetDefaultLayoutInternal(int webPartId)
        {
            return GetWebPartLayouts()
                .Where("WebPartLayoutWebPartID", QueryOperator.Equals, webPartId)
                .WhereTrue("WebPartLayoutIsDefault")
                .TopN(1).FirstOrDefault();
        }

        #endregion


        #region "Full name methods"

        /// <summary>
        /// Creates new dictionary for caching the objects by full name
        /// </summary>
        public ProviderInfoDictionary<string> GetFullNameDictionary()
        {
            return new ProviderInfoDictionary<string>(WebPartLayoutInfo.OBJECT_TYPE, "WebPartLayoutWebPartID;WebPartLayoutCodeName");
        }


        /// <summary>
        /// Gets the where condition that searches the object based on the given full name
        /// </summary>
        /// <param name="fullName">Object full name</param>
        public string GetFullNameWhereCondition(string fullName)
        {
            string webPartCodeName;
            string webPartLayoutCodeName;

            // Parse the full name
            if (ObjectHelper.ParseFullName(fullName, out webPartCodeName, out webPartLayoutCodeName, "|"))
            {
                // Get the web part
                WebPartInfo wpi = WebPartInfoProvider.GetWebPartInfo(webPartCodeName);
                if (wpi != null)
                {
                    return new WhereCondition().WhereEquals("WebPartLayoutWebPartID", wpi.WebPartID).WhereEquals("WebPartLayoutCodeName", webPartLayoutCodeName).ToString(true);
                }
            }

            return null;
        }

        #endregion
    }
}