using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.MacroEngine;
using CMS.Modules;

[assembly: RegisterObjectType(typeof(FormUserControlInfo), FormUserControlInfo.OBJECT_TYPE)]
namespace CMS.FormEngine
{
    /// <summary>
    /// FormUserControl info data container class.
    /// </summary>
    public class FormUserControlInfo : AbstractInfo<FormUserControlInfo>
    {
        #region "Private variables"

        private string mUserControlMergedParameters;

        #endregion


        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.formusercontrol";


        /// <summary>
        /// Type information
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(FormUserControlInfoProvider), OBJECT_TYPE, "CMS.FormUserControl", "UserControlID", "UserControlLastModified", "UserControlGUID", "UserControlCodeName", "UserControlDisplayName", null, null, null, null)
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
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("UserControlResourceID", ResourceInfo.OBJECT_TYPE),
                new ObjectDependency("UserControlParentID", OBJECT_TYPE, ObjectDependencyEnum.Required)
            },
            ResourceIDColumn = "UserControlResourceID",
            DefaultOrderBy = "UserControlParentID, UserControlDisplayName",
            ThumbnailGUIDColumn = "UserControlThumbnailGUID",
            FormDefinitionColumn = "UserControlParameters",
            HasMetaFiles = true,
            AssemblyNameColumn = "UserControlAssemblyName",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
                // Keep order to ensure export of inherited form controls
                OrderBy = "UserControlParentID, UserControlGUID",
            },
            DefaultData = new DefaultDataSettings
            {
                OrderBy = "UserControlParentID"
            },
            SerializationSettings =
            {
                StructuredFields = new List<IStructuredField>
                {
                    new StructuredField("UserControlParameters")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of the user control
        /// </summary>
        [DatabaseField]
        public int UserControlID
        {
            get
            {
                return GetIntegerValue("UserControlID", 0);
            }
            set
            {
                SetValue("UserControlID", value);
            }
        }


        /// <summary>
        /// Display name of the user control
        /// </summary>
        [DatabaseField]
        public string UserControlDisplayName
        {
            get
            {
                return GetStringValue("UserControlDisplayName", String.Empty);
            }
            set
            {
                SetValue("UserControlDisplayName", value);
            }
        }


        /// <summary>
        /// Code name of the user control
        /// </summary>
        [DatabaseField]
        public string UserControlCodeName
        {
            get
            {
                return GetStringValue("UserControlCodeName", String.Empty);
            }
            set
            {
                SetValue("UserControlCodeName", value);
            }
        }


        /// <summary>
        /// File name of the user control
        /// </summary>
        [DatabaseField]
        public string UserControlFileName
        {
            get
            {
                return GetStringValue("UserControlFileName", String.Empty);
            }
            set
            {
                SetValue("UserControlFileName", value);
            }
        }


        /// <summary>
        /// Indicates if user control can be used for Text values.
        /// </summary>
        [DatabaseField]
        public bool UserControlForText
        {
            get
            {
                return GetBooleanValue("UserControlForText", false);
            }
            set
            {
                SetValue("UserControlForText", value);
            }
        }


        /// <summary>
        /// Indicates if user control can be used for Long Text values.
        /// </summary>
        [DatabaseField]
        public bool UserControlForLongText
        {
            get
            {
                return GetBooleanValue("UserControlForLongText", false);
            }
            set
            {
                SetValue("UserControlForLongText", value);
            }
        }


        /// <summary>
        /// Indicates if user control can be used for Integer values.
        /// </summary>
        [DatabaseField]
        public bool UserControlForInteger
        {
            get
            {
                return GetBooleanValue("UserControlForInteger", false);
            }
            set
            {
                SetValue("UserControlForInteger", value);
            }
        }


        /// <summary>
        /// Indicates if user control can be used for Decimal values.
        /// </summary>
        [DatabaseField]
        public bool UserControlForDecimal
        {
            get
            {
                return GetBooleanValue("UserControlForDecimal", false);
            }
            set
            {
                SetValue("UserControlForDecimal", value);
            }
        }


        /// <summary>
        /// Indicates if user control can be used for DateTime values.
        /// </summary>
        [DatabaseField]
        public bool UserControlForDateTime
        {
            get
            {
                return GetBooleanValue("UserControlForDateTime", false);
            }
            set
            {
                SetValue("UserControlForDateTime", value);
            }
        }


        /// <summary>
        /// Indicates if user control can be used for Boolean values.
        /// </summary>
        [DatabaseField]
        public bool UserControlForBoolean
        {
            get
            {
                return GetBooleanValue("UserControlForBoolean", false);
            }
            set
            {
                SetValue("UserControlForBoolean", value);
            }
        }


        /// <summary>
        /// Indicates if user control can be used for Files.
        /// </summary>
        [DatabaseField]
        public bool UserControlForFile
        {
            get
            {
                return GetBooleanValue("UserControlForFile", false);
            }
            set
            {
                SetValue("UserControlForFile", value);
            }
        }


        /// <summary>
        /// Indicates if user control can be used for Document attachments.
        /// </summary>
        [DatabaseField]
        public bool UserControlForDocAttachments
        {
            get
            {
                return GetBooleanValue("UserControlForDocAttachments", false);
            }
            set
            {
                SetValue("UserControlForDocAttachments", value);
            }
        }


        /// <summary>
        /// Indicates if user control can be used for Document relationships.
        /// </summary>
        [DatabaseField]
        public bool UserControlForDocRelationships
        {
            get
            {
                return GetBooleanValue("UserControlForDocRelationships", false);
            }
            set
            {
                SetValue("UserControlForDocRelationships", value);
            }
        }


        /// <summary>
        /// Indicates if control should be used for GUID.
        /// </summary>
        [DatabaseField]
        public bool UserControlForGUID
        {
            get
            {
                return GetBooleanValue("UserControlForGuid", false);
            }
            set
            {
                SetValue("UserControlForGuid", value);
            }
        }


        /// <summary>
        /// Indicates if control should be used for visibility.
        /// </summary>
        [DatabaseField]
        public bool UserControlForVisibility
        {
            get
            {
                return GetBooleanValue("UserControlForVisibility", false);
            }
            set
            {
                SetValue("UserControlForVisibility", value);
            }
        }


        /// <summary>
        /// Indicates if user control can be used for binary values.
        /// </summary>
        [DatabaseField]
        public bool UserControlForBinary
        {
            get
            {
                return GetBooleanValue("UserControlForBinary", false);
            }
            set
            {
                SetValue("UserControlForBinary", value);
            }
        }


        /// <summary>
        /// Indicates if user control can be used for BizForms.
        /// </summary>
        [DatabaseField]
        public bool UserControlShowInBizForms
        {
            get
            {
                return GetBooleanValue("UserControlShowInBizForms", false);
            }
            set
            {
                SetValue("UserControlShowInBizForms", value);
            }
        }


        /// <summary>
        /// Indicates default data type for control.
        /// </summary>
        [DatabaseField]
        public string UserControlDefaultDataType
        {
            get
            {
                return GetStringValue("UserControlDefaultDataType", String.Empty);
            }
            set
            {
                SetValue("UserControlDefaultDataType", value);
            }
        }


        /// <summary>
        /// Indicates default data type size.
        /// </summary>
        [DatabaseField]
        public int UserControlDefaultDataTypeSize
        {
            get
            {
                return GetIntegerValue("UserControlDefaultDataTypeSize", 0);
            }
            set
            {
                SetValue("UserControlDefaultDataTypeSize", value);
            }
        }


        /// <summary>
        /// Indicates if user control can be displayed for Document types.
        /// </summary>
        [DatabaseField]
        public bool UserControlShowInDocumentTypes
        {
            get
            {
                return GetBooleanValue("UserControlShowInDocumentTypes", false);
            }
            set
            {
                SetValue("UserControlShowInDocumentTypes", value);
            }
        }


        /// <summary>
        /// Indicates if user control can be displayed in System tables.
        /// </summary>
        [DatabaseField]
        public bool UserControlShowInSystemTables
        {
            get
            {
                return GetBooleanValue("UserControlShowInSystemTables", false);
            }
            set
            {
                SetValue("UserControlShowInSystemTables", value);
            }
        }


        /// <summary>
        /// Indicates if user control can be displayed in Web parts.
        /// </summary>
        [DatabaseField]
        public bool UserControlShowInWebParts
        {
            get
            {
                return GetBooleanValue("UserControlShowInWebParts", false);
            }
            set
            {
                SetValue("UserControlShowInWebParts", value);
            }
        }


        /// <summary>
        /// Indicates if user control can be displayed in Reports.
        /// </summary>
        [DatabaseField]
        public bool UserControlShowInReports
        {
            get
            {
                return GetBooleanValue("UserControlShowInReports", false);
            }
            set
            {
                SetValue("UserControlShowInReports", value);
            }
        }


        /// <summary>
        /// Enables or disables use of the form control in custom tables.
        /// </summary>
        [DatabaseField]
        public bool UserControlShowInCustomTables
        {
            get
            {
                return GetBooleanValue("UserControlShowInCustomTables", false);
            }
            set
            {
                SetValue("UserControlShowInCustomTables", value);
            }
        }


        /// <summary>
        /// User control GUID
        /// </summary>
        [DatabaseField]
        public virtual Guid UserControlGUID
        {
            get
            {
                return GetGuidValue("UserControlGUID", Guid.Empty);
            }
            set
            {
                SetValue("UserControlGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified
        /// </summary>
        [DatabaseField]
        public virtual DateTime UserControlLastModified
        {
            get
            {
                return GetDateTimeValue("UserControlLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("UserControlLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Parameters for control
        /// </summary>
        [DatabaseField]
        public string UserControlParameters
        {
            get
            {
                return GetStringValue("UserControlParameters", String.Empty);
            }
            set
            {
                SetValue("UserControlParameters", value);
            }
        }


        /// <summary>
        /// Control resource (module) ID
        /// </summary>
        [DatabaseField]
        public int UserControlResourceID
        {
            get
            {
                return GetIntegerValue("UserControlResourceID", 0);
            }
            set
            {
                SetValue("UserControlResourceID", value, (value > 0));
            }
        }


        /// <summary>
        /// Type of the user control
        /// </summary>
        [DatabaseField(ValueType = typeof(int))]
        public FormUserControlTypeEnum UserControlType
        {
            get
            {
                return FormUserControlInfoProvider.GetTypeEnum(GetIntegerValue("UserControlType", -1));
            }
            set
            {
                SetValue("UserControlType", (int)value, FormUserControlTypeEnum.Unspecified);
            }
        }


        /// <summary>
        /// ID of the parent user control. Is equal to 0 if the control has no parent control
        /// </summary>
        [DatabaseField]
        public int UserControlParentID
        {
            get
            {
                return GetIntegerValue("UserControlParentID", 0);
            }
            set
            {
                SetValue("UserControlParentID", value);
            }
        }


        /// <summary>
        /// Merged user control parameters from original and inherited control. If current control is not inherited returns UserControlParameters
        /// </summary>
        public string UserControlMergedParameters
        {
            get
            {
                if (mUserControlMergedParameters == null)
                {
                    if (UserControlParentID > 0)
                    {
                        FormUserControlInfo parent = FormUserControlInfoProvider.GetFormUserControlInfo(UserControlParentID);
                        if (parent != null)
                        {
                            mUserControlMergedParameters = FormUserControlInfoProvider.MergeDefaultValues(parent.UserControlParameters, UserControlParameters);
                        }
                    }
                    else
                    {
                        mUserControlMergedParameters = UserControlParameters;
                    }

                    return mUserControlMergedParameters;
                }

                return mUserControlMergedParameters;
            }
            set
            {
                mUserControlMergedParameters = value;
            }
        }


        /// <summary>
        /// Description of user control
        /// </summary>
        [DatabaseField]
        public string UserControlDescription
        {
            get
            {
                return GetStringValue("UserControlDescription", String.Empty);
            }
            set
            {
                SetValue("UserControlDescription", value);
            }
        }


        /// <summary>
        /// Control thumbnail metafile GUID
        /// </summary>
        [DatabaseField]
        public virtual Guid UserControlThumbnailGUID
        {
            get
            {
                return GetGuidValue("UserControlThumbnailGUID", Guid.Empty);
            }
            set
            {
                SetValue("UserControlThumbnailGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Gets or sets control's priority.
        /// </summary>
        [DatabaseField]
        public virtual int UserControlPriority
        {
            get
            {
                return GetIntegerValue("UserControlPriority", (int)ObjectPriorityEnum.Low);
            }
            set
            {
                SetValue("UserControlPriority", value, (int)ObjectPriorityEnum.Low);
            }
        }


        /// <summary>
        /// User control assembly name.
        /// </summary>
        [DatabaseField]
        public virtual string UserControlAssemblyName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("UserControlAssemblyName"), String.Empty);
            }
            set
            {
                SetValue("UserControlAssemblyName", value, String.Empty);
            }
        }


        /// <summary>
        /// User control class name.
        /// </summary>
		[DatabaseField]
        public virtual string UserControlClassName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("UserControlClassName"), String.Empty);
            }
            set
            {
                SetValue("UserControlClassName", value, String.Empty);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty FormUserControlInfo structure.
        /// </summary>
        public FormUserControlInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates FormUserControlInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the class info data</param>
        public FormUserControlInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            FormUserControlInfoProvider.DeleteFormUserControlInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            FormUserControlInfoProvider.SetFormUserControlInfo(this);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            UserControlType = FormUserControlTypeEnum.Input;
            UserControlForText = false;
            UserControlForLongText = false;
            UserControlForInteger = false;
            UserControlForDecimal = false;
            UserControlForDateTime = false;
            UserControlForBoolean = false;
            UserControlForFile = false;
            UserControlForDocAttachments = false;
            UserControlForGUID = false;
            UserControlForVisibility = false;
            UserControlForBinary = false;
            UserControlForDocRelationships = false;
            UserControlShowInBizForms = false;
            UserControlDefaultDataType = "Text";
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

            Hashtable cParams = settings.CustomParameters;
            if (cParams != null)
            {
                UserControlFileName = ValidationHelper.GetString(cParams["cms.formusercontrol" + ".filename"], String.Empty);
                copyFiles = ValidationHelper.GetBoolean(cParams["cms.formusercontrol" + ".files"], false);
            }

            Insert();

            // Copy files if required
            if (copyFiles)
            {
                // Get source file path
                string srcFile = CMSHttpContext.Current.Server.MapPath(originalObject.GetStringValue("UserControlFileName", String.Empty));

                if (File.Exists(srcFile))
                {
                    // Get destination file path
                    string dstFile = URLHelper.GetPhysicalPath(UserControlFileName);

                    // Read .aspx file, replace classname and save as new file
                    FileHelper.CloneControlSource(srcFile, dstFile, UserControlFileName);
                }
            }
        }


        /// <summary>
        /// Gets the list of object types that may use the form control
        /// </summary>
        [MacroMethod]
        public IEnumerable<string> GetUsageObjectTypes()
        {
            var usages = ObjectTypeManager.GetObjectTypes(ObjectTypeManager.AllObjectTypes, t => (t.FormDefinitionColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                && ((t.OriginalTypeInfo == null) || (t.OriginalTypeInfo.TypeCondition != null)));

            usages.Add(SettingsKeyInfo.OBJECT_TYPE);

            return usages;
        }


        /// <summary>
        /// Gets the objects using the form control as a query with result columns ObjectType, ObjectID.
        /// </summary>
        [MacroMethod]
        public IDataQuery GetUsages()
        {
            var q = new MultiObjectQuery();

            // Pre-filter by searching control name in XML
            var searchFor = "<controlname>" + UserControlCodeName + "</controlname>";

            // Add all types with form definition columns
            var formDefTypes = ObjectTypeManager.GetTypeInfos(ObjectTypeManager.AllObjectTypes, t => (t.FormDefinitionColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                && ((t.OriginalTypeInfo == null) || (t.OriginalTypeInfo.TypeCondition != null)));

            foreach (var type in formDefTypes)
            {
                var ti = type;

                q.Type(type.ObjectType, t => t
                    .Columns(
                        ti.ObjectType.AsValue(true).AsColumn("ObjectType"),
                        new QueryColumn(ti.IDColumn).As("ObjectID")
                    )
                    .WhereContains(ti.FormDefinitionColumn, searchFor)
                );
            }

            // Add controls which inherit the control, this type is already listed by form definitions, so just extend it
            q.Type(OBJECT_TYPE, t => t
                .Or()
                .WhereEquals("UserControlParentID", UserControlID)
            );

            // Include global settings keys
            q.Type(SettingsKeyInfo.OBJECT_TYPE, t => t
                .Columns(
                    SettingsKeyInfo.OBJECT_TYPE.AsValue(true).AsColumn("ObjectType"),
                    new QueryColumn("KeyID").As("ObjectID")
                )
                .OnlyGlobal()
                .WhereEquals("KeyEditingControlPath", UserControlCodeName)
            );

            // Order by source type by default to keep usages from various types grouped
            q.DefaultOrderByType = true;

            // Data is generally inconsistent, so any global condition should be use on the results
            q.UseGlobalWhereOnResult = true;

            return q;
        }

        #endregion
    }
}