using System;
using System.Linq;
using System.Text;

namespace CMS.Localization
{
    /// <summary>
    /// Localization events.
    /// </summary>
    public static class LocalizationEvents
    {
        /// <summary>
        /// Fires when looking for substitution macro value.
        /// </summary>
        public static LocalizationHandler ResolveSubstitutionMacro = new LocalizationHandler
        {
            Name = "LocalizationEvents.ResolveSubstitutionMacro", 
            Debug = false
        };
    }
}
