using CMS.MacroEngine;
using CMS.Base;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// Wrapper class to provide basic licensing methods namespace in the MacroEngine.
    /// </summary>
    [Extension(typeof(LicenseMethods))]
    internal class LicenseHelperNamespace : MacroNamespace<LicenseHelperNamespace>
    {
    }
}