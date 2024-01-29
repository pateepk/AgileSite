using CMS.Base;
using CMS.MacroEngine;

[assembly: RegisterMacroNamespace(typeof(DateTimeNamespace), AllowAnonymous = true)]

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide basic DateTime namespace in the MacroEngine.
    /// </summary>
    [Extension(typeof(DateTimeStaticFields))]
    [Extension(typeof(DateTimeStaticMethods))]
    public class DateTimeNamespace : MacroNamespace<DateTimeNamespace>
    {
    }
}