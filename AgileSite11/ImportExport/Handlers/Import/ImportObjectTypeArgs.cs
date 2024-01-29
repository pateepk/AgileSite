using System;

using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Event arguments for events based on object type
    /// </summary>
    public class ImportObjectTypeArgs : ImportBaseEventArgs
    {
        /// <summary>
        /// Object type which is imported
        /// </summary>
        public string ObjectType
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