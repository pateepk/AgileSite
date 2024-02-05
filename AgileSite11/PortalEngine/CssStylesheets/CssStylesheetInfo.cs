using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.Helpers;
using CMS.IO;
using CMS.PortalEngine;
using CMS.SiteProvider;

[assembly: RegisterObjectType(typeof(CssStylesheetInfo), CssStylesheetInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// CSS style sheet info data container class.
    /// </summary>
    public class CssStylesheetInfo : AbstractInfo<CssStylesheetInfo>, IThemeInfo
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.cssstylesheet";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CssStylesheetInfoProvider), OBJECT_TYPE, "CMS.CssStylesheet", "StylesheetID", "StylesheetLastModified", "StylesheetGUID", "StylesheetName", "StylesheetDisplayName", null, null, null, null)
        {
            Extends = new List<ExtraColumn>()
            {
                new ExtraColumn(SiteInfo.OBJECT_TYPE, "SiteDefaultStylesheetID"), 
                new ExtraColumn(SiteInfo.OBJECT_TYPE, "SiteDefaultEditorStylesheet"),
            },
            ModuleName = ModuleName.DESIGN,
            SupportsLocking = true,
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
            },
            HasExternalColumns = true,
            VersionGUIDColumn = "StylesheetVersionGUID",
            CSSColumn = EXTERNAL_COLUMN_CSS,
            SupportsVersioning = true,
            TouchCacheDependencies = true,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                }
            },
            LogEvents = true,
            ContinuousIntegrationSettings = 
            {
                Enabled = true
            }
        };

        #endregion


        #region "Constants"

        /// <summary>
        /// External column name for style sheet text.
        /// </summary>
        public const string EXTERNAL_COLUMN_CSS = "StylesheetText";


        /// <summary>
        /// External column name for style sheet dynamic code.
        /// </summary>
        private const string EXTERNAL_COLUMN_DYNAMICCODE = "StylesheetDynamicCode";


        /// <summary>
        /// The constant represents plain CSS option for stylesheet type.
        /// </summary>
        public const string PLAIN_CSS = "plaincss";


        /// <summary>
        /// Common CSS file extension.
        /// </summary>
        private const string CSS_EXTENSION = ".css";


        /// <summary>
        /// Permission name for destroying CSS stylesheets.
        /// </summary>
        public const string PERMISSION_DESTROY_CSS_STYLESHEETS = "DestroyCMSCSSStylesheet";


        /// <summary>
        /// Permission name for modifying CSS stylesheets.
        /// </summary>
        public const string PERMISSION_MODIFY_CSS_STYLESHEETS = "ModifyCMSCSSStylesheet";


        /// <summary>
        /// Permission name for reading CSS stylesheets.
        /// </summary>
        public const string PERMISSION_READ_CSS_STYLESHEETS = "ReadCMSCSSStylesheet";

        #endregion


        #region "Properties"

        /// <summary>
        /// The stylesheet Id.
        /// </summary>
        [DatabaseField]
        public int StylesheetID
        {
            get
            {
                return GetIntegerValue("StylesheetID", 0);
            }
            set
            {
                SetValue("StylesheetID", value);
            }
        }


        /// <summary>
        /// The stylesheet display name.
        /// </summary>
        [DatabaseField]
        public string StylesheetDisplayName
        {
            get
            {
                return GetStringValue("StylesheetDisplayName", "");
            }
            set
            {
                SetValue("StylesheetDisplayName", value);
            }
        }


        /// <summary>
        /// The stylesheet name.
        /// </summary>
        [DatabaseField]
        public string StylesheetName
        {
            get
            {
                return GetStringValue("StylesheetName", "");
            }
            set
            {
                SetValue("StylesheetName", value);
            }
        }


        /// <summary>
        /// The stylesheet text.
        /// </summary>
        [DatabaseField]
        public string StylesheetText
        {
            get
            {
                return GetStringValue("StylesheetText", "");
            }
            set
            {
                SetValue("StylesheetText", value);
            }
        }


        /// <summary>
        /// Stylesheet version GUID.
        /// </summary>
        [DatabaseField]
        public string StylesheetVersionGUID
        {
            get
            {
                return GetStringValue("StylesheetVersionGUID", "");
            }
            set
            {
                SetValue("StylesheetVersionGUID", value);
            }
        }


        /// <summary>
        /// Stylesheet GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid StylesheetGUID
        {
            get
            {
                return GetGuidValue("StylesheetGUID", Guid.Empty);
            }
            set
            {
                SetValue("StylesheetGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime StylesheetLastModified
        {
            get
            {
                return GetDateTimeValue("StylesheetLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("StylesheetLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// The stylesheet dynamic code.
        /// </summary>
        [DatabaseField]
        public string StylesheetDynamicCode
        {
            get
            {
                return GetStringValue("StylesheetDynamicCode", String.Empty);
            }
            set
            {
                SetValue("StylesheetDynamicCode", value);
            }
        }


        /// <summary>
        /// The stylesheet dynamic code straight from the data class without external column mapping.
        /// </summary>
        [DatabaseMapping(false)]
        internal string StylesheetDynamicCodeInternal
        {
            get
            {
                // Get data straightly from the DataClass in order to obtain correct data for special external column
                return ValidationHelper.GetString(DataClass.GetValue("StylesheetDynamicCode"), String.Empty);
            }
        }


        /// <summary>
        /// The stylesheet dynamic language.
        /// </summary>
        [DatabaseField]
        public string StylesheetDynamicLanguage
        {
            get
            {
                return GetStringValue("StylesheetDynamicLanguage", String.Empty);
            }
            set
            {
                SetValue("StylesheetDynamicLanguage", value);
            }
        }


        /// <summary>
        /// Indicates whether the theme path points at an external storage.
        /// </summary>
        [RegisterProperty(Hidden = true)]
        public bool UsesExternalStorage
        {
            get
            {
                return StorageHelper.IsExternalStorage(GetThemePath());
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CssStylesheetInfoProvider.DeleteCssStylesheetInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CssStylesheetInfoProvider.SetCssStylesheetInfo(this);
        }


        /// <summary>
        /// Returns virtual relative path for specific column
        /// </summary>
        /// <param name="externalColumnName">External column name</param>
        /// <param name="versionGuid">Version GUID. If not defined physical path is generated</param>
        protected override string GetVirtualFileRelativePath(string externalColumnName, string versionGuid)
        {
            // Ensure extension
            string extension = GetExternalColumnExtension();
            string directory = CssStylesheetInfoProvider.CSSStylesheetsDirectory;

            return VirtualPathHelper.GetVirtualFileRelativePath(StylesheetName, extension, directory, null, null);
        }


        /// <summary>
        /// Returns path to externally stored CSS Stylesheet codes.
        /// </summary>
        protected override void RegisterExternalColumns()
        {
            base.RegisterExternalColumns();

            ExternalColumnSettings<CssStylesheetInfo> settingsCss = new ExternalColumnSettings<CssStylesheetInfo>()
            {
                StoragePath = m => m.GetVirtualFileRelativePath(EXTERNAL_COLUMN_CSS, null),
                StoreInExternalStorageSettingsKey = "CMSStoreCSSStylesheetsInFS",
                DependencyColumns = new List<string>() { "StylesheetDynamicLanguage" }
            };

            RegisterExternalColumn("StylesheetText", settingsCss);

            ExternalColumnSettings<CssStylesheetInfo> settingsDynamicCode = new ExternalColumnSettings<CssStylesheetInfo>()
            {
                StoragePath = m => m.GetVirtualFileRelativePath(EXTERNAL_COLUMN_DYNAMICCODE, null),
                StoreInExternalStorageSettingsKey = "CMSStoreCSSStylesheetsInFS"
            };

            RegisterExternalColumn(EXTERNAL_COLUMN_DYNAMICCODE, settingsDynamicCode);
        }


        /// <summary>
        /// Indicates whether a given column is allowed to be saved externally.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <returns>Returns true if column is allowed to be saved externally</returns>
        protected override bool AllowExternalColumn(string columnName)
        {
            // Which of the registered external columns will be saved externally depends on the stylesheet language.
            // This covers the case where the unprocessed dynamic code is to be saved externally, i.e. do not save StylesheetText column externally because it holds processed CSS code.
            if (columnName.EqualsCSafe("StylesheetText", true) && !IsPlainCss())
            {
                return false;
            }

            // This covers the case where the plain CSS is to be saved externally, i.e. do not save StylesheetDynamicCode column externally because it doesn't contain any value.
            if (columnName.EqualsCSafe("StylesheetDynamicCode", true) && IsPlainCss())
            {
                return false;
            }

            return base.AllowExternalColumn(columnName);
        }


        /// <summary>
        /// Updates DB version of externally stored columns with the data from external storage.
        /// </summary>
        protected override void UpdateExternalColumns()
        {
            // External columns get updated only if some was modified externally
            if (IsModifiedExternally())
            {
                // Try to get StylesheetText external column value
                object data = GetExternalColumnData(EXTERNAL_COLUMN_CSS, true);

                if (data != null)
                {
                    DataClass.SetValue(EXTERNAL_COLUMN_CSS, data);
                }

                // Try to get StylesheetDynamicCode external column value
                data = GetExternalColumnData(EXTERNAL_COLUMN_DYNAMICCODE, true);

                if (data != null)
                {
                    DataClass.SetValue(EXTERNAL_COLUMN_DYNAMICCODE, data);
                }

                SetObject();
            }
        }


        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <returns>The value as an object</returns>
        public override object GetValue(string columnName)
        {
            /* If stylesheet objects are stored externally then either only StylesheetText (in case of plain CSS) column 
             * or only StylesheetDynamicCode column (in case of dynamic language) is stored externally. Under these conditions, 
             * the StylesheetText value must always give plain CSS, therefore, in case of dynamic language, 
             * StylesheetDynamicCode value is parsed and returned as the result. */
            if (columnName.EqualsCSafe("StylesheetText", true))
            {
                if (!IsPlainCss() && CssStylesheetInfoProvider.StoreCSSStyleSheetsInExternalStorage && (StylesheetID > 0))
                {
                    string output = null;

                    // Try to retrieve parsed CSS from cache
                    using (var cs = new CachedSection<string>(ref output, 10, true, null, "cssprocessedoutput", StylesheetID))
                    {
                        if (cs.LoadData)
                        {
                            string error = CssStylesheetInfoProvider.TryParseCss(base.GetValue("StylesheetDynamicCode") as string, StylesheetDynamicLanguage, out output);
                            cs.Data = String.IsNullOrEmpty(error) ? output : error;
                            cs.CacheDependency = CacheHelper.GetCacheDependency("cms.cssstylesheet|byname|" + StylesheetName.ToLowerCSafe());
                        }
                    }

                    return output;
                }
            }

            return base.GetValue(columnName);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty CssStyleSheet structure.
        /// </summary>
        public CssStylesheetInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates CssStylesheetInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the class info data</param>
        public CssStylesheetInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Indicates whether CSS stylesheet is plain text.
        /// </summary>
        public bool IsPlainCss()
        {
            if (!String.IsNullOrEmpty(StylesheetDynamicLanguage))
            {
                if (ValidationHelper.GetString(StylesheetDynamicLanguage, PLAIN_CSS) != PLAIN_CSS)
                {
                    CssPreprocessor p = CssStylesheetInfoProvider.GetCssPreprocessor(StylesheetDynamicLanguage);
                    return (p == null);
                }
            }

            return true;
        }


        /// <summary>
        /// Gets the theme path for the object
        /// </summary>
        public string GetThemePath()
        {
            return "~/App_Themes/" + ValidationHelper.GetSafeFileName(StylesheetName);
        }


        /// <summary>
        /// Check whether CssStylesheet is in site given by id.
        /// </summary>
        /// <param name="siteId">ID of site to check</param>
        /// <returns>Returns true if CssStyleSheet is in site</returns>
        public Boolean IsInSite(int siteId)
        {
            DataSet result = ConnectionHelper.ExecuteQuery("cms.cssstylesheet.isinsite", null, "(SiteID = " + siteId + " AND StylesheetID = " + StylesheetID + ")");

            return ((result.Tables.Count > 0) && (result.Tables[0].Rows.Count > 0));
        }


        /// <summary>
        /// Converts PermissionEnum to permission codename which will be checked when CheckPermission() is called. 
        /// </summary>
        /// <param name="permission">Permission to convert to string</param>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Destroy:
                    return PERMISSION_DESTROY_CSS_STYLESHEETS;
                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                    return PERMISSION_MODIFY_CSS_STYLESHEETS;
                case PermissionsEnum.Read:
                    return PERMISSION_READ_CSS_STYLESHEETS;
            }
            return base.GetPermissionName(permission);
        }


        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            switch (permission)
            {
                case PermissionsEnum.Destroy:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure) ||
                           userInfo.IsAuthorizedPerResource(ModuleName.DESIGN, PERMISSION_DESTROY_CSS_STYLESHEETS, siteName, exceptionOnFailure);
                case PermissionsEnum.Delete:
                    return userInfo.IsAuthorizedPerResource(ModuleName.DESIGN, PERMISSION_MODIFY_CSS_STYLESHEETS, siteName, exceptionOnFailure);
                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure) ||
                           userInfo.IsAuthorizedPerResource(ModuleName.DESIGN, PERMISSION_MODIFY_CSS_STYLESHEETS, siteName, exceptionOnFailure);
                case PermissionsEnum.Read:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure) ||
                           userInfo.IsAuthorizedPerResource(ModuleName.DESIGN, PERMISSION_READ_CSS_STYLESHEETS, siteName, exceptionOnFailure);
                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            bool copyFiles = false;

            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                copyFiles = ValidationHelper.GetBoolean(p["cms.cssstylesheet" + ".appthemes"], false);
            }

            if (copyFiles)
            {
                // Copy files from App_Themes
                string sourcePath = "~/App_Themes/" + originalObject.Generalized.ObjectCodeName;
                string targetPath = "~/App_Themes/" + ObjectCodeName;

                FileHelper.CopyDirectory(sourcePath, targetPath);
            }

            Insert();
        }


        /// <summary>
        /// The method retrieves the file extension of the stylesheet.
        /// </summary>
        /// <returns>Returns the file extension as string</returns>
        private string GetExternalColumnExtension()
        {
            string ext = CSS_EXTENSION;

            if (!IsPlainCss())
            {
                CssPreprocessor cssPreprocessor = CssStylesheetInfoProvider.GetCssPreprocessor(StylesheetDynamicLanguage);

                if ((cssPreprocessor != null) && (!String.IsNullOrEmpty(cssPreprocessor.Extension)))
                {
                    ext = cssPreprocessor.Extension;
                }
            }

            return ext;
        }

        #endregion
    }
}