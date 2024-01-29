using System;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Provides helper methods for web part partial cache functionality.
    /// </summary>
    public static class PartialCacheHelper
    {
        /// <summary>
        /// Value indicating that VaryByParam/VaryByCustom are not configured yet.
        /// </summary>
        public const string VARY_BY_NOT_CONFIGURED = "notconfigured";


        /// <summary>
        /// Adds the OutputCache directive into the given ASCX markup when not present yet.
        /// </summary>
        internal static string AddOutputCacheDirective(string layoutCode)
        {
            int outputCacheDirectiveIndex = layoutCode.IndexOf("<%@ OutputCache", StringComparison.OrdinalIgnoreCase);

            if (outputCacheDirectiveIndex < 0)
            {
                // Default partial cache directive. Some of the properties will be changed later according to the web part settings (Duration, VaryByCustom, VaryByParams).
                // Customization:
                //   It the customer needs to customize the "VaryByParam/VaryByCustom" parameter, he/she has to create a custom web part layout with his/her own cache directive and set the specific value.
                //
                //   VaryByParams note:
                //     Portal engine rewrites the current URL to the "PortalTemplate.aspx?aliaspath=..." page.
                //     That means that the customer should then specify at least the "aliaspath" value in the "VaryByParam" parameter 
                //     to make sure that different pages would not share the same cached web part output.
                //
                //   VaryByCustom note:
                //     Use always "control;" prefix
                //     If "PreserveOnPostback" is enabled, then add a suffix ";preserveonpostback"
                layoutCode = String.Format(@"<%@ OutputCache Duration=""31536000"" VaryByParam=""{0}"" VaryByCustom=""{0}"" %>", VARY_BY_NOT_CONFIGURED) + layoutCode;
            }

            return layoutCode;
        }
    }
}
