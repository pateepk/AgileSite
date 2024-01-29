using CMS.Core;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Base class for macro namespaces.
    /// </summary>
    public class MacroNamespace<TNamespace> : IMacroNamespace 
        where TNamespace : class, new()
    {
        /// <summary>
        /// Returns singleton instance of the namespace
        /// </summary>
        public static TNamespace Instance
        {
            get
            {
                return ObjectFactory<TNamespace>.StaticSingleton();
            }
        }
    }
}