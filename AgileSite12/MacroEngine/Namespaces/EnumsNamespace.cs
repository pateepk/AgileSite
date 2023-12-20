using CMS.Base;
using CMS.MacroEngine;

[assembly: RegisterMacroNamespace(typeof(EnumsNamespace), AllowAnonymous = true)]

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide enumerations namespace in the MacroEngine.
    /// </summary>
    [Extension(typeof(EnumMethods))]
    public class EnumsNamespace : MacroNamespace<EnumsNamespace>
    {
    }
}