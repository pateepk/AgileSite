using System;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// New site wizard enumeration.
    /// </summary>
    public enum NewSiteCreationEnum
    {
        /// <summary>
        /// Blank site
        /// </summary>
        BlankSite = 0,

        /// <summary>
        /// Web site template
        /// </summary>
        WebSiteTemplate = 1,

        /// <summary>
        /// Using wizard
        /// </summary>
        UsingWizard = 2
    }


    /// <summary>
    /// Enumeration for the object import result.
    /// </summary>
    public enum ResultEnum
    {
        /// <summary>
        /// Success.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Error.
        /// </summary>
        Error = 1,

        /// <summary>
        /// Warning.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Progress.
        /// </summary>
        Progress = 3
    }
}