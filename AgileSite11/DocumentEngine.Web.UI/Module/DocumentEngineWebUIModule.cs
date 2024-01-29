using CMS;
using CMS.Base;
using CMS.DocumentEngine.Web.UI;
using CMS.DataEngine;
using CMS.MacroEngine;

[assembly: RegisterModule(typeof(DocumentEngineWebUIModule))]

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Represents the DocumentEngine Web UI module.
    /// </summary>
    internal class DocumentEngineWebUIModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DocumentEngineWebUIModule()
            : base(new DocumentEngineWebUIModuleMetadata())
        {
        }


        /// <summary>
        /// Pre-initializes the module
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            DebugHelper.RegisterDebug(ViewStateDebug.Settings);
        }
        

        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            // Register transformation resolvers
            MacroResolverStorage.RegisterGetResolverHandler(TransformationResolvers.GetResolver);

            // Register transformation methods
            MacroContext.GlobalResolver.AddAnonymousSourceData(TransformationNamespace.Instance);
            MacroContext.GlobalResolver.SetNamedSourceData(new MacroField("Transformation", () => TransformationNamespace.Instance), false);

            BackwardCompatibilityHandlers.Init();
        }
    }
}
