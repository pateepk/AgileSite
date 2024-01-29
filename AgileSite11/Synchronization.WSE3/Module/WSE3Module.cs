using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Synchronization.WSE3;

[assembly: RegisterModule(typeof(WSE3Module))]

namespace CMS.Synchronization.WSE3
{
    /// <summary>
    /// Represents the WSE3 module.
    /// </summary>
    public class WSE3Module : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WSE3Module()
            : base(new WSE3ModuleMetadata())
        {
        }
    }
}