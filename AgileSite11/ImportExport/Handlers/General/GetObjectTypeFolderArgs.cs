using System;

using CMS.Base;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Event arguments for event to get object type package folder
    /// </summary>
    public class GetObjectTypeFolderArgs : CMSEventArgs
    {
        /// <summary>
        /// Import/Export settings
        /// </summary>
        public AbstractImportExportSettings Settings
        {
            get;
            set;
        }  


        /// <summary>
        /// Object type which is imported
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if site object type is processed
        /// </summary>
        public bool SiteObjects
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the object type package folder
        /// </summary>
        public string Folder
        {
            get;
            set;
        }
    }
}