using System;
using System.Linq;
using System.Text;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Container for shared settings used by operations with variants. 
    /// </summary>
    public class VariantSettings
    {
        /// <summary>
        /// The variant id
        /// </summary>
        public int ID
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the variant
        /// </summary>
        public String Name
        {
            get;
            set;
        }


        /// <summary>
        /// Display name of the variant
        /// </summary>
        public String DisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// The variant description
        /// </summary>
        public String Description
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the variant is enabled
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        }


        /// <summary>
        /// The condition under variant is shown.
        /// </summary>
        public String Condition
        {
            get;
            set;
        }


        /// <summary>
        /// The zone id
        /// </summary>
        public String ZoneID
        {
            get;
            set;
        }


        /// <summary>
        /// The web part instance GUID (Guid.Empty for zone variant)
        /// </summary>
        public Guid InstanceGuid
        {
            get;
            set;
        }


        /// <summary>
        /// The page template id
        /// </summary>
        public int PageTemplateID
        {
            get;
            set;
        }


        /// <summary>
        /// The document id
        /// </summary>
        public int DocumentID
        {
            get;
            set;
        }
    }
}