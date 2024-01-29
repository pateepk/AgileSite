using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Modules;
using CMS.Core;

[assembly: RegisterObjectType(typeof(ResourceInfo), ResourceInfo.OBJECT_TYPE)]

namespace CMS.Modules
{
    /// <summary>
    /// Resource info data container class.
    /// </summary>
    public class ResourceInfo : AbstractInfo<ResourceInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.RESOURCE;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ResourceInfoProvider), OBJECT_TYPE, "CMS.Resource", "ResourceID", "ResourceLastModified", "ResourceGUID", "ResourceName", "ResourceDisplayName", null, null, null, null)
        {
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
            SupportsVersioning = false,
            ImportExportSettings =
            {
                AllowSingleExport = false,
                LogExport = true,
                IsExportable = true,
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                }
            },
            DefaultData = new DefaultDataSettings
            {
                ExcludedColumns = new List<string> { "ResourceIsInDevelopment" },
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Variables"

        List<String> mPermissionNames;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the resource ID.
        /// </summary>
        [DatabaseField]
        public int ResourceID
        {
            get
            {
                return GetIntegerValue("ResourceID", 0);
            }
            set
            {
                SetValue("ResourceID", value);
            }
        }


        /// <summary>
        /// List of permission names for current module
        /// </summary>
        public List<String> PermissionNames
        {
            get
            {
                if (mPermissionNames == null)
                {
                    mPermissionNames = new List<string>();

                    DataSet ds = PermissionNameInfoProvider.GetPermissionNames()
                                    .Where("ResourceID = " + ResourceID);
                    
                    // Store permission names to list
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            mPermissionNames.Add(dr["PermissionName"].ToString(""));
                        }
                    }
                }

                return mPermissionNames;
            }
            set
            {
                mPermissionNames = value;
            }
        }


        /// <summary>
        /// Gets or sets the resource display name.
        /// </summary>
        [DatabaseField]
        public string ResourceDisplayName
        {
            get
            {
                return GetStringValue("ResourceDisplayName", "");
            }
            set
            {
                SetValue("ResourceDisplayName", value);
            }
        }


        /// <summary>
        /// Gets or sets the resource name.
        /// </summary>
        [DatabaseField]
        public string ResourceName
        {
            get
            {
                return GetStringValue("ResourceName", "");
            }
            set
            {
                SetValue("ResourceName", value);
            }
        }


        /// <summary>
        /// Gets or sets the resource description. Required if you want to create an installation package.
        /// </summary>
        [DatabaseField]
        public string ResourceDescription
        {
            get
            {
                return GetStringValue("ResourceDescription", "");
            }
            set
            {
                SetValue("ResourceDescription", value);
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether this module has associated files or assemblies.
        /// </summary>
        /// <remarks>
        /// Modules with associated files or assemblies are considered available only when these files are present.
        /// Associated files and files are detected by convention. The application checks whether there is a {ResourceName} subfolder in the CMSModules folder or there is a registered module entry with the same name.
        /// </remarks>
        [DatabaseField]
        public bool ResourceHasFiles
        {
            get
            {
                return GetBooleanValue("ResourceHasFiles", false);
            }
            set
            {
                SetValue("ResourceHasFiles", value);
            }
        }


        /// <summary>
        /// Gets or sets the value saying if the resource is shown in Development.
        /// </summary>
        [DatabaseField]
        public bool ShowInDevelopment
        {
            get
            {
                return GetBooleanValue("ShowInDevelopment", false);
            }
            set
            {
                SetValue("ShowInDevelopment", value);
            }
        }


        /// <summary>
        /// Gets or sets the resource url.
        /// </summary>
        [DatabaseField]
        public string ResourceUrl
        {
            get
            {
                return GetStringValue("ResourceUrl", "");
            }
            set
            {
                SetValue("ResourceUrl", value);
            }
        }


        /// <summary>
        /// Resource GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ResourceGUID
        {
            get
            {
                return GetGuidValue("ResourceGUID", Guid.Empty);
            }
            set
            {
                SetValue("ResourceGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ResourceLastModified
        {
            get
            {
                return GetDateTimeValue("ResourceLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ResourceLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Version of the module for the purpose of installation package creation. Required if you want to create an installation package.
        /// Valid module version is in a format like "1.2.3".
        /// </summary>
        [DatabaseField]
        public virtual string ResourceVersion
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ResourceVersion"), "1.0.0");
            }
            set
            {
                SetValue("ResourceVersion", value, String.Empty);
            }
        }


        /// <summary>
        /// Author or comma-separated list of authors of the module for the purpose of installation package creation. Required if you want to create an installation package.
        /// </summary>
        [DatabaseField]
        public virtual string ResourceAuthor
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ResourceAuthor"), String.Empty);
            }
            set
            {
                SetValue("ResourceAuthor", value, String.Empty);
            }
        }


        /// <summary>
        /// State of the module. Applies to installed modules only.
        /// </summary>
        /// <seealso cref="ModuleInstallationState"/>
        [DatabaseField]
        public virtual string ResourceInstallationState
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ResourceInstallationState"), String.Empty);
            }
            set
            {
                SetValue("ResourceInstallationState", value, String.Empty);
            }
        }


        /// <summary>
        /// Currently installed version of the module. Applies to installed modules only.
        /// </summary>
        [DatabaseField]
        public virtual string ResourceInstalledVersion
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ResourceInstalledVersion"), String.Empty);
            }
            set
            {
                SetValue("ResourceInstalledVersion", value, String.Empty);
            }
        }


        /// <summary>
        /// Indicates if resource is in development. Custom system module is always considered as developed and cannot be set to false
        /// </summary>
        [DatabaseField]
        public bool ResourceIsInDevelopment
        {
            get
            {
                if (ResourceName.EqualsCSafe(ModuleName.CUSTOMSYSTEM, true))
                {
                    return true;
                }
                return GetBooleanValue("ResourceIsInDevelopment", false);
            }
            set
            {
                if (ResourceName.EqualsCSafe(ModuleName.CUSTOMSYSTEM, true))
                {
                    SetValue("ResourceIsInDevelopment", true);
                }
                else
                {
                    SetValue("ResourceIsInDevelopment", value);
                }
            }
        }


        /// <summary>
        /// Indicates if resource is editable (resource is in development or development mode is enabled).
        /// </summary>
        [RegisterProperty]
        public bool IsEditable
        {
            get
            {
                return ResourceIsInDevelopment || SystemContext.DevelopmentMode;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ResourceInfoProvider.DeleteResourceInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ResourceInfoProvider.SetResourceInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty ResourceInfo structure.
        /// </summary>
        public ResourceInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates an empty ResourceInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the class info data</param>
        public ResourceInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Cloned module behaves as new (under development)
            ResourceIsInDevelopment = true;

            base.InsertAsCloneInternal(settings, result, originalObject);

            // Load parameters
            bool permissions = true;

            var p = settings.CustomParameters;
            if (p != null)
            {
                // Exclude permissions if requested
                permissions = ValidationHelper.GetBoolean(p[PredefinedObjectType.RESOURCE + ".permissions"], true);
            }

            if (permissions)
            {
                // Do not exclude permissions
                if (settings.ExcludedChildTypes.Contains(PermissionNameInfo.OBJECT_TYPE_RESOURCE))
                {
                    settings.ExcludedChildTypes.Remove(PermissionNameInfo.OBJECT_TYPE_RESOURCE);
                }
            }
            else
            {
                // Exclude permissions
                settings.ExcludedChildTypes.Add(PermissionNameInfo.OBJECT_TYPE_RESOURCE);
            }
            
            // Clone UIElements manually (cannot be cloned using Children collection because of the hierarchy of UIElements)
            if (settings.IncludeChildren && !settings.ExcludedChildTypes.Contains(UIElementInfo.OBJECT_TYPE))
            {
                settings.ExcludedChildTypes.Add(UIElementInfo.OBJECT_TYPE);

                int originalParentId = settings.ParentID;
                settings.ParentID = ResourceID;

                UIElementInfo root = UIElementInfoProvider.GetRootUIElementInfo(originalObject.Generalized.ObjectID);
                if (root != null)
                {
                    root.Generalized.InsertAsClone(settings, result);
                }

                settings.ParentID = originalParentId;

                // Refresh child counts for UIElements
                UIElementInfoProvider.RefreshDataCounts();
            }
        }

        #endregion
    }
}