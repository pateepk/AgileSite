using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document alias action mode
    /// </summary>
    public enum AliasActionModeEnum
    {
        /// <summary>
        /// Use site settings
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation(null)]
        UseSiteSettings,

        /// <summary>
        /// Redirect to main URL
        /// </summary>
        [EnumStringRepresentation("redirect")]
        RedirectToMainURL,

        /// <summary>
        /// Do not redirect to main URL and ignore site settings for document aliases
        /// </summary>
        [EnumStringRepresentation("donothing")]
        DoNothing
    }
}
