using System;
using System.Linq;
using System.Text;
using System.Xml;

using CMS.Base;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Class used for handling saving variants
    /// </summary>
    public class SetVariantEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Varaint's (zone or webpart) ID
        /// </summary>
        public int VariantID
        {
            get;
            set;
        }


        /// <summary>
        /// XML node (webpart or zone) definition
        /// </summary>
        public XmlNode XmlDefinition
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
        /// Settings object with variant information.
        /// </summary>
        public VariantSettings Variant
        {
            get;
            set;
        }
    }
}
