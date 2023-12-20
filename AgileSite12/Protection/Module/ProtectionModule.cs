using CMS;
using CMS.DataEngine;
using CMS.Protection;

[assembly: RegisterModule(typeof(ProtectionModule))]

namespace CMS.Protection
{
    /// <summary>
    /// Represents the Protection module.
    /// </summary>
    public class ProtectionModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ProtectionModule()
            : base(new ProtectionModuleMetadata())
        {
        }
    }
}