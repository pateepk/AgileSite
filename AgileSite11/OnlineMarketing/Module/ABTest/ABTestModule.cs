using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineMarketing;

[assembly: RegisterModule(typeof(ABTestModule))]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Represents the A/B tests module.
    /// </summary>
    internal class ABTestModule : Module
    {
        /// <summary>
        /// Initializes a new instance of the ABTestModule class.
        /// </summary>
        public ABTestModule() : base(new ABTestModuleMetadata())
        {

        }
    }
}