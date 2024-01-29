using System;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object data event arguments
    /// </summary>
    public class ObjectDataEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Data set returned by object query or custom data set
        /// </summary>
        public DataSet Data
        {
            get;
            set;
        }


        /// <summary>
        /// Object query.
        /// </summary>
        public IObjectQuery Query
        {
            get;
            set;
        }


        /// <summary>
        /// Number of total records when paging is used. If value is less than 0, value is calculated as number of rows in Data.
        /// </summary>
        public int TotalRecords
        {
            get;
            set;
        }
    }
}
