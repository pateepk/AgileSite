using CMS.Base;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Class for storing values of macro special fields.
    /// </summary>
    public class SpecialFieldMacro : AbstractDataContainer<SpecialFieldMacro>
    {
        /// <summary>
        /// Represents 'none' or 'global' macro record.
        /// </summary>
        [RegisterColumn("None")]
        public const string NONE = "##NONE_RECORD##";


        /// <summary>
        /// Represents 'none' or 'global' macro record.
        /// </summary>
        [RegisterColumn("Default")]
        public const string DEFAULT = "##DEFAULT_RECORD##";


        /// <summary>
        /// Represents 'all' macro record.
        /// </summary>
        [RegisterColumn("All")]
        public const string ALL = "##ALL_RECORDS##";


        /// <summary>
        /// Represents 'global' macro record.
        /// </summary>
        [RegisterColumn("Global")]
        public const string GLOBAL = "##GLOBAL_RECORD##";


        /// <summary>
        /// Represents 'global and site' macro record.
        /// </summary>
        [RegisterColumn("GlobalAndSite")]
        public const string GLOBAL_AND_SITE = "##GLOBAL_AND_SITE_RECORD##";
    }
}