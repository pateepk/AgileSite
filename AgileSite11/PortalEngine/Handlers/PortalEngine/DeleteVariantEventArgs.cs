using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Argument class for deleting variants
    /// </summary>
    public class DeleteVariantEventArgs : CMSEventArgs
    {
        /// <summary>
        /// ID of variant's zone
        /// </summary>
        public String ZoneID
        {
            get;
            set;
        }


        /// <summary>
        /// ID of variant's page template
        /// </summary>
        public int PageTemplateID
        {
            get;
            set;
        }


        /// <summary>
        /// ID of variant's document
        /// </summary>
        public int DocumentID
        {
            get;
            set;
        }
    }
}
