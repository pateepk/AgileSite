using System.Collections.Generic;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Contains helper methods used in Ecommerce administration interface.
    /// </summary>
    public static class EcommerceUIHelper
    {
        /// <summary>
        /// Gets the message containing list of dependencies for specified ecommerce info object.
        /// </summary>
        /// <param name="infoObj">Info object.</param>
        public static string GetDependencyMessage(BaseInfo infoObj)
        {
            var message = ResHelper.GetString("Ecommerce.DeleteDisabled");

            if (infoObj == null)
            {
                return message;
            }

            var dependencies = infoObj.Generalized.GetDependenciesNames();
            return FormatDependencyMessage(dependencies, message, "<br/>");
        }


        /// <summary>
        /// Formats the message containing list of dependencies.
        /// </summary>
        /// <param name="dependencies">List of object dependencies.</param>
        /// <param name="message">Main message.</param>
        /// <param name="separator">Dependencies separator.</param>
        /// <returns></returns>
        public static string FormatDependencyMessage(List<string> dependencies, string message, string separator)
        {
            const int maxDependencies = 5;

            // Truncate message if there are too many dependencies
            var truncate = dependencies.Count > maxDependencies;
            if (truncate)
            {
                dependencies.RemoveRange(maxDependencies, dependencies.Count - maxDependencies);
            }

            dependencies = dependencies.ConvertAll(HTMLHelper.HTMLEncode);

            return string.Format("{0}{1}{2}{3}",
                message,
                separator,
                TextHelper.Join(separator, dependencies),
                truncate ? ResHelper.GetString("general.ellipsis") : string.Empty);
        }



        /// <summary>
        /// Checks if the site specified by the site ID has a main currency defined.
        /// Returns null if the main currency is defined, otherwise returns a warning message.
        /// </summary>
        /// <param name="siteId">ID of site to check</param>
        public static string CheckMainCurrency(int siteId)
        {
            if (CurrencyInfoProvider.GetMainCurrency(siteId) == null)
            {
                bool usingGlobal = ECommerceSettings.UseGlobalCurrencies(siteId);

                return ResHelper.GetString(usingGlobal ? "com.noglobalmaincurrency" : "com.nomaincurrency");
            }

            return null;
        }
    }
}
