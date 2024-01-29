using System.Collections.Generic;
using System.ComponentModel;

using CMS;
using CMS.Base.Web.UI.Internal;

[assembly: RegisterImplementation(typeof(IClientLocalizationProvider), typeof(ClientLocalizationProvider), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Base.Web.UI.Internal
{
    /// <summary>
    /// Provides method for obtaining all the localized strings.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IClientLocalizationProvider
    {
        /// <summary>
        /// Resolves all localization string for the given <paramref name="moduleName" />. This method assumes there exists
        /// localization file in the module folder.
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <returns>Collection of all strings obtained from the localization file localized to the current preferred UI culture</returns>
        IDictionary<string, string> GetClientLocalization(string moduleName);
    }
}