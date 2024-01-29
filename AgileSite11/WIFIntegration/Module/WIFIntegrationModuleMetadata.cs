using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Core;

namespace CMS.WIFIntegration
{
    /// <summary>
    /// Represents the WIF Integration module metadata.
    /// </summary>
    public class WIFIntegrationModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WIFIntegrationModuleMetadata()
            : base(ModuleName.WIFINTEGRATION)
        {
        }
    }
}
