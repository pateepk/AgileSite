using CMS.Base;
using CMS.Globalization;
using CMS.MacroEngine;

[assembly: RegisterMacroNamespace(typeof(GlobalizationNamespace), AllowAnonymous = true)]

namespace CMS.Globalization
{
    /// <summary>
    /// Macro namespace for globalization macro methods.
    /// </summary>
    [Extension(typeof(GlobalizationMethods))]
    internal sealed class GlobalizationNamespace : MacroNamespace<GlobalizationNamespace>
    {
    }
}