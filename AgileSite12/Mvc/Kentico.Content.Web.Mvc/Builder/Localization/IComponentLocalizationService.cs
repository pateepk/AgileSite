using CMS;
using CMS.Core;

using Kentico.Builder.Web.Mvc;

[assembly: RegisterImplementation(typeof(IComponentLocalizationService), typeof(ComponentLocalizationService), Priority = RegistrationPriority.SystemDefault, Lifestyle = CMS.Core.Lifestyle.Transient)]

namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Encapsulates retrieving of localizable strings for administration UI components.
    /// </summary>
    internal interface IComponentLocalizationService
    {
        /// <summary>
        /// Gets the culture code of a culture used to localize the resource strings in administration UI.
        /// </summary>
        string GetCultureCode();


        /// <summary>
        /// Gets the default culture code to be used as fallback for localization in administration UI.
        /// </summary>
        string GetDefaultCultureCode();


        /// <summary>
        /// Gets the string by the specified resource key.
        /// </summary>
        /// <param name="resourceKey">Resource key</param>
        /// <remarks>Culture of administration UI is used to retrieve the translation.</remarks>
        string GetString(string resourceKey);


        /// <summary>
        /// Replaces "{$stringname$}" expressions in given text with localized strings.
        /// </summary>
        /// <param name="inputText">Input text with localizable expressions.</param>
        /// <remarks>Culture of administration UI is used to retrieve the translation.</remarks>
        string LocalizeString(string inputText);
    }
}