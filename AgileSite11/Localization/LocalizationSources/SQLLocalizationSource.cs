using System;
using System.Linq;
using System.Text;

using CMS.Core;

namespace CMS.Localization
{
    internal class SQLLocalizationSource : ILocalizationStringSource
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static SQLLocalizationSource Instance
        {
            get
            {
                return ObjectFactory<SQLLocalizationSource>.StaticSingleton();
            }
        }


        /// <summary>
        /// Source default culture
        /// </summary>
        public string DefaultCulture
        {
            get
            {
                return ResourceStringInfoProvider.DefaultUICulture;
            }
        }


        /// <summary>
        /// Returns specified string from the localization string source.
        /// </summary>
        /// <param name="stringName">Name of the string</param>
        /// <param name="culture">Culture</param>
        public string GetString(string stringName, string culture)
        {
            return ResourceStringInfoProvider.GetString(stringName, culture, null, false);
        }
    }
}
