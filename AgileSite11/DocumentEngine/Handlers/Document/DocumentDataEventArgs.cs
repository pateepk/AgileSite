using System;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document data event arguments
    /// </summary>
    public class DocumentDataEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Data set returned by document query or custom data set
        /// </summary>
        public DataSet Data
        {
            get;
            set;
        }


        /// <summary>
        /// Document query.
        /// </summary>
        public IDocumentQuery Query
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

