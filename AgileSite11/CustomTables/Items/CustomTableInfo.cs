using System;
using System.Collections.Generic;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.CustomTables;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(CustomTableInfo), CustomTableInfo.OBJECT_TYPE_CUSTOMTABLE)]

namespace CMS.CustomTables
{
    /// <summary>
    /// Specialized class for the custom table info
    /// </summary>
    public class CustomTableInfo : DataClassInfo
    {
        #region "Type information properties"

        /// <summary>
        /// Object type for custom table
        /// </summary>
        public const string OBJECT_TYPE_CUSTOMTABLE = PredefinedObjectType.CUSTOMTABLECLASS;


        /// <summary>
        /// Type information for custom tables.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOCUSTOMTABLE = new ObjectTypeInfo(typeof(DataClassInfoProvider), OBJECT_TYPE_CUSTOMTABLE, "CMS.Class", "ClassID", "ClassLastModified", "ClassGUID", "ClassName", "ClassDisplayName", null, null, null, null)
        {
            OriginalTypeInfo = TYPEINFO,
            MacroCollectionName = "CMS.CustomTable",
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                }
            },
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
            CheckDependenciesOnDelete = true,
            HasExternalColumns = true,
            DeleteObjectWithAPI = true,
            VersionGUIDColumn = "ClassVersionGUID",
            FormDefinitionColumn = "ClassFormDefinition",
            CodeColumn = EXTERNAL_COLUMN_CODE,
            TypeCondition = new TypeCondition().WhereEquals("ClassIsCustomTable", true),
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField<DataDefinition>("ClassFormDefinition"),
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
                return TYPEINFOCUSTOMTABLE;
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


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomTableInfo()
            : base(true)
        {
        }


        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("Items", m => ((CustomTableInfo)m).GetCustomTableItems());
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
            if (permission == PermissionsEnum.Destroy)
            {
                // Destroy permission for custom table is same as Delete
                permission = PermissionsEnum.Delete;
            }

            string permissionName = permission.ToString();
            if (String.IsNullOrEmpty(permissionName))
            {
                return CheckPermissions(permission, siteName, userInfo, exceptionOnFailure);
            }

            var result = userInfo.IsAuthorizedPerResource(ModuleName.CUSTOMTABLES, permissionName, siteName, false) || (!String.IsNullOrEmpty(ClassName) && userInfo.IsAuthorizedPerClassName(ClassName, permissionName, siteName, false));

            if (exceptionOnFailure && !result)
            {
                throw new PermissionCheckException(ModuleName.CUSTOMTABLES, permissionName, siteName);
            }

            return result;
        }


        /// <summary>
        /// Returns the custom table items.
        /// In order to limit properties available in macros, provide custom table items via <see cref="InfoObjectCollection"/> type.
        /// </summary>
        private IInfoObjectCollection GetCustomTableItems()
        {
            return new InfoObjectCollection(CustomTableItemProvider.GetObjectType(ClassName));
        }

        #endregion
    }
}
