using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Captcha enumerator.
    /// </summary>
    public enum CaptchaEnum
    {
        /// <summary>
        /// Default simple captcha - image with numbers
        /// </summary>
        [EnumStringRepresentation("simple")]
        Default = 0,

        /// <summary>
        /// Logic captcha
        /// </summary>
        [EnumStringRepresentation("logic")]
        Logic = 1,

        /// <summary>
        /// Text captcha
        /// </summary>
        [EnumStringRepresentation("text")]
        Text = 2,

        /// <summary>
        /// Image and audio reCaptcha
        /// </summary>
        [EnumStringRepresentation("recaptcha")]
        ReCaptcha = 3
    }
}
