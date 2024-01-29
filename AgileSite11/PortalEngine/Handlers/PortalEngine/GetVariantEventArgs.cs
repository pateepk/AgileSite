using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Argument class for event raised when variant is requested.
    /// </summary>
    public class GetVariantEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Current page template ID
        /// </summary>
        public int PageTemplateID
        {
            get;
            set;
        }


        /// <summary>
        /// Type of variant event is using.
        /// </summary>
        public VariantModeEnum VariantType
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the variant to find.s
        /// </summary>
        public String VariantName
        {
            get;
            set;
        }


        /// <summary>
        /// ID of return variant
        /// </summary>
        public int VariantID
        {
            get;
            set;
        }


        /// <summary>
        /// If set, adds check to SQL query whether the variant document id is or is not null.
        /// When null, skips the check.
        /// </summary>
        public bool? DocumentIdIsNull
        {
            get;
            set;
        }
    }
}
