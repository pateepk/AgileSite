using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Manages the resource prefixes on the controls level
    /// </summary>
    internal interface IResourcePrefixManager
    {
        /// <summary>
        /// Prefix for the resource strings which are used for the localization by the control and its child controls. 
        /// </summary>
        string ResourcePrefix
        {
            get;
        }

        
        /// <summary>
        /// List of cached resource prefixes for the parent hierarchy
        /// </summary>
        ICollection<string> ResourcePrefixes
        {
            get;
        }
    }
}
