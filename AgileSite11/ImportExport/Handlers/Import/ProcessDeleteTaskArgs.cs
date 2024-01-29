using System;

using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Process delete task event arguments
    /// </summary>
    public class ProcessDeleteTaskArgs : ImportBaseEventArgs
    {
        /// <summary>
        /// Delete task to be processed
        /// </summary>
        public ExportTaskInfo Task
        {
            get;
            set;
        }


        /// <summary>
        /// Existing object to be deleted
        /// </summary>
        public BaseInfo Object
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