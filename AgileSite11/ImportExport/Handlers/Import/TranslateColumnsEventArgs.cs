using System;

using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Translate columns event arguments
    /// </summary>
    public class TranslateColumnsEventArgs : ImportEventArgs
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage
        {
            get;
            set;
        }
    }
}