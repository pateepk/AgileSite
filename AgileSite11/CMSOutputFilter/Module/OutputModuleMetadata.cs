using System;
using System.Linq;
using System.Text;

using CMS.Core;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Represents the Output filter module metadata.
    /// </summary>
    internal class OutputModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public OutputModuleMetadata()
            : base(ModuleName.OUTPUTFILTER)
        {
        }
    }
}
