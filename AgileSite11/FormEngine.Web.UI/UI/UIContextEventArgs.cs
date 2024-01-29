using System;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Event argument class for UI context
    /// </summary>
    public class UIContextEventArgs : EventArgs
    {
        /// <summary>
        /// Name of column name
        /// </summary>
        public String ColumnName
        {
            get;
            set;
        }


        /// <summary>
        /// Returned value
        /// </summary>
        public object Result
        {
            get;
            set;
        }
    }
}
