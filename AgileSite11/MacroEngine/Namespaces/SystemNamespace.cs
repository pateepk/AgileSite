using System;

using CMS.Base;
using CMS.MacroEngine;

[assembly: RegisterMacroNamespace(typeof(SystemNamespace), AllowAnonymous = true)]

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide basic system namespace in the MacroEngine.
    /// </summary>
    [Extension(typeof(SystemFields))]
    [Extension(typeof(SystemMethods))]
    [Extension(typeof(CacheMethods))]
    [Extension(typeof(DebugMethods))]
    public class SystemNamespace : MacroNamespace<SystemNamespace>
    {
    }
}