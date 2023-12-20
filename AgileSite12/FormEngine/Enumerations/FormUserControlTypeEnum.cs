namespace CMS.FormEngine
{
    /// <summary>
    /// Controls type enum.
    /// </summary>
    public enum FormUserControlTypeEnum
    {
        /// <summary>
        /// Type is not specified.
        /// </summary>
        Unspecified = -1,

        /// <summary>
        /// Input controls.
        /// </summary>
        Input = 0,

        /// <summary>
        /// Complex controls with multiple settings.
        /// </summary>
        Multifield = 1,

        /// <summary>
        /// Selectors.
        /// </summary>
        Selector = 2,

        /// <summary>
        /// Uploaders.
        /// </summary>
        Uploader = 3,

        /// <summary>
        /// Controls for displaying information.
        /// </summary>
        Viewer = 4,

        /// <summary>
        /// Visibility selectors.
        /// </summary>
        Visibility = 5,

        /// <summary>
        /// Filter control.
        /// </summary>
        Filter = 6,

        /// <summary>
        /// Captcha control
        /// </summary>
        Captcha = 7
    }
}