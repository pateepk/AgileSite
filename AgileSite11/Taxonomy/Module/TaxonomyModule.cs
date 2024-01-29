using CMS;
using CMS.DataEngine;
using CMS.Taxonomy;

[assembly: RegisterModule(typeof(TaxonomyModule))]

namespace CMS.Taxonomy
{
    /// <summary>
    /// Represents the Taxonomy module.
    /// </summary>
    public class TaxonomyModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public TaxonomyModule()
            : base(new TaxonomyModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Import/export handlers
            ImportSpecialActions.Init();
            ExportSpecialActions.Init();

            RegisterContext<TaxonomyContext>("TaxonomyContext");
        }
    }
}