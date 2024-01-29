using CMS.Base;
using CMS.MacroEngine;

[assembly: RegisterMacroNamespace(typeof(ConvertNamespace), AllowAnonymous = true)]

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide basic conversion namespace in the MacroEngine.
    /// </summary>
    [Extension(typeof(ConvertMethods))]
    public class ConvertNamespace : MacroNamespace<ConvertNamespace>
    {
    }
}