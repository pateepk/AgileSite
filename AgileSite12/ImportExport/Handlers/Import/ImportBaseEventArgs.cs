using System;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Import/Export base event arguments
    /// </summary>
    public class ImportExportBaseEventArgs<SettingsType> : CMSEventArgs
        where SettingsType : AbstractImportExportSettings
    {
        /// <summary>
        /// Import settings
        /// </summary>
        public SettingsType Settings
        {
            get;
            set;
        }


        /// <summary>
        /// Translation helper
        /// </summary>
        public TranslationHelper TranslationHelper
        {
            get;
            set;
        }
    }


    /// <summary>
    /// Import base event arguments
    /// </summary>
    public class ImportBaseEventArgs : ImportExportBaseEventArgs<SiteImportSettings>
    {
    }
}