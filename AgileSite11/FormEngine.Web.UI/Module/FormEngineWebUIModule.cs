using CMS;
using CMS.Core;
using CMS.FormEngine.Web.UI;
using CMS.MacroEngine;

[assembly: RegisterModule(typeof(FormEngineWebUIModule))]

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Module registration.
    /// </summary>
    internal class FormEngineWebUIModule : ModuleEntry
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FormEngineWebUIModule()
            : base(new ModuleMetadata(ModuleName.FORMENGINEWEBUI))
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            // Register form definition resolvers
            MacroResolverStorage.RegisterGetResolverHandler(FormEngineWebUIResolvers.GetResolver);
            
            MacroContext.GlobalResolver.SetHiddenNamedSourceData("SpecialFieldValue", (x) => new SpecialFieldValue());
            MacroContext.GlobalResolver.SetHiddenNamedSourceData("SpecialFieldMacro", (x) => new SpecialFieldMacro());

            FormEngineWebUISynchronization.Init();
        }
    }
}