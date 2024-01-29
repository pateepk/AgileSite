using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class representing export/import settings in the type information of objects.
    /// </summary>
    /// <remarks>
    /// Use in the <see cref="ObjectTypeInfo.ImportExportSettings"/> property of <see cref="ObjectTypeInfo"/>.
    /// </remarks>
    [Serializable]
    public class ImportExportSettings : AbstractDataContainer<ImportExportSettings>
    {
        #region "Variables"

        /// <summary>
        /// Indicates whether the system logs export tasks when objects of the type are deleted.
        /// </summary>
        private bool mLogExport;


        /// <summary>
        /// Indicates whether the import progress log includes a record for objects of the type.
        /// </summary>
        private bool mLogProgress = true;


        /// <summary>        
        /// Indicates whether the object type supports export of individual objects from UniGrid listing pages.
        /// </summary>
        private bool? mAllowSingleExport;


        /// <summary>
        /// Determines how the system includes objects of the type when exporting parent objects.
        /// </summary>
        private IncludeToParentEnum mIncludeToExportParentDataSet = IncludeToParentEnum.None;


        /// <summary>
        /// Determines the range of objects which will go to the web template export.
        /// </summary>
        private ObjectRangeEnum mIncludeToWebTemplateExport = ObjectRangeEnum.Default;


        /// <summary>        
        /// Where condition that defines which objects of the type are available for export. Does not affect the single object export.
        /// </summary>
        private string mWhereCondition;


        /// <summary>
        /// Order by clause that sets the order of objects in the XML data of export packages.
        /// </summary>
        private string mOrderBy;

        #endregion


        #region "Properties"

        private ObjectTypeInfo TypeInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the object type supports export of individual objects from UniGrid listing pages.
        /// Default value is true when the object is exportable and logs export tasks, otherwise false.
        /// </summary>
        public bool AllowSingleExport
        {
            get
            {
                if (mAllowSingleExport == null)
                {
                    mAllowSingleExport = LogExport && IsExportable;
                }

                return mAllowSingleExport.Value;
            }
            set
            {
                mAllowSingleExport = value;
            }
        }


        /// <summary>
        /// Indicates whether the system logs export tasks when objects of the type are deleted. The delete tasks can be included in export packages.
        /// False by default.
        /// </summary>
        /// <remarks>
        /// When both IsExportable and LogExport are true, SingleObjectExport defaults to true. 
        /// When set to true, the object type must also provide at least one location in property ObjectTreeLocations.
        /// </remarks>
        public bool LogExport
        {
            get
            {
                return mLogExport && !TypeInfo.IsBinding;
            }
            set
            {
                mLogExport = value;
            }
        }


        /// <summary>
        /// Indicates whether the import progress log includes a record for objects of the type. True by default.
        /// </summary>
        public bool LogProgress
        {
            get
            {
                return mLogProgress;
            }
            set
            {
                mLogProgress = value;
            }
        }


        /// <summary>
        /// Indicates whether all objects of the type are automatically selected when creating export packages. For internal objects that are not visible in the object tree. False by default.
        /// </summary>
        public bool IsAutomaticallySelected
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the object type can be exported either through regular Export/Import or through SingleObjectExport.
        /// </summary>
        public bool SupportsExport
        {
            get
            {
                return IsExportable || AllowSingleExport;
            }
        }


        /// <summary>
        /// Indicates whether the object type can be exported through regular Export/Import.
        /// False by default.
        /// </summary>
        /// <remarks>
        /// When both IsExportable and LogExport are true, SingleObjectExport defaults to true. 
        /// When set to true, the object type must also provide at least one location in property ObjectTreeLocations.
        /// </remarks>
        public bool IsExportable
        {
            get;
            set;
        }


        /// <summary>
        /// Determines how the system includes objects of the type when exporting parent objects.
        /// </summary>
        public IncludeToParentEnum IncludeToExportParentDataSet
        {
            get
            {
                return mIncludeToExportParentDataSet;
            }
            set
            {
                if (value == IncludeToParentEnum.Default)
                {
                    throw new NotSupportedException("Setting property to default value is not supported.");
                }

                mIncludeToExportParentDataSet = value;
            }
        }


        /// <summary>
        /// Determines the range of objects which will go to the web template export.
        /// If Default, the default selection of objects for web template export is preserved.
        /// If None, this object type will never go to web template export.
        /// If All, this object type will always go to web template export (both site and global objects).
        /// If Site, global objects are excluded.
        /// If Global, site objects are excluded.
        /// </summary>
        public ObjectRangeEnum IncludeToWebTemplateExport
        {
            get
            {
                return mIncludeToWebTemplateExport;
            }
            set
            {
                mIncludeToWebTemplateExport = value;
            }
        }


        /// <summary>
        /// Indicates if object is excluded depending type. Default is false.
        /// </summary>
        public bool ExcludedDependingType
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition that defines which objects of the type are available for export. Does not affect the single object export.
        /// </summary>
        public string WhereCondition
        {
            get
            {
                if (mWhereCondition == null)
                {
                    return TypeInfo.WhereCondition;
                }

                return mWhereCondition;
            }
            set
            {
                mWhereCondition = value;
            }
        }


        /// <summary>
        /// Order by clause that sets the order of objects in the XML data of export packages. Used to ensure that export packages containing the same objects always have matching XML data.
        /// </summary>
        public string OrderBy
        {
            get
            {
                if (mOrderBy == null)
                {
                    var orderBy = ObjectTypeInfo.GetFirstKnownColumn(TypeInfo.GUIDColumn, TypeInfo.CodeNameColumn);
                    if (orderBy == null)
                    {
                        if (TypeInfo.IsBinding)
                        {
                            // For binding, use list of all binding columns as order
                            orderBy = SqlHelper.JoinColumnList(TypeInfo.GetBindingColumns());
                        }
                        else
                        {
                            orderBy = ObjectTypeInfo.GetFirstKnownColumn(TypeInfo.IDColumn, TypeInfo.DisplayNameColumn, TypeInfo.ParentIDColumn, TypeInfo.SiteIDColumn, TypeInfo.OrderColumn);
                        }
                    }

                    mOrderBy = orderBy;
                }

                return mOrderBy;
            }
            set
            {
                mOrderBy = value;
            }
        }


        /// <summary>
        /// Sets the locations of the object type within the object tree in the export/import wizard.
        /// </summary>
        public List<ObjectTreeLocation> ObjectTreeLocations
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the import process always checks the existing object when importing this object type.
        /// Use for object types that may be automatically created by another object type during import, such as automatically created scheduled tasks for recalculation etc.
        /// </summary>
        public bool AlwaysCheckExisting
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeInfo">Related type information</param>
        public ImportExportSettings(ObjectTypeInfo typeInfo)
        {
            TypeInfo = typeInfo;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Property registration.
        /// </summary>
        protected override void RegisterColumns()
        {
            base.RegisterColumns();

            RegisterColumn("AllowSingleExport", m => m.AllowSingleExport);
            RegisterColumn("LogExport", m => m.LogExport);
            RegisterColumn("IsExportable", m => m.IsExportable);
            RegisterColumn("SupportsExport", m => m.SupportsExport);
            RegisterColumn("WhereCondition", m => m.WhereCondition);
            RegisterColumn("OrderBy", m => m.OrderBy);
        }

        #endregion
    }
}