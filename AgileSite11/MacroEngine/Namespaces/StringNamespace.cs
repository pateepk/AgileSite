using CMS.Base;
using CMS.MacroEngine;

[assembly: RegisterMacroNamespace(typeof(StringNamespace), AllowAnonymous = true)]

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide static string namespace in the MacroEngine.
    /// </summary>
    [Extension(typeof(StringStaticMethods))]
    [Extension(typeof(StringStaticFields))]
    public class StringNamespace : MacroNamespace<StringNamespace>
    {
    }
}