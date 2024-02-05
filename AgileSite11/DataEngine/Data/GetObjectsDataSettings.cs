using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Configuration class for <see cref="ObjectHelper.GetObjectsData(GetObjectsDataSettings)"/>.
    /// </summary>
    public class GetObjectsDataSettings
    {
        private WhereCondition mWhereCondition;


        #region "Properties"

        /// <summary>
        /// Operation type.
        /// </summary>
        public OperationTypeEnum Operation
        {
            get;
            set;
        }


        /// <summary>
        /// Main info object for which the data should be retrieved.
        /// </summary>
        public GeneralizedInfo InfoObject
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition.
        /// </summary>
        public WhereCondition WhereCondition
        {
            get
            {
                return mWhereCondition ?? (mWhereCondition = new WhereCondition());
            }
            set
            {
                mWhereCondition = value;
            }
        }


        /// <summary>
        /// Order by.
        /// </summary>
        public string OrderBy
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if child object data should be included into selection.
        /// </summary>
        public bool IncludeChildData
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if binary data should be included into selection.
        /// </summary>
        public bool IncludeBinaryData
        {
            get;
            set;
        }


        /// <summary>
        /// Translation table to initialize with data bindings.
        /// </summary>
        public TranslationHelper TranslationTable
        {
            get;
            set;
        }


        /// <summary>
        /// List of names which are used to filter out objects having codename or display name that starts with one of these names.
        /// </summary>
        public string[] ExcludedNames
        {
            get;
            set;
        }


        /// <summary>
        /// Site ID specifies that only site child objects and bindings are included into selection.
        /// </summary>
        public int SiteId
        {
            get;
            set;
        }

        #endregion


        #region "Constructor and methods"

        /// <summary>
        /// Constructor of the configuration class.
        /// </summary>
        /// <param name="operation">Operation type</param>
        /// <param name="infoObj">Main info object</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns for the main objects</param>
        /// <param name="childData">If true, child objects data are included</param>
        /// <param name="binaryData">If true, binary data are included</param>
        /// <param name="th">Translation table to initialize with data bindings</param>
        /// <param name="excludedNames">Objects with codename or display name starting with these names will be filtered out</param>
        public GetObjectsDataSettings(OperationTypeEnum operation, GeneralizedInfo infoObj, WhereCondition where, string orderBy, bool childData, bool binaryData, TranslationHelper th, string[] excludedNames)
        {
            Operation = operation;
            InfoObject = infoObj;
            WhereCondition = where;
            OrderBy = orderBy;
            IncludeChildData = childData;
            IncludeBinaryData = binaryData;
            TranslationTable = th;
            ExcludedNames = excludedNames;
        }


        /// <summary>
        /// Creates clone of current settings object.
        /// </summary>
        public GetObjectsDataSettings Clone()
        {
            var clone = new GetObjectsDataSettings(Operation, InfoObject.Clone(), WhereCondition, OrderBy, IncludeChildData, IncludeBinaryData, TranslationTable, ExcludedNames)
            {
                SiteId = SiteId
            };

            return clone;
        }

        #endregion
    }
}
