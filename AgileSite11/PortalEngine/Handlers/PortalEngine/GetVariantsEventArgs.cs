using System;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Variants handling event arguments
    /// </summary>
    public class GetVariantsEventArgs: CMSEventArgs
    {
        /// <summary>
        /// Variant's zone ID
        /// </summary>
        public String ZoneID
        {
            get;
            set;
        }


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
        /// Data set with loaded variants.
        /// </summary>
        public DataSet Variants
        {
            get;
            set;
        }


        /// <summary>
        /// Instance guid
        /// </summary>
        public Guid InstanceGuid
        {
            get;
            set;
        }


        /// <summary>
        /// Document ID related to variants (widgets)
        /// </summary>
        public int DocumentID
        {
            get;
            set;
        }
    }
}
