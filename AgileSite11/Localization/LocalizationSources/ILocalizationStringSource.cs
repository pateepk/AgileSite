using System;
using System.Linq;
using System.Text;

namespace CMS.Localization
{
    internal interface ILocalizationStringSource
    {
        /// <summary>
        /// Source default culture
        /// </summary>
        string DefaultCulture
        {
            get;
        }

        /// <summary>
        /// Returns specified string from the localization string source.
        /// </summary>
        /// <param name="stringName">Name of the string</param>
        /// <param name="culture">Culture</param>
        string GetString(string stringName, string culture);        
    }
}
