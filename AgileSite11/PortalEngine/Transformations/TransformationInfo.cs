using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Xml;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(TransformationInfo), TransformationInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// Class to use for storing the transformation data.
    /// </summary>
    public class TransformationInfo : AbstractInfo<TransformationInfo>, IThemeInfo
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.TRANSFORMATION;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TransformationInfoProvider), OBJECT_TYPE, "CMS.Transformation", "TransformationID", "TransformationLastModified", "TransformationGUID", "TransformationName", "TransformationName", null, null, "TransformationClassID", DataClassInfo.OBJECT_TYPE)
        {
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,
            ModuleName = ModuleName.DESIGN,
            SupportsLocking = true,
            HasExternalColumns = true,
            VersionGUIDColumn = "TransformationVersionGUID",
            CodeColumn = EXTERNAL_COLUMN_CODE,
            CSSColumn = EXTERNAL_COLUMN_CSS,
            RegisterAsChildToObjectTypes = new List<string> { DataClassInfo.OBJECT_TYPE, DataClassInfo.OBJECT_TYPE_SYSTEMTABLE, PredefinedObjectType.DOCUMENTTYPE, PredefinedObjectType.CUSTOMTABLECLASS },
            DefaultData = new DefaultDataSettings
            {
                ExcludedColumns = new List<string> { "TransformationPreferredDocument" }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Variables"

        private string mTransformationFullName;
        private XmlDocument mTransformationHierarchicalXMLDocument;

        /// <summary>
        ///  External column name for Transformation Code
        /// </summary>
        public const string EXTERNAL_COLUMN_CODE = "TransformationCode";

        /// <summary>
        ///  External column name for Transformation CSS
        /// </summary>
        public const string EXTERNAL_COLUMN_CSS = "TransformationCSS";

        #endregion


        #region "Properties"

        /// <summary>
        /// Transformation ID.
        /// </summary>
        [DatabaseField]
        public int TransformationID
        {
            get
            {
                return GetIntegerValue("TransformationID", 0);
            }
            set
            {
                SetValue("TransformationID", value);
            }
        }


        /// <summary>
        /// Transformation Name.
        /// </summary>
        [DatabaseField]
        public string TransformationName
        {
            get
            {
                return GetStringValue("TransformationName", string.Empty);
            }
            set
            {
                SetValue("TransformationName", value);
                mTransformationFullName = null;
            }
        }


        /// <summary>
        /// Transformation Full Name.
        /// </summary>
        public string TransformationFullName
        {
            get
            {
                if (mTransformationFullName == null)
                {
                    // Get the class
                    var ci = DataClassInfoProvider.GetDataClassInfo(TransformationClassID);
                    if (ci == null)
                    {
                        return "";
                    }

                    // Build the full name
                    mTransformationFullName = ObjectHelper.BuildFullName(ci.ClassName, TransformationName);
                }

                return mTransformationFullName;
            }
            set
            {
                mTransformationFullName = value;
            }
        }


        /// <summary>
        /// Object full name if defined
        /// </summary>
        protected override string ObjectFullName
        {
            get
            {
                return TransformationFullName;
            }
        }


        /// <summary>
        /// Preferred document for transformation preview. Format aliaspath;siteID;culturecode
        /// </summary>
        [DatabaseField]
        public string TransformationPreferredDocument
        {
            get
            {
                return GetStringValue("TransformationPreferredDocument", string.Empty);
            }
            set
            {
                SetValue("TransformationPreferredDocument", value);
            }
        }


        /// <summary>
        /// Transformation Code.
        /// </summary>
        [DatabaseField]
        public string TransformationCode
        {
            get
            {
                return GetStringValue("TransformationCode", string.Empty);
            }
            set
            {
                SetValue("TransformationCode", value);
            }
        }


        /// <summary>
        /// Transformation CSS.
        /// </summary>
        [DatabaseField]
        public string TransformationCSS
        {
            get
            {
                return GetStringValue("TransformationCSS", string.Empty);
            }
            set
            {
                SetValue("TransformationCSS", value);
            }
        }


        /// <summary>
        /// Transformation type.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public TransformationTypeEnum TransformationType
        {
            get
            {
                var type = GetStringValue("TransformationType", null);
                return type.ToEnum<TransformationTypeEnum>();
            }
            set
            {
                SetValue("TransformationType", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// ID of the class.
        /// </summary>
        [DatabaseField]
        public int TransformationClassID
        {
            get
            {
                return GetIntegerValue("TransformationClassID", 0);
            }
            set
            {
                SetValue("TransformationClassID", value);
                mTransformationFullName = null;
            }
        }


        /// <summary>
        /// Transformation version GUID.
        /// </summary>
        [DatabaseField]
        public string TransformationVersionGUID
        {
            get
            {
                return GetStringValue("TransformationVersionGUID", string.Empty);
            }
            set
            {
                SetValue("TransformationVersionGUID", value);
            }
        }


        /// <summary>
        /// Transformation last modified.
        /// </summary>
        [DatabaseField]
        public DateTime TransformationLastModified
        {
            get
            {
                return GetDateTimeValue("TransformationLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TransformationLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Transformation GUID.
        /// </summary>
        [DatabaseField]
        public Guid TransformationGUID
        {
            get
            {
                return GetGuidValue("TransformationGUID", Guid.Empty);
            }
            set
            {
                SetValue("TransformationGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Indicates whether this transformation is hierarchical.
        /// </summary>
        [DatabaseField]
        public bool TransformationIsHierarchical
        {
            get
            {
                return GetBooleanValue("TransformationIsHierarchical", false);
            }
            set
            {
                SetValue("TransformationIsHierarchical", value);
                mTransformationHierarchicalXMLDocument = null;
            }
        }


        /// <summary>
        /// Definition of all hierarchical transformations.
        /// </summary>
        [DatabaseField]
        public string TransformationHierarchicalXML
        {
            get
            {
                return GetStringValue("TransformationHierarchicalXML", string.Empty);
            }
            set
            {
                SetValue("TransformationHierarchicalXML", value);
                mTransformationHierarchicalXMLDocument = null;
            }
        }


        /// <summary>
        /// Gets the hierarchical transformation XML Document object.
        /// </summary>
        public XmlDocument TransformationHierarchicalXMLDocument
        {
            get
            {
                if (TransformationIsHierarchical)
                {
                    if (!string.IsNullOrEmpty(TransformationHierarchicalXML) && (mTransformationHierarchicalXMLDocument == null))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(TransformationHierarchicalXML);
                        mTransformationHierarchicalXMLDocument = doc;
                    }
                    return mTransformationHierarchicalXMLDocument;
                }
                return null;
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


        /// <summary>
        /// Indicates whether edit delete buttons should be used within the processing of transformation
        /// </summary>
        public EditModeButtonEnum EditDeleteButtonsMode
        {
            get
            {
                return PortalContext.EditDeleteButtonsMode;
            }
            set
            {
                PortalContext.EditDeleteButtonsMode = value;
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Returns transformation preferred document array (alias path,site ID, culturecode)
        /// </summary>
        public string[] GetPreferredPreviewDocument()
        {
            return TransformationPreferredDocument.Split(';');
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            TransformationInfoProvider.DeleteTransformation(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            TransformationInfoProvider.SetTransformation(this);
        }


        /// <summary>
        /// Returns virtual relative path for specific column
        /// </summary>
        /// <param name="externalColumnName">External column name</param>
        /// <param name="versionGuid">Version GUID. If not defined physical path is generated</param>
        protected override string GetVirtualFileRelativePath(string externalColumnName, string versionGuid)
        {
            // Ensure extension
            string extension = "." + TransformationType.ToStringRepresentation();
            if (EXTERNAL_COLUMN_CSS.EqualsCSafe(externalColumnName, true))
            {
                extension = ".css";
            }

            // Keep original version GUID
            string originalVersionGuid = versionGuid;

            bool storedExternally = (TransformationInfoProvider.StoreTransformationsInExternalStorage || SettingsKeyInfoProvider.DeploymentMode);

            // Do not use version GUID for files stored externally
            if (storedExternally)
            {
                versionGuid = null;
            }

            string path = String.Empty;
            string directory = TransformationInfoProvider.TransformationsDirectory;

            DataClassInfo ci = DataClassInfoProvider.GetDataClassInfo(TransformationClassID);
            if (ci != null)
            {
                // Transformation name
                string name = TransformationName;

                if (!storedExternally && VirtualPathHelper.UsingVirtualPathProvider && (EditDeleteButtonsMode != EditModeButtonEnum.None))
                {
                    name += VirtualPathHelper.URLParametersSeparator + "showeditdelete-" + (int)EditDeleteButtonsMode;
                }

                // Prefix: class name
                string prefix = ci.ClassName;

                path = VirtualPathHelper.GetVirtualFileRelativePath(name, extension, directory, prefix, versionGuid);
                if (!SettingsKeyInfoProvider.DeploymentMode && TransformationInfoProvider.StoreTransformationsInExternalStorage && !FileHelper.FileExists(path))
                {
                    path = VirtualPathHelper.GetVirtualFileRelativePath(name, extension, directory, prefix, originalVersionGuid);
                }
            }

            return path;
        }


        /// <summary>
        /// Returns path to externally stored transformation codes.
        /// </summary>
        protected override void RegisterExternalColumns()
        {
            base.RegisterExternalColumns();

            ExternalColumnSettings<TransformationInfo> settings = new ExternalColumnSettings<TransformationInfo>()
            {
                StoragePath = m => m.GetVirtualFileRelativePath(EXTERNAL_COLUMN_CODE, null),
                StoreInExternalStorageSettingsKey = "CMSStoreTransformationsInFS",
                SetDataTransformation = (m, data, readOnly) => TransformationInfoProvider.AddTransformationDirectives(ValidationHelper.GetString(data, ""), true, EditDeleteButtonsMode, m.TransformationType),
                GetDataTransformation = (m, data) => TransformationInfoProvider.RemoveDirectives(ValidationHelper.GetString(data, ""))
            };

            // CSS Component
            ExternalColumnSettings<TransformationInfo> cssSettings = new ExternalColumnSettings<TransformationInfo>()
            {
                StoragePath = m => m.GetVirtualFileRelativePath(EXTERNAL_COLUMN_CSS, null),
                StoreInExternalStorageSettingsKey = "CMSStoreTransformationsInFS"
            };

            RegisterExternalColumn(EXTERNAL_COLUMN_CODE, settings);
            RegisterExternalColumn(EXTERNAL_COLUMN_CSS, cssSettings);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty TransformationInfo structure.
        /// </summary>
        public TransformationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates the TransformationInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Data row with the TransformationInfo info data</param>
        public TransformationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Loads the object default data
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            TransformationCode = String.Empty;
            TransformationType = TransformationTypeEnum.Ascx;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the theme path for the object
        /// </summary>
        public string GetThemePath()
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(TransformationClassID);
            if (dci != null)
            {
                return "~/App_Themes/Components/Transformations/" + ValidationHelper.GetSafeFileName(dci.ClassName) + "/" + ValidationHelper.GetSafeFileName(TransformationName);
            }

            return null;
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
            bool copyFiles = false;

            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                copyFiles = ValidationHelper.GetBoolean(p[PredefinedObjectType.TRANSFORMATION + ".appthemes"], false);
            }

            if (copyFiles)
            {
                DataClassInfo originalParentClass = DataClassInfoProvider.GetDataClassInfo(originalObject.Generalized.ObjectParentID);
                DataClassInfo cloneParentClass = DataClassInfoProvider.GetDataClassInfo(TransformationClassID);

                // Copy files from App_Themes
                if ((originalParentClass != null) && (cloneParentClass != null))
                {
                    string sourcePath = "~/App_Themes/Components/Transformations/" + originalParentClass.ClassName + "/" + originalObject.Generalized.ObjectCodeName;
                    string targetPath = "~/App_Themes/Components/Transformations/" + cloneParentClass.ClassName + "/" + ObjectCodeName;

                    FileHelper.CopyDirectory(sourcePath, targetPath);
                }
            }

            Insert();
        }


        #endregion
    }
}