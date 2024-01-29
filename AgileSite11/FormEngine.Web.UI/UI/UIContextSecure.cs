using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Wrapper class for UIContext secure parameters.
    /// </summary>
    public class UIContextSecure : ISimpleDataContainer
    {
        #region "Constructor"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Current UI context</param>
        public UIContextSecure(UIContext context)
        {
            mContext = context;
        }

        #endregion


        #region "Variables"

        private UIContext mContext;

        #endregion 


        #region "Properties"

        /// <summary>
        /// Indexer property
        /// </summary>
        /// <param name="key">Key name to collection</param>
        public object this[String key]
        {
            get
            {
                return mContext.Data.GetKeyValue(key, false, false);
            }
            set
            {
                mContext.Data[key] = value;
            }
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            return mContext.Data.GetKeyValue(columnName, false, false);
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public bool SetValue(string columnName, object value)
        {
            mContext.Data[columnName] = value;
            return true;
        }

        #endregion
    }
}