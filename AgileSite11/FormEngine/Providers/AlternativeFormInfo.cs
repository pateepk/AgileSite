using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Modules;

[assembly: RegisterObjectType(typeof(AlternativeFormInfo), AlternativeFormInfo.OBJECT_TYPE)]

namespace CMS.FormEngine
{
    /// <summary>
    /// AlternativeFormInfo data container class.
    /// </summary>
    public class AlternativeFormInfo : AbstractInfo<AlternativeFormInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.ALTERNATIVEFORM;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AlternativeFormInfoProvider), OBJECT_TYPE, "CMS.AlternativeForm", "FormID", "FormLastModified", "FormGUID", "FormName", "FormDisplayName", null, null, "FormClassID", DataClassInfo.OBJECT_TYPE)
        {
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("FormCoupledClassID", DataClassInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired)
            },
            ModuleName = "cms.form",
            SupportsLocking = true,
            HasExternalColumns = true,
            VersionGUIDColumn = "FormVersionGUID",
            CodeColumn = EXTERNAL_COLUMN_CODE,
            RegisterAsChildToObjectTypes = new List<string> { DataClassInfo.OBJECT_TYPE, DataClassInfo.OBJECT_TYPE_SYSTEMTABLE, PredefinedObjectType.DOCUMENTTYPE, PredefinedObjectType.CUSTOMTABLECLASS, PredefinedObjectType.FORMCLASS },
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.Incremental,
            },
            CustomizedColumnsColumn = "FormCustomizedColumns",
            IsCustomColumn = "FormIsCustom",
            FormDefinitionColumn = "FormDefinition",
            DefaultData = new DefaultDataSettings
            {
                ExcludedColumns = new List<string> { "FormCustomizedColumns", "FormIsCustom" }
            },
            SerializationSettings = 
            {
                StructuredFields = new List<IStructuredField>
                {
                    // FormInfo processing would modify the incomplete alternative form definition difference
                    new StructuredField("FormDefinition")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Variables"

        /// <summary>
        ///  External column name for Form Layout
        /// </summary>
        public const string EXTERNAL_COLUMN_CODE = "FormLayout";

        // Form full name
        private string mFullName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Form version GUID.
        /// </summary>
        [DatabaseField]
        public string FormVersionGUID
        {
            get
            {
                return GetStringValue("FormVersionGUID", String.Empty);
            }
            set
            {
                SetValue("FormVersionGUID", value);
            }
        }


        /// <summary>
        /// Form display name.
        /// </summary>
        [DatabaseField]
        public virtual string FormDisplayName
        {
            get
            {
                return GetStringValue("FormDisplayName", String.Empty);
            }
            set
            {
                SetValue("FormDisplayName", value);
            }
        }


        /// <summary>
        /// Form definition.
        /// </summary>
        [DatabaseField]
        public virtual string FormDefinition
        {
            get
            {
                return GetStringValue("FormDefinition", String.Empty);
            }
            set
            {
                SetValue("FormDefinition", value);
            }
        }


        /// <summary>
        /// Form code name.
        /// </summary>
        [DatabaseField]
        public virtual string FormName
        {
            get
            {
                return GetStringValue("FormName", String.Empty);
            }
            set
            {
                SetValue("FormName", value);

                mFullName = null;
            }
        }


        /// <summary>
        /// ID of the form.
        /// </summary>
        [DatabaseField]
        public virtual int FormID
        {
            get
            {
                return GetIntegerValue("FormID", 0);
            }
            set
            {
                SetValue("FormID", value);
            }
        }


        /// <summary>
        /// Form layout.
        /// </summary>
        [DatabaseField]
        public virtual string FormLayout
        {
            get
            {
                return GetStringValue("FormLayout", null);
            }
            set
            {
                SetValue("FormLayout", value);
            }
        }


        /// <summary>
        /// Form layout type.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public LayoutTypeEnum FormLayoutType
        {
            get
            {
                return LayoutHelper.GetLayoutTypeEnum(GetStringValue("FormLayoutType", String.Empty));
            }
            set
            {
                SetValue("FormLayoutType", LayoutHelper.GetLayoutTypeCode(value));
            }
        }


        /// <summary>
        /// Form class ID.
        /// </summary>
        [DatabaseField]
        public virtual int FormClassID
        {
            get
            {
                return GetIntegerValue("FormClassID", 0);
            }
            set
            {
                SetValue("FormClassID", value);

                mFullName = null;
            }
        }


        /// <summary>
        /// Form last modified DateTime.
        /// </summary>
        [DatabaseField]
        public virtual DateTime FormLastModified
        {
            get
            {
                return GetDateTimeValue("FormLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("FormLastModified", value);
            }
        }


        /// <summary>
        /// Form GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid FormGUID
        {
            get
            {
                return GetGuidValue("FormGUID", Guid.Empty);
            }
            set
            {
                SetValue("FormGUID", value);
            }
        }


        /// <summary>
        /// Alternative form full name ("classname"."formname").
        /// </summary>
        public virtual string FullName
        {
            get
            {
                if (String.IsNullOrEmpty(mFullName))
                {
                    var dci = DataClassInfoProvider.GetDataClassInfo(FormClassID);

                    mFullName = (dci != null) ? ObjectHelper.BuildFullName(dci.ClassName, FormName) : FormName;
                }

                return mFullName;
            }
        }


        /// <summary>
        /// Object full name if defined
        /// </summary>
        protected override string ObjectFullName
        {
            get
            {
                return FullName;
            }
        }


        /// <summary>
        /// Form hides new parent's fields.
        /// </summary>
        [DatabaseField]
        public virtual bool FormHideNewParentFields
        {
            get
            {
                return GetBooleanValue("FormHideNewParentFields", false);
            }
            set
            {
                SetValue("FormHideNewParentFields", value);
            }
        }


        /// <summary>
        /// Form coupled class ID.
        /// </summary>
        [DatabaseField]
        public virtual int FormCoupledClassID
        {
            get
            {
                return GetIntegerValue("FormCoupledClassID", 0);
            }
            set
            {
                SetValue("FormCoupledClassID", value, value > 0);
            }
        }


        /// <summary>
        /// Form 'Is custom' flag.
        /// </summary>
        [DatabaseField]
        public virtual bool FormIsCustom
        {
            get
            {
                return GetBooleanValue("FormIsCustom", false);
            }
            set
            {
                SetValue("FormIsCustom", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            AlternativeFormInfoProvider.DeleteAlternativeFormInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            AlternativeFormInfoProvider.SetAlternativeFormInfo(this);
        }


        /// <summary>
        /// Returns virtual relative path for specific column
        /// </summary>
        /// <param name="externalColumnName">External column name</param>
        /// <param name="versionGuid">Version GUID. If not defined physical path is generated</param>
        protected override string GetVirtualFileRelativePath(string externalColumnName, string versionGuid)
        {
            // Ensure extension
            string extension = (FormLayoutType == LayoutTypeEnum.Html) ? ".html" : ".ascx";

            // Keep original version GUID
            string originalVersionGuid = versionGuid;
            bool storedExternally = (AlternativeFormInfoProvider.StoreAlternativeFormsInExternalStorage || SettingsKeyInfoProvider.DeploymentMode);

            // Do not use version GUID for files stored externally
            if (storedExternally)
            {
                versionGuid = null;
            }

            string directory = AlternativeFormInfoProvider.FormLayoutsDirectory;
            string path = String.Empty;

            // Get parent
            DataClassInfo classInfo = DataClassInfoProvider.GetDataClassInfo(FormClassID);
            if (classInfo != null)
            {
                // If file should be in FS but wasn't found, use DB version
                path = VirtualPathHelper.GetVirtualFileRelativePath(FormName, extension, directory, classInfo.ClassName, versionGuid);
                if (!SettingsKeyInfoProvider.DeploymentMode && AlternativeFormInfoProvider.StoreAlternativeFormsInExternalStorage && !FileHelper.FileExists(path))
                {
                    path = VirtualPathHelper.GetVirtualFileRelativePath(FormName, extension, directory, classInfo.ClassName, originalVersionGuid);
                }
            }
            return path;
        }


        /// <summary>
        /// Returns path to externally stored layout codes.
        /// </summary>
        protected override void RegisterExternalColumns()
        {
            base.RegisterExternalColumns();

            // Code
            ExternalColumnSettings<AlternativeFormInfo> settings = new ExternalColumnSettings<AlternativeFormInfo>
            {
                StoragePath = m => m.GetVirtualFileRelativePath(EXTERNAL_COLUMN_CODE, null),
                StoreInExternalStorageSettingsKey = "CMSStoreAltFormLayoutsInFS",
                SetDataTransformation = (m, data, readOnly) => LayoutHelper.AddLayoutDirectives(ValidationHelper.GetString(data, String.Empty), m.FormLayoutType),
                GetDataTransformation = (m, data) => VirtualPathHelper.RemoveDirectives(ValidationHelper.GetString(data, String.Empty), LayoutHelper.DefaultDirectives)
            };

            RegisterExternalColumn(EXTERNAL_COLUMN_CODE, settings);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty AlternativeFormInfo object.
        /// </summary>
        public AlternativeFormInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AlternativeFormInfo object from the given DataRow.
        /// </summary>
        public AlternativeFormInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Converts permissions enum to permission code name when CheckPermission() is called.
        /// </summary>
        /// <param name="permission">Permissions enum</param>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return "ReadForm";

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return "EditForm";

                default:
                    return base.GetPermissionName(permission);
            }
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
                    return userInfo.IsAuthorizedPerResource("cms.globalpermissions", "DestroyObjects", siteName, false) ||
                           userInfo.IsAuthorizedPerResource("cms.form", "EditForm", siteName, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }


        /// <summary>
        /// Adds given field to form definition with visible set to false.
        /// </summary>
        public void HideField(FormFieldInfo ffiUpdated)
        {
            // Fake visibility for that field
            bool tempVisible = ffiUpdated.Visible;
            ffiUpdated.Visible = false;

            // Add it to xml definition of alternative form
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.LoadXml(FormDefinition);
            }
            catch (XmlException)
            {
                // Malformed FormDefinition, use empty one.
                xml = FormInfo.GetEmptyFormDocument();
            }

            // Remove the node if already exists
            foreach (XmlNode node in xml.SelectNodes(ffiUpdated.GetXPathExpression()))
            {
                node.ParentNode.RemoveChild(node);
            }

            xml.DocumentElement.AppendChild(ffiUpdated.GetHiddenNode(xml));
            FormDefinition = xml.OuterXml;

            ffiUpdated.Visible = tempVisible;
        }


        /// <summary>
        /// Gets the automatic code name for the object.
        /// </summary>
        protected override string GetAutomaticCodeName()
        {
            // Dots are not allowed in alternative form name
            return ValidationHelper.GetIdentifier(base.GetAutomaticCodeName());
        }


        /// <summary>
        /// Returns the default object installation data
        /// </summary>
        /// <param name="excludedNames">Objects with display names and code names starting with these expressions are filtered out.</param>
        protected override DataSet GetDefaultData(IEnumerable<string> excludedNames = null)
        {
            var data = base.GetDefaultData(excludedNames);

            if (!DataHelper.DataSourceIsEmpty(data))
            {
                var customizedColumnsColumn = TypeInfo.CustomizedColumnsColumn;

                // Set FormCustomizedColumns column to its correct default value instead of NULL (i.e. "")
                var dt = data.Tables[0];
                DataHelper.EnsureColumn(dt, customizedColumnsColumn, typeof(string));
                DataHelper.SetColumnValues(dt, customizedColumnsColumn, "");
            }

            return data;
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Mark alternative form as custom if module is not in development and development mode is off
            var classInfo = DataClassInfoProvider.GetDataClassInfo(FormClassID);
            var resource = ResourceInfoProvider.GetResourceInfo(classInfo.ClassResourceID);

            FormIsCustom = !SystemContext.DevelopmentMode && (resource != null) && !resource.ResourceIsInDevelopment;

            base.InsertAsCloneInternal(settings, result, originalObject);
        }

        #endregion
    }
}