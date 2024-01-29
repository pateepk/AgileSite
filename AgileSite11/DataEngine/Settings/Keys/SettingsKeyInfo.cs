using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(SettingsKeyInfo), SettingsKeyInfo.OBJECT_TYPE)]

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents a Settings key.
    /// </summary>
    public class SettingsKeyInfo : AbstractInfo<SettingsKeyInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.settingskey";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SettingsKeyInfoProvider), OBJECT_TYPE, "CMS.SettingsKey", "KeyID", "KeyLastModified", "KeyGUID", "KeyName", "KeyDisplayName", null, "SiteID", null, null)
        {
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("KeyCategoryID", SettingsCategoryInfo.OBJECT_TYPE, ObjectDependencyEnum.Required) },
            SynchronizationSettings = 
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION),
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            AllowRestore = false,
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                IsExportable = true,
                LogExport = true,
                WhereCondition = "KeyCategoryID IS NOT NULL",
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION),
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                },
            },
            SupportsGlobalObjects = true,
            OrderColumn = "KeyOrder",
            DefaultData = new DefaultDataSettings
            {
                ExcludedColumns = new List<string> { "KeyValue", "KeyLastModified" }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                StructuredFields = new[]
                {
                    new StructuredField("KeyFormControlSettings")
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the key ID.
        /// </summary>
        [DatabaseField]
        public virtual int KeyID
        {
            get
            {
                return GetIntegerValue("KeyID", 0);
            }
            set
            {
                SetValue("KeyID", value);
            }
        }


        /// <summary>
        /// Gets or sets the key name.
        /// </summary>
        [DatabaseField]
        public virtual string KeyName
        {
            get
            {
                return GetStringValue("KeyName", null);
            }
            set
            {
                SetValue("KeyName", value);
            }
        }


        /// <summary>
        /// Gets or sets the key display name.
        /// </summary>
        [DatabaseField]
        public virtual string KeyDisplayName
        {
            get
            {
                return GetStringValue("KeyDisplayName", null);
            }
            set
            {
                SetValue("KeyDisplayName", value);
            }
        }


        /// <summary>
        /// Gets or sets the key description.
        /// </summary>
        [DatabaseField]
        public virtual string KeyDescription
        {
            get
            {
                return GetStringValue("KeyDescription", null);
            }
            set
            {
                SetValue("KeyDescription", value);
            }
        }


        /// <summary>
        /// Gets or sets the key explanation text.
        /// </summary>
        [DatabaseField]
        public virtual string KeyExplanationText
        {
            get
            {
                return GetStringValue("KeyExplanationText", null);
            }
            set
            {
                SetValue("KeyExplanationText", value);
            }
        }


        /// <summary>
        /// Gets or sets the key value.
        /// </summary>
        [DatabaseField]
        public virtual string KeyValue
        {
            get
            {
                return GetStringValue("KeyValue", null);
            }
            set
            {
                SetValue("KeyValue", value);
            }
        }


        /// <summary>
        /// Gets or sets the key type.
        /// </summary>
        [DatabaseField]
        public virtual string KeyType
        {
            get
            {
                return GetStringValue("KeyType", null);
            }
            set
            {
                SetValue("KeyType", value);
            }
        }


        /// <summary>
        /// Gets or sets the key category ID.
        /// </summary>
        [DatabaseField]
        public virtual int KeyCategoryID
        {
            get
            {
                return GetIntegerValue("KeyCategoryID", 0);
            }
            set
            {
                SetValue("KeyCategoryID", value, value > 0);
            }
        }


        /// <summary>
        /// Gets or sets the key site ID.
        /// </summary>
        [DatabaseField]
        public virtual int SiteID
        {
            get
            {
                return GetIntegerValue("SiteID", 0);
            }
            set
            {
                SetValue("SiteID", value, value > 0);
            }
        }


        /// <summary>
        /// Time stamp.
        /// </summary>
        [DatabaseField]
        public virtual DateTime KeyLastModified
        {
            get
            {
                return GetDateTimeValue("KeyLastModified", DateTime.MinValue);
            }
            set
            {
                SetValue("KeyLastModified", value);
            }
        }


        /// <summary>
        /// Key GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid KeyGUID
        {
            get
            {
                return GetGuidValue("KeyGUID", Guid.Empty);
            }
            set
            {
                SetValue("KeyGUID", value, value != Guid.Empty);
            }
        }


        /// <summary>
        /// Gets or sets the key order.
        /// </summary>
        [DatabaseField]
        public virtual int KeyOrder
        {
            get
            {
                return GetIntegerValue("KeyOrder", 0);
            }
            set
            {
                SetValue("KeyOrder", value, value > 0);
            }
        }


        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        [DatabaseField]
        public virtual string KeyDefaultValue
        {
            get
            {
                return GetStringValue("KeyDefaultValue", null);
            }
            set
            {
                SetValue("KeyDefaultValue", value);
            }
        }


        /// <summary>
        /// Gets or sets regular expression for validation.
        /// </summary>
        [DatabaseField]
        public virtual string KeyValidation
        {
            get
            {
                return GetStringValue("KeyValidation", null);
            }
            set
            {
                SetValue("KeyValidation", value);
            }
        }


        /// <summary>
        /// Gets or sets the path to formcontrol which can be used to edit the settings.
        /// </summary>
        [DatabaseField]
        public virtual string KeyEditingControlPath
        {
            get
            {
                return GetStringValue("KeyEditingControlPath", null);
            }
            set
            {
                SetValue("KeyEditingControlPath", value);
            }
        }


        /// <summary>
        /// Gets or sets the form control settings serialized in XML.
        /// </summary>
        [DatabaseField]
        public virtual string KeyFormControlSettings
        {
            get
            {
                return GetStringValue("KeyFormControlSettings", null);
            }
            set
            {
                SetValue("KeyFormControlSettings", value);
            }
        }


        /// <summary>
        /// Indicates if the settings key is only global.
        /// </summary>
        [DatabaseField]
        public virtual bool KeyIsGlobal
        {
            get
            {
                return GetBooleanValue("KeyIsGlobal", false);
            }
            set
            {
                SetValue("KeyIsGlobal", value);
            }
        }


        /// <summary>
        /// Indicates whether the settings key is custom or system default.
        /// </summary>
        [DatabaseField]
        public bool KeyIsCustom
        {
            get
            {
                return GetBooleanValue("KeyIsCustom", false);
            }
            set
            {
                SetValue("KeyIsCustom", value);
            }
        }


        /// <summary>
        /// Indicates whether the settings key is hidden or not. If it is set to true, such key will not be displayed in
        /// the settings section and users will not be able to edit value of such key.
        /// </summary>
        [DatabaseField]
        public bool KeyIsHidden
        {
            get
            {
                return GetBooleanValue("KeyIsHidden", false);
            }
            set
            {
                SetValue("KeyIsHidden", value);
            }
        }


        /// <summary>
        /// If true, synchronization tasks are logged on the object update.
        /// </summary>
        protected override SynchronizationTypeEnum LogSynchronization
        {
            get
            {
                // Excluded staging settings keys
                if (KeyName != null)
                {
                    switch (KeyName.ToLowerInvariant())
                    {
                        case "cmsexportlogobjectchanges":
                        case "cmsstaginglogstagingchanges":
                        case "cmsstaginglogobjectchanges":
                        case "cmsstaginglogchanges":
                        case "cmsstaginglogdatachanges":
                        case "cmsstagingserviceenabled":
                        case "cmsstagingserviceauthentication":
                        case "cmsstagingserviceusername":
                        case "cmsstagingservicepassword":
                        case "cmsstagingservicex509clientbase64keyid":
                        case "cmsstagingservicex509serverbase64keyid":
                            return SynchronizationTypeEnum.None;
                    }
                }

                return base.LogSynchronization;
            }
            set
            {
                base.LogSynchronization = value;
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SettingsKeyInfoProvider.DeleteSettingsKeyInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SettingsKeyInfoProvider.SetSettingsKeyInfo(this);
        }


        /// <summary>
        /// Creates where condition according to Parent, Group and Site settings.
        /// </summary>
        protected override WhereCondition GetSiblingsWhereCondition()
        {
            return base.GetSiblingsWhereCondition().WhereNull("SiteID").WhereEquals("KeyCategoryID", KeyCategoryID);
        }


        /// <summary>
        /// Gets collection of dependency keys to be touched when modifying the current object.
        /// </summary>
        protected override ICollection<string> GetCacheDependencies()
        {
            var keys = base.GetCacheDependencies();

            // Add custom key
            var custom = "cms.settingskey|";
            if (!IsGlobal)
            {
                custom += SiteID + "|";
            }
            custom += KeyName.ToLowerCSafe();
            keys.Add(custom);

            return keys;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty SettingsKeyInfo structure.
        /// </summary>
        public SettingsKeyInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates the DataClassInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the class info data</param>
        public SettingsKeyInfo(DataRow dr)
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
        protected internal override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Insert the global setting
            Insert();

            // Clone also site setting keys if global setting is cloned
            if (SiteID == 0)
            {
                // Get all related site settings with the same key
                var keys = SettingsKeyInfoProvider.GetSettingsKeys()
                    .WhereNotNull("SiteID")
                    .WhereEquals("KeyName", originalObject.Generalized.ObjectCodeName);

                foreach (var key in keys)
                {
                    settings.KeepOriginalSiteID = true;

                    key.Generalized.InsertAsClone(settings, result);
                }
            }
        }

        
        /// <summary>
        /// Returns names of all columns that should be exported with default data as a comma separated string. 
        /// </summary>
        protected override string GetDefaultDataExportColumns()
        {
            string columns = base.GetDefaultDataExportColumns();

            // Restore settings key default values
            columns = SqlHelper.AddColumns(columns, SqlHelper.AddColumnAlias("KeyDefaultValue", "KeyValue"));

            return columns;
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
                var expression = CodeNameColumn + " = 'CMSDBVersion'";
                var dbVersionKey = data.Tables[0].Select(expression).FirstOrDefault();
                if (dbVersionKey != null)
                {
                    dbVersionKey["KeyValue"] = "";
                    dbVersionKey["KeyDefaultValue"] = "";
                }
            }

            return data;
        }

        #endregion
    }
}