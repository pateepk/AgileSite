using System;

using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Import event arguments
    /// </summary>
    public class ImportEventArgs : ImportBaseEventArgs
    {
        /// <summary>
        /// Object being currently imported
        /// </summary>
        public BaseInfo Object
        {
            get;
            set;
        }


        /// <summary>
        /// Parent object of the object being currently imported
        /// </summary>
        public BaseInfo ParentObject
        {
            get;
            set;
        }


        /// <summary>
        /// Import parameters
        /// </summary>
        public ImportParameters Parameters
        {
            get;
            set;
        }
    }
}