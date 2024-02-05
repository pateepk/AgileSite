using System;

using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Get binary data source path event arguments
    /// </summary>
    public class GetBinaryDataSourcePathEventArgs: ImportEventArgs
    {
        /// <summary>
        /// Source path
        /// </summary>
        public string Path
        {
            get;
            set;
        }
    }
}