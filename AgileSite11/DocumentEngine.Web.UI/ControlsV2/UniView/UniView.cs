using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data;

using CMS.Helpers;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.PortalEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// UniView control is basic control to display data based on selected templates.
    /// </summary>
    [ToolboxItem(false)]
    public class UniView : CompositeDataBoundControl, IUniPageable
    {
        #region "Variables"

        private string mRelationColumnId = string.Empty;

        private int mAlternatingRange = 1;
        private int mGlobalIndex;

        private object mOriginalDataSource;
        private UniViewItemType mNextItemType = UniViewItemType.Item;
        private UniViewItemType mCurrentItemType = UniViewItemType.Item;
        private int mPagerForceNumberOfResults = -1;
        private string mSelectedItemColumnName = String.Empty;
        private Dictionary<string, ITemplate> mTemplates;
        private bool mUseNearestForHeaderAndFooter;

        // Level counts for grouped datasource which is saved in viewstate
        private readonly ArrayList mLevelCounts = null;

        #endregion


        #region "Template properties"

        /// <summary>
        /// Default item template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(UniViewItem)), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate ItemTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Alternating item template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(UniViewItem)), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate AlternatingItemTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// First item template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(UniViewItem)), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate FirstItemTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Last item template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(UniViewItem)), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate LastItemTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Header item template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(UniViewItem)), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate HeaderTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Footer item template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(UniViewItem)), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate FooterTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Separator item template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(UniViewItem)), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate SeparatorTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Single item template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(UniViewItem)), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate SingleItemTemplate
        {
            get;
            set;
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the value that indicates whether data from nearest data item should be used
        /// for header or footer item. This value is ignored if OuterData property is set.
        /// </summary>
        public bool UseNearestItemForHeaderAndFooter
        {
            get
            {
                return mUseNearestForHeaderAndFooter;
            }
            set
            {
                mUseNearestForHeaderAndFooter = true;
            }
        }


        /// <summary>
        /// Gets or sets the column name which should be used for selected item comparison.
        /// </summary>
        public string SelectedItemColumnName
        {
            get
            {
                return mSelectedItemColumnName;
            }
            set
            {
                mSelectedItemColumnName = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which indicates selected item.
        /// </summary>
        public object SelectedItemValue
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the transformation collection.
        /// </summary>
        public HierarchicalTransformations Transformations
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the object from which data-bound control retrieves its list of data item.
        /// </summary>
        public override object DataSource
        {
            get
            {
                return mOriginalDataSource;
            }
            set
            {
                // Save original datasource
                mOriginalDataSource = value;

                if (value is DataSet)
                {
                    base.DataSource = ((DataSet)value).Tables[0].DefaultView;
                }
                else if (value is DataTable)
                {
                    base.DataSource = ((DataTable)value).DefaultView;
                }
                else
                {
                    base.DataSource = value;
                }
            }
        }


        /// <summary>
        /// Data generated in header and footer.
        /// </summary>
        public object OuterData
        {
            get;
            set;
        }


        /// <summary>
        /// Gets DataRowView representation.
        /// </summary>
        private object OuterDataInternal
        {
            get
            {
                return ObjectToDataRowView(OuterData);
            }
        }


        /// <summary>
        /// Gets or sets the display mode for hierarchical datasource.
        /// </summary>
        public HierarchicalDisplayModeEnum HierarchicalDisplayMode
        {
            get;
            set;
        }


        /// <summary>
        /// Column from which specifies value of current level (for example parent document id).
        /// </summary>        
        public string RelationColumnID
        {
            get
            {
                return mRelationColumnId;
            }
            set
            {
                mRelationColumnId = value;
            }
        }


        /// <summary>
        /// Item / AlternatingItem template changing interval
        /// </summary>
        public int AlternatingRange
        {
            get
            {
                return mAlternatingRange;
            }
            set
            {
                mAlternatingRange = value;
            }
        }


        /// <summary>
        /// Hide Outer data (header, footer) for single item.
        /// </summary>
        public bool HideHeaderAndFooterForSingleItem
        {
            get;
            set;
        }


        /// <summary>
        /// Alternating start position.
        /// </summary>
        public int AlternatingStartPosition
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Removes default webcontrol span tag.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            RenderContents(writer);
        }


        /// <summary>
        /// Creates child controls.
        /// </summary>
        /// <param name="dataSource">Data source</param>
        /// <param name="dataBinding">Databind</param>
        protected override int CreateChildControls(IEnumerable dataSource, bool dataBinding)
        {
            Controls.Clear();
            mNextItemType = UniViewItemType.Item;
            mGlobalIndex = 0;

            if (OnPageBinding != null)
            {
                OnPageBinding(this, null);
            }

            var grouped = base.DataSource as IGroupedData;
            if (grouped != null)
            {
                if (HierarchicalDisplayMode == HierarchicalDisplayModeEnum.Separate)
                {
                    GenerateSeparateHierarchicalContent(grouped);
                }
                else if (HierarchicalDisplayMode == HierarchicalDisplayModeEnum.Inner)
                {
                    GenerateInnerHierarchicalContent(grouped, grouped.TopItem, 0);
                }
            }
            else
            {
                if (!dataBinding && (ViewState["ItemCount"] != null))
                {
                    GenerateContent(false);
                }
                else
                {
                    GenerateContent(true);
                }
            }

            return ValidationHelper.GetInteger(ViewState["ItemCount"], 0);
        }


        /// <summary>
        /// Returns default item template if selected template is empty.
        /// </summary>
        protected ITemplate GetTemplate(ITemplate template)
        {
            return template ?? ItemTemplate;
        }


        /// <summary>
        /// Returns value from specified data item object.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="dataItem">Data item</param>
        protected object GetDataItemValue(string columnName, object dataItem)
        {
            if (dataItem is DataRowView)
            {
                DataRowView drv = (DataRowView)dataItem;
                if (drv.DataView.Table.Columns.Contains(columnName))
                {
                    return drv[columnName];
                }
            }
            else if (dataItem is DataRow)
            {
                DataRow dr = (DataRow)dataItem;
                if (dr.Table.Columns.Contains(columnName))
                {
                    return dr[columnName];
                }
            }

            return null;
        }


        /// <summary>
        /// Creates new UniView item and initialize object with selected template.
        /// </summary>
        /// <param name="dataItem">Item data</param>
        /// <param name="index">Item index</param>
        /// <param name="type">Item type</param>
        /// <param name="level">Current item level</param>
        protected UniViewItem CreateHierarchicalItem(object dataItem, int index, UniViewItemType type, int level)
        {
            ITemplate itemTemplate;
            string transName = String.Empty;

            // Check whether transformation collection is defined
            if (Transformations != null)
            {
                // Initialize itemplate collection
                if (mTemplates == null)
                {
                    mTemplates = new Dictionary<string, ITemplate>(StringComparer.InvariantCultureIgnoreCase);
                }

                // Get current transformation
                if (!String.IsNullOrEmpty(SelectedItemColumnName) && (SelectedItemValue != null) && (dataItem != null) && (type != UniViewItemType.Header) && (type != UniViewItemType.Footer))
                {
                    DataRowView drv = dataItem as DataRowView;
                    if ((drv != null) && (drv.Row.Table.Columns.Contains(SelectedItemColumnName)) && drv[SelectedItemColumnName].Equals(SelectedItemValue))
                    {
                        transName = Transformations.GetTransformationName(UniViewItemType.CurrentItem, level, Convert.ToString(GetDataItemValue(Transformations.ConditionColumnName, dataItem)));
                    }
                }

                if (String.IsNullOrEmpty(transName))
                {
                    if ((type == UniViewItemType.Header) || (type == UniViewItemType.Footer))
                    {
                        // Try to get transformation name for specified level and doc. type and item type
                        transName = Transformations.GetTransformationName(type, level, Convert.ToString(GetDataItemValue(Transformations.ConditionColumnName, dataItem)));

                        // If transformation wasn't found, try find transformation for all document types
                        if (String.IsNullOrEmpty(transName))
                        {
                            transName = Transformations.GetTransformationName(type, level, String.Empty);
                        }
                    }
                    else
                    {
                        // Try to get transformation name for specified level and doc. type and item type
                        transName = Transformations.GetTransformationName(type, level, Convert.ToString(GetDataItemValue(Transformations.ConditionColumnName, dataItem)));

                        // If transformation wasn't found, try find transformation for all document types
                        if (String.IsNullOrEmpty(transName))
                        {
                            transName = Transformations.GetTransformationName(type, level, String.Empty);

                            // If transformation wasn't found, try find transformation for default item type
                            if ((type != UniViewItemType.Separator) && String.IsNullOrEmpty(transName))
                            {
                                transName = Transformations.GetTransformationName(mCurrentItemType, level, Convert.ToString(GetDataItemValue(Transformations.ConditionColumnName, dataItem)));

                                // If transformation wasn't found, try find transformation for default item type and all doc types
                                if (String.IsNullOrEmpty(transName))
                                {
                                    transName = Transformations.GetTransformationName(mCurrentItemType, level, String.Empty);

                                    // Try find default transformation for item
                                    if (String.IsNullOrEmpty(transName) && mCurrentItemType == UniViewItemType.AlternatingItem)
                                    {
                                        transName = Transformations.GetTransformationName(UniViewItemType.Item, level, Convert.ToString(GetDataItemValue(Transformations.ConditionColumnName, dataItem)));

                                        // If transformation wasn't found, try find transformation for default item type and all doc types
                                        if (String.IsNullOrEmpty(transName))
                                        {
                                            transName = Transformations.GetTransformationName(UniViewItemType.Item, level, String.Empty);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            // If transformation name is not defined, load standard transformation
            if (String.IsNullOrEmpty(transName))
            {
                return CreateItem(dataItem, index, type);
            }
            if (mTemplates.ContainsKey(transName))
            {
                itemTemplate = mTemplates[transName];
            }
            else
            {
                EditModeButtonEnum editModeEnum = EditModeButtonEnum.None;

                // Check if template type can use Edit/Delete buttons
                if ((type != UniViewItemType.Separator) && (type != UniViewItemType.Header) && (type != UniViewItemType.Footer))
                {
                    editModeEnum = (Transformations != null) ? Transformations.EditButtonsMode : EditModeButtonEnum.None;
                }

                itemTemplate = TransformationHelper.LoadTransformation(this, transName, editModeEnum);
                mTemplates.Add(transName, itemTemplate);
            }

            return InstatiateIn(itemTemplate, dataItem, index);
        }


        /// <summary>
        /// Creates and initialize new UniView item type.
        /// </summary>
        /// <param name="itemTemplate">ITemplate</param>
        /// <param name="dataItem">Data item</param>
        /// <param name="index">Item index</param>
        private UniViewItem InstatiateIn(ITemplate itemTemplate, object dataItem, int index)
        {
            if (itemTemplate == null)
            {
                return null;
            }

            var container = new UniViewItem(dataItem, index)
            {
                ID = "ctl" + index.ToString().PadLeft(2, '0')
            };

            itemTemplate.InstantiateIn(container);

            return container;
        }


        /// <summary>
        /// Creates new UniView item and initialize object with selected template.
        /// </summary>
        /// <param name="dataItem">Item data</param>
        /// <param name="index">Item index</param>
        /// <param name="type">Item type</param>
        /// <returns>New UniView item object</returns>
        protected UniViewItem CreateItem(object dataItem, int index, UniViewItemType type)
        {
            // Set default template
            ITemplate itemTemplate = null;

            // Select item template with accordance to the selected type
            switch (type)
            {
                case UniViewItemType.Item:
                    itemTemplate = ItemTemplate;
                    break;

                case UniViewItemType.AlternatingItem:
                    itemTemplate = GetTemplate(AlternatingItemTemplate);
                    break;

                case UniViewItemType.FirstItem:
                    itemTemplate = GetTemplate(FirstItemTemplate);
                    break;

                case UniViewItemType.LastItem:
                    itemTemplate = GetTemplate(LastItemTemplate);
                    break;

                case UniViewItemType.Header:
                    itemTemplate = HeaderTemplate;
                    break;

                case UniViewItemType.Footer:
                    itemTemplate = FooterTemplate;
                    break;

                case UniViewItemType.Separator:
                    itemTemplate = SeparatorTemplate;
                    break;

                case UniViewItemType.SingleItem:
                    itemTemplate = GetTemplate(SingleItemTemplate);
                    break;
            }

            return InstatiateIn(itemTemplate, dataItem, index);
        }


        /// <summary>
        /// Add UniView item to the controls collection and bind it.
        /// </summary>
        /// <param name="item">UniView item</param>
        /// <param name="itemIndex">Item index</param>
        /// <param name="container">Parent container</param>
        protected int ProcessItem(UniViewItem item, int itemIndex, Control container = null)
        {
            if (item == null)
            {
                return itemIndex;
            }

            // Fire on item created event if is defined
            if (OnItemCreated != null)
            {
                OnItemCreated(this, item);
            }

            if (container == null)
            {
                container = this;
            }

            // Add to the container
            container.Controls.Add(item);

            // Bind object
            item.DataBind();

            // Fire on item databound if is defined
            if (OnItemDataBound != null)
            {
                OnItemDataBound(this, item);
            }

            return itemIndex + 1;
        }


        /// <summary>
        /// Try to get items count from IEnumerable object.
        /// </summary>
        /// <param name="enumerable">IEnumerable object</param>
        protected int GetItemsCount(IEnumerable enumerable)
        {
            var coll = enumerable as ICollection;
            return coll != null ? coll.Count : -1;
        }


        /// <summary>
        /// Returns item type based on current index and item count.
        /// </summary>
        /// <param name="index">Item index</param>
        /// <param name="count">Item count</param>
        protected UniViewItemType GetItemType(int index, int count)
        {
            // Single item
            if (count == 1)
            {
                return UniViewItemType.SingleItem;
            }

            // Keep current item type
            mCurrentItemType = mNextItemType;

            var item = UniViewItemType.Item;

            if (((index + 1) >= AlternatingStartPosition) && (AlternatingRange > 0))
            {
                item = mNextItemType;

                // Relative item with dependence on current index and start position
                int relItem = ((index + 1) - AlternatingStartPosition + 1);

                if ((AlternatingRange == 1) || ((relItem % AlternatingRange) == 0))
                {
                    mNextItemType = mNextItemType == UniViewItemType.Item ? UniViewItemType.AlternatingItem : UniViewItemType.Item;
                }
            }


            // First item
            if ((index == 0) && ((FirstItemTemplate != null) || (Transformations != null)))
            {
                return UniViewItemType.FirstItem;
            }
            // Last item
            if ((index == (count - 1)) && ((LastItemTemplate != null) || (Transformations != null)))
            {
                return UniViewItemType.LastItem;
            }


            return item;
        }


        /// <summary>
        /// Default method to the content generating.
        /// </summary>
        /// <param name="useDataSource">Indicates whether datasource is used</param>
        protected int GenerateContent(bool useDataSource)
        {
            int currentItemIndex = 0;

            // Get inside count from view state if is present
            int insideCount = useDataSource ? GetItemsCount((IEnumerable)base.DataSource) : (int)ViewState["ItemCount"];

            int indexOfTheContentItem = 0;

            IEnumerable dataSource = useDataSource ? (IEnumerable)base.DataSource : new object[(int)ViewState["ItemCount"]];

            if (!DataHelper.DataSourceIsEmpty(dataSource))
            {
                // Header item
                UniViewItem container;
                if (!((insideCount == 1) && HideHeaderAndFooterForSingleItem))
                {
                    object dataItem = OuterDataInternal;
                    if (UseNearestItemForHeaderAndFooter && (dataItem == null) && (dataSource != null))
                    {
                        IEnumerator enumerator = dataSource.GetEnumerator();
                        if (enumerator.MoveNext())
                        {
                            dataItem = enumerator.Current;
                        }
                    }

                    container = CreateItem(dataItem, currentItemIndex, UniViewItemType.Header);
                    currentItemIndex = ProcessItem(container, currentItemIndex);
                }

                object lastDataItem = null;

                // Loop through all items in the collection
                foreach (object dataItem in dataSource)
                {
                    // Get item type
                    UniViewItemType type = GetItemType(indexOfTheContentItem, insideCount);

                    // Get item
                    container = CreateItem(dataItem, currentItemIndex, type);

                    // Add item to the control collection and bind it
                    currentItemIndex = ProcessItem(container, currentItemIndex);

                    // Process separator
                    if (indexOfTheContentItem < (insideCount - 1))
                    {
                        container = CreateItem(null, currentItemIndex, UniViewItemType.Separator);
                        currentItemIndex = ProcessItem(container, currentItemIndex);
                    }

                    // Increment content index
                    indexOfTheContentItem++;

                    // Set last data item
                    lastDataItem = dataItem;
                }

                // Footer item
                if (!((insideCount == 1) && HideHeaderAndFooterForSingleItem))
                {
                    object dataItem = OuterDataInternal;
                    if (UseNearestItemForHeaderAndFooter && (dataItem == null) && (lastDataItem != null))
                    {
                        dataItem = lastDataItem;
                    }

                    container = CreateItem(dataItem, currentItemIndex, UniViewItemType.Footer);
                    currentItemIndex = ProcessItem(container, currentItemIndex);
                }

                // Save items count
                if (useDataSource)
                {
                    ViewState["ItemCount"] = insideCount;
                }
            }

            // Return count of the items
            return currentItemIndex;
        }


        /// <summary>
        /// Separate hierarchical content generating.
        /// </summary>
        /// <param name="grouped">Grouped dataSource</param>
        protected int GenerateSeparateHierarchicalContent(IGroupedData grouped)
        {
            // Indention level
            object itemsKey = grouped.TopItem ?? 0;

            // Get items from first level
            var items = grouped.GetItems(itemsKey);

            Queue queue = new Queue();

            int deepLevel = 0;

            // Loop through all levels 
            while ((items != null) && (items.Count > 0))
            {
                int levelIndex = 0;
                int levelCount = items.Count;

                UniViewItem container;

                if (!((levelCount == 1) && HideHeaderAndFooterForSingleItem))
                {
                    object dataItem = OuterDataInternal;
                    if (UseNearestItemForHeaderAndFooter && (dataItem == null))
                    {
                        dataItem = items[0];
                    }

                    // Header item
                    container = CreateHierarchicalItem(dataItem, mGlobalIndex, UniViewItemType.Header, deepLevel);
                    mGlobalIndex = ProcessItem(container, mGlobalIndex);
                }

                // Loop through all items in the collection
                foreach (var dataItem in items)
                {
                    if (RelationColumnID != string.Empty)
                    {
                        var key = DataHelper.GetDataContainerItem(dataItem, RelationColumnID);
                        queue.Enqueue(key);
                    }

                    // Get item type
                    UniViewItemType type = GetItemType(levelIndex, levelCount);

                    // Get item
                    container = CreateHierarchicalItem(dataItem, mGlobalIndex, type, deepLevel);

                    // Add item to the control collection and bind it
                    mGlobalIndex = ProcessItem(container, mGlobalIndex);

                    // Process separator
                    if (levelIndex < (levelCount - 1))
                    {
                        container = CreateHierarchicalItem(null, mGlobalIndex, UniViewItemType.Separator, deepLevel);
                        mGlobalIndex = ProcessItem(container, mGlobalIndex);
                    }

                    levelIndex++;
                }

                mCurrentItemType = UniViewItemType.Item;
                mNextItemType = UniViewItemType.Item;

                if (!((levelCount == 1) && HideHeaderAndFooterForSingleItem))
                {
                    object dataItem = OuterDataInternal;
                    if (UseNearestItemForHeaderAndFooter && (dataItem == null))
                    {
                        dataItem = items[items.Count - 1];
                    }

                    // Footer item
                    container = CreateHierarchicalItem(dataItem, mGlobalIndex, UniViewItemType.Footer, deepLevel);
                    mGlobalIndex = ProcessItem(container, mGlobalIndex);
                }

                items = null;

                while ((queue.Count > 0) && (items == null))
                {
                    items = grouped.GetItems(queue.Dequeue());
                    deepLevel++;
                }
            }

            ViewState["LevelItemsCount"] = mLevelCounts;
            ViewState["ItemCount"] = mGlobalIndex;

            // Return count of the items
            return mGlobalIndex;
        }



        /// <summary>
        /// Inner hierarchical content generating.
        /// </summary>
        /// <param name="grouped">Grouped dataSource</param>
        /// <param name="key">Item key</param>
        /// <param name="level">Indent level</param>
        /// <param name="parent">Parent item</param>
        protected int GenerateInnerHierarchicalContent(IGroupedData grouped, object key, int level, UniViewItem parent = null)
        {
            if (key == null)
            {
                key = 0;
            }

            // Get items from first level
            var items = grouped.GetItems(key);

            if (items != null)
            {
                int levelIndex = 0;
                int levelCount = items.Count;

                UniViewItem container;

                // Try find inner content placeholder
                Control innerPlaceHolder = null;
                if (parent != null)
                {
                    SubLevelPlaceHolder plc = ControlsHelper.GetControlOfTypeRecursive<SubLevelPlaceHolder>(parent);
                    if (plc != null)
                    {
                        innerPlaceHolder = plc;
                    }
                }

                if (!((levelCount == 1) && HideHeaderAndFooterForSingleItem))
                {
                    object dataItem = OuterDataInternal;
                    if (UseNearestItemForHeaderAndFooter && (dataItem == null))
                    {
                        dataItem = items[0];
                    }

                    // Header item
                    container = CreateHierarchicalItem(dataItem, mGlobalIndex, UniViewItemType.Header, level);
                    mGlobalIndex = ProcessItem(container, mGlobalIndex, innerPlaceHolder);
                }

                // Loop through all items in the collection
                foreach (var dataItem in items)
                {
                    // Get item type
                    UniViewItemType type = GetItemType(levelIndex, levelCount);

                    // Get item
                    container = CreateHierarchicalItem(dataItem, mGlobalIndex, type, level);

                    // Add item to the control collection and bind it
                    mGlobalIndex = ProcessItem(container, mGlobalIndex, innerPlaceHolder);

                    // Recursive calling
                    if (RelationColumnID == string.Empty)
                    {
                        key = ValidationHelper.GetInteger(key, 0) + 1;
                    }
                    else
                    {
                        key = DataHelper.GetDataContainerItem(dataItem, RelationColumnID);
                    }

                    UniViewItemType ni = mNextItemType;
                    UniViewItemType cit = mCurrentItemType;
                    mNextItemType = UniViewItemType.Item;
                    mCurrentItemType = UniViewItemType.Item;
                    // Generate inner content recursive
                    GenerateInnerHierarchicalContent(grouped, key, level + 1, container);
                    mNextItemType = ni;
                    mCurrentItemType = cit;

                    // Process separator
                    if (levelIndex < (levelCount - 1))
                    {
                        container = CreateHierarchicalItem(null, mGlobalIndex, UniViewItemType.Separator, level);
                        mGlobalIndex = ProcessItem(container, mGlobalIndex, innerPlaceHolder);
                    }

                    levelIndex++;
                }

                if (!((levelCount == 1) && HideHeaderAndFooterForSingleItem))
                {
                    object dataItem = OuterDataInternal;
                    if (UseNearestItemForHeaderAndFooter && (dataItem == null))
                    {
                        dataItem = items[items.Count - 1];
                    }

                    // Footer item
                    container = CreateHierarchicalItem(dataItem, mGlobalIndex, UniViewItemType.Footer, level);
                    mGlobalIndex = ProcessItem(container, mGlobalIndex, innerPlaceHolder);
                }
            }

            ViewState["ItemCount"] = mGlobalIndex;

            return mGlobalIndex;
        }


        /// <summary>
        /// Converts object (DataSet, DataTable, DataRow) to DataRowView.
        /// </summary>
        /// <param name="dataSource">Object to convert</param>
        protected DataRowView ObjectToDataRowView(object dataSource)
        {
            if (dataSource is DataSet)
            {
                DataSet ds = (DataSet)dataSource;
                return ds.Tables[0].DefaultView[0];
            }
            if (dataSource is DataTable)
            {
                DataTable dt = (DataTable)dataSource;
                return dt.DefaultView[0];
            }
            if (dataSource is DataRow)
            {
                DataRow dr = (DataRow)dataSource;
                DataTable result = dr.Table.Clone();
                result.Rows.Add(dr.ItemArray);

                return result.DefaultView[0];
            }
            if (dataSource is DataRowView)
            {
                DataRowView drv = (DataRowView)dataSource;
                return drv;
            }
            return null;
        }

        #endregion


        #region "Events"

        /// <summary>
        /// Delegate for data bind event.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="item">UniView item</param>
        public delegate void UniViewItemEventHandler(object sender, UniViewItem item);


        /// <summary>
        /// Occurs when the control bind UniView item.
        /// </summary>
        public event UniViewItemEventHandler OnItemDataBound;


        /// <summary>
        /// Occurs when an item is created in the UniView control.
        /// </summary>
        public event UniViewItemEventHandler OnItemCreated;

        #endregion


        #region "IUniPageable Members"

        /// <summary>
        /// Occurs when the control bind data.
        /// </summary>
        public event EventHandler<EventArgs> OnPageBinding;


        /// <summary>
        /// Occurs when the pager change the page and current mode is postback => reload data
        /// </summary>
        public event EventHandler<EventArgs> OnPageChanged;


        /// <summary>
        /// Evokes control databind.
        /// </summary>
        public void ReBind()
        {
            RaiseOnPageChanged(this, null);

            DataBind();
        }


        /// <summary>
        /// Raises OnPageChanged event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        internal void RaiseOnPageChanged(object sender, EventArgs e)
        {
            if (OnPageChanged != null)
            {
                OnPageChanged(sender, e);
            }
        }


        /// <summary>
        /// Gets or sets the IUniPageable.PagerDataItem value.
        /// </summary>
        public object PagerDataItem
        {
            get
            {
                return DataSource;
            }
            set
            {
                DataSource = value;
            }
        }


        /// <summary>
        /// Pager control.
        /// </summary>
        public UniPager UniPagerControl
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the number of result. Enables proceed "fake" datasets, where number 
        /// of results in the dataset is not correspondent to the real number of results
        /// This property must be equal -1 if should be disabled
        /// </summary>
        public int PagerForceNumberOfResults
        {
            get
            {
                return mPagerForceNumberOfResults;
            }
            set
            {
                mPagerForceNumberOfResults = value;
            }
        }

        #endregion
    }
}