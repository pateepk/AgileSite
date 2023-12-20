using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.FormEngine;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(DocumentTypeInfo), DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Specialized class for the document type info.
    /// </summary>
    public class DocumentTypeInfo : DataClassInfo
    {
        #region "Type information properties"

        /// <summary>
        /// Object type for document type
        /// </summary>
        public const string OBJECT_TYPE_DOCUMENTTYPE = PredefinedObjectType.DOCUMENTTYPE;


        /// <summary>
        /// Type information for document type.
        /// </summary>
        public static ObjectTypeInfo TYPEINFODOCUMENTTYPE = new ObjectTypeInfo(typeof(DataClassInfoProvider), OBJECT_TYPE_DOCUMENTTYPE, "CMS.Class", "ClassID", "ClassLastModified", "ClassGUID", "ClassName", "ClassDisplayName", null, null, null, null)
        {
            OriginalTypeInfo = TYPEINFO,
            MacroCollectionName = "CMS.DocumentType",
            DependsOn = new List<ObjectDependency>
            { 
                new ObjectDependency("ClassInheritsFromClassID", OBJECT_TYPE_DOCUMENTTYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("ClassResourceID", PredefinedObjectType.RESOURCE)
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                }
            },

            CheckDependenciesOnDelete = true,
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
            },
            HasExternalColumns = true,
            DeleteObjectWithAPI = true,
            VersionGUIDColumn = "ClassVersionGUID",
            FormDefinitionColumn = "ClassFormDefinition",
            ResourceIDColumn = "ClassResourceID",
            CodeColumn = EXTERNAL_COLUMN_CODE,
            TypeCondition = new TypeCondition().WhereEquals("ClassIsDocumentType", true),
            HasMetaFiles = true,
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField<FormInfo>("ClassFormDefinition"),
                    new StructuredField<SearchSettings>("ClassSearchSettings"),
                    new StructuredField("ClassContactMapping"),
                    new StructuredField<ClassCodeGenerationSettings>("ClassCodeGenerationSettings")
                },
                ExcludedFieldNames = { "ClassXmlSchema" }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Variables"

        private static RegisteredProperties<DataClassInfo> mLocalRegisteredProperties;

        #endregion


        #region "Properties"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                return TYPEINFODOCUMENTTYPE;
            }
        }


        /// <summary>
        /// Local registered properties
        /// </summary>
        protected override RegisteredProperties<DataClassInfo> RegisteredProperties
        {
            get
            {
                return mLocalRegisteredProperties ?? (mLocalRegisteredProperties = new RegisteredProperties<DataClassInfo>(RegisterProperties));
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentTypeInfo()
            : base(true)
        {
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            string tableName = String.Empty;

            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                tableName = ValidationHelper.GetString(p["cms.class" + ".tablename"], "");
            }

            bool isContainer = String.IsNullOrEmpty(tableName);

            if (!isContainer)
            {
                // Update primary key name
                FormInfo fi = new FormInfo(ClassFormDefinition);
                FormFieldInfo ffi = fi.GetFields<FormFieldInfo>().FirstOrDefault(t => t.PrimaryKey);
                if (ffi != null)
                {
                    string oldPrimaryKey = ffi.Name;
                    string codeName = settings.CodeName.Substring(settings.CodeName.IndexOf('.') + 1);
                    ffi.Name = TextHelper.FirstLetterToUpper(codeName + "ID");

                    var searchSettings = ClassSearchSettingsInfos;
                    searchSettings.RenameColumn(oldPrimaryKey, ffi.Name);
                    ClassSearchSettings = searchSettings.GetData();

                    ClassFormDefinition = fi.GetXmlDefinition();
                }
            }

            base.InsertAsCloneInternal(settings, result, originalObject);
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
            string permissionName = permission.ToString();
            if (String.IsNullOrEmpty(permissionName))
            {
                return CheckPermissions(permission, siteName, userInfo, exceptionOnFailure);
            }

            var result = userInfo.IsAuthorizedPerResource(ModuleName.CONTENT, permissionName, siteName, false) || (!String.IsNullOrEmpty(ClassName) && userInfo.IsAuthorizedPerClassName(ClassName, permissionName, siteName, false));

            if (exceptionOnFailure && !result)
            {
                throw new PermissionCheckException(ModuleName.CONTENT, permissionName, siteName);
            }

            return result;
        }

        #endregion
    }
}
