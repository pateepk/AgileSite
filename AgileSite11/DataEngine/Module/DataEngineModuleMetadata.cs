using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents the Data Engine module metadata.
    /// </summary>
    public class DataEngineModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DataEngineModuleMetadata()
            : base(ModuleName.DATAENGINE)
        {
        }
    }
}