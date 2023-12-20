using System.Collections.Generic;
using System.Data;

using CMS.Core;
using CMS.Helpers;
using System.Collections;

namespace CMS.DataEngine
{
    /// <summary>
    /// CMS DataSet class. Enhancement to DataSet to provide strongly typed collection side-by-side with the data.
    /// </summary>
    public class InfoDataSet<TInfo> : DataSet, IReadOnlyFlag, IEnumerable<TInfo>, IInfoDataSet
        where TInfo : BaseInfo
    {
        #region "Variables"

        /// <summary>ct
        /// Info object.
        /// </summary>
        private TInfo mObject;


        /// <summary>
        /// Items in the DataSet
        /// </summary>
        protected InfoObjectCollection<TInfo> mItems;


        #endregion


        #region "Properties"

        /// <summary>
        /// Info object instance the provider is working with.
        /// </summary>
        internal TInfo Object
        {
            get
            {
                return mObject ?? (mObject = ObjectFactory<TInfo>.New());
            }
            set
            {
                mObject = value;
            }
        }


        /// <summary>
        /// Items in the DataSet
        /// </summary>
        public InfoObjectCollection<TInfo> Items
        {
            get
            {
                return mItems ?? (mItems = NewCollection());
            }
            protected set
            {
                mItems = value;
            }
        }


        /// <summary>
        /// If true, the dataset is cached (not allowed to modify, must be cloned)
        /// </summary>
        public bool IsReadOnly
        {
            get;
            set;
        }
        
        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public InfoDataSet()
        {
        }


        /// <summary>
        /// Constructor, creates new DataSet using data from the given DataSet.
        /// Removes data from the original data set.
        /// </summary>
        /// <param name="sourceData">Source data</param>
        public InfoDataSet(DataSet sourceData)
        {
            if (sourceData == null)
            {
                return;
            }

            // Ensure not cached data
            sourceData = sourceData.AsModifyable();

            // Transfer the tables to this DataSet
            DataHelper.TransferTables(this, sourceData);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Source items from which the DataSet will be created</param>
        public InfoDataSet(params TInfo[] items)
        {
            CreateEmptyDataSet();

            AddItems(items);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates new collection of the objects
        /// </summary>
        protected virtual InfoObjectCollection<TInfo> NewCollection()
        {
            var collection = new InfoObjectCollection<TInfo>(this);
            collection.Object = Object;

            return collection;
        }


        /// <summary>
        /// Clones the DataSet
        /// </summary>
        public new InfoDataSet<TInfo> Clone()
        {
            DataSet ds = base.Clone();

            return new InfoDataSet<TInfo>(ds);
        }


        /// <summary>
        /// Gets new instance of the object hosted in this DataSet
        /// </summary>
        /// <param name="dr">Data row with the source data</param>
        public TInfo GetNewObject(DataRow dr)
        {
            // Initialize new object
            return Items.CreateNewObject(dr);
        }


        /// <summary>
        /// Adds the items to the data set
        /// </summary>
        /// <param name="items">Items to add</param>
        protected void AddItems(params TInfo[] items)
        {
            var dt = Tables[0];

            // Process all items
            foreach (var item in items)
            {
                var dr = dt.NewRow();

                // Fill in the data
                foreach (DataColumn col in dt.Columns)
                {
                    var colName = col.ColumnName;

                    dr[colName] = DataHelper.GetDBNull(item.GetValue(colName));
                }

                dt.Rows.Add(dr);

                // Set the item on particular index
                // Must be set after dataset was updated
                Items[dt.Rows.Count - 1] = item;
            }
        }


        /// <summary>
        /// Creates the empty DataSet based on the current type
        /// </summary>
        protected void CreateEmptyDataSet()
        {
            // Get empty dataset for data
            var csi = Object.TypeInfo.ClassStructureInfo;
            if (csi != null)
            {
                var ds = csi.GetNewDataSet();

                DataHelper.TransferTables(this, ds);
            }
        }

        #endregion


        #region "IEnumerable<TInfo> Members"

        /// <summary>
        /// Gets the strongly typed enumerator for the collection
        /// </summary>
        public IEnumerator<TInfo> GetEnumerator()
        {
            // Encapsulate the enumerator and cast the results
            var baseEnum = GetObjectEnumerator();

            while (baseEnum.MoveNext())
            {
                yield return baseEnum.Current;
            }
        }


        /// <summary>
        /// Gets the object enumerator for the collection
        /// </summary>
        protected IEnumerator<TInfo> GetObjectEnumerator()
        {
            return Items.GetEnumerator();
        }
        
        #endregion


        #region "IEnumerable Members"

        /// <summary>
        /// Returns the enumerator for this collection
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetObjectEnumerator();
        }

        #endregion


        #region "IInfoDataSet members"

        /// <summary>
        /// Gets new instance of the object hosted in this DataSet
        /// </summary>
        /// <param name="dr">Data row with the source data</param>
        BaseInfo IInfoDataSet.GetNewObject(DataRow dr)
        {
            return GetNewObject(dr);
        }

        #endregion
    }
}