using CMS.Base;
using CMS.MacroEngine;

[assembly: RegisterMacroNamespace(typeof(UtilNamespace), AllowAnonymous = true)]

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide util namespace in the MacroEngine.
    /// </summary>
    [Extension(typeof(UtilMethods))]
    public class UtilNamespace : MacroNamespace<UtilNamespace>
    {
    }
}