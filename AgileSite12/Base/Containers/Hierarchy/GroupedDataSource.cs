using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace CMS.Base
{
    /// <summary>
    /// Class providing DataSource grouping - one pass DataSource grouping.
    /// </summary>
    public class GroupedDataSource : IEnumerable, IGroupedData
    {
        private object mTopItem = null;
        private int firstLevel = -1;
        private IDictionary<object, List<DataRowView>> mGroups = null;
        private readonly object locker = new object();


        /// <summary>
        /// Data source.
        /// </summary>
        public object DataSource
        {
            get;
            protected set;
        }


        /// <summary>
        /// Column name for grouping.
        /// </summary>
        public string ColumnName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Column name for level value.
        /// </summary>
        public string LevelColumnName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the first available item.
        /// </summary>
        public object TopItem
        {
            get
            {
                LoadGroups();
                return mTopItem;
            }
        }


        /// <summary>
        /// Constructor, initializes the GroupedDataSource.
        /// </summary>
        /// <param name="dataSource">Data source</param>
        /// <param name="columnName">Column name to use for item grouping</param>
        public GroupedDataSource(object dataSource, string columnName)
        {
            DataSource = dataSource;
            ColumnName = columnName;
        }


        /// <summary>
        /// Constructor, initializes the GroupedDataSource.
        /// </summary>
        /// <param name="dataSource">Data source</param>
        /// <param name="columnName">Column name to use for item grouping</param>
        /// <param name="levelColumnName">Column name to use for level item</param>
        public GroupedDataSource(object dataSource, string columnName, string levelColumnName)
        {
            DataSource = dataSource;
            ColumnName = columnName;
            LevelColumnName = levelColumnName;
        }


        /// <summary>
        /// Loads the data groups.
        /// </summary>
        protected void LoadGroups()
        {
            if (mGroups == null)
            {
                lock (locker)
                {
                    if (mGroups == null)
                    {
                        var groups = new Dictionary<object, List<DataRowView>>();

                        if (DataSource != null)
                        {
                            if (DataSource is DataSet)
                            {
                                // Add all rows from all tables
                                DataSet ds = (DataSet)DataSource;
                                foreach (DataTable dt in ds.Tables)
                                {
                                    foreach (DataRowView dr in dt.DefaultView)
                                    {
                                        AddDataRow(groups, dr);
                                    }
                                }
                            }
                            else if (DataSource is DataTable)
                            {
                                // Add all rows
                                DataTable dt = (DataTable)DataSource;
                                foreach (DataRowView dr in dt.DefaultView)
                                {
                                    AddDataRow(groups, dr);
                                }
                            }
                            else if (DataSource is DataView)
                            {
                                // Add all rows
                                DataView dv = (DataView)DataSource;
                                foreach (DataRowView dr in dv.Table.DefaultView)
                                {
                                    AddDataRow(groups, dr);
                                }
                            }
                        }

                        mGroups = groups;
                    }
                }
            }
        }


        /// <summary>
        /// Adds the item to the group table.
        /// </summary>
        /// <param name="groupTable">Group table</param>
        /// <param name="dr">DataRowView to add</param>
        protected void AddDataRow(IDictionary<object, List<DataRowView>> groupTable, DataRowView dr)
        {
            object key = dr[ColumnName];

            // Ensure DBNULL/null value key
            if ((key == DBNull.Value) || (key == null))
            {
                key = 0;
            }

            if (!String.IsNullOrEmpty(LevelColumnName))
            {
                int level = Convert.ToInt32(dr[LevelColumnName]);
                if ((firstLevel == -1) || (level < firstLevel))
                {
                    firstLevel = level;
                    mTopItem = key;
                }
            }

            List<DataRowView> list = EnsureList(groupTable, key);
            list.Add(dr);
        }


        /// <summary>
        /// Ensures the list with specified key value.
        /// </summary>
        /// <param name="groupTable">Group table</param>
        /// <param name="key">List key</param>
        protected List<DataRowView> EnsureList(IDictionary<object, List<DataRowView>> groupTable, object key)
        {
            List<DataRowView> list;
            if (!groupTable.TryGetValue(key, out list))
            {
                list = new List<DataRowView>();
                groupTable[key] = list;
            }
            return list;
        }


        /// <summary>
        /// Gets the children based on the given key
        /// </summary>
        /// <param name="key">Key</param>
        public IList GetItems(object key)
        {
            return GetGroupView(key);
        }


        /// <summary>
        /// Returns the array list of items from specified group, type of the item depends on the DataSource
        /// DataSet, DataView, DataTable -> DataRowView
        /// </summary>
        /// <param name="itemValue">Key item value</param>
        public List<DataRowView> GetGroupView(object itemValue)
        {
            LoadGroups();

            List<DataRowView> list;
            mGroups.TryGetValue(itemValue, out list);
            return list;
        }


        /// <summary>
        /// Returns the array list of items from specified group, type of the item depends on the DataSource
        /// DataSet, DataView, DataTable -> DataRow
        /// </summary>
        /// <param name="itemValue">Key item value</param>
        /// <remarks>This method is slower than GetGroupView and creates a copy of the group</remarks>
        public List<DataRowView> GetGroup(object itemValue)
        {
            // Get the DataRowView group
            var groupView = GetGroupView(itemValue);
            if (groupView == null)
            {
                return null;
            }
            else
            {
                // Build the DataRow group
                var group = new List<DataRowView>();
                group.AddRange(groupView);

                return group;
            }
        }


        /// <summary>
        /// Returns the item group as an DataTable.
        /// </summary>
        /// <param name="itemValue">Item value</param>
        /// <remarks>This method is slower than GetGroupView and creates a copy of the group</remarks>
        public DataTable GetGroupDataTable(object itemValue)
        {
            var items = GetGroupView(itemValue);
            DataTable result = null;

            if ((items != null) && (items.Count > 0))
            {
                result = new DataTable();
                result = items[0].Row.Table.Clone();

                // Copy the rows
                foreach (DataRowView dr in items)
                {
                    result.Rows.Add(dr.Row.ItemArray);
                }
            }

            return result;
        }


        /// <summary>
        /// Returns grouped datasource enumerator.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            LoadGroups();
            return mGroups.GetEnumerator();
        }
    }
}