using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineMarketing;

[assembly: RegisterModule(typeof(MVTestModule))]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Represents the MVT tests module.
    /// </summary>
    internal class MVTestModule : Module
    {
        /// <summary>
        /// Initializes a new instance of the MVTestModule class.
        /// </summary>
        public MVTestModule() : base(new MVTestModuleMetadata())
        {

        }
    }
}