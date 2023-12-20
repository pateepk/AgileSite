using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(QueryInfo), QueryInfo.OBJECT_TYPE)]

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents a query and its parameters.
    /// </summary>
    public class QueryInfo : AbstractInfo<QueryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.query";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(QueryInfoProvider), OBJECT_TYPE, "CMS.Query", "QueryID", "QueryLastModified", "QueryGUID", "QueryName", null, null, null, "ClassID", DataClassInfo.OBJECT_TYPE)
        {
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,            
            RegisterAsChildToObjectTypes = new List<string> { DataClassInfo.OBJECT_TYPE, DataClassInfo.OBJECT_TYPE_SYSTEMTABLE, PredefinedObjectType.DOCUMENTTYPE, PredefinedObjectType.CUSTOMTABLECLASS },
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.Incremental },
            IsCustomColumn = "QueryIsCustom",
            DefaultData = new DefaultDataSettings(),
            ContinuousIntegrationSettings = 
            {
                Enabled = true
            },
        };

        #endregion


        #region "Variables"

        private string mQueryFullName;
        private string mQueryClassName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Query ID
        /// </summary>
        [DatabaseField]
        public virtual int QueryID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("QueryID"), 0);
            }
            set
            {
                SetValue("QueryID", value);
            }
        }


        /// <summary>
        /// Query name
        /// </summary>
        [DatabaseField]
        public virtual string QueryName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("QueryName"), "");
            }
            set
            {
                SetValue("QueryName", value);
                mQueryFullName = null;
            }
        }


        /// <summary>
        /// Query type ID
        /// </summary>
        [DatabaseField]
        public virtual int QueryTypeID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("QueryTypeID"), 0);
            }
            set
            {
                SetValue("QueryTypeID", value);
            }
        }


        /// <summary>
        /// Query text
        /// </summary>
        [DatabaseField]
        public virtual string QueryText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("QueryText"), "");
            }
            set
            {
                SetValue("QueryText", value);
            }
        }


        /// <summary>
        /// Query requires transaction
        /// </summary>
        [DatabaseField]
        public virtual bool QueryRequiresTransaction
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("QueryRequiresTransaction"), false);
            }
            set
            {
                SetValue("QueryRequiresTransaction", value);
            }
        }


        /// <summary>
        /// Class ID
        /// </summary>
        [DatabaseField]
        public virtual int ClassID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ClassID"), 0);
            }
            set
            {
                SetValue("ClassID", value, 0);

                mQueryClassName = null;
                mQueryFullName = null;
            }
        }


        /// <summary>
        /// Query is locked
        /// </summary>
        [DatabaseField]
        public virtual bool QueryIsLocked
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("QueryIsLocked"), false);
            }
            set
            {
                SetValue("QueryIsLocked", value);
            }
        }


        /// <summary>
        /// Query last modified
        /// </summary>
        [DatabaseField]
        public virtual DateTime QueryLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("QueryLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("QueryLastModified", value);
            }
        }


        /// <summary>
        /// Query GUID
        /// </summary>
        [DatabaseField]
        public virtual Guid QueryGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("QueryGUID"), Guid.Empty);
            }
            set
            {
                SetValue("QueryGUID", value);
            }
        }


        /// <summary>
        /// Query is custom
        /// </summary>
        [DatabaseField]
        public virtual bool QueryIsCustom
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("QueryIsCustom"), false);
            }
            set
            {
                SetValue("QueryIsCustom", value);
            }
        }


        /// <summary>
        /// Query connection string name
        /// </summary>
        [DatabaseField]
        public virtual string QueryConnectionString
        {
            get
            {
                return ValidationHelper.GetString(GetValue("QueryConnectionString"), "");
            }
            set
            {
                SetValue("QueryConnectionString", value);
            }
        }

        #endregion


        #region "Properties"


        /// <summary>
        /// Query full name.
        /// </summary>
        public string QueryFullName
        {
            get
            {
                return mQueryFullName ?? (mQueryFullName = ObjectHelper.BuildFullName(QueryClassName, QueryName));
            }
            internal set
            {
                mQueryFullName = value;
            }
        }


        /// <summary>
        /// Object full name if defined
        /// </summary>
        protected override string ObjectFullName
        {
            get
            {
                return QueryFullName;
            }
        }


        /// <summary>
        /// Query class name.
        /// </summary>
        public string QueryClassName
        {
            get
            {
                return mQueryClassName ?? (mQueryClassName = DataClassInfoProvider.GetClassName(ClassID) ?? "");
            }
        }


        /// <summary>
        /// Query type (SQL command or stored procedure).
        /// </summary>
        public QueryTypeEnum QueryType
        {
            get
            {
                return QueryInfoProvider.GetQueryType(QueryTypeID);
            }
            set
            {
                QueryTypeID = QueryInfoProvider.GetQueryTypeID(value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Object name.
        /// </summary>
        protected override string ObjectDisplayName
        {
            get
            {
                return QueryFullName;
            }
        }


        /// <summary>
        /// Indicates if the object versioning is supported.
        /// </summary>
        protected override bool SupportsVersioning
        {
            get
            {
                // Only custom queries support versioning
                return base.SupportsVersioning && QueryIsCustom;
            }
            set
            {
                base.SupportsVersioning = value;
            }
        }


        /// <summary>
        /// If true, the query is a virtually built query
        /// </summary>
        internal bool IsVirtual
        {
            get;
            set;
        }


        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            QueryType = QueryTypeEnum.SQLQuery;
            QueryRequiresTransaction = false;
            QueryIsLocked = false;

            // Override automatic class ID from type condition
            ClassID = 0;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates a new QueryInfo object from the given DataRow.
        /// </summary>
        /// <param name="typeInfo">Type info</param>
        /// <param name="dr">DataRow with the object data</param>
        public QueryInfo(ObjectTypeInfo typeInfo, DataRow dr)
            : base(typeInfo, dr)
        {
        }

        /// <summary>
        /// Constructor, creates an empty Query structure.
        /// </summary>
        protected QueryInfo(ObjectTypeInfo typeInfo)
            : base(typeInfo)
        {
        }


        /// <summary>
        /// Constructor, creates an empty Query structure.
        /// </summary>
        public QueryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates the Query object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Data row with the Query info data</param>
        public QueryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
            if (dr != null)
            {
                LoadClassName(new DataRowContainer(dr));
            }
        }


        /// <summary>
        /// Loads the object data from given data container.
        /// </summary>
        /// <param name="settings">Data settings</param>
        protected internal override void LoadData(LoadDataSettings settings)
        {
            base.LoadData(settings);

            LoadClassName(settings.Data);
        }


        /// <summary>
        /// Loads the class name from the source data
        /// </summary>
        /// <param name="data">Source data</param>
        private void LoadClassName(IDataContainer data)
        {
            if (data == null)
            {
                return;
            }

            // Get class name
            string className = DataHelper.GetStringValue(data, "ClassName");
            if (!String.IsNullOrEmpty(className))
            {
                mQueryClassName = className;
                mQueryFullName = className.ToLowerCSafe() + "." + QueryName.ToLowerCSafe();
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            QueryInfoProvider.DeleteQueryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            QueryInfoProvider.SetQueryInfo(this);
        }

        #endregion
    }
}