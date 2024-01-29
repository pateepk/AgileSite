using CMS.MacroEngine;
using CMS.Base.Web.UI.Internal;

[assembly: RegisterMacroNamespace(typeof(SystemUINamespace), AllowAnonymous = false, Hidden = true)]

namespace CMS.Base.Web.UI.Internal
{
    /// <summary>
    /// Wrapper class to provide hidden UI namespace in the MacroEngine.
    /// </summary>
    internal class SystemUINamespace : MacroNamespace<SystemUINamespace>
    {
    }
}