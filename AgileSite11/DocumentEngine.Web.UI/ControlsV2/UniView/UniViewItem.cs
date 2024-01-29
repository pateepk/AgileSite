using System;
using System.ComponentModel;
using System.Web.UI;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// UniView item type.
    /// </summary>
    [ToolboxItem(false)]
    public class UniViewItem : Control, IDataItemContainer, INamingContainer
    {
        #region "Constructor"

        /// <summary>
        /// Uniview item constructor.
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="index">Item index</param>
        public UniViewItem(object data, int index)
        {
            DataItem = data;
            DataItemIndex = index;
            DisplayIndex = index;
        }

        #endregion


        #region "IDataItemContainer Members"

        /// <summary>
        /// Data item.
        /// </summary>
        public object DataItem
        {
            get;
            private set;
        }


        /// <summary>
        /// Item index.
        /// </summary>
        public int DataItemIndex
        {
            get;
            private set;
        }


        /// <summary>
        /// Display index.
        /// </summary>
        public int DisplayIndex
        {
            get;
            private set;
        }

        #endregion
    }
}