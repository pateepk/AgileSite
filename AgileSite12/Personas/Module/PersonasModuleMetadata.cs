using CMS.Core;

namespace CMS.Personas
{
    /// <summary>
    /// Represents metadata for the Personas module.
    /// </summary>
    internal class PersonasModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public PersonasModuleMetadata() : base(ModuleName.PERSONAS)
        {
            RootPath = "~/CMSModules/Personas/";
        }
    }
}