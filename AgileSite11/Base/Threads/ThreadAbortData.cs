using System;
using System.Linq;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Container class used as state info for Thread.Abort method parameter
    /// </summary>
    internal class ThreadAbortData : ISimpleDataContainer
    {
        #region "Constructor"

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadAbortData"/> class.
        /// </summary>
        public ThreadAbortData()
        {
            AbortReason = CMSThread.ABORT_REASON_STOP;
        }

        #endregion


        #region "Variables"

        private readonly StringSafeDictionary<object> properties = new StringSafeDictionary<object>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the abort reason.         
        /// </summary>
        public string AbortReason
        {
            get;
            set;
        }

        #endregion


        #region "Methods & Indexer"

        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified column name.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                SetValue(columnName, value);
            }
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <returns></returns>
        public object GetValue(string columnName)
        {
            return properties[columnName];
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param>
        /// <returns></returns>
        public bool SetValue(string columnName, object value)
        {
            properties[columnName] = value;
            return true;
        }


        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return AbortReason;
        }

        #endregion
    }
}
