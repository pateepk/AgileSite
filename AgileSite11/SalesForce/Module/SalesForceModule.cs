using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.SalesForce;

[assembly: RegisterModule(typeof(SalesForceModule))]

namespace CMS.SalesForce
{
    internal class SalesForceModule : Module
    {
        private const string MODULE_NAME = "CMS.SalesForce";

        /// <summary>
        /// Default constructor
        /// </summary>
        public SalesForceModule()
            : base(new ModuleMetadata(MODULE_NAME))
        {
        }



        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            SalesForceHandlers.Init();
        }
    }
}
