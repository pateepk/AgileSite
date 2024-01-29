using System;
using System.Collections;
using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// Class providing enumerable grouping - one pass enumerable grouping.
    /// </summary>
    public class GroupedEnumerable<ItemType> : IEnumerable, IGroupedData
    {
        private object mTopItem = null;
        private int firstLevel = -1;
        private IDictionary<object, List<ItemType>> mGroups = null;
        private readonly object locker = new object();


        /// <summary>
        /// Data source.
        /// </summary>
        public IEnumerable<ItemType> Items
        {
            get;
            protected set;
        }


        /// <summary>
        /// Lambda to evaluate the key of the item
        /// </summary>
        public Func<ItemType, object> GetItemKey
        {
            get;
            protected set;
        }


        /// <summary>
        /// Lambda to evaluate the level of the item
        /// </summary>
        public Func<ItemType, int> GetItemLevel
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
        /// Constructor
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="getItemKey">Lambda to evaluate the key of the item</param>
        /// <param name="getItemLevel">Lambda to evaluate the level of the item</param>
        public GroupedEnumerable(IEnumerable<ItemType> data, Func<ItemType, object> getItemKey, Func<ItemType, int> getItemLevel = null)
        {
            Items = data;

            GetItemKey = getItemKey;
            GetItemLevel = getItemLevel;
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
                        var groups = new Dictionary<object, List<ItemType>>();

                        if (Items != null)
                        {
                            // Add all items
                            foreach (var item in Items)
                            {
                                AddItem(groups, item);
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
        /// <param name="groupTable">Table containing the groups</param>
        /// <param name="item">Item to add</param>
        protected void AddItem(IDictionary<object, List<ItemType>> groupTable, ItemType item)
        {
            object key = GetItemKey(item);

            // Ensure DBNULL/null value key
            if ((key == DBNull.Value)  || (key == null))
            {
                key = 0;
            }

            if (GetItemLevel != null)
            {
                int level = GetItemLevel(item);
                if ((firstLevel == -1) || (level < firstLevel))
                {
                    firstLevel = level;
                    mTopItem = key;
                }
            }

            var list = EnsureList(groupTable, key);
            list.Add(item);
        }


        /// <summary>
        /// Ensures the list with specified key value.
        /// </summary>
        /// <param name="groupTable">Group table</param>
        /// <param name="key">List key</param>
        protected List<ItemType> EnsureList(IDictionary<object, List<ItemType>> groupTable, object key)
        {
            List<ItemType> list;
            if (!groupTable.TryGetValue(key, out list))
            {
                list = new List<ItemType>();
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
        public List<ItemType> GetGroupView(object itemValue)
        {
            LoadGroups();

            List<ItemType> result;
            mGroups.TryGetValue(itemValue, out result);
            return result;
        }


        /// <summary>
        /// Returns the array list of items from specified group, type of the item depends on the DataSource
        /// DataSet, DataView, DataTable -> DataRow
        /// </summary>
        /// <param name="itemValue">Key item value</param>
        /// <remarks>This method is slower than GetGroupView and creates a copy of the group</remarks>
        public List<ItemType> GetGroup(object itemValue)
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
                var group = new List<ItemType>();
                group.AddRange(groupView);

                return group;
            }
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