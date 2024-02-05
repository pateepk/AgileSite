using CMS.Core;

namespace CMS.WinServiceEngine
{
    /// <summary>
    /// Represents the Windows service engine module metadata.
    /// </summary>
    public class WinServiceEngineModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WinServiceEngineModuleMetadata()
            : base(ModuleName.WINSERVICEENGINE)
        {
        }
    }
}