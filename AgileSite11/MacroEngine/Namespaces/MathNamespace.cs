using CMS.Base;
using CMS.MacroEngine;

[assembly: RegisterMacroNamespace(typeof(MathNamespace), AllowAnonymous = true)]

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide System.Math namespace in the MacroEngine.
    /// </summary>
    [Extension(typeof(MathMethods))]
    [Extension(typeof(MathFields))]
    public class MathNamespace : MacroNamespace<MathNamespace>
    {
    }
}