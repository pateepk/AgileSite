using CMS.Helpers;
using CMS.Membership;

namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Encapsulates retrieving of localizable strings for administration UI components.
    /// </summary>
    internal class ComponentLocalizationService : IComponentLocalizationService
    {
        private readonly string cultureCode = MembershipContext.AuthenticatedUser.PreferredUICultureCode;


        /// <summary>
        /// Gets the culture code of a culture used to localize the resource strings in administration UI.
        /// </summary>
        /// <returns></returns>
        public string GetCultureCode()
        {
            return cultureCode;
        }


        /// <summary>
        /// Gets the default culture code to be used as fallback for localization in administration UI.
        /// </summary>
        public string GetDefaultCultureCode()
        {
            return CultureHelper.DefaultUICultureCode;
        }


        /// <summary>
        /// Gets the string by the specified resource key.
        /// </summary>
        /// <param name="resourceKey">Resource key.</param>
        /// <remarks>Culture of administration UI is used to retrieve the translation.</remarks>
        public string GetString(string resourceKey)
        {
            return ResHelper.GetString(resourceKey, cultureCode);
        }


        /// <summary>
        /// Replaces "{$stringname$}" expressions in given text with localized strings.
        /// </summary>
        /// <param name="inputText">Input text with localizable expressions.</param>
        /// <remarks>Culture of administration UI is used to retrieve the translation.</remarks>
        public string LocalizeString(string inputText)
        {
            return ResHelper.LocalizeString(inputText, cultureCode);
        }
    }
}