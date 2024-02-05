using System;
using System.ComponentModel;
using System.Web.UI;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// UniView item type.
    /// </summary>
    [ToolboxItem(false)]
    public class DataPagerItem : Control, IDataItemContainer, INamingContainer
    {
        #region "Variables"

        private readonly object mDataItem;
        private readonly int mDataItemIndex;
        private readonly int mDisplayIndex;

        #endregion


        #region "Constructor"

        /// <summary>
        /// Uniview item constructor.
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="index">Item index</param>
        public DataPagerItem(object data, int index)
        {
            mDataItem = data;
            mDataItemIndex = index;
            mDisplayIndex = index;
            EnableViewState = false;
        }

        #endregion


        #region "IDataItemContainer Members"

        /// <summary>
        /// Data item.
        /// </summary>
        public object DataItem
        {
            get
            {
                return mDataItem;
            }
        }


        /// <summary>
        /// Item index.
        /// </summary>
        public int DataItemIndex
        {
            get
            {
                return mDataItemIndex;
            }
        }


        /// <summary>
        /// Display index.
        /// </summary>
        public int DisplayIndex
        {
            get
            {
                return mDisplayIndex;
            }
        }

        #endregion
    }
}