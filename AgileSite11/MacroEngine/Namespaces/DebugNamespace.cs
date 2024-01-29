using CMS.Base;
using CMS.MacroEngine;

[assembly: RegisterMacroNamespace(typeof(DebugNamespace), Hidden = true)]

namespace CMS.MacroEngine
{
    /// <summary>
    /// Class to provide debug namespace in the MacroEngine.
    /// </summary>
    [Extension(typeof(DebugSettingsMethods))]
    [Extension(typeof(DebugSettingsFields))]
    public class DebugNamespace : MacroNamespace<DebugNamespace>
    {
    }
}
