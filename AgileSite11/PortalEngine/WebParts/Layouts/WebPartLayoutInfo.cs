using System;
using System.Data;
using System.Text.RegularExpressions;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(WebPartLayoutInfo), WebPartLayoutInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// WebPartLayoutInfo data container class.
    /// </summary>
    public class WebPartLayoutInfo : AbstractInfo<WebPartLayoutInfo>, IThemeInfo
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.webpartlayout";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WebPartLayoutInfoProvider), OBJECT_TYPE, "CMS.WebPartLayout", "WebPartLayoutID", "WebPartLayoutLastModified", "WebPartLayoutGUID", "WebPartLayoutCodeName", "WebPartLayoutDisplayName", null, null, "WebPartLayoutWebPartID", WebPartInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.DESIGN,
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.Incremental, LogExport = false },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,
            SupportsLocking = true,
            HasExternalColumns = true,
            VersionGUIDColumn = "WebPartLayoutVersionGUID",
            CodeColumn = EXTERNAL_COLUMN_CODE,
            CSSColumn = EXTERNAL_COLUMN_CSS,
            DefaultData = new DefaultDataSettings
            {
                ExcludedPrefixes = { "custom" }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Constants"

        /// <summary>
        ///  External column name for WebPart Layout Code
        /// </summary>
        public const string EXTERNAL_COLUMN_CODE = "WebPartLayoutCode";

        /// <summary>
        ///  External column name for WebPart Layout CSS
        /// </summary>
        public const string EXTERNAL_COLUMN_CSS = "WebPartLayoutCSS";

        #endregion


        #region "Variables"

        /// <summary>
        /// Web part layout full name.
        /// </summary>
        protected string mWebPartLayoutFullName;

        /// <summary>
        /// Regular expressions for external layout code storage processing.
        /// </summary>
        private static Regex mCodeFileRegex;
        private static Regex mCodeBehindRegex;
        private static Regex mInheritsRegex;
        private static Regex mInheritsRevertRegex;

        #endregion


        #region "Properties"

        /// <summary>
        /// Regular expression for external layout code storage SET processing.
        /// </summary>
        private static Regex InheritsRegex
        {
            get
            {
                if (mInheritsRegex == null)
                {
                    mInheritsRegex = RegexHelper.GetRegex("Inherits=\"([^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
                return mInheritsRegex;
            }
        }


        /// <summary>
        /// Regular expression for external layout code storage GET processing.
        /// </summary>
        private static Regex InheritsRevertRegex
        {
            get
            {
                if (mInheritsRevertRegex == null)
                {
                    mInheritsRevertRegex = RegexHelper.GetRegex("Inherits=\"([^\"]*)_CMSWebDeploy_[^\"]+\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
                return mInheritsRevertRegex;
            }
        }


        /// <summary>
        /// Regular expression for external layout code storage processing.
        /// </summary>
        private static Regex CodeFileRegex
        {
            get
            {
                if (mCodeFileRegex == null)
                {
                    mCodeFileRegex = RegexHelper.GetRegex("CodeFile=\"([^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
                return mCodeFileRegex;
            }
        }


        /// <summary>
        /// Regular expression for external layout code storage processing.
        /// </summary>
        private static Regex CodeBehindRegex
        {
            get
            {
                if (mCodeBehindRegex == null)
                {
                    mCodeBehindRegex = RegexHelper.GetRegex("Codebehind=\"([^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
                return mCodeBehindRegex;
            }
        }


        /// <summary>
        /// Layout version GUID.
        /// </summary>
        [DatabaseField]
        public virtual string WebPartLayoutVersionGUID
        {
            get
            {
                return GetStringValue("WebPartLayoutVersionGUID", "");
            }
            set
            {
                SetValue("WebPartLayoutVersionGUID", value);
            }
        }


        /// <summary>
        /// Layout description.
        /// </summary>
        [DatabaseField]
        public virtual string WebPartLayoutDescription
        {
            get
            {
                return GetStringValue("WebPartLayoutDescription", "");
            }
            set
            {
                SetValue("WebPartLayoutDescription", value);
            }
        }


        /// <summary>
        /// Layout display name.
        /// </summary>
        [DatabaseField]
        public virtual string WebPartLayoutDisplayName
        {
            get
            {
                return GetStringValue("WebPartLayoutDisplayName", "");
            }
            set
            {
                SetValue("WebPartLayoutDisplayName", value);
            }
        }


        /// <summary>
        /// Layout code name.
        /// </summary>
        [DatabaseField]
        public virtual string WebPartLayoutCodeName
        {
            get
            {
                return GetStringValue("WebPartLayoutCodeName", "");
            }
            set
            {
                SetValue("WebPartLayoutCodeName", value);
                mWebPartLayoutFullName = null;
            }
        }


        /// <summary>
        /// Layout CSS.
        /// </summary>
        [DatabaseField]
        public virtual string WebPartLayoutCSS
        {
            get
            {
                return GetStringValue("WebPartLayoutCSS", "");
            }
            set
            {
                SetValue("WebPartLayoutCSS", value);
            }
        }


        /// <summary>
        /// Layout ID.
        /// </summary>
        [DatabaseField]
        public virtual int WebPartLayoutID
        {
            get
            {
                return GetIntegerValue("WebPartLayoutID", 0);
            }
            set
            {
                SetValue("WebPartLayoutID", value);
            }
        }


        /// <summary>
        /// Layout code.
        /// </summary>
        [DatabaseField]
        public virtual string WebPartLayoutCode
        {
            get
            {
                return GetStringValue("WebPartLayoutCode", "");
            }
            set
            {
                SetValue("WebPartLayoutCode", value);
            }
        }


        /// <summary>
        /// Layout associated to webpart.
        /// </summary>
        [DatabaseField]
        public virtual int WebPartLayoutWebPartID
        {
            get
            {
                return GetIntegerValue("WebPartLayoutWebPartID", 0);
            }
            set
            {
                SetValue("WebPartLayoutWebPartID", value);
                mWebPartLayoutFullName = null;
            }
        }


        /// <summary>
        /// Indicates whether this layout is used as a default layout for the parent web part.
        /// </summary>
        [DatabaseField]
        public virtual bool WebPartLayoutIsDefault
        {
            get
            {
                return GetBooleanValue("WebPartLayoutIsDefault", false);
            }
            set
            {
                SetValue("WebPartLayoutIsDefault", value);
            }
        }


        /// <summary>
        /// WebPart layout GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid WebPartLayoutGUID
        {
            get
            {
                return GetGuidValue("WebPartLayoutGUID", Guid.Empty);
            }
            set
            {
                SetValue("WebPartLayoutGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime WebPartLayoutLastModified
        {
            get
            {
                return GetDateTimeValue("WebPartLayoutLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("WebPartLayoutLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Web part layout full name.
        /// </summary>
        public string WebPartLayoutFullName
        {
            get
            {
                if (mWebPartLayoutFullName == null)
                {
                    // Get the parent web part
                    var wpi = WebPartInfoProvider.GetWebPartInfo(WebPartLayoutWebPartID);
                    if (wpi == null)
                    {
                        return "";
                    }

                    mWebPartLayoutFullName = ObjectHelper.BuildFullName(wpi.WebPartName, WebPartLayoutCodeName, "|");
                }

                return mWebPartLayoutFullName;
            }
            set
            {
                mWebPartLayoutFullName = value;
            }
        }


        /// <summary>
        /// Object full name if defined
        /// </summary>
        protected override string ObjectFullName
        {
            get
            {
                return WebPartLayoutFullName;
            }
        }


        /// <summary>
        /// Indicates whether the theme path points at an external storage.
        /// </summary>
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
            WebPartLayoutInfoProvider.DeleteWebPartLayoutInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WebPartLayoutInfoProvider.SetWebPartLayoutInfo(this);
        }


        /// <summary>
        /// Returns virtual relative path for specific column
        /// </summary>
        /// <param name="externalColumnName">External column name</param>
        /// <param name="versionGuid">Version GUID. If not defined physical path is generated</param>
        protected override string GetVirtualFileRelativePath(string externalColumnName, string versionGuid)
        {
            string extension = ".ascx";
            if (EXTERNAL_COLUMN_CSS.EqualsCSafe(externalColumnName, true))
            {
                extension = ".css";
            }

            // Keep original version GUID
            string originalVersionGuid = versionGuid;

            // Do not use version GUID for files stored externally
            if (WebPartLayoutInfoProvider.StoreWebPartLayoutsInExternalStorage || SettingsKeyInfoProvider.DeploymentMode)
            {
                versionGuid = null;
            }

            string path = String.Empty;

            // Get parent
            WebPartInfo wpi = WebPartInfoProvider.GetWebPartInfo(WebPartLayoutWebPartID);
            if (wpi != null)
            {
                // If file should be in FS but wasn't found, use DB version
                path = VirtualPathHelper.GetVirtualFileRelativePath(WebPartLayoutCodeName, extension, WebPartLayoutInfoProvider.WebPartLayoutsDirectory, wpi.WebPartName, versionGuid);
                if (!SettingsKeyInfoProvider.DeploymentMode && WebPartLayoutInfoProvider.StoreWebPartLayoutsInExternalStorage && !FileHelper.FileExists(path))
                {
                    path = VirtualPathHelper.GetVirtualFileRelativePath(WebPartLayoutCodeName, extension, WebPartLayoutInfoProvider.WebPartLayoutsDirectory, wpi.WebPartName, originalVersionGuid);
                }
            }

            return path;
        }


        /// <summary>
        /// Returns path to externally stored web part layout codes.
        /// </summary>
        protected override void RegisterExternalColumns()
        {
            base.RegisterExternalColumns();

            // Web part layout code
            ExternalColumnSettings<WebPartLayoutInfo> settings = new ExternalColumnSettings<WebPartLayoutInfo>
            {
                StoragePath = m => m.GetVirtualFileRelativePath(EXTERNAL_COLUMN_CODE, null),
                StoreInExternalStorageSettingsKey = "CMSStoreWebPartLayoutsInFS",
                SetDataTransformation = ProcessLayoutSet,
                GetDataTransformation = ProcessLayoutGet
            };

            // CSS Component
            ExternalColumnSettings<WebPartLayoutInfo> cssSettings = new ExternalColumnSettings<WebPartLayoutInfo>
            {
                StoragePath = m => m.GetVirtualFileRelativePath(EXTERNAL_COLUMN_CSS, null),
                StoreInExternalStorageSettingsKey = "CMSStoreWebPartLayoutsInFS"
            };

            RegisterExternalColumn(EXTERNAL_COLUMN_CODE, settings);
            RegisterExternalColumn(EXTERNAL_COLUMN_CSS, cssSettings);

        }


        /// <summary>
        /// Processes the layout code before it's stored in FS.
        /// </summary>
        /// <param name="info">WebPartLayout object</param>
        /// <param name="data">Data to process</param>
        /// <param name="readOnly">If true, transformation is called only in read-only mode - the codebehind should not be processed</param>
        private static object ProcessLayoutSet(WebPartLayoutInfo info, object data, bool readOnly)
        {
            if (SettingsKeyInfoProvider.DeploymentMode)
            {
                // Get layout filename
                string filename = info.GetVirtualFileRelativePath(EXTERNAL_COLUMN_CODE, null);

                // Get path to the code file
                string codeFilePath = URLHelper.GetVirtualPath(filename) + ".cs";

                // Layout code to transform
                string webPartLayoutCode = ValidationHelper.GetString(data, "");

                if (string.IsNullOrEmpty(webPartLayoutCode))
                {
                    // Do not process the layout when the layout code is empty
                    return data;
                }

                // Get path to the original code behind file
                string originalCodeFilePath = String.Empty;
                if (CodeFileRegex.IsMatch(webPartLayoutCode, 0))
                {
                    // CodeFile path
                    originalCodeFilePath = CodeFileRegex.Match(webPartLayoutCode).Result("$1");
                    webPartLayoutCode = CodeFileRegex.Replace(webPartLayoutCode, "CodeFile=\"" + codeFilePath + "\"");
                }
                else if (CodeBehindRegex.IsMatch(webPartLayoutCode, 0))
                {
                    // Codebehind path
                    originalCodeFilePath = CodeBehindRegex.Match(webPartLayoutCode).Result("$1");
                    webPartLayoutCode = CodeBehindRegex.Replace(webPartLayoutCode, "CodeFile=\"" + codeFilePath + "\"");
                }

                // Get original class name
                string originalClassName = String.Empty;
                string codePostfix = Convert.ToString(info.WebPartLayoutID);
                // Use GUID value for new items
                if (info.WebPartLayoutID <= 0)
                {
                    codePostfix = Guid.NewGuid().ToString("N");
                }

                if (InheritsRegex.IsMatch(webPartLayoutCode))
                {
                    originalClassName = InheritsRegex.Match(webPartLayoutCode).Result("$1");
                    webPartLayoutCode = InheritsRegex.Replace(webPartLayoutCode, "Inherits=\"$1_CMSWebDeploy_" + codePostfix + "\"");
                }

                // Read original codefile and change classname and save the webpart
                if (!readOnly)
                {
                    if (!FileHelper.FileExists(originalCodeFilePath))
                    {
                        throw new WebPartLayoutException("Original code behind file ('" + originalCodeFilePath + "') for web part " + info.Generalized.ObjectCodeName + " does not exist.");
                    }

                    if (!String.IsNullOrEmpty(originalCodeFilePath))
                    {
                        string codeFileCode = File.ReadAllText(URLHelper.GetPhysicalPath(originalCodeFilePath));
                        codeFileCode = codeFileCode.Replace(originalClassName, originalClassName + "_CMSWebDeploy_" + codePostfix);
                        codeFileCode = TextHelper.EnsureLineEndings(codeFileCode, "\r\n");

                        string filePath = URLHelper.GetPhysicalPath(codeFilePath);
                        string dirPath = Path.GetDirectoryName(filePath);

                        if (!Directory.Exists(dirPath))
                        {
                            Directory.CreateDirectory(dirPath);
                        }

                        File.WriteAllText(filePath, codeFileCode);
                    }
                }

                return webPartLayoutCode;
            }
            else
            {
                // No transformation needed when not in deployment mode.
                return data;
            }
        }


        /// <summary>
        /// Processes the layout code before it's stored in FS.
        /// </summary>
        /// <param name="info">WebPartLayout object</param>
        /// <param name="data">Data to process</param>
        private static object ProcessLayoutGet(WebPartLayoutInfo info, object data)
        {
            // Transform only in deployment mode
            if (SettingsKeyInfoProvider.DeploymentMode)
            {
                // Layout code to transform
                string webPartLayoutCode = ValidationHelper.GetString(data, "");

                // Get path to the original code behind file
                string originalCodeFilePath = ((WebPartInfo)info.Parent).WebPartFileName + ".cs";

                if (CodeFileRegex.IsMatch(webPartLayoutCode, 0))
                {
                    // Replace CodeFile path
                    webPartLayoutCode = CodeFileRegex.Replace(webPartLayoutCode, "CodeFile=\"~/CMSWebParts/" + originalCodeFilePath.TrimStart('/') + "\"");
                }
                else
                {
                    // Replace Codebehind path
                    webPartLayoutCode = CodeBehindRegex.Replace(webPartLayoutCode, "Codebehind=\"~/CMSWebParts/" + originalCodeFilePath.TrimStart('/') + "\"");
                }
                webPartLayoutCode = InheritsRevertRegex.Replace(webPartLayoutCode, "Inherits=\"$1\"");

                return webPartLayoutCode;
            }

            // No transformation if it's not deployment mode
            return data;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WebPartLayoutInfo object.
        /// </summary>
        public WebPartLayoutInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WebPartLayoutInfo object from the given DataRow.
        /// </summary>
        public WebPartLayoutInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
            // Get class name
            if ((dr != null) && dr.Table.Columns.Contains("WebPartName"))
            {
                string webPartName = Convert.ToString(dr["WebPartName"]);
                mWebPartLayoutFullName = ObjectHelper.BuildFullName(webPartName, WebPartLayoutCodeName, "|");
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the theme path for the object
        /// </summary>
        public string GetThemePath()
        {
            WebPartInfo wpi = Parent as WebPartInfo;

            return "~/App_Themes/Components/WebParts/" + ValidationHelper.GetSafeFileName(wpi.WebPartName) + "/Layouts/" + ValidationHelper.GetSafeFileName(WebPartLayoutCodeName);
        }


        /// <summary>
        /// Converts PermissionEnum to permission codename which will be checked when CheckPermission() is called. 
        /// </summary>
        /// <param name="permission">Permission to convert to string</param>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Modify:
                case PermissionsEnum.Read:
                    return "design";
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
                           UserInfoProvider.IsAuthorizedPerResource(ModuleName.DESIGN, "Destroy" + TypeInfo.ObjectType.Replace(".", ""), siteName, (UserInfo)userInfo, exceptionOnFailure);

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
            Insert();

            if ((settings.CloneBase == null) || (settings.CloneBase.TypeInfo.ObjectType == TypeInfo.ObjectType))
            {
                bool copyFiles = false;

                var p = settings.CustomParameters;
                if (p != null)
                {
                    copyFiles = ValidationHelper.GetBoolean(p["cms.webpartlayout" + ".appthemes"], false);
                }

                if (copyFiles)
                {
                    var webPart = WebPartInfoProvider.GetWebPartInfo(WebPartLayoutWebPartID);
                    if (webPart != null)
                    {
                        // Copy files from App_Themes
                        string sourcePath = "~/App_Themes/Components/WebParts/" + webPart.WebPartName + "/Layouts/" + originalObject.Generalized.ObjectCodeName;
                        string targetPath = "~/App_Themes/Components/WebParts/" + webPart.WebPartName + "/Layouts/" + ObjectCodeName;

                        FileHelper.CopyDirectory(sourcePath, targetPath);
                    }
                }
            }
        }

        #endregion
    }
}
