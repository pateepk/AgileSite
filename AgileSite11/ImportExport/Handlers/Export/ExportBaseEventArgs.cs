using System;

using CMS.Base;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Export base event arguments
    /// </summary>
    public class ExportBaseEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Export settings
        /// </summary>
        public SiteExportSettings Settings 
        { 
            get; 
            set; 
        }
    }
}