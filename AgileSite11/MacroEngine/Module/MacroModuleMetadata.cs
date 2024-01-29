using CMS.Core;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Represents the Macro module metadata.
    /// </summary>
    public class MacroModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MacroModuleMetadata()
            : base(ModuleName.MACROENGINE)
        {
        }
    }
}