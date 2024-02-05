using System;
using System.Linq;
using System.Text;

using CMS.Core;

namespace CMS.Localization
{
    internal class FileLocalizationSource : ILocalizationStringSource
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static FileLocalizationSource Instance
        {
            get
            {
                return ObjectFactory<FileLocalizationSource>.StaticSingleton();
            }
        }


        /// <summary>
        /// Source default culture
        /// </summary>
        public string DefaultCulture
        {
            get
            {
                return LocalizationHelper.DefaultManager.Culture;
            }
        }


        /// <summary>
        /// Returns specified string from the localization string source.
        /// </summary>
        /// <param name="stringName">Name of the string</param>
        /// <param name="culture">Culture</param>
        public string GetString(string stringName, string culture)
        {
            return LocalizationHelper.GetFileString(stringName, culture, null, false);
        }
    }
}
